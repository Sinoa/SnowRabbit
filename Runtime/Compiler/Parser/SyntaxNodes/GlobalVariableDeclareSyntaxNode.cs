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
    /// グローバル変数宣言構文を表す構文ノードクラスです
    /// </summary>
    public class GlobalVariableDeclareSyntaxNode : SyntaxNode
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


            // global で始まっていないのなら生成出来ない
            if (token.Kind != SrTokenKind.Global) return null;


            // non_void_types <identifier> ';' を解析する
            var globalVariableDeclare = new GlobalVariableDeclareSyntaxNode();
            context.Lexer.ReadNextToken();
            CheckSyntaxAndAddNode(NonVoidTypesSyntaxNode.Create(context), globalVariableDeclare, context);
            CheckSyntaxAndAddNode(IdentifierSyntaxNode.Create(context), globalVariableDeclare, context);
            CheckTokenAndReadNext(TokenKind.Semicolon, context);


            // 結果を返す
            return globalVariableDeclare;
        }


        /// <summary>
        /// node が null でないなら parentNode に追加し null の場合はコンパイルエラーを出します
        /// </summary>
        /// <param name="node">チェックする構文ノード</param>
        /// <param name="parentNode">チェックをパスした場合にノードを持つ親ノード</param>
        /// <param name="context">現在のコンテキスト</param>
        private static void CheckSyntaxAndAddNode(SyntaxNode node, SyntaxNode parentNode, LocalCompileContext context)
        {
            // ノードが null なら
            if (node == null)
            {
                // 不明なトークンとしてコンパイルエラーを出す
                context.ThrowSyntaxError(new SrUnknownTokenSyntaxErrorException(ref context.Lexer.LastReadToken));
                return;
            }


            // null でないなら素直に追加
            parentNode.Add(node);
        }


        /// <summary>
        /// 該当のトークンが登場しているかチェックして問題がなければ次のトークンを読み込みます
        /// </summary>
        /// <param name="tokenKind">チェックするトークン種別</param>
        /// <param name="context">現在のコンテキスト</param>
        private static void CheckTokenAndReadNext(int tokenKind, LocalCompileContext context)
        {
            // 該当トークンで無いなら
            ref var token = ref context.Lexer.LastReadToken;
            if (token.Kind != tokenKind)
            {
                // 不明なトークンとしてコンパイルエラーとする
                context.ThrowSyntaxError(new SrUnknownTokenSyntaxErrorException(ref token));
            }


            // 次のトークンを読み込む
            context.Lexer.ReadNextToken();
        }
    }
}
