﻿// zlib/libpng License
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
using System.Collections.Generic;
using System.IO;
using CarrotCompilerCollection.IO;
using CarrotCompilerCollection.Utility;

namespace CarrotCompilerCollection.Compiler
{
    /// <summary>
    /// Carrot スクリプトコードの構文解析及び実行コードを生成するクラスです
    /// </summary>
    public class CccParser
    {
        // メンバ変数定義
        private CccBinaryCoder coder;
        private IScriptStorage storage;
        private ICccParserLogger logger;
        private Stack<ParserContext> contextStack;
        private ParserContext currentContext;



        #region Constructor and initialize
        /// <summary>
        /// CccParser クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="logger">構文解析ログを出力するロガー</param>
        /// <exception cref="ArgumentNullException">storage が null です</exception>
        /// <exception cref="ArgumentNullException">logger が null です</exception>
        public CccParser(IScriptStorage storage, ICccParserLogger logger)
        {
            // 初期化をする
            contextStack = new Stack<ParserContext>();
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        #endregion


        #region Compile function
        /// <summary>
        /// コンパイルを行い実行コードを出力します
        /// </summary>
        /// <param name="scriptName">コンパイルするスクリプト名</param>
        /// <param name="outputStream">コンパイルした結果を出力する出力ストリーム</param>
        public void Compile(string scriptName, Stream outputStream)
        {
            // レキサのリセットとバイナリコーダーの初期化をする
            lexer.Reset(reader ?? throw new ArgumentNullException(nameof(reader)));
            coder = new CccBinaryCoder(outputStream ?? throw new ArgumentNullException(nameof(outputStream)));


            // コンパイルを行い実行コードを出力する
            ParseCompileUnit();
            coder.OutputExecuteCode();
        }


        /// <summary>
        /// 新しいコンテキストを用意してネストされたスクリプトのコンパイルを行います
        /// </summary>
        /// <param name="scriptName">コンパイルするべきスクリプト名</param>
        private void CompileNestScript(string scriptName)
        {
            // コンテキストスタックをなめる
            foreach (var context in contextStack)
            {
                // もし同じスクリプト名が登場したら
                if (context.ScriptName == scriptName)
                {
                    // 再帰コンパイルしている恐れがあるため例外を吐く
                    ThrowExceptionRecursiveScriptCompile(scriptName);
                    return;
                }
            }
        }
        #endregion


        /// <summary>
        /// コンパイル単位のルートになる構文解析関数です
        /// </summary>
        private void ParseCompileUnit()
        {
        }


        #region exception thrower
        /// <summary>
        /// 再帰コンパイルしようとしている例外をスローします
        /// </summary>
        /// <param name="scriptName">再帰コンパイルを検知した直近のスクリプト名</param>
        /// <exception cref="InvalidOperationException">スクリプト '{scriptName}' にて再帰コンパイルを検出しました</exception>
        private void ThrowExceptionRecursiveScriptCompile(string scriptName)
        {
            // 再帰コンパイルしようとしている恐れのコンパイルエラーを吐く
            ThrowExceptionCompileError($"スクリプト '{scriptName}' にて再帰コンパイルを検出しました", 0);
        }


        /// <summary>
        /// 例外としてコンパイルエラーをスローします
        /// </summary>
        /// <param name="message">スローするメッセージ</param>
        private void ThrowExceptionCompileError(string message, uint errorCode)
        {
            // 現在のコンテキストから必要な情報を取り出してロガーにエラーを書き込んで例外を吐く
            var scriptName = currentContext.ScriptName;
            var lexer = currentContext.Lexer;
            ref var token = ref lexer.LastReadToken;
            logger.Write(CccParserLogType.Error, scriptName, token.LineNumber, token.ColumnNumber, errorCode, message);
            throw new CccCompileErrorException(message);
        }
        #endregion



        #region ParserContext
        /// <summary>
        /// 構文解析コンテキストクラスです
        /// </summary>
        private class ParserContext
        {
            /// <summary>
            /// このコンテキストを保持している構文解析器
            /// </summary>
            public CccParser Parser { get; private set; }

            /// <summary>
            /// 構文解析している対象のスクリプト名を取得します
            /// </summary>
            public string ScriptName { get; private set; }

            /// <summary>
            /// このコンテキストにおける字句解析器を取得します
            /// </summary>
            public CccLexer Lexer { get; private set; }



            /// <summary>
            /// ParserContext クラスのインスタンスを初期化します
            /// </summary>
            /// <param name="scriptName">構文解析対象のスクリプト名</param>
            /// <param name="parser">このコンテキストを持つ構文解析器</param>
            public ParserContext(string scriptName, CccParser parser)
            {
                // 構文解析器を覚える
                Parser = parser ?? throw new ArgumentNullException(nameof(parser));


                // スクリプト名を覚えてレキサの初期化をする
                ScriptName = scriptName;
                Lexer = new CccLexer();


                // スクリプトを開いてレキサをリセットする
                Lexer.Reset(parser.storage.Open(scriptName));
            }
        }
        #endregion
    }
}