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
    /// 外部スクリプトコンパイルディレクティブ構文を表す構文ノードクラスです
    /// </summary>
    public class ScriptCompileDirectiveSyntaxNode : SyntaxNode
    {
        // メンバ変数定義
        private Token scriptName;



        /// <summary>
        /// この構文ノードが対応する構文ノードを生成します
        /// </summary>
        /// <param name="context">コンパイルする対象となる翻訳単位コンテキスト</param>
        /// <returns>構文ノードを生成出来た場合は構文ノードのインスタンスを、生成出来ない場合は null を返します</returns>
        public static SyntaxNode Create(LocalCompileContext context)
        {
            // compile トークンではないなら構文ノードは生成出来ない
            ref var token = ref context.Lexer.LastReadToken;
            if (token.Kind != SrTokenKind.Compile) return null;


            // 次のトークンを読み込んで文字列でないなら構文エラー
            context.Lexer.ReadNextToken();
            if (token.Kind != TokenKind.String)
            {
                // 不明なトークンとして処理する
                context.ThrowSyntaxError(new SrUnknownTokenSyntaxErrorException(ref token));
                return null;
            }


            // スクリプトコンパイル構文ノードを生成
            var scriptCompileDirective = new ScriptCompileDirectiveSyntaxNode();
            scriptCompileDirective.scriptName = token;


            // 次のトークンを読み込んで返す
            context.Lexer.ReadNextToken();
            return scriptCompileDirective;
        }
    }
}
