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

namespace SnowRabbit.RuntimeEngine
{
    /// <summary>
    /// 仮想マシンが使用する値の最小単位を表現した構造体です
    /// </summary>
    public struct SrValue : IEquatable<SrValue>
    {
        /// <summary>
        /// プリミティブ型としての値
        /// </summary>
        public SrPrimitive Primitive;


        /// <summary>
        /// 参照型としての値
        /// </summary>
        public object Object;



        /// <summary>
        /// sbyte から SrValue へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">sbyte の値</param>
        public static implicit operator SrValue(sbyte value)
        {
            // sbyte を受け取る
            SrValue srValue = default;
            srValue.Primitive.Sbyte = value;
            return srValue;
        }


        /// <summary>
        /// byte から SrValue へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">byte の値</param>
        public static implicit operator SrValue(byte value)
        {
            // byte を受け取る
            SrValue srValue = default;
            srValue.Primitive.Byte = value;
            return srValue;
        }


        /// <summary>
        /// short から SrValue へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">short の値</param>
        public static implicit operator SrValue(short value)
        {
            // short を受け取る
            SrValue srValue = default;
            srValue.Primitive.Short = value;
            return srValue;
        }


        /// <summary>
        /// ushort から SrValue へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">ushort の値</param>
        public static implicit operator SrValue(ushort value)
        {
            // ushort を受け取る
            SrValue srValue = default;
            srValue.Primitive.Ushort = value;
            return srValue;
        }


        /// <summary>
        /// char から SrValue へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">char の値</param>
        public static implicit operator SrValue(char value)
        {
            // char を受け取る
            SrValue srValue = default;
            srValue.Primitive.Char = value;
            return srValue;
        }


        /// <summary>
        /// int から SrValue へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">int の値</param>
        public static implicit operator SrValue(int value)
        {
            // int を受け取る
            SrValue srValue = default;
            srValue.Primitive.Int = value;
            return srValue;
        }


        /// <summary>
        /// uint から SrValue へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">uint の値</param>
        public static implicit operator SrValue(uint value)
        {
            // uint を受け取る
            SrValue srValue = default;
            srValue.Primitive.Uint = value;
            return srValue;
        }


        /// <summary>
        /// long から SrValue へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">long の値</param>
        public static implicit operator SrValue(long value)
        {
            // long を受け取る
            SrValue srValue = default;
            srValue.Primitive.Long = value;
            return srValue;
        }


        /// <summary>
        /// ulong から SrValue へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">ulong の値</param>
        public static implicit operator SrValue(ulong value)
        {
            // ulong を受け取る
            SrValue srValue = default;
            srValue.Primitive.Ulong = value;
            return srValue;
        }


        /// <summary>
        /// float から SrValue へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">float の値</param>
        public static implicit operator SrValue(float value)
        {
            // float を受け取る
            SrValue srValue = default;
            srValue.Primitive.Float = value;
            return srValue;
        }


        /// <summary>
        /// double から SrValue へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">double の値</param>
        public static implicit operator SrValue(double value)
        {
            // double を受け取る
            SrValue srValue = default;
            srValue.Primitive.Double = value;
            return srValue;
        }


        /// <summary>
        /// string から SrValue へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">string の値</param>
        public static implicit operator SrValue(string value)
        {
            // string を受け取る
            SrValue srValue = default;
            srValue.Object = value;
            return srValue;
        }


        /// <summary>
        /// SrInstruction から SrValue へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrInstruction の値</param>
        public static implicit operator SrValue(SrInstruction value)
        {
            // SrInstruction を受け取る
            SrValue srValue = default;
            srValue.Primitive.Instruction = value;
            return srValue;
        }


        /// <summary>
        /// SrValue から sbyte へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        public static implicit operator sbyte(SrValue value) => value.Primitive.Sbyte;


        /// <summary>
        /// SrValue から byte へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        public static implicit operator byte(SrValue value) => value.Primitive.Byte;


        /// <summary>
        /// SrValue から short へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        public static implicit operator short(SrValue value) => value.Primitive.Short;


        /// <summary>
        /// SrValue から ushort へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        public static implicit operator ushort(SrValue value) => value.Primitive.Ushort;


        /// <summary>
        /// SrValue から char へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        public static implicit operator char(SrValue value) => value.Primitive.Char;


        /// <summary>
        /// SrValue から int へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        public static implicit operator int(SrValue value) => value.Primitive.Int;


        /// <summary>
        /// SrValue から uint へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        public static implicit operator uint(SrValue value) => value.Primitive.Uint;


        /// <summary>
        /// SrValue から long へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        public static implicit operator long(SrValue value) => value.Primitive.Long;


        /// <summary>
        /// SrValue から ulong へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        public static implicit operator ulong(SrValue value) => value.Primitive.Ulong;


        /// <summary>
        /// SrValue から float へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        public static implicit operator float(SrValue value) => value.Primitive.Float;


        /// <summary>
        /// SrValue から double へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        public static implicit operator double(SrValue value) => value.Primitive.Double;


        /// <summary>
        /// SrValue から string へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        public static implicit operator string(SrValue value) => value.Object as string;


        /// <summary>
        /// SrValue から SrInstruction へのキャストオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        public static implicit operator SrInstruction(SrValue value) => value.Primitive.Instruction;


        /// <summary>
        /// true の評価のオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        /// <returns>true の条件を結果を返します</returns>
        public static bool operator true(SrValue value) => value.Primitive.Long != 0 || value.Object != null;


        /// <summary>
        /// false の評価のオーバーロードです
        /// </summary>
        /// <param name="value">SrValue インスタンス</param>
        /// <returns>false の条件を結果を返します</returns>
        public static bool operator false(SrValue value) => value.Primitive.Long == 0 || value.Object == null;


        /// <summary>
        /// SrValue の等価確認をします
        /// </summary>
        /// <param name="other">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public bool Equals(SrValue other) => Primitive == other.Primitive && Object == other.Object;


        /// <summary>
        /// 等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>等価の結果を返します</returns>
        public static bool operator ==(SrValue left, SrValue right) => left.Equals(right);


        /// <summary>
        /// 非等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>非等価の結果を返します</returns>
        public static bool operator !=(SrValue left, SrValue right) => !left.Equals(right);


        /// <summary>
        /// object の等価オーバーロードです
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public override bool Equals(object obj) => obj is SrValue ? Equals((SrValue)obj) : false;


        /// <summary>
        /// ハッシュコードを取得します
        /// </summary>
        /// <returns>ハッシュコードを返します</returns>
        public override int GetHashCode() => Object == null ? Primitive.GetHashCode() : base.GetHashCode();
    }
}