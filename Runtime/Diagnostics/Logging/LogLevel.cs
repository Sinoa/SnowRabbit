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

namespace SnowRabbit.Diagnostics.Logging
{
    /// <summary>
    /// ログ出力レベルを指定します
    /// </summary>
    public enum LogLevel : int
    {
        /// <summary>
        /// トレースレベルの出力です。出力するには "TRACE" コンパイル定数を指定する必要があります。
        /// </summary>
        Trace = 0,

        /// <summary>
        /// デバッグレベルの出力です。出力するには "DEBUG" コンパイル定数を指定する必要があります。
        /// </summary>
        Debug = 1,

        /// <summary>
        /// 情報レベルの出力です
        /// </summary>
        Info = 2,

        /// <summary>
        /// 警告レベルの出力です
        /// </summary>
        Warning = 3,

        /// <summary>
        /// エラーレベルの出力です
        /// </summary>
        Error = 4,

        /// <summary>
        /// 致命的問題レベルの出力です
        /// </summary>
        Fatal = 5,
    }
}
