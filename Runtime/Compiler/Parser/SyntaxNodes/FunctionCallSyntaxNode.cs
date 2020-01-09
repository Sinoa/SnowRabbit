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
using SnowRabbit.Compiler.Parser.SyntaxErrors;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 関数呼び出し構文を表す構文ノードクラスです
    /// </summary>
    public class FunctionCallSyntaxNode : SyntaxNode
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


            // 一次式構文ノードを生成して失敗したら null を返す
            var primaryExpression = PrimaryExpressionSyntaxNode.Create(context);
            if (primaryExpression == null) return null;


            // もしオープンパーレントークンが来ていないなら
            if (token.Kind != TokenKind.OpenParen)
            {
                // 一次式をそのまま返す
                return primaryExpression;
            }


            // 引数リスト構文ノードを生成する
            context.Lexer.ReadNextToken();
            SyntaxNode parameterList = null;
            if (token.Kind != TokenKind.CloseParen)
            {
                parameterList = ParameterListSyntaxNode.Create(context);
            }


            // 最後にクローズパーレンが来ていないなら
            CheckTokenAndReadNext(TokenKind.CloseParen, context, new SrNotClosedSymbolSyntaxErrorException("(", ")"));


            // 関数呼び出しノードを生成して返す
            var functionCall = new FunctionCallSyntaxNode();
            functionCall.Add(primaryExpression);
            functionCall.Add(parameterList);
            return functionCall;
        }
    }
}
