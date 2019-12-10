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

namespace SnowRabbit.Compiler.Lexer
{
    /// <summary>
    /// SnowRabbit スクリプトにおけるトークン種別の定数を持ったクラスです
    /// </summary>
    public class SrTokenKind : TokenKind
    {
        /// <summary>
        /// キーワードオフセット
        /// </summary>
        public const int KeywordOffset = UserDefineOffset + 0;

        /// <summary>
        /// using
        /// </summary>
        public const int Using = KeywordOffset + 0;

        /// <summary>
        /// void
        /// </summary>
        public const int TypeVoid = KeywordOffset + 5;

        /// <summary>
        /// int
        /// </summary>
        public const int TypeInt = KeywordOffset + 10;

        /// <summary>
        /// number
        /// </summary>
        public const int TypeNumber = KeywordOffset + 15;

        /// <summary>
        /// string
        /// </summary>
        public const int TypeString = KeywordOffset + 20;

        /// <summary>
        /// object
        /// </summary>
        public const int TypeObject = KeywordOffset + 21;

        /// <summary>
        /// end
        /// </summary>
        public const int End = KeywordOffset + 25;

        /// <summary>
        /// function
        /// </summary>
        public const int Function = KeywordOffset + 30;

        /// <summary>
        /// global
        /// </summary>
        public const int Global = KeywordOffset + 35;

        /// <summary>
        /// local
        /// </summary>
        public const int Local = KeywordOffset + 40;

        /// <summary>
        /// if
        /// </summary>
        public const int If = KeywordOffset + 45;

        /// <summary>
        /// else
        /// </summary>
        public const int Else = KeywordOffset + 50;

        /// <summary>
        /// for
        /// </summary>
        public const int For = KeywordOffset + 60;

        /// <summary>
        /// while
        /// </summary>
        public const int While = KeywordOffset + 65;

        /// <summary>
        /// link
        /// </summary>
        public const int Link = KeywordOffset + 70;

        /// <summary>
        /// compile
        /// </summary>
        public const int Compile = KeywordOffset + 75;

        /// <summary>
        /// const
        /// </summary>
        public const int Const = KeywordOffset + 80;

        /// <summary>
        /// return
        /// </summary>
        public const int Return = KeywordOffset + 85;

        /// <summary>
        /// break
        /// </summary>
        public const int Break = KeywordOffset + 90;
    }
}
