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

using System.Collections.Generic;
using TextProcessorLib;

namespace CarrotCompilerCollection.Compiler
{
    #region Kind
    /// <summary>
    /// Carrot スクリプトにおけるトークン種別の定数を持ったクラスです
    /// </summary>
    public class CccTokenKind : TokenKind
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
        /// ifelse
        /// </summary>
        public const int IfElse = KeywordOffset + 55;

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
    #endregion



    #region Lexer
    /// <summary>
    /// Carrot スクリプト用レキサクラスです
    /// </summary>
    public class CccLexer : TokenReader
    {
        /// <summary>
        /// Carrot スクリプトで使用するトークンをセットアップします
        /// </summary>
        /// <param name="tokenTable">セットアップするトークンテーブル</param>
        protected override void SetupToken(Dictionary<string, int> tokenTable)
        {
            // キーワードトークンの追加
            tokenTable["using"] = CccTokenKind.Using;
            tokenTable["void"] = CccTokenKind.TypeVoid;
            tokenTable["int"] = CccTokenKind.TypeInt;
            tokenTable["number"] = CccTokenKind.TypeNumber;
            tokenTable["string"] = CccTokenKind.TypeString;
            tokenTable["end"] = CccTokenKind.End;
            tokenTable["function"] = CccTokenKind.Function;
            tokenTable["global"] = CccTokenKind.Global;
            tokenTable["local"] = CccTokenKind.Local;
            tokenTable["if"] = CccTokenKind.If;
            tokenTable["else"] = CccTokenKind.Else;
            tokenTable["ifelse"] = CccTokenKind.IfElse;
            tokenTable["for"] = CccTokenKind.For;
            tokenTable["while"] = CccTokenKind.While;
            tokenTable["link"] = CccTokenKind.Link;
            tokenTable["compile"] = CccTokenKind.Compile;
            tokenTable["const"] = CccTokenKind.Const;
            tokenTable["return"] = CccTokenKind.Return;
            tokenTable["break"] = CccTokenKind.Break;
        }
    }
    #endregion
}