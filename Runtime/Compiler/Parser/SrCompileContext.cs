﻿// zlib/libpng License
//
// Copyright(c) 2020 Sinoa
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the authors be held liable for any damages arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it freely,
// subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software.
//    If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.

using System;
using System.Collections.Generic;
using SnowRabbit.Compiler.Assembler;
using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.Compiler.Lexer;
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
        private List<SrAssemblyCode> headCodeList = new List<SrAssemblyCode>(1024);
        private List<SrAssemblyCode> bodyCodeList = new List<SrAssemblyCode>(1024);
        private List<SrAssemblyCode> tailCodeList = new List<SrAssemblyCode>(1024);



        /// <summary>
        /// 現在のコンテキストが持っているアセンブリデータ
        /// </summary>
        public SrAssemblyData AssemblyData { get; } = new SrAssemblyData();


        /// <summary>
        /// 現在コンパイルをしている関数名
        /// </summary>
        public string CurrentCompileFunctionName { get; private set; }


        /// <summary>
        /// 現在書き込まれているコードリスト
        /// </summary>
        public IReadOnlyList<SrAssemblyCode> CodeList;



        /// <summary>
        /// 次に使用するべき仮想アドレスを取得します
        /// </summary>
        /// <returns>使用するべき仮想アドレスを返します</returns>
        private int GetNextVirtualAddress()
        {
            // ひたすらデクリメントし続けるアドレスを返す
            return nextVirtualAddress--;
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


        public SrStringSymbol CreateOrGetStringSymbol(string text)
        {
            var symbol = AssemblyData.GetStringSymbol(text);
            if (symbol != null) return symbol;


            symbol = new SrStringSymbol(text, GetNextVirtualAddress());
            AssemblyData.AddSymbol(symbol);
            return symbol;
        }


        public SrScriptFunctionSymbol EnterFunctionCompile(SrRuntimeType returnType, string functionName)
        {
            var symbol = new SrScriptFunctionSymbol(functionName, GetNextVirtualAddress());
            symbol.ReturnType = returnType;
            if (!AssemblyData.AddSymbol(symbol)) return null;
            CurrentCompileFunctionName = functionName;
            return symbol;
        }


        public void ExitFunctionCompile()
        {
            var codeLength = headCodeList.Count + bodyCodeList.Count + tailCodeList.Count;
            var codeArray = new SrAssemblyCode[codeLength];
            headCodeList.CopyTo(codeArray);
            bodyCodeList.CopyTo(codeArray, headCodeList.Count);
            tailCodeList.CopyTo(codeArray, headCodeList.Count + bodyCodeList.Count);


            AssemblyData.SetFunctionCode(CurrentCompileFunctionName, codeArray);
            CurrentCompileFunctionName = null;


            headCodeList.Clear();
            bodyCodeList.Clear();
            tailCodeList.Clear();
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
    }
}
