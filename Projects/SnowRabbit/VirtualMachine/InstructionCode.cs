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
    /// 仮想マシンが実行できる命令コードを表現した共用体です
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct InstructionCode
    {
        #region Whole byte array
        /// <summary>
        /// 命令コード全体を表すバイト配列です
        /// </summary>
        [FieldOffset(0)]
        public fixed byte ByteCode[8];
        #endregion


        #region OpCode and register
        /// <summary>
        /// 実行する命令コード
        /// </summary>
        [FieldOffset(0)]
        public OpCode OpCode;

        /// <summary>
        /// 命令コードが使用するレジスタの位置を示す引数Aです
        /// </summary>
        [FieldOffset(1)]
        public byte Ra;

        /// <summary>
        /// 命令コードが使用するレジスタの位置を示す引数Bです
        /// </summary>
        [FieldOffset(2)]
        public byte Rb;

        /// <summary>
        /// 命令コードが使用するレジスタの位置を示す引数Cです
        /// </summary>
        [FieldOffset(3)]
        public byte Rc;
        #endregion


        #region General region
        /// <summary>
        /// 命令コードが使用する汎用バイト配列です
        /// </summary>
        [FieldOffset(4)]
        public fixed byte GeneralByteCode[4];

        /// <summary>
        /// 命令コードが使用する汎用整数です
        /// </summary>
        [FieldOffset(4)]
        public uint General;
        #endregion


        #region Immediate region
        /// <summary>
        /// 命令コードが使用する即値バイト配列です
        /// </summary>
        [FieldOffset(4)]
        public fixed byte ImmediateByteCode[4];

        /// <summary>
        /// 命令コードが使用する符号なし8bit整数即値です
        /// </summary>
        [FieldOffset(4)]
        public byte ImmediateByte;

        /// <summary>
        /// 命令コードが使用する符号付き8bit整数即値です
        /// </summary>
        [FieldOffset(4)]
        public sbyte ImmediateSbyte;

        /// <summary>
        /// 命令コードが使用する符号なし16bit整数即値です
        /// </summary>
        [FieldOffset(4)]
        public ushort ImmediateUshort;

        /// <summary>
        /// 命令コードが使用する符号付き16bit整数即値です
        /// </summary>
        [FieldOffset(4)]
        public short ImmediateShort;

        /// <summary>
        /// 命令コードが使用する符号なし32bit整数即値です
        /// </summary>
        [FieldOffset(4)]
        public uint ImmediateUint;

        /// <summary>
        /// 命令コードが使用する符号付き32bit整数即値です
        /// </summary>
        [FieldOffset(4)]
        public int ImmediateInt;

        /// <summary>
        /// 命令コードが使用する32bit浮動小数点即値です
        /// </summary>
        [FieldOffset(4)]
        public float ImmediateFloat;
        #endregion
    }
}