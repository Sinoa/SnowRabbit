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

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 式構文を表す構文ノードクラスです
    /// </summary>
    public class ExpressionSyntaxNode : SyntaxNode
    {
        /// <summary>
        /// 単純式
        /// </summary>
        public SimpleExpressionSyntaxNode SimpleExpression { get; private set; }



        /// <summary>
        /// この構文ノードが対応する構文ノードを生成します
        /// </summary>
        /// <param name="context">コンパイルする対象となる翻訳単位コンテキスト</param>
        /// <returns>構文ノードを生成出来た場合は構文ノードのインスタンスを、生成出来ない場合は null を返します</returns>
        public static SyntaxNode Create(LocalCompileContext context)
        {
            // 単純式構文の生成関数を呼んで null が返ってきたら null を返す
            var simpleExpression = SimpleExpressionSyntaxNode.Create(context);
            if (simpleExpression == null) return null;


            // 結果を自分にぶら下げて返す
            var expression = new ExpressionSyntaxNode();
            expression.SimpleExpression = (SimpleExpressionSyntaxNode)simpleExpression;
            return expression;
        }
    }
}