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

namespace SnowRabbit
{
    /// <summary>
    /// 内部用の文字列を取り扱うクラスです
    /// </summary>
    internal static class InternalString
    {
        /// <summary>
        /// Conditional 属性に設定する文字列を保持したクラスです
        /// </summary>
        public static class Conditional
        {
            /// <summary>
            /// RELEASE コンパイル定数
            /// </summary>
            public const string RELEASE = "RELEASE";


            /// <summary>
            /// DEBUG コンパイル定数
            /// </summary>
            public const string DEBUG = "DEBUG";


            /// <summary>
            /// TRACE コンパイル定数
            /// </summary>
            public const string TRACE = "TRACE";


            /// <summary>
            /// SR_PROFILING コンパイル定数
            /// </summary>
            public const string PROFILING = "SR_PROFILING";
        }



        /// <summary>
        /// ロガーに渡すタグ文字列を保持したクラスです
        /// </summary>
        public static class LogTag
        {
            /// <summary>
            /// プロファイラに設定されたログのタグ文字列
            /// </summary>
            public const string PROFILER = "Profiler";
        }



        /// <summary>
        /// ロガーに渡すメッセージ文字列を保持したクラスです
        /// </summary>
        public static class LogMessage
        {
            /// <summary>
            /// プロファイラ関連のログメッセージを保持したクラスです
            /// </summary>
            public static class Profiler
            {
                /// <summary>
                /// まだ準備が出来ていないログメッセージ
                /// </summary>
                public const string NOT_READY = "プロファイラの準備が出来ていません";


                /// <summary>
                /// 既に準備が出来ているログメッセージ
                /// </summary>
                public const string ALREADY_READY = "プロファイラは既に準備が出来ています";
            }
        }



        /// <summary>
        /// 例外メッセージ文字列を保持したクラスです
        /// </summary>
        public static class ExceptionMessage
        {
            /// <summary>
            /// プロファイラ関連の例外メッセージを保持したクラスです
            /// </summary>
            public static class Profiler
            {
                /// <summary>
                /// プロファイル値名が無効な時の例外メッセージ
                /// </summary>
                public const string INVALID_VALUE_NAME = "プロファイル値名が null または 空文字列 です";
            }
        }
    }
}