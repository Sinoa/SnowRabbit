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

namespace CarrotAssemblerLib.Common
{
    /// <summary>
    /// トークンの表現を定義している構造体です
    /// </summary>
    public readonly struct Token
    {
        /// <summary>
        /// 無効なトークンを表します
        /// </summary>
        public readonly static Token InvalidToken = new Token(TokenKind.Unknown, null, 0, 0, 0);

        /// <summary>
        /// トークンの種別
        /// </summary>
        public readonly TokenKind Kind;

        /// <summary>
        /// トークンの文字列本体
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// トークンが整数で表現される時の整数値
        /// </summary>
        public readonly long Integer;

        /// <summary>
        /// トークンが現れた行番号
        /// </summary>
        public readonly int LineNumber;

        /// <summary>
        /// トークンが現れた最初の列番号
        /// </summary>
        public readonly int ColumnNumber;



        /// <summary>
        /// Token 構造体のインスタンスを初期化します
        /// </summary>
        /// <param name="kind">トークンの種別</param>
        /// <param name="text">トークン文字列</param>
        /// <param name="integer">トークン整数</param>
        /// <param name="lineNumber">出現行番号</param>
        /// <param name="columnNumber">出現列番号</param>
        public Token(TokenKind kind, string text, long integer, int lineNumber, int columnNumber)
        {
            // 各種メンバ変数の初期化
            Kind = kind;
            Text = text;
            Integer = integer;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }


        /// <summary>
        /// このトークンが無効なトークンであるかどうかを判断します
        /// </summary>
        /// <returns>無効なトークンの場合は true を、有効なトークンの場合は false を返します</returns>
        public bool IsInvalid()
        {
            return
                Kind == TokenKind.Unknown &&
                Text == null &&
                Integer == 0 &&
                LineNumber == 0 &&
                ColumnNumber == 0;
        }
    }
}