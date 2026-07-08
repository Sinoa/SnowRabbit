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

using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.RuntimeEngine;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 後置インクリメント/デクリメント式（a++ / a--）を表す構文ノードクラスです。
    /// 前置（++a / --a）と同じトークン種別を使用するため、構文ノードの型で区別されます。
    /// 式の値は変数を変更する前の値になります。
    /// </summary>
    /// <remarks>
    /// 子ノード構造:
    /// - Children[0]: 対象の変数（IdentifierSyntaxNode）
    /// </remarks>
    public class PostfixUnaryExpressionSyntaxNode : ExpressionSyntaxNode
    {
        /// <summary>
        /// PostfixUnaryExpressionSyntaxNode クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="token">対応する演算子トークン（++ または --）</param>
        public PostfixUnaryExpressionSyntaxNode(in Token token) : base(token)
        {
        }


        public override void Compile(SrCompileContext context)
        {
            var operand = Children[0];
            var isIncrement = Token.Kind == TokenKind.DoublePlus;
            var registerIndex = CompileExpressionValue(operand, context, out var operandType);
            if (operandType != SrRuntimeType.Integer)
            {
                // 後置インクリメント/デクリメントは整数のみ対応
                throw context.ErrorReporter.InvalidUnaryOperation(Token, isIncrement ? "++" : "--", operandType);
            }


            // 変数を更新して書き戻した後、式の値を変更前の値へ戻す
            // （追加レジスタを使用せずに変更前の値を式の結果とするための方式）
            var instruction = new SrInstruction();
            instruction.Set(isIncrement ? OpCode.Addl : OpCode.Subl, registerIndex, registerIndex, 0, 1);
            context.AddBodyCode(instruction, false);
            StoreResult(operand, registerIndex, context);
            instruction.Set(isIncrement ? OpCode.Subl : OpCode.Addl, registerIndex, registerIndex, 0, 1);
            context.AddBodyCode(instruction, false);


            ResultRegisterIndex = registerIndex;
            ResultType = SrRuntimeType.Integer;
        }
    }
}
