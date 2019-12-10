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

using System;

namespace SnowRabbit.Compiler.Lexer
{
    /// <summary>
    /// トークンの表現を定義している構造体です
    /// </summary>
    public readonly struct Token : IEquatable<Token>
    {
        /// <summary>
        /// トークンの種別
        /// </summary>
        public readonly int Kind;

        /// <summary>
        /// トークンの文字列本体
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// トークンが整数で表現される時の整数値
        /// </summary>
        public readonly long Integer;

        /// <summary>
        /// トークンが実数で表現される時の実数値
        /// </summary>
        public readonly double Number;

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
        /// <param name="number">トークン実数</param>
        /// <param name="lineNumber">出現行番号</param>
        /// <param name="columnNumber">出現列番号</param>
        public Token(int kind, string text, long integer, double number, int lineNumber, int columnNumber)
        {
            // 各種メンバ変数の初期化
            Kind = kind;
            Text = text;
            Integer = integer;
            Number = number;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }


        /// <summary>
        /// Token の等価確認をします
        /// </summary>
        /// <param name="other">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public bool Equals(Token other) =>
            Kind == other.Kind &&
            Text == other.Text &&
            Integer == other.Integer &&
            Number == other.Number &&
            LineNumber == other.LineNumber &&
            ColumnNumber == other.ColumnNumber;


        /// <summary>
        /// object の等価オーバーロードです
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public override bool Equals(object obj) => obj is Token ? Equals((Token)obj) : false;


        /// <summary>
        /// ハッシュコードを取得します
        /// </summary>
        /// <returns>ハッシュコードを返します</returns>
        public override int GetHashCode() => base.GetHashCode();


        /// <summary>
        /// 等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>等価の結果を返します</returns>
        public static bool operator ==(Token left, Token right) => left.Equals(right);


        /// <summary>
        /// 非等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>非等価の結果を返します</returns>
        public static bool operator !=(Token left, Token right) => !left.Equals(right);
    }
}
