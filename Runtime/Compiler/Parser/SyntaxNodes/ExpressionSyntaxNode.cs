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
        private static readonly Dictionary<int, int> operationPriorityTable;
        private static readonly HashSet<int> assignmentTokenKinds;



        static ExpressionSyntaxNode()
        {
            operationPriorityTable = new Dictionary<int, int>()
            {
                { TokenKind.Asterisk, 5 }, { TokenKind.Slash, 5 },                      // * /
                { TokenKind.Plus, 10 }, { TokenKind.Minus, 10 },                        // + -
                { TokenKind.DoubleOpenAngle, 15 }, { TokenKind.DoubleCloseAngle, 15 },  // << >>
                { TokenKind.OpenAngle, 20 }, { TokenKind.CloseAngle, 20 },              //
                { TokenKind.LesserEqual, 20 }, { TokenKind.GreaterEqual, 20 },          // < > <= >=
                { TokenKind.DoubleEqual, 25 }, { TokenKind.NotEqual, 25 },              // == !=
                { TokenKind.And, 30 },                                                  // &
                { TokenKind.Circumflex, 35 },                                           // ^
                { TokenKind.Verticalbar, 40 },                                          // |
                { TokenKind.DoubleAnd, 45 },                                            // &&
                { TokenKind.DoubleVerticalbar, 50 },                                    // ||
                { TokenKind.Equal, 55 }, { TokenKind.PlusEqual, 55 },                   //
                { TokenKind.MinusEqual, 55 }, { TokenKind.AsteriskEqual, 55 },          //
                { TokenKind.SlashEqual, 55 }, { TokenKind.AndEqual, 55 },               //
                { TokenKind.VerticalbarEqual, 55 }, { TokenKind.CircumflexEqual, 55 },  // = += -= *= /= &= |= ^=
            };


            assignmentTokenKinds = new HashSet<int>()
            {
                TokenKind.Equal, TokenKind.PlusEqual, TokenKind.MinusEqual, TokenKind.AsteriskEqual,
                TokenKind.SlashEqual, TokenKind.AndEqual, TokenKind.VerticalbarEqual, TokenKind.CircumflexEqual,
            };
        }


        /// <summary>
        /// ExpressionSyntaxNode クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="token">対応するトークン</param>
        public ExpressionSyntaxNode(in Token token) : base(token)
        {
        }


        public override void Compile(SrCompileContext context)
        {
            if (Children.Count == 1)
            {
                CompileUnaryExpression(Children[0], Token, context);
            }
            else if (Children.Count == 2)
            {
                CompileExpression(Children[0], Children[1], Token, context);
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


            if (returnType == SrRuntimeType.String || returnType == SrRuntimeType.Object || returnType == SrRuntimeType.Void)
            {
                // 文字列 オブジェクト void に対する単項式は今の所未実装
                throw new System.Exception();
            }


            expression.Compile(context);


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
            if (assignmentTokenKinds.Contains(operation.Kind))
            {
                if (!(leftExpression is IdentifierSyntaxNode))
                {
                    // 代入先は変数でなければいけない
                    throw new System.Exception();
                }


                var variableSymbol = context.AssemblyData.GetVariableSymbol(leftExpression.Token.Text, context.CurrentCompileFunctionName);
                if (variableSymbol == null)
                {
                    // 代入先は変数でなければいけない
                    throw new System.Exception();
                }


                rightExpression.Compile(context);
                var instruction = new SrInstruction();
                instruction.Set(OpCode.Strl, SrvmProcessor.RegisterAIndex, 0, 0, variableSymbol.InitialAddress);
                context.AddBodyCode(instruction, true);
                return;
            }


            if (leftExpression is ExpressionSyntaxNode && rightExpression is ExpressionSyntaxNode)
            {
                var leftOperationPriority = operationPriorityTable[leftExpression.Token.Kind];
                var rightOperationPriority = operationPriorityTable[rightExpression.Token.Kind];
                if (leftOperationPriority >= rightOperationPriority)
                {
                    leftExpression.Compile(context);
                }
                else
                {
                    rightExpression.Compile(context);
                }
            }
        }
    }
}