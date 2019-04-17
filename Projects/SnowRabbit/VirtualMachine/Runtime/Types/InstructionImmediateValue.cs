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

namespace SnowRabbit.VirtualMachine.Runtime
{
    /// <summary>
    /// 命令コード型のImmediate部の表現をした共用体です
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct InstructionImmediateValue
    {
        /// <summary>
        /// 命令コードが使用するバイト配列即値です
        /// </summary>
        [FieldOffset(0)]
        public fixed byte ByteCode[4];

        /// <summary>
        /// 命令コードが使用する符号なし8bit整数即値です
        /// </summary>
        [FieldOffset(0)]
        public byte Byte;

        /// <summary>
        /// 命令コードが使用する符号付き8bit整数即値です
        /// </summary>
        [FieldOffset(0)]
        public sbyte Sbyte;

        /// <summary>
        /// 命令コードが使用する符号なし16bit整数即値です
        /// </summary>
        [FieldOffset(0)]
        public ushort Ushort;

        /// <summary>
        /// 命令コードが使用する符号付き16bit整数即値です
        /// </summary>
        [FieldOffset(0)]
        public short Short;

        /// <summary>
        /// 命令コードが使用する符号なし32bit整数即値です
        /// </summary>
        [FieldOffset(0)]
        public uint Uint;

        /// <summary>
        /// 命令コードが使用する符号付き32bit整数即値です
        /// </summary>
        [FieldOffset(0)]
        public int Int;

        /// <summary>
        /// 命令コードが使用する32bit浮動小数点即値です
        /// </summary>
        [FieldOffset(0)]
        public float Float;
    }
}