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

using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// return文を表す構文ノードクラスです。
    /// 関数から値を返す、または関数を終了する際に使用されます。
    /// </summary>
    public class ReturnStatementSyntaxNode : SyntaxNode
    {
        /// <summary>
        /// ReturnStatementSyntaxNode クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="token">対応するトークン</param>
        public ReturnStatementSyntaxNode(in Token token) : base(token)
        {
        }

        public override void Compile(SrCompileContext context)
        {
            var functionSymbol = context.AssemblyData.GetFunctionSymbol(context.CurrentCompileFunctionName);
            var functionName = context.CurrentCompileFunctionName;
            if (Children.Count > 0)
            {
                if (functionSymbol.ReturnType == SrRuntimeType.Void)
                {
                    // void関数で戻り値を返そうとしている
                    throw context.ErrorReporter.VoidReturnWithValue(Token, functionName);
                }


                // 戻り値の式を評価して、関数の戻り値型へ暗黙変換できるか検査する
                var valueRegisterIndex = ExpressionSyntaxNode.CompileStatementExpressionValue(Children[0], context, out var valueType);
                if (!ExpressionSyntaxNode.TryEmitImplicitConversion(valueRegisterIndex, valueType, functionSymbol.ReturnType, context))
                {
                    throw context.ErrorReporter.InvalidCast(Token, valueType, functionSymbol.ReturnType);
                }


                // 返却規約（エピローグが rax を r29 へ移す）に合わせて結果を rax へ移す
                if (valueRegisterIndex != SrvmProcessor.RegisterAIndex)
                {
                    var moveInstruction = new SrInstruction();
                    moveInstruction.Set(OpCode.Mov, SrvmProcessor.RegisterAIndex, valueRegisterIndex);
                    context.AddBodyCode(moveInstruction, false);
                }
            }
            else
            {
                if (functionSymbol.ReturnType != SrRuntimeType.Void)
                {
                    // 非void関数で戻り値がない
                    throw context.ErrorReporter.NonVoidReturnWithoutValue(Token, functionName, functionSymbol.ReturnType);
                }
            }


            var instruction = new SrInstruction();
            instruction.Set(OpCode.Brl, 0, 0, 0, context.CurrentFunctionLeaveLabelSymbol.InitialAddress);
            context.AddBodyCode(instruction, true);
        }


        /// <summary>
        /// return文はこの文自体が関数からの脱出であるため、常に true を返します
        /// </summary>
        /// <returns>常に true を返します</returns>
        public override bool AlwaysReturns()
        {
            return true;
        }
    }
}
