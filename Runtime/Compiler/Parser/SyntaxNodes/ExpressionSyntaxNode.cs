// zlib/libpng License
//
// Copyright(c) 2019 Sinoa
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

using System.Collections.Generic;
using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 式構文を表す構文ノードクラスです
    /// </summary>
    public class ExpressionSyntaxNode : SyntaxNode
    {
        // メンバ変数定義
        private HashSet<byte> usedRegisterIndexs;
        private Stack<byte> freeRegisterStack;



        /// <summary>
        /// この式構文による結果を出力する先のレジスタインデックス
        /// </summary>
        public byte ResultRegisterIndex { get; private set; }



        /// <summary>
        /// ExpressionSyntaxNode クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="token">対応するトークン</param>
        public ExpressionSyntaxNode(in Token token) : base(token)
        {
        }


        #region Register control utility
        private static Stack<byte> CreateFreeRegisterStack()
        {
            return new Stack<byte>(new byte[]
            {
                SrvmProcessor.RegisterR28Index,
                SrvmProcessor.RegisterR27Index,
                SrvmProcessor.RegisterR26Index,
                SrvmProcessor.RegisterR25Index,
                SrvmProcessor.RegisterR24Index,
                SrvmProcessor.RegisterR23Index,
                SrvmProcessor.RegisterR22Index,
                SrvmProcessor.RegisterR21Index,
                SrvmProcessor.RegisterR20Index,
                SrvmProcessor.RegisterR19Index,
                SrvmProcessor.RegisterR18Index,
                SrvmProcessor.RegisterR17Index,
                SrvmProcessor.RegisterR16Index,
                SrvmProcessor.RegisterR15Index,
                SrvmProcessor.RegisterR14Index,
                SrvmProcessor.RegisterR13Index,
                SrvmProcessor.RegisterR12Index,
                SrvmProcessor.RegisterR11Index,
                SrvmProcessor.RegisterR10Index,
                SrvmProcessor.RegisterR9Index,
                SrvmProcessor.RegisterR8Index,
                SrvmProcessor.RegisterDIndex,
                SrvmProcessor.RegisterCIndex,
                SrvmProcessor.RegisterBIndex,
                SrvmProcessor.RegisterAIndex,
            });
        }


        private void InitializeRegisterInformation()
        {
            if (Parent is ExpressionSyntaxNode parentExpression)
            {
                usedRegisterIndexs = parentExpression.usedRegisterIndexs;
                freeRegisterStack = parentExpression.freeRegisterStack;
                return;
            }


            usedRegisterIndexs = new HashSet<byte>();
            freeRegisterStack = CreateFreeRegisterStack();
        }


        private byte TakeFreeRegisterIndex()
        {
            if (freeRegisterStack.Count == 0)
            {
                // 空きレジスタがもうない
                throw new System.Exception();
            }


            var freeRegisterIndex = freeRegisterStack.Pop();
            usedRegisterIndexs.Add(freeRegisterIndex);
            return freeRegisterIndex;
        }


        private void ReleaseRegister(byte registerIndex)
        {
            freeRegisterStack.Push(registerIndex);
        }
        #endregion


        #region Load Store control
        #endregion





        public override void Compile(SrCompileContext context)
        {
            InitializeRegisterInformation();


            if (Children.Count == 1)
            {
                CompileUnaryExpression(Children[0], Token, context);
            }
            else if (Children.Count == 2)
            {
                CompileExpression(Children[0], Children[1], Token, context);
            }
        }


        private void CompileStoreCode(IdentifierSyntaxNode node, byte targetRegisterIndex, SrCompileContext context)
        {
            var variableSymbol = context.AssemblyData.GetVariableSymbol(node.Token.Text, context.CurrentCompileFunctionName);
            var instruction = new SrInstruction();
            if (variableSymbol is SrGlobalVariableSymbol)
            {
                instruction.Set(OpCode.Strl, targetRegisterIndex, 0, 0, variableSymbol.InitialAddress);
                context.AddBodyCode(instruction, true);
            }
            else if (variableSymbol is SrLocalVariableSymbol)
            {
                instruction.Set(OpCode.Str, targetRegisterIndex, SrvmProcessor.RegisterBPIndex, 0, -variableSymbol.Address - 1);
                context.AddBodyCode(instruction, false);
            }
            else if (variableSymbol is SrParameterVariableSymbol)
            {
                instruction.Set(OpCode.Str, targetRegisterIndex, SrvmProcessor.RegisterBPIndex, 0, variableSymbol.Address + 1);
                context.AddBodyCode(instruction, false);
            }
        }


        private void CompileLoadCode(IdentifierSyntaxNode node, byte targetRegisterIndex, SrCompileContext context)
        {
            var variableSymbol = context.AssemblyData.GetVariableSymbol(node.Token.Text, context.CurrentCompileFunctionName);
            var instruction = new SrInstruction();
            if (variableSymbol is SrGlobalVariableSymbol)
            {
                instruction.Set(OpCode.Ldrl, targetRegisterIndex, 0, 0, variableSymbol.InitialAddress);
                context.AddBodyCode(instruction, true);
            }
            else if (variableSymbol is SrLocalVariableSymbol)
            {
                instruction.Set(OpCode.Ldr, targetRegisterIndex, SrvmProcessor.RegisterBPIndex, 0, -variableSymbol.Address - 1);
                context.AddBodyCode(instruction, false);
            }
            else if (variableSymbol is SrParameterVariableSymbol)
            {
                instruction.Set(OpCode.Ldr, targetRegisterIndex, SrvmProcessor.RegisterBPIndex, 0, variableSymbol.Address + 1);
                context.AddBodyCode(instruction, false);
            }
        }


        private void CompileUnaryExpression(SyntaxNode expression, in Token operation, SrCompileContext context)
        {
            SrRuntimeType returnType = SrRuntimeType.Void;
            if (expression is LiteralSyntaxNode)
            {
                returnType = context.ToRuntimeType(expression.Token.Kind);
            }


            if (expression is FunctionCallSyntaxNode functionCallExpression)
            {
                var functionSymbol = context.AssemblyData.GetFunctionSymbol(functionCallExpression.Children[0].Token.Text);
                if (functionSymbol == null)
                {
                    // そんな関数はなかった
                    throw new System.Exception();
                }


                returnType = functionSymbol.ReturnType;
            }


            if (expression is IdentifierSyntaxNode identifierExpression)
            {
                var targetRegister = TakeFreeRegisterIndex();

            }


            if (returnType == SrRuntimeType.String || returnType == SrRuntimeType.Object || returnType == SrRuntimeType.Void)
            {
                // 文字列 オブジェクト void に対する単項式は今の所未実装
                throw new System.Exception();
            }


            if (expression is LiteralSyntaxNode literalSyntax)
            {
                literalSyntax.StoreRegisterIndex = TakeFreeRegisterIndex();
                expression.Compile(context);
                ReleaseRegister(literalSyntax.StoreRegisterIndex);
            }
            else
            {
                expression.Compile(context);
            }


            var instruction = new SrInstruction();
            switch (operation.Kind)
            {
                case TokenKind.Plus:
                    break;


                case TokenKind.Minus:
                    instruction.Set(OpCode.Neg, SrvmProcessor.RegisterAIndex, SrvmProcessor.RegisterAIndex);
                    context.AddBodyCode(instruction, false);
                    break;


                case TokenKind.Exclamation:
                    if (returnType != SrRuntimeType.Boolean)
                    {
                        // 論理反転は今の所booleanのみ対応
                        throw new System.Exception();
                    }
                    instruction.Set(OpCode.Neg, SrvmProcessor.RegisterAIndex, SrvmProcessor.RegisterAIndex);
                    context.AddBodyCode(instruction, false);
                    break;


                case TokenKind.DoublePlus:
                    if (returnType != SrRuntimeType.Integer)
                    {
                        // インクリメント操作は整数のみ対応
                        throw new System.Exception();
                    }
                    instruction.Set(OpCode.Addl, SrvmProcessor.RegisterAIndex, SrvmProcessor.RegisterAIndex, 0, 1);
                    context.AddBodyCode(instruction, false);
                    break;


                case TokenKind.DoubleMinus:
                    if (returnType != SrRuntimeType.Integer)
                    {
                        // デクリメント操作は整数のみ対応
                        throw new System.Exception();
                    }
                    instruction.Set(OpCode.Subl, SrvmProcessor.RegisterAIndex, SrvmProcessor.RegisterAIndex, 0, 1);
                    context.AddBodyCode(instruction, false);
                    break;
            }
        }


        private void CompileExpression(SyntaxNode leftExpression, SyntaxNode rightExpression, in Token operation, SrCompileContext context)
        {
        }
    }
}