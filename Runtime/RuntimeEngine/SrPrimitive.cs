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
using System.Runtime.InteropServices;

namespace SnowRabbit.RuntimeEngine
{
    /// <summary>
    /// 仮想マシンが使用するプリミティブ型として表現する構造体です
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct SrPrimitive : IEquatable<SrPrimitive>
    {
        /// <summary>
        /// 符号付き8bit整数
        /// </summary>
        [FieldOffset(0)]
        public sbyte Sbyte;


        /// <summary>
        /// 符号なし8bit整数
        /// </summary>
        [FieldOffset(0)]
        public byte Byte;


        /// <summary>
        /// 符号なし16bit整数（Unicode）
        /// </summary>
        [FieldOffset(0)]
        public char Char;


        /// <summary>
        /// 符号付き16bit整数
        /// </summary>
        [FieldOffset(0)]
        public short Short;


        /// <summary>
        /// 符号なし16bit整数
        /// </summary>
        [FieldOffset(0)]
        public ushort Ushort;


        /// <summary>
        /// 符号付き32bit整数
        /// </summary>
        [FieldOffset(0)]
        public int Int;


        /// <summary>
        /// 符号なし32bit整数
        /// </summary>
        [FieldOffset(0)]
        public uint Uint;


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
        /// 32bit浮動小数点
        /// </summary>
        [FieldOffset(0)]
        public float Float;


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



        /// <summary>
        /// SrPrimitive の等価確認をします
        /// </summary>
        /// <param name="other">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public bool Equals(SrPrimitive other) => Ulong == other.Ulong;


        /// <summary>
        /// 等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>等価の結果を返します</returns>
        public static bool operator ==(SrPrimitive left, SrPrimitive right) => left.Equals(right);


        /// <summary>
        /// 非等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>非等価の結果を返します</returns>
        public static bool operator !=(SrPrimitive left, SrPrimitive right) => !left.Equals(right);


        /// <summary>
        /// object の等価オーバーロードです
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public override bool Equals(object obj) => obj is SrPrimitive ? Equals((SrPrimitive)obj) : false;


        /// <summary>
        /// ハッシュコードを取得します
        /// </summary>
        /// <returns>ハッシュコードを返します</returns>
        public override int GetHashCode() => Ulong.GetHashCode();
    }
}