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
using System.IO;

namespace SnowRabbit.Compiler.Lexer
{
    /// <summary>
    /// SnowRabbit スクリプト用レキサクラスです
    /// </summary>
    public class SrLexer : TokenReader
    {
        /// <summary>
        /// SrLexer クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="name">レキサの名前</param>
        /// <param name="reader">字句解析に用いるテキストリーダー</param>
        public SrLexer(string name, TextReader reader) : base(name, reader)
        {
        }


        /// <summary>
        /// Carrot スクリプトで使用するトークンをセットアップします
        /// </summary>
        /// <param name="tokenTable">セットアップするトークンテーブル</param>
        protected override void SetupToken(Dictionary<string, int> tokenTable)
        {
            // キーワードトークンの追加
            tokenTable["using"] = SrTokenKind.Using;
            tokenTable["void"] = SrTokenKind.TypeVoid;
            tokenTable["int"] = SrTokenKind.TypeInt;
            tokenTable["number"] = SrTokenKind.TypeNumber;
            tokenTable["string"] = SrTokenKind.TypeString;
            tokenTable["object"] = SrTokenKind.TypeObject;
            tokenTable["bool"] = SrTokenKind.TypeBool;
            tokenTable["end"] = SrTokenKind.End;
            tokenTable["function"] = SrTokenKind.Function;
            tokenTable["global"] = SrTokenKind.Global;
            tokenTable["local"] = SrTokenKind.Local;
            tokenTable["if"] = SrTokenKind.If;
            tokenTable["else"] = SrTokenKind.Else;
            tokenTable["for"] = SrTokenKind.For;
            tokenTable["while"] = SrTokenKind.While;
            tokenTable["link"] = SrTokenKind.Link;
            tokenTable["compile"] = SrTokenKind.Compile;
            tokenTable["const"] = SrTokenKind.Const;
            tokenTable["return"] = SrTokenKind.Return;
            tokenTable["break"] = SrTokenKind.Break;
            tokenTable["true"] = SrTokenKind.True;
            tokenTable["false"] = SrTokenKind.False;
            tokenTable["null"] = SrTokenKind.Null;
        }
    }
}
