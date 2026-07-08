// zlib License
//
// Copyright (c) 2026 Sinoa
//
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software
// in a product, an acknowledgment in the product documentation would be
// appreciated but is not required.
//
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
//
// 3. This notice may not be removed or altered from any source
// distribution.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.Compiler.Parser;
using SnowRabbit.IO;
using SnowRabbit.RuntimeEngine;

namespace SnowRabbit.Compiler.Assembler
{
    /// <summary>
    /// オブジェクトファイル (SROB) の内容をコンパイルコンテキストのアセンブリデータへマージするクラスです。
    /// インポートは本体スクリプトのコンパイル前に行い、シンボルの InitialAddress（負の仮想アドレス）を
    /// コンテキストの採番へ振り直した上で、未解決命令の即値も同時に書き換えます。
    /// </summary>
    internal static class SrObjectImporter
    {
        /// <summary>
        /// オブジェクトデータをコンパイルコンテキストへインポートします
        /// </summary>
        /// <param name="objectData">インポートするオブジェクトデータ</param>
        /// <param name="context">インポート先のコンパイルコンテキスト</param>
        /// <param name="linkToken">エラー報告に使用する #link のトークン</param>
        /// <param name="objectPath">エラー報告や復元リテラルの出所として使用するオブジェクトファイルのパス</param>
        public static void Import(SrObjectData objectData, SrCompileContext context, in Token linkToken, string objectPath)
        {
            var assemblyData = context.AssemblyData;
            var remap = new Dictionary<int, int>();
            var dedupedPeripheralGlobalNames = new HashSet<string>();


            // パス1a: ペリフェラル関数（同名同シグネチャは既存へ合流させる）
            foreach (var symbolData in objectData.Symbols)
            {
                if (symbolData.Kind != SrSymbolKind.PeripheralFunction) continue;


                var existingSymbol = assemblyData.GetGlobalSymbol(symbolData.Name);
                if (existingSymbol != null)
                {
                    var parameterTypes = symbolData.Parameters.Select(x => x.Type).ToArray();
                    if (existingSymbol is SrPeripheralFunctionSymbol existingPeripheral &&
                        existingPeripheral.SignatureEquals(symbolData.Type, symbolData.PeripheralName, symbolData.PeripheralFunctionName, parameterTypes))
                    {
                        // 同一シグネチャなら既存シンボルへ合流し、ペアのグローバル変数も既存側へ合流させる
                        remap[symbolData.InitialAddress] = existingPeripheral.InitialAddress;
                        dedupedPeripheralGlobalNames.Add(existingPeripheral.PeripheralGlobalVariableName);
                        continue;
                    }


                    // 同名で内容の異なるシンボルは衝突
                    throw context.ErrorReporter.PredefinedSymbol(linkToken, symbolData.Name);
                }


                var peripheral = new SrPeripheralFunctionSymbol(symbolData.Name, context.AllocateVirtualAddress());
                peripheral.PeripheralName = symbolData.PeripheralName;
                peripheral.PeripheralFunctionName = symbolData.PeripheralFunctionName;
                peripheral.ReturnType = symbolData.Type;
                AddParameters(peripheral, symbolData.Parameters);
                assemblyData.AddSymbol(peripheral);
                remap[symbolData.InitialAddress] = peripheral.InitialAddress;
            }


            // パス1b: スクリプト関数・グローバル変数・定数
            foreach (var symbolData in objectData.Symbols)
            {
                switch (symbolData.Kind)
                {
                    case SrSymbolKind.ScriptFunction:
                        ImportScriptFunction(symbolData, context, linkToken, remap);
                        break;

                    case SrSymbolKind.GlobalVariable:
                        ImportGlobalVariable(symbolData, context, linkToken, objectPath, remap, dedupedPeripheralGlobalNames);
                        break;

                    case SrSymbolKind.Constant:
                        ImportConstant(symbolData, context, linkToken, objectPath, remap);
                        break;
                }
            }


            // パス1c: ラベル（名前は関数名を含むため、関数の衝突検査を通過していれば衝突しない）
            foreach (var symbolData in objectData.Symbols)
            {
                if (symbolData.Kind != SrSymbolKind.Label) continue;


                var label = new SrLabelSymbol(symbolData.Name, context.AllocateVirtualAddress());
                label.FunctionName = symbolData.FunctionName;
                label.Address = symbolData.LabelAddress;
                if (!assemblyData.AddSymbol(label))
                {
                    throw new SrMalformedObjectDataException($"ラベル '{symbolData.Name}' が重複しています");
                }
                remap[symbolData.InitialAddress] = label.InitialAddress;
            }


            // パス1d: 文字列（テキスト単位で既存と合流させる）
            foreach (var stringData in objectData.Strings)
            {
                var existingString = assemblyData.GetStringSymbol(stringData.Text);
                if (existingString != null)
                {
                    remap[stringData.InitialAddress] = existingString.InitialAddress;
                    continue;
                }


                var stringSymbol = new SrStringSymbol(stringData.Text, context.AllocateVirtualAddress());
                assemblyData.AddSymbol(stringSymbol);
                remap[stringData.InitialAddress] = stringSymbol.InitialAddress;
            }


            // パス2: 関数コードの未解決即値を新しい仮想アドレスへ書き換えて登録する
            foreach (var functionCode in objectData.FunctionCodes)
            {
                if (assemblyData.GetFunctionCode(functionCode.FunctionName) != null)
                {
                    // 関数シンボルの衝突検査を通過してコードだけ重複することは無いはずだが、壊れたオブジェクトとして防御する
                    throw new SrMalformedObjectDataException($"関数コード '{functionCode.FunctionName}' が重複しています");
                }


                var codes = new SrAssemblyCode[functionCode.Codes.Count];
                for (int i = 0; i < codes.Length; ++i)
                {
                    var instructionData = functionCode.Codes[i];
                    var instruction = default(SrInstruction);
                    instruction.Raw = instructionData.Raw;
                    if (instructionData.UnresolvedAddress)
                    {
                        if (!remap.TryGetValue(instruction.Int, out var remappedAddress))
                        {
                            throw new SrMalformedObjectDataException($"関数 '{functionCode.FunctionName}' の未解決参照 '{instruction.Int}' に対応するシンボルがありません");
                        }
                        instruction.Int = remappedAddress;
                    }
                    codes[i] = new SrAssemblyCode(instruction, instructionData.UnresolvedAddress);
                }
                assemblyData.SetFunctionCode(functionCode.FunctionName, codes);
            }
        }


        private static void ImportScriptFunction(SrObjectSymbolData symbolData, SrCompileContext context, in Token linkToken, Dictionary<int, int> remap)
        {
            if (context.AssemblyData.GetGlobalSymbol(symbolData.Name) != null)
            {
                throw context.ErrorReporter.PredefinedSymbol(linkToken, symbolData.Name);
            }


            var function = new SrScriptFunctionSymbol(symbolData.Name, context.AllocateVirtualAddress());
            function.ReturnType = symbolData.Type;
            AddParameters(function, symbolData.Parameters);
            foreach (var localVariable in symbolData.LocalVariables)
            {
                function.AddOrGetLocalVariable(localVariable.Name, localVariable.Type);
            }
            foreach (var register in symbolData.UsedRegisters)
            {
                function.UsedRegisterSet.Add(register);
            }
            context.AssemblyData.AddSymbol(function);
            remap[symbolData.InitialAddress] = function.InitialAddress;
        }


        private static void ImportGlobalVariable(SrObjectSymbolData symbolData, SrCompileContext context, in Token linkToken, string objectPath, Dictionary<int, int> remap, HashSet<string> dedupedPeripheralGlobalNames)
        {
            // dedupe されたペリフェラル関数のペアとなるグローバル変数は、既存側へ合流させる
            if (dedupedPeripheralGlobalNames.Contains(symbolData.Name))
            {
                var existingGlobal = context.AssemblyData.GetGlobalSymbol(symbolData.Name);
                if (existingGlobal == null)
                {
                    throw new SrMalformedObjectDataException($"ペリフェラル関数のグローバル変数 '{symbolData.Name}' が見つかりません");
                }
                remap[symbolData.InitialAddress] = existingGlobal.InitialAddress;
                return;
            }


            if (context.AssemblyData.GetGlobalSymbol(symbolData.Name) != null)
            {
                throw context.ErrorReporter.PredefinedSymbol(linkToken, symbolData.Name);
            }


            var globalVariable = new SrGlobalVariableSymbol(symbolData.Name, context.AllocateVirtualAddress());
            globalVariable.Type = symbolData.Type;
            globalVariable.InitializeLiteral = RebuildLiteralToken(symbolData, objectPath);
            context.AssemblyData.AddSymbol(globalVariable);
            remap[symbolData.InitialAddress] = globalVariable.InitialAddress;
        }


        private static void ImportConstant(SrObjectSymbolData symbolData, SrCompileContext context, in Token linkToken, string objectPath, Dictionary<int, int> remap)
        {
            if (context.AssemblyData.GetGlobalSymbol(symbolData.Name) != null)
            {
                throw context.ErrorReporter.PredefinedSymbol(linkToken, symbolData.Name);
            }


            var constant = new SrConstantSymbol(symbolData.Name, context.AllocateVirtualAddress());
            constant.ConstantValue = RebuildLiteralToken(symbolData, objectPath);
            constant.Type = symbolData.Type;
            context.AssemblyData.AddSymbol(constant);
            remap[symbolData.InitialAddress] = constant.InitialAddress;
        }


        private static void AddParameters(SrFunctionSymbol function, List<SrObjectTypedNameData> parameters)
        {
            foreach (var parameter in parameters)
            {
                function.AddOrGetParameter(parameter.Name, parameter.Type);
            }
        }


        private static Token RebuildLiteralToken(SrObjectSymbolData symbolData, string objectPath)
        {
            // リテラルを持たない場合は既定のトークン（Text == null）を返す
            if (!symbolData.HasLiteral) return default;
            return new Token(symbolData.LiteralKind, symbolData.LiteralText, symbolData.LiteralInteger, symbolData.LiteralNumber, objectPath, 0, 0);
        }
    }
}
