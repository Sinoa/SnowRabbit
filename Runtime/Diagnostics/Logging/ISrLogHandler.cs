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
    /// SrLogger によって実際にログ操作をするインターフェイスです
    /// </summary>
    public interface ISrLogHandler
    {
        /// <summary>
        /// トレースレベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ</param>
        /// <param name="message">ログに出力するメッセージ</param>
        void Trace(string tag, string message);


        /// <summary>
        /// デバッグレベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ</param>
        /// <param name="message">ログに出力するメッセージ</param>
        void Debug(string tag, string message);


        /// <summary>
        /// 情報レベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ</param>
        /// <param name="message">ログに出力するメッセージ</param>
        void Info(string tag, string message);


        /// <summary>
        /// 警告レベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ</param>
        /// <param name="message">ログに出力するメッセージ</param>
        void Warning(string tag, string message);


        /// <summary>
        /// エラーレベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ</param>
        /// <param name="message">ログに出力するメッセージ</param>
        void Error(string tag, string message);


        /// <summary>
        /// 致命的な問題レベルのログを出力します
        /// </summary>
        /// <param name="tag">ログに紐付けるタグ</param>
        /// <param name="message">ログに出力するメッセージ</param>
        /// <param name="exception">致命的な問題となった原因の例外</param>
        void Fatal(string tag, string message, Exception exception);
    }
}