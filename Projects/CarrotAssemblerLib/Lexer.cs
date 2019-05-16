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
using TextProcessorLib;

namespace CarrotAssemblerLib
{
    /// <summary>
    /// CarrotAssembler の追加トークン種別を保持するクラスです
    /// </summary>
    public class CarrotAsmTokenKind : TokenKind
    {
        /// <summary>
        /// 定数定義
        /// 'const'
        /// </summary>
        public const int Const = UserDefineOffset + 0;

        /// <summary>
        /// グローバル
        /// 'global'
        /// </summary>
        public const int Global = UserDefineOffset + 1;
    }



    /// <summary>
    /// CarrotAssember の追加レキサ実装クラスです
    /// </summary>
    public class CarrotAsmLexer : TokenReader
    {
        /// <summary>
        /// CarrotAssembler のキーワードテーブルID
        /// </summary>
        protected override int KeywordTableID => 1;



        /// <summary>
        /// CarrotAsmLexer クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="inputStream">字句解析対象となるストリーム</param>
        public CarrotAsmLexer(Stream inputStream) : base(inputStream)
        {
        }


        /// <summary>
        /// トークンテーブルを生成します
        /// </summary>
        /// <returns>生成したトークンテーブルを返します</returns>
        protected override Dictionary<string, int> CreateTokenTable()
        {
            // 標準トークンテーブルを生成して追加のキーワードを登録して返す
            var tokenTable = CreateDefaultTokenTable();
            tokenTable["const"] = CarrotAsmTokenKind.Const;
            tokenTable["global"] = CarrotAsmTokenKind.Global;
            return tokenTable;
        }
    }
}