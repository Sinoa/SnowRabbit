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
using System.Collections.Generic;
using CarrotCompilerCollection.Utility;

namespace CarrotCompilerCollection.Compiler
{
    /// <summary>
    /// Cccコンパイラのあらゆるメッセージを取り扱うクラスです
    /// </summary>
    internal static class CompilerMessage
    {
        // 定数定義
        public const uint Error_RecursiveScriptCompile = 0x8000_0001U;

        // クラス変数宣言
        private static readonly Dictionary<uint, Message> MessageTable;



        /// <summary>
        /// クラスの初期化を行います
        /// </summary>
        static CompilerMessage()
        {
            // 各種文字列パターン
            // スクリプト名 => %SCRIPT_NAME%
            // 識別子n(n=>0) => %ID_n%

            // メッセージテーブルを構築する
            MessageTable = new Dictionary<uint, Message>()
            {
                { Error_RecursiveScriptCompile, new Message(CccParserLogType.Error, Error_RecursiveScriptCompile, "スクリプト名 '%SCRIPT_NAME%' のスクリプトが見つかりませんでした。", null) },
            };
        }



        /// <summary>
        /// コンパイラのメッセージを表した構造体です
        /// </summary>
        private readonly struct Message
        {
            /// <summary>
            /// メッセージのログタイプ
            /// </summary>
            public readonly CccParserLogType LogType;

            /// <summary>
            /// メッセージのコード
            /// </summary>
            public readonly uint Code;

            /// <summary>
            /// メッセージパターン
            /// </summary>
            public readonly string MessagePattern;

            /// <summary>
            /// メッセージが例外を投げる事ができる場合の例外スロー関数
            /// </summary>
            public readonly Action ThrowException;



            /// <summary>
            /// Message インスタンスの初期化をします
            /// </summary>
            /// <param name="logType">メッセージのログタイプ</param>
            /// <param name="code">メッセージのコード</param>
            /// <param name="messagePattern">メッセージパターン</param>
            /// <param name="throwException">例外を投げる関数</param>
            public Message(CccParserLogType logType, uint code, string messagePattern, Action throwException)
            {
                // 初期化
                LogType = logType;
                Code = code;
                MessagePattern = messagePattern;
                ThrowException = throwException;
            }
        }
    }
}