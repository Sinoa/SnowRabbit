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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SnowRabbit.Compiler.Assembler;
using SnowRabbit.Compiler.Assembler.Symbols;

namespace SnowRabbit.IO
{
    /// <summary>
    /// アドレス解決前のアセンブリデータを、リンク可能なオブジェクトファイル (SROB) として書き込むクラスです
    /// </summary>
    public class SrObjectDataWriter : SrDisposable
    {
        // メンバ変数定義
        private bool disposed;
        private readonly SrBinaryIO binaryIO;



        /// <summary>
        /// SrObjectDataWriter クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">書き込み先のストリーム</param>
        /// <param name="leaveOpen">このインスタンスを破棄した時にストリームを開いたままにする場合は true</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        /// <exception cref="ArgumentException">stream に書き込みが許可されていません</exception>
        public SrObjectDataWriter(Stream stream, bool leaveOpen)
        {
            if (!(stream ?? throw new ArgumentNullException(nameof(stream))).CanWrite)
            {
                throw new ArgumentException("stream に書き込みが許可されていません");
            }


            binaryIO = new SrBinaryIO(stream, leaveOpen);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposed) return;


            if (disposing)
            {
                binaryIO.Dispose();
            }


            disposed = true;
            base.Dispose(disposing);
        }


        /// <summary>
        /// 指定されたアセンブリデータをオブジェクトファイルとして書き込みます
        /// </summary>
        /// <param name="data">書き込むアセンブリデータ</param>
        /// <exception cref="ArgumentNullException">data が null です</exception>
        public void Write(SrAssemblyData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));


            // 文字列シンボルは専用テーブルとして別枠で書き込むため、グローバルシンボル表からは除外する
            var symbols = data.GetSymbolAll<SrSymbol>().Where(x => !(x is SrStringSymbol)).ToArray();
            var strings = data.GetSymbolAll<SrStringSymbol>().ToArray();


            // ヘッダを書き込む
            binaryIO.Write(SrObjectData.MagicNumber);
            binaryIO.Write(SrObjectData.FormatVersion);
            binaryIO.Write(symbols.Length);
            binaryIO.Write(strings.Length);
            binaryIO.Write(data.functionCodeTable.Count);


            // シンボル表を書き込む
            foreach (var symbol in symbols)
            {
                WriteSymbol(symbol);
            }


            // 文字列表を書き込む
            foreach (var stringSymbol in strings)
            {
                binaryIO.Write(stringSymbol.String);
                binaryIO.Write(stringSymbol.InitialAddress);
            }


            // 関数コード表を書き込む
            foreach (var function in data.functionCodeTable)
            {
                binaryIO.Write(function.Key);
                binaryIO.Write(function.Value.Length);
                foreach (var code in function.Value)
                {
                    binaryIO.Write(code.Instruction.Raw);
                    binaryIO.Write((byte)(code.UnresolvedAddress ? 1 : 0));
                }
            }
        }


        private void WriteSymbol(SrSymbol symbol)
        {
            binaryIO.Write((int)symbol.Kind);
            binaryIO.Write(symbol.Name);
            binaryIO.Write(symbol.InitialAddress);


            switch (symbol)
            {
                case SrPeripheralFunctionSymbol peripheralFunction:
                    binaryIO.Write((int)peripheralFunction.ReturnType);
                    binaryIO.Write(peripheralFunction.PeripheralName);
                    binaryIO.Write(peripheralFunction.PeripheralFunctionName);
                    WriteParameters(peripheralFunction.ParameterTable);
                    return;

                case SrScriptFunctionSymbol scriptFunction:
                    binaryIO.Write((int)scriptFunction.ReturnType);
                    WriteParameters(scriptFunction.ParameterTable);
                    WriteLocalVariables(scriptFunction.LocalVariableTable);
                    binaryIO.Write(scriptFunction.UsedRegisterSet.Count);
                    foreach (var register in scriptFunction.UsedRegisterSet)
                    {
                        binaryIO.Write(register);
                    }
                    return;

                case SrGlobalVariableSymbol globalVariable:
                    binaryIO.Write((int)globalVariable.Type);
                    WriteLiteral(globalVariable.InitializeLiteral);
                    return;

                case SrConstantSymbol constant:
                    binaryIO.Write((int)constant.Type);
                    WriteLiteral(constant.ConstantValue);
                    return;

                case SrLabelSymbol label:
                    binaryIO.Write(label.FunctionName ?? string.Empty);
                    binaryIO.Write(label.Address);
                    return;
            }


            // オブジェクトへ書き込めないシンボル種別（ローカル変数などのトップレベルに現れないはずのシンボル）
            throw new SrMalformedObjectDataException($"オブジェクトファイルへ書き込めないシンボル種別 '{symbol.Kind}' です");
        }


        private void WriteParameters(Dictionary<string, SrParameterVariableSymbol> parameterTable)
        {
            // パラメータは位置順に書き込む（読み込み時に同じ順で再構築される）
            var parameters = parameterTable.Values.OrderBy(x => x.Position).ToArray();
            binaryIO.Write(parameters.Length);
            foreach (var parameter in parameters)
            {
                binaryIO.Write(parameter.Name);
                binaryIO.Write((int)parameter.Type);
            }
        }


        private void WriteLocalVariables(Dictionary<string, SrLocalVariableSymbol> localVariableTable)
        {
            // ローカル変数は割り当て順（InitialAddress順）に書き込む
            var localVariables = localVariableTable.Values.OrderBy(x => x.InitialAddress).ToArray();
            binaryIO.Write(localVariables.Length);
            foreach (var localVariable in localVariables)
            {
                binaryIO.Write(localVariable.Name);
                binaryIO.Write((int)localVariable.Type);
            }
        }


        private void WriteLiteral(in Compiler.Lexer.Token literal)
        {
            // Text が null のトークンは「リテラル無し」として扱う（SrBinaryIO は null 文字列を書き込めないため）
            var hasLiteral = literal.Text != null;
            binaryIO.Write((byte)(hasLiteral ? 1 : 0));
            if (!hasLiteral) return;


            binaryIO.Write(literal.Kind);
            binaryIO.Write(literal.Text);
            binaryIO.Write(literal.Integer);
            binaryIO.Write(literal.Number);
        }
    }
}
