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

namespace SnowRabbit.Compiler.Parser.SyntaxTokens
{
    /// <summary>
    /// 構文上に登場するトークンを表現する構文トークン抽象クラスです
    /// </summary>
    public abstract class SyntaxToken
    {
        // メンバ変数定義
        protected Token token;



        /// <summary>
        /// トークンが現れた行番号
        /// </summary>
        public int LineNumber => token.LineNumber;


        /// <summary>
        /// トークンが現れた列番号
        /// </summary>
        public int ColumnNumber => token.ColumnNumber;



        /// <summary>
        /// SyntaxToken クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="token">適応するトークンへの参照</param>
        protected SyntaxToken(in Token token)
        {
            // トークンを受け取る
            this.token = token;
        }
    }
}
