// zlib/libpng License
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

using SnowRabbit.Compiler.Lexer;
using SnowRabbit.Compiler.Parser.SyntaxErrors;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// ステートメント構文を表す構文ノードクラスです
    /// </summary>
    public class StatementSyntaxNode : SyntaxNode
    {
        /// <summary>
        /// この構文ノードが対応する構文ノードを生成します
        /// </summary>
        /// <param name="context">コンパイルする対象となる翻訳単位コンテキスト</param>
        /// <returns>構文ノードを生成出来た場合は構文ノードのインスタンスを、生成出来ない場合は null を返します</returns>
        public static SyntaxNode Create(LocalCompileContext context)
        {
            // 各ステートメントの生成を試みて生成出来たものを返す
            return
                EmptyStatementSyntaxNode.Create(context) ??
                LocalVarDeclareSyntaxNode.Create(context) ??
                WhileStatementSyntaxNode.Create(context) ??
                IfStatementSyntaxNode.Create(context) ??
                BreakStatementSyntaxNode.Create(context) ??
                ReturnStatementSyntaxNode.Create(context) ??
                CreateExpressionSyntaxNode(context);
        }


        /// <summary>
        /// 式構文ノードを生成します
        /// </summary>
        /// <param name="context">現在のコンテキスト</param>
        /// <returns>式構文であれば構文ノードを返しますが、構文に一致しない場合は null を返します</returns>
        private static SyntaxNode CreateExpressionSyntaxNode(LocalCompileContext context)
        {
            // トークンの参照を取得して式ノードを作る
            ref var token = ref context.Lexer.LastReadToken;
            var expression = ExpressionSyntaxNode.Create(context);
            CheckTokenAndReadNext(TokenKind.Semicolon, context);
            return expression;
        }
    }
}
