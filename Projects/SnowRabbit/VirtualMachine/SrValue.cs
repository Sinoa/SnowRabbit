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

using System.Runtime.InteropServices;

namespace SnowRabbit.VirtualMachine
{
    /// <summary>
    /// 仮想マシンの変数型を扱う型でメモリレイアウトを定義する共用体です
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct SrValue
    {
        #region Type of Number
        /// <summary>
        /// 1つの64bit浮動小数点
        /// </summary>
        [FieldOffset(0)]
        public fixed double Double[1];

        /// <summary>
        /// 2つの32bit浮動小数点
        /// </summary>
        [FieldOffset(0)]
        public fixed float Float[2];
        #endregion


        #region Type of Signed Integer
        /// <summary>
        /// 1つの符号付き64bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed long Long[1];

        /// <summary>
        /// 2つの符号付き32bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed int Int[2];

        /// <summary>
        /// 4つの符号付き16bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed short Short[4];

        /// <summary>
        /// 8つの符号付き8bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed sbyte Sbyte[8];
        #endregion


        #region Type of Unsigned Integer
        /// <summary>
        /// 1つの符号なし64bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed ulong Ulong[1];

        /// <summary>
        /// 2つの符号なし32bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed uint Uint[2];

        /// <summary>
        /// 4つの符号なし16bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed ushort Ushort[4];

        /// <summary>
        /// 8つの符号なし8bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed byte Byte[8];
        #endregion


        #region Type of General
        /// <summary>
        /// 4つのUnicode文字
        /// </summary>
        [FieldOffset(0)]
        public fixed char Char[4];

        /// <summary>
        /// 8つのブール
        /// </summary>
        [FieldOffset(0)]
        public fixed bool Bool[8];
        #endregion
    }
}