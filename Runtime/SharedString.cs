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
    internal static class SharedString
    {
        /// <summary>
        /// Conditional 属性に設定する文字列を保持したクラスです
        /// </summary>
        internal static class Conditional
        {
            /// <summary>
            /// RELEASE コンパイル定数
            /// </summary>
            internal const string RELEASE = "RELEASE";


            /// <summary>
            /// DEBUG コンパイル定数
            /// </summary>
            internal const string DEBUG = "DEBUG";


            /// <summary>
            /// TRACE コンパイル定数
            /// </summary>
            internal const string TRACE = "TRACE";
        }



        /// <summary>
        /// ロガーに渡すタグ文字列を保持したクラスです
        /// </summary>
        internal static class LogTag
        {
            /// <summary>
            /// 仮想マシンタグ
            /// </summary>
            internal const string VIRTUAL_MACHINE = "VirtualMachine";


            /// <summary>
            /// 周辺機器タグ
            /// </summary>
            internal const string PERIPHERAL = "Peripheral";
        }
    }
}