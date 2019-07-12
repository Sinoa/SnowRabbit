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
using System.IO;
using CarrotCompilerCollection.Coder;
using CarrotCompilerCollection.Utility;

namespace CarrotCompilerCollection.TextProcess
{
    /// <summary>
    /// Carrot スクリプトコードの構文解析及び実行コードを生成するクラスです
    /// </summary>
    public class CccParser
    {
        // メンバ変数定義
        private CccLexer lexer;
        private CccBinaryCoder coder;
        private ICccParserLogger logger;



        /// <summary>
        /// CccParser クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="logger">構文解析ログを出力するロガー</param>
        /// <exception cref="ArgumentNullException">logger が null です</exception>
        public CccParser(ICccParserLogger logger)
        {
            // 初期化をする
            lexer = new CccLexer();
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// コンパイルを行い実行コードを出力します
        /// </summary>
        /// <param name="reader">コンパイルするスクリプトコードを読み取るテキストリーダ</param>
        /// <param name="outputStream">コンパイルした結果を出力する出力ストリーム</param>
        public void Compile(TextReader reader, Stream outputStream)
        {
            // レキサのリセットとバイナリコーダーの初期化をする
            lexer.Reset(reader ?? throw new ArgumentNullException(nameof(reader)));
            coder = new CccBinaryCoder(outputStream ?? throw new ArgumentNullException(nameof(outputStream)));


            // コンパイルを行い実行コードを出力する
            ParseCompileUnit();
            coder.OutputExecuteCode();
        }


        /// <summary>
        /// コンパイル単位のルートになる構文解析関数です
        /// </summary>
        private void ParseCompileUnit()
        {
        }
    }
}