// zlib License
// 
// Copyright (c) 2020 Sinoa
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
using SnowRabbit.Compiler.Assembler;
using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.Compiler.Reporter;
using SnowRabbit.RuntimeEngine;

namespace SnowRabbit.Compiler.Parser
{
    /// <summary>
    /// 構文ノードによるコンパイルコンテキストを持つクラスです
    /// </summary>
    public class SrCompileContext
    {
        // メンバ変数定義
        private int nextVirtualAddress = -1;
        private readonly List<SrAssemblyCode> headCodeList = new List<SrAssemblyCode>(1024);
        private readonly List<SrAssemblyCode> bodyCodeList = new List<SrAssemblyCode>(1024);
        private readonly List<SrAssemblyCode> tailCodeList = new List<SrAssemblyCode>(1024);
        private readonly Stack<SrLabelSymbol> breakTargetAddressStack = new Stack<SrLabelSymbol>();
        private readonly List<SrLabelSymbol> patchTargetLabelList = new List<SrLabelSymbol>();

        // 空きレジスタ管理スタック（文の境界でリセットして再利用する）
        private readonly Stack<byte> pooledFreeRegisterStack = new Stack<byte>(25);


        /// <summary>
        /// 現在のコンテキストが持っているアセンブリデータ
        /// </summary>
        public SrAssemblyData AssemblyData { get; } = new SrAssemblyData();


        /// <summary>
        /// リンク可能なオブジェクトとしてコンパイルするモードかどうか。
        /// オブジェクトモードでは main 関数が不要になり、スタートアップコード（___init）を生成しません。
        /// </summary>
        public bool IsObjectCompileMode { get; }


        public SyntaxErrorReporter ErrorReporter { get; }


        /// <summary>
        /// 現在コンパイルをしている関数名
        /// </summary>
        public string CurrentCompileFunctionName { get; private set; }


        /// <summary>
        /// 現在コンパイルをしている関数のシンボル
        /// </summary>
        public SrScriptFunctionSymbol CurrentFunctionSymbol { get; private set; }


        public SrLabelSymbol CurrentFunctionLeaveLabelSymbol { get; private set; }


        public SrLabelSymbol CurrentBreakTargetLabel => breakTargetAddressStack.Count > 0 ? breakTargetAddressStack.Peek() : null;


        public IReadOnlyList<SrAssemblyCode> HeadCodeList { get; }


        /// <summary>
        /// 現在書き込まれているコードリスト
        /// </summary>
        public IReadOnlyList<SrAssemblyCode> BodyCodeList { get; }


        public IReadOnlyList<SrAssemblyCode> TailCodeList { get; }


        public SrCompileContext() : this(new SrCompileReportConsolePrinter())
        {
        }


        public SrCompileContext(ISrCompileReportPrinter printer) : this(printer, false)
        {
        }


        public SrCompileContext(ISrCompileReportPrinter printer, bool isObjectCompileMode)
        {
            HeadCodeList = headCodeList.AsReadOnly();
            BodyCodeList = bodyCodeList.AsReadOnly();
            TailCodeList = tailCodeList.AsReadOnly();
            ErrorReporter = new SyntaxErrorReporter(printer);
            IsObjectCompileMode = isObjectCompileMode;
        }


        /// <summary>
        /// 次に使用するべき仮想アドレスを取得します
        /// </summary>
        /// <returns>使用するべき仮想アドレスを返します</returns>
        private int GetNextVirtualAddress()
        {
            // ひたすらデクリメントし続けるアドレスを返す
            return nextVirtualAddress--;
        }


        /// <summary>
        /// 新しい仮想アドレスを割り当てます。
        /// オブジェクトファイルのインポートなど、シンボル生成メソッドを経由しないシンボル構築に使用します。
        /// </summary>
        /// <returns>割り当てられた仮想アドレスを返します</returns>
        public int AllocateVirtualAddress()
        {
            return GetNextVirtualAddress();
        }


        public SrRuntimeType ToRuntimeType(int typeKind)
        {
            return
                typeKind == SrTokenKind.TypeVoid ? SrRuntimeType.Void :
                typeKind == SrTokenKind.TypeInt ? SrRuntimeType.Integer :
                typeKind == SrTokenKind.TypeNumber ? SrRuntimeType.Number :
                typeKind == SrTokenKind.TypeString ? SrRuntimeType.String :
                typeKind == SrTokenKind.TypeObject ? SrRuntimeType.Object :
                typeKind == SrTokenKind.TypeBool ? SrRuntimeType.Boolean :
                typeKind == TokenKind.Integer ? SrRuntimeType.Integer :
                typeKind == TokenKind.Number ? SrRuntimeType.Number :
                typeKind == TokenKind.String ? SrRuntimeType.String :
                typeKind == SrTokenKind.Null ? SrRuntimeType.Object :
                typeKind == SrTokenKind.True ? SrRuntimeType.Boolean :
                typeKind == SrTokenKind.False ? SrRuntimeType.Boolean :
                SrRuntimeType.Void;
        }


        public SrPeripheralFunctionSymbol CreatePeripheralFunctionSymbol(SrRuntimeType returnType, string functionName, string peripheralName, string peripheralFuncName)
        {
            var symbol = new SrPeripheralFunctionSymbol(functionName, GetNextVirtualAddress());
            symbol.PeripheralName = peripheralName;
            symbol.PeripheralFunctionName = peripheralFuncName;
            symbol.ReturnType = returnType;
            return AssemblyData.AddSymbol(symbol) ? symbol : null;
        }


        public SrGlobalVariableSymbol CreateGlobalVariableSymbol(SrRuntimeType type, string name, in Token literal)
        {
            var symbol = new SrGlobalVariableSymbol(name, GetNextVirtualAddress());
            symbol.Type = type;
            symbol.InitializeLiteral = literal;
            return AssemblyData.AddSymbol(symbol) ? symbol : null;
        }


        public SrConstantSymbol CreateConstantSymbol(string name, in Token literal)
        {
            var symbol = new SrConstantSymbol(name, GetNextVirtualAddress());
            symbol.ConstantValue = literal;
            symbol.Type = ToRuntimeType(literal.Kind);
            return AssemblyData.AddSymbol(symbol) ? symbol : null;
        }


        public SrLabelSymbol CreateLabelSymbol(string labelName)
        {
            var symbol = new SrLabelSymbol(labelName, GetNextVirtualAddress());
            return AssemblyData.AddSymbol(symbol) ? symbol : null;
        }


        public SrStringSymbol CreateOrGetStringSymbol(string text)
        {
            var symbol = AssemblyData.GetStringSymbol(text);
            if (symbol != null) return symbol;

            symbol = new SrStringSymbol(text, GetNextVirtualAddress());
            AssemblyData.AddSymbol(symbol);
            return symbol;
        }


        public SrLabelSymbol EnterNestedBlock(string blockName)
        {
            if (string.IsNullOrWhiteSpace(CurrentCompileFunctionName))
            {
                // 関数定義前にネストされたブロックは許容しない
                throw new System.InvalidOperationException();
            }

            if (string.IsNullOrWhiteSpace(blockName))
            {
                // 有効なブロック名であるべき
                throw new System.Exception();
            }

            var labelSymbol = CreateLabelSymbol($"___NB_{CurrentCompileFunctionName}_{blockName}_{GetNextVirtualAddress()}___");
            labelSymbol.FunctionName = CurrentCompileFunctionName;
            breakTargetAddressStack.Push(labelSymbol);
            return labelSymbol;
        }


        public void ExitNestedBlock()
        {
            var symbol = breakTargetAddressStack.Pop();
            symbol.Address = bodyCodeList.Count;
            patchTargetLabelList.Add(symbol);
        }


        public SrScriptFunctionSymbol EnterFunctionCompile(SrRuntimeType returnType, string functionName)
        {
            var symbol = new SrScriptFunctionSymbol(functionName, GetNextVirtualAddress());
            var leaveLabel = CreateLabelSymbol($"___{functionName}_LeaveLable___");
            symbol.ReturnType = returnType;
            leaveLabel.FunctionName = functionName;
            if (!AssemblyData.AddSymbol(symbol)) return null;
            CurrentCompileFunctionName = functionName;
            CurrentFunctionSymbol = symbol;
            CurrentFunctionLeaveLabelSymbol = leaveLabel;
            return symbol;
        }


        public void ExitFunctionCompile()
        {
            var codeLength = headCodeList.Count + bodyCodeList.Count + tailCodeList.Count;
            var codeArray = new SrAssemblyCode[codeLength];
            headCodeList.CopyTo(codeArray);
            bodyCodeList.CopyTo(codeArray, headCodeList.Count);
            tailCodeList.CopyTo(codeArray, headCodeList.Count + bodyCodeList.Count);

            foreach (var labelSymbol in patchTargetLabelList)
            {
                labelSymbol.Address += headCodeList.Count;
            }

            AssemblyData.SetFunctionCode(CurrentCompileFunctionName, codeArray);
            CurrentFunctionLeaveLabelSymbol.Address = HeadCodeList.Count + bodyCodeList.Count;

            CurrentCompileFunctionName = null;
            CurrentFunctionSymbol = null;
            CurrentFunctionLeaveLabelSymbol = null;

            headCodeList.Clear();
            bodyCodeList.Clear();
            tailCodeList.Clear();
            patchTargetLabelList.Clear();
        }


        public void AddHeadCode(in SrInstruction instruction, bool unresolved)
        {
            headCodeList.Add(new SrAssemblyCode(instruction, unresolved));
        }


        public void AddBodyCode(in SrInstruction instruction, bool unresolved)
        {
            bodyCodeList.Add(new SrAssemblyCode(instruction, unresolved));
        }


        public void AddTailCode(in SrInstruction instruction, bool unresolved)
        {
            tailCodeList.Add(new SrAssemblyCode(instruction, unresolved));
        }


        public void UpdateHeadCode(int index, in SrInstruction instruction, bool unresolved)
        {
            headCodeList[index] = new SrAssemblyCode(instruction, unresolved);
        }


        public void UpdateBodyCode(int index, in SrInstruction instruction, bool unresolved)
        {
            bodyCodeList[index] = new SrAssemblyCode(instruction, unresolved);
        }


        public void UpdateTailCode(int index, in SrInstruction instruction, bool unresolved)
        {
            tailCodeList[index] = new SrAssemblyCode(instruction, unresolved);
        }


        #region レジスタプール管理
        /// <summary>
        /// 空きレジスタを1つ取得します。取得したレジスタは現在コンパイル中の関数の使用済みレジスタ集合へ記録されます。
        /// </summary>
        /// <param name="token">レジスタ枯渇時のエラー報告に使用するトークン</param>
        /// <returns>取得したレジスタ番号を返します</returns>
        public byte TakeFreeRegisterIndex(in Token token)
        {
            if (pooledFreeRegisterStack.Count == 0)
            {
                // 空きレジスタが枯渇した（式が複雑すぎる）
                throw ErrorReporter.RegisterExhausted(token);
            }


            // 取得したレジスタは関数のプロローグ/エピローグで退避されるように、取得時点で使用済みとして記録する
            var registerIndex = pooledFreeRegisterStack.Pop();
            CurrentFunctionSymbol?.UsedRegisterSet.Add(registerIndex);
            return registerIndex;
        }


        /// <summary>
        /// 使い終わったレジスタを空きレジスタとして返却します
        /// </summary>
        /// <param name="registerIndex">返却するレジスタ番号</param>
        public void ReleaseRegisterIndex(byte registerIndex)
        {
            pooledFreeRegisterStack.Push(registerIndex);
        }


        /// <summary>
        /// レジスタプールをリセットして再利用可能な状態にします。
        /// 文の境界（文のコンパイル開始時）に呼び出してください。
        /// </summary>
        public void ResetRegisterPool()
        {
            pooledFreeRegisterStack.Clear();

            // 使用可能なレジスタをスタックにプッシュ（逆順でプッシュして期待順序でポップ）
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR28Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR27Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR26Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR25Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR24Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR23Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR22Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR21Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR20Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR19Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR18Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR17Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR16Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR15Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR14Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR13Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR12Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR11Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR10Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR9Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterR8Index);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterDIndex);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterCIndex);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterBIndex);
            pooledFreeRegisterStack.Push(RuntimeEngine.VirtualMachine.SrvmProcessor.RegisterAIndex);
        }
        #endregion
    }
}
