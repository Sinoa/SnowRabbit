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

namespace CarrotCompilerCollection.Utility
{
    /// <summary>
    /// コンソール向け構文解析ロガー抽象クラスです
    /// </summary>
    public class CccConsoleParserLogger : ICccParserLogger
    {
        // クラス変数宣言
        private static readonly Dictionary<CccParserLogType, ConsoleColorPattern> ConsoleColorTable;



        /// <summary>
        /// CccConsoleParserLogger クラスの初期化をします
        /// </summary>
        static CccConsoleParserLogger()
        {
            // コンソールカラーテーブルの用意
            ConsoleColorTable = new Dictionary<CccParserLogType, ConsoleColorPattern>()
            {
                { CccParserLogType.Info, new ConsoleColorPattern(){ IsDefault = true, ForegroundColor = ConsoleColor.White, BackgroundColor = ConsoleColor.Black } },
                { CccParserLogType.Warning, new ConsoleColorPattern(){ IsDefault = false, ForegroundColor = ConsoleColor.Yellow, BackgroundColor = ConsoleColor.Black } },
                { CccParserLogType.Error, new ConsoleColorPattern(){ IsDefault = true, ForegroundColor = ConsoleColor.Red, BackgroundColor = ConsoleColor.Gray } }
            };
        }


        /// <summary>
        /// ログを書き込みます
        /// </summary>
        /// <param name="type">ログの種類</param>
        /// <param name="scriptName">構文解析をしていた対象のスクリプト名</param>
        /// <param name="lineNumber">ログを出すタイミングになった行番号。ただし、正確な位置を保証しているわけではありません。</param>
        /// <param name="columnNumber">ログを出すタイミングになった列番号。ただし、正確な位置を保証しているわけではありません。</param>
        /// <param name="code">構文解析がログを出力する要因となったログコード</param>
        /// <param name="message">構文解析が出力するメッセージ</param>
        public void Write(CccParserLogType type, string scriptName, int lineNumber, int columnNumber, uint code, string message)
        {
            // コンソールカラーパターンを取得するが取得に失敗したら
            if (!ConsoleColorTable.TryGetValue(type, out var pattern))
            {
                // デフォルトカラーを利用するようにする
                pattern.IsDefault = true;
            }


            // もし指定されたログタイプで色を変更するなら
            if (!pattern.IsDefault)
            {
                // コンソール色を変更する
                Console.ForegroundColor = pattern.ForegroundColor;
                Console.BackgroundColor = pattern.BackgroundColor;
            }


            // ログを出力してカラーをリセットする
            Console.WriteLine($"{scriptName} at (Line:{lineNumber}, Column:{columnNumber}) {type.ToString()} C{code}: {message}");
            Console.ResetColor();
        }



        /// <summary>
        /// コンソールカラーパターンを保持する構造体です
        /// </summary>
        private struct ConsoleColorPattern
        {
            /// <summary>
            /// デフォルトカラーのままかどうか
            /// </summary>
            public bool IsDefault;

            /// <summary>
            /// 全景カラー
            /// </summary>
            public ConsoleColor ForegroundColor;

            /// <summary>
            /// 背景カラー
            /// </summary>
            public ConsoleColor BackgroundColor;
        }
    }
}