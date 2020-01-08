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

using SnowRabbit.Compiler.Lexer;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 条件論理積構文を表す構文ノードクラスです
    /// </summary>
    public class ConditionAndExpressionSyntaxNode : SyntaxNode
    {
        /// <summary>
        /// この構文ノードが対応する構文ノードを生成します
        /// </summary>
        /// <param name="context">コンパイルする対象となる翻訳単位コンテキスト</param>
        /// <returns>構文ノードを生成出来た場合は構文ノードのインスタンスを、生成出来ない場合は null を返します</returns>
        public static SyntaxNode Create(LocalCompileContext context)
        {
            // トークンの参照を取得する
            ref var token = ref context.Lexer.LastReadToken;


            // 論理和式構文を生成して条件論理積トークンが続く間ループ
            var expression = OrExpressionSyntaxNode.Create(context);
            while (token.Kind == TokenKind.DoubleAnd)
            {
                // トークンを読み込んで理和式構文を生成する
                context.Lexer.ReadNextToken();
                var rightExpression = OrExpressionSyntaxNode.Create(context);


                // 条件論理積構文を生成する
                var conditionAndExpression = new ConditionAndExpressionSyntaxNode();
                conditionAndExpression.Add(expression);
                conditionAndExpression.Add(rightExpression);


                // 自身が左辺になる
                expression = conditionAndExpression;
            }


            // 最終的な式を返す
            return expression;
        }
    }
}
