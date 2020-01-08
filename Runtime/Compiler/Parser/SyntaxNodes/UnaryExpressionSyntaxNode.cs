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
    /// 単項式構文を表す構文ノードクラスです
    /// </summary>
    public class UnaryExpressionSyntaxNode : SyntaxNode
    {
        // メンバ変数定義
        private Token operation;



        /// <summary>
        /// この構文ノードが対応する構文ノードを生成します
        /// </summary>
        /// <param name="context">コンパイルする対象となる翻訳単位コンテキスト</param>
        /// <returns>構文ノードを生成出来た場合は構文ノードのインスタンスを、生成出来ない場合は null を返します</returns>
        public static SyntaxNode Create(LocalCompileContext context)
        {
            // トークンの参照を取得する
            ref var token = ref context.Lexer.LastReadToken;


            // 単項式として処理する演算がある場合は
            if (token.Kind == TokenKind.Plus || token.Kind == TokenKind.Minus || token.Kind == TokenKind.Exclamation)
            {
                // 単項式構文を生成して処理するべきトークンを覚える
                var unaryExpression = new UnaryExpressionSyntaxNode();
                unaryExpression.operation = token;


                // トークンを読み込んで自身の生成関数を再帰呼び出しする
                context.Lexer.ReadNextToken();
                var expression = Create(context);


                // 再帰呼び出しした対象を自身にぶら下げて返す
                unaryExpression.Add(expression);
                return unaryExpression;
            }
        }
    }
}
