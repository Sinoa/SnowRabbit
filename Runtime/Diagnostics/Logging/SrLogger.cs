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
using System.Diagnostics;

namespace SnowRabbit.Diagnostics.Logging
{
    /// <summary>
    /// SnowRabbit 全体で使用されるログクラスです
    /// </summary>
    public static class SrLogger
    {
        // クラス変数宣言
        private static ISrLogHandler logHandler = SrConsoleLogHandler.Instance;



        /// <summary>
        /// 実際のログ操作を行うハンドラ
        /// </summary>
        public static ISrLogHandler LogHandler { get => logHandler; set => logHandler = value ?? SrNullLogHandler.Instance; }



        /// <summary>
        /// トレースレベルのログを出力します。この関数は "TRACE" コンパイル定数を定義しない限り呼び出しは消去されます。
        /// ログを出力するためには "TRACE" コンパイル定数を定義してください。
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ。null を渡された場合は、空文字列として扱われます。</param>
        /// <param name="message">ログに出力するメッセージ。null を渡された場合は、空文字列として扱われます。</param>
        [Conditional("TRACE")]
        public static void Trace(string tag, string message)
        {
            // ハンドラの対応する出力を行う
            logHandler.Trace(tag ?? string.Empty, message ?? string.Empty);
        }


        /// <summary>
        /// デバッグレベルのログを出力します。この関数は "DEBUG" コンパイル定数を定義しない限り呼び出しは消去されます。
        /// ログを出力するためには "DEBUG" コンパイル定数を定義してください。
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ。null を渡された場合は、空文字列として扱われます。</param>
        /// <param name="message">ログに出力するメッセージ。null を渡された場合は、空文字列として扱われます。</param>
        [Conditional("DEBUG")]
        public static void Debug(string tag, string message)
        {
            // ハンドラの対応する出力を行う
            logHandler.Debug(tag ?? string.Empty, message ?? string.Empty);
        }


        /// <summary>
        /// 情報レベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ。null を渡された場合は、空文字列として扱われます。</param>
        /// <param name="message">ログに出力するメッセージ。null を渡された場合は、空文字列として扱われます。</param>
        public static void Info(string tag, string message)
        {
            // ハンドラの対応する出力を行う
            logHandler.Info(tag ?? string.Empty, message ?? string.Empty);
        }


        /// <summary>
        /// アプリケーション警告レベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ。null を渡された場合は、空文字列として扱われます。</param>
        /// <param name="message">ログに出力するメッセージ。null を渡された場合は、空文字列として扱われます。</param>
        public static void Warning(string tag, string message)
        {
            // ハンドラの対応する出力を行う
            logHandler.Warning(tag ?? string.Empty, message ?? string.Empty);
        }


        /// <summary>
        /// アプリケーションエラーレベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ。null を渡された場合は、空文字列として扱われます。</param>
        /// <param name="message">ログに出力するメッセージ。null を渡された場合は、空文字列として扱われます。</param>
        public static void Error(string tag, string message)
        {
            // ハンドラの対応する出力を行う
            logHandler.Error(tag ?? string.Empty, message ?? string.Empty);
        }


        /// <summary>
        /// 致命的な問題としてログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ。null を渡された場合は、空文字列として扱われます。</param>
        /// <param name="message">ログに出力するメッセージ。null を渡された場合は、空文字列として扱われます。</param>
        /// <param name="exception">致命的な問題となった原因の例外</param>
        /// <exception cref="ArgumentNullException">exception が null です</exception>
        public static void Fatal(string tag, string message, Exception exception)
        {
            // ハンドラの対応する出力を行う
            logHandler.Fatal(tag ?? string.Empty, message ?? string.Empty, exception ?? throw new ArgumentNullException(nameof(exception)));
        }
    }
}
