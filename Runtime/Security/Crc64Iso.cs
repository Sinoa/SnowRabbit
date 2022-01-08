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

namespace SnowRabbit.Security
{
    /// <summary>
    /// CRC64-ISOを提供するクラスです
    /// </summary>
    public class Crc64Iso : Crc64
    {
        /// <summary>
        /// CRC64Isoのインスタンスの初期化を行います
        /// </summary>
        public Crc64Iso()
        {
            // ISOで定義された右回りCRC64多項式の定数値でテーブルを初期化する
            InitializeTable(0xD800000000000000UL);
        }


        /// <summary>
        /// 指定されたバッファ全体を、CRCの計算をします
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <returns>計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        public override unsafe ulong Calculate(byte[] buffer)
        {
            // バッファのアドレスを取得
            fixed (byte* p = buffer)
            {
                // ulong.MaxValueによるXOR反転を利用した計算を行い結果を返す
                return Calculate(ulong.MaxValue, p, buffer.Length) ^ ulong.MaxValue;
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRCの計算を行います
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <param name="index">バッファの開始位置</param>
        /// <param name="count">バッファから取り出す量</param>
        /// <returns>計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">index または index, count 合計値がbufferの範囲を超えます</exception>
        public override unsafe ulong Calculate(byte[] buffer, int index, int count)
        {
            // もし buffer が null なら
            if (buffer == null)
            {
                // そもそも計算が出来ない
                throw new System.ArgumentNullException(nameof(buffer));
            }


            // 指定された index と count で境界を超えないか確認して、超えるなら
            if (index < 0 || buffer.Length <= index + count)
            {
                // 境界を超えるアクセスは非常に危険
                throw new System.ArgumentOutOfRangeException($"{nameof(index)} or {nameof(count)}", $"指定された範囲では {nameof(buffer)} の範囲を超えます");
            }


            // バッファのアドレスを取得
            fixed (byte* p = buffer)
            {
                // ulong.MaxValueによるXOR反転を利用した計算を行い結果を返す
                return Calculate(ulong.MaxValue, p + index, count) ^ ulong.MaxValue;
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRCの計算を行います
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のポインタ</param>
        /// <param name="count">計算するバイトの数</param>
        /// <returns>計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        public override unsafe ulong Calculate(byte* buffer, int count)
        {
            // ulong.MaxValueによるXOR反転を利用した計算を行い結果を返す
            return Calculate(ulong.MaxValue, buffer, count) ^ ulong.MaxValue;
        }
    }
}
