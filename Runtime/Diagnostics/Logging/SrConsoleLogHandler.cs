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

namespace SnowRabbit.Diagnostics.Logging
{
    /// <summary>
    /// コンソールウィンドウにログを出力するログハンドラクラスです
    /// </summary>
    public class SrConsoleLogHandler : ISrLogHandler
    {
        /// <summary>
        /// SrConsoleLogHandler へのインスタンス
        /// </summary>
        public static readonly ISrLogHandler Instance = new SrConsoleLogHandler();



        /// <summary>
        /// トレースレベルの背景色
        /// </summary>
        public ConsoleColor TraceBackgroundColor { get; set; } = ConsoleColor.Black;


        /// <summary>
        /// トレースレベルの前景色
        /// </summary>
        public ConsoleColor TraceForegroundColor { get; set; } = ConsoleColor.Cyan;


        /// <summary>
        /// デバッグレベルの背景色
        /// </summary>
        public ConsoleColor DebugBackgroundColor { get; set; } = ConsoleColor.Black;


        /// <summary>
        /// デバッグレベルの前景色
        /// </summary>
        public ConsoleColor DebugForegroundColor { get; set; } = ConsoleColor.DarkGray;


        /// <summary>
        /// 情報レベルの背景色
        /// </summary>
        public ConsoleColor InfoBackgroundColor { get; set; } = ConsoleColor.Black;


        /// <summary>
        /// 情報レベルの前景色
        /// </summary>
        public ConsoleColor InfoForegroundColor { get; set; } = ConsoleColor.White;


        /// <summary>
        /// 警告レベルの背景色
        /// </summary>
        public ConsoleColor WarningBackgroundColor { get; set; } = ConsoleColor.Black;


        /// <summary>
        /// 警告レベルの前景色
        /// </summary>
        public ConsoleColor WarningForegroundColor { get; set; } = ConsoleColor.DarkYellow;


        /// <summary>
        /// エラーレベルの背景色
        /// </summary>
        public ConsoleColor ErrorBackgroundColor { get; set; } = ConsoleColor.Black;


        /// <summary>
        /// エラーレベルの前景色
        /// </summary>
        public ConsoleColor ErrorForegroundColor { get; set; } = ConsoleColor.Red;


        /// <summary>
        /// 致命的問題レベルの背景色
        /// </summary>
        public ConsoleColor FatalBackgroundColor { get; set; } = ConsoleColor.Gray;


        /// <summary>
        /// 致命的問題レベルの前景色
        /// </summary>
        public ConsoleColor FatalForegroundColor { get; set; } = ConsoleColor.DarkRed;



        /// <summary>
        /// トレースレベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ</param>
        /// <param name="message">ログに出力するメッセージ</param>
        public void Trace(string tag, string message)
        {
            // テキストをフォーマットして書き込む
            Write($"{tag}:{message}", TraceBackgroundColor, TraceForegroundColor);
        }


        /// <summary>
        /// デバッグレベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ</param>
        /// <param name="message">ログに出力するメッセージ</param>
        public void Debug(string tag, string message)
        {
            // テキストをフォーマットして書き込む
            Write($"{tag}:{message}", DebugBackgroundColor, DebugForegroundColor);
        }


        /// <summary>
        /// 情報レベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ</param>
        /// <param name="message">ログに出力するメッセージ</param>
        public void Info(string tag, string message)
        {
            // テキストをフォーマットして書き込む
            Write($"{tag}:{message}", InfoBackgroundColor, InfoForegroundColor);
        }


        /// <summary>
        /// 警告レベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ</param>
        /// <param name="message">ログに出力するメッセージ</param>
        public void Warning(string tag, string message)
        {
            // テキストをフォーマットして書き込む
            Write($"{tag}:{message}", WarningBackgroundColor, WarningForegroundColor);
        }


        /// <summary>
        /// エラーレベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ</param>
        /// <param name="message">ログに出力するメッセージ</param>
        public void Error(string tag, string message)
        {
            // テキストをフォーマットして書き込む
            Write($"{tag}:{message}", ErrorBackgroundColor, ErrorForegroundColor);
        }


        /// <summary>
        /// 致命的な問題レベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ</param>
        /// <param name="message">ログに出力するメッセージ</param>
        /// <param name="exception">致命的な問題となった原因の例外</param>
        public void Fatal(string tag, string message, Exception exception)
        {
            // テキストをフォーマットして書き込む
            Write($"{tag}:{message}\n  [{exception.Message}]", FatalBackgroundColor, FatalForegroundColor);
        }


        /// <summary>
        /// 指定されたテキストと色を使ってコンソールに書き込みます
        /// </summary>
        /// <param name="text">出力するテキスト</param>
        /// <param name="background">出力するテキストの背景色</param>
        /// <param name="foreground">出力するテキストの前景色</param>
        private void Write(string text, ConsoleColor background, ConsoleColor foreground)
        {
            // 指定された色を設定してログを出す
            var backgroundColor = Console.BackgroundColor;
            var foregroundColor = Console.ForegroundColor;
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
            Console.WriteLine(text);
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
        }
    }
}