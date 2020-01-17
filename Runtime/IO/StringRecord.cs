using System;

namespace SnowRabbit.IO
{
    public struct StringRecord : IEquatable<StringRecord>
    {
        public int Address;
        public int Offset;
        public int Length;



        /// <summary>
        /// StringRecord の等価確認をします
        /// </summary>
        /// <param name="other">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public bool Equals(StringRecord other) => Address == other.Address && Offset == other.Offset && Length == other.Length;


        /// <summary>
        /// 等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>等価の結果を返します</returns>
        public static bool operator ==(StringRecord left, StringRecord right) => left.Equals(right);


        /// <summary>
        /// 非等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>非等価の結果を返します</returns>
        public static bool operator !=(StringRecord left, StringRecord right) => !left.Equals(right);


        /// <summary>
        /// object の等価オーバーロードです
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public override bool Equals(object obj) => obj is StringRecord ? Equals((StringRecord)obj) : false;


        /// <summary>
        /// ハッシュコードを取得します
        /// </summary>
        /// <returns>ハッシュコードを返します</returns>
        public override int GetHashCode() => base.GetHashCode();
    }
}
