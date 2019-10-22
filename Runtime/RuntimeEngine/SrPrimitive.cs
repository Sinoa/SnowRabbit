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

namespace SnowRabbit.RuntimeEngine
{
    /// <summary>
    /// 仮想マシンが使用するプリミティブ型として表現する構造体です
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct SrPrimitive
    {
        /// <summary>
        /// 符号付き64bit整数
        /// </summary>
        [FieldOffset(0)]
        public long Long;


        /// <summary>
        /// 符号なし64bit整数
        /// </summary>
        [FieldOffset(0)]
        public ulong Ulong;


        /// <summary>
        /// 64bit浮動小数点
        /// </summary>
        [FieldOffset(0)]
        public double Double;


        /// <summary>
        /// 仮想マシンの命令
        /// </summary>
        [FieldOffset(0)]
        public SrInstruction Instruction;
    }
}