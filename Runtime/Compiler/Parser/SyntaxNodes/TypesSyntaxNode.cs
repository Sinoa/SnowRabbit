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
    /// 型構文を表現する構文ノードクラスです
    /// </summary>
    public class TypesSyntaxNode : SyntaxNode
    {
        // メンバ変数定義
        private Token type;



        /// <summary>
        /// この構文ノードが対応する構文ノードを生成します
        /// </summary>
        /// <param name="context">コンパイルする対象となる翻訳単位コンテキスト</param>
        /// <returns>構文ノードを生成出来た場合は構文ノードのインスタンスを、生成出来ない場合は null を返します</returns>
        public static SyntaxNode Create(LocalCompileContext context)
        {
            // void, int, number, string, object, bool のいずれかどうかを判断する
            ref var token = ref context.Lexer.LastReadToken;
            var isType =
                token.Kind == SrTokenKind.TypeVoid ||
                token.Kind == SrTokenKind.TypeInt ||
                token.Kind == SrTokenKind.TypeNumber ||
                token.Kind == SrTokenKind.TypeString ||
                token.Kind == SrTokenKind.TypeObject ||
                token.Kind == SrTokenKind.TypeBool;


            // 扱える型でないなら
            if (!isType)
            {
                // コンパイルエラーとして処理する
                context.ThrowSyntaxError(new SrUnknownTokenSyntaxErrorException(ref token));
                return null;
            }


            // 扱えるならノードを生成してトークンを覚える
            var types = new TypesSyntaxNode();
            types.type = token;


            // 次のトークンを読み込んで返す
            context.Lexer.ReadNextToken();
            return types;
        }
    }
}