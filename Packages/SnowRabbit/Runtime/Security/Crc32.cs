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
    /// CRC32向けの基本クラスです。
    /// CRCの32bit長のクラスを実装する場合はこのクラスを継承して下さい。
    /// </summary>
    public abstract class Crc32 : Crc<uint>
    {
        // 以下メンバ変数定義
        private uint[] table;



        /// <summary>
        /// CRC32向けのテーブルを初期化します
        /// </summary>
        /// <param name="polynomial">CRC32で利用する多項式の値</param>
        protected override void InitializeTable(uint polynomial)
        {
            // 既にテーブルが作成済みなら何もせず終了
            if (table != null) return;


            // テーブル用配列を生成してテーブルの要素分ループする
            table = new uint[256];
            for (uint i = 0U; i < (uint)table.Length; ++i)
            {
                // 要素の計算を行う
                uint num = ((i & 1) * polynomial) ^ (i >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);
                num = ((num & 1) * polynomial) ^ (num >> 1);


                // 計算結果を入れる
                table[i] = num;
            }
        }


        /// <summary>
        /// 指定されたバッファ全体を、CRC32の計算を行います
        /// </summary>
        /// <remarks>
        /// バッファが複数に分かれて、継続して計算する場合は、この関数が返したハッシュ値をそのまま continusHash パラメータに渡して計算を行って下さい。
        /// また、初回の計算をする前に continusHash へ uint.MaxValue をセットし、すべてのバッファ処理が終了後 uint.MaxValue の XOR 反転を行って下さい。
        /// </remarks>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <returns>CRC32計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        public override unsafe uint Calculate(uint continusHash, byte[] buffer)
        {
            // バッファのアドレスを取得
            fixed (byte* p = buffer)
            {
                // ポインタ版関数を呼ぶ
                return Calculate(continusHash, p, buffer.Length);
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRC32の計算を行います
        /// </summary>
        /// <remarks>
        /// バッファが複数に分かれて、継続して計算する場合は、この関数が返したハッシュ値をそのまま continusHash パラメータに渡して計算を行って下さい。
        /// また、初回の計算をする前に continusHash へ uint.MaxValue をセットし、すべてのバッファ処理が終了後 uint.MaxValue の XOR 反転を行って下さい。
        /// </remarks>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <param name="index">バッファの開始位置</param>
        /// <param name="count">バッファから取り出す量</param>
        /// <returns>CRC32計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">index または index, count 合計値がbufferの範囲を超えます</exception>
        public override unsafe uint Calculate(uint continusHash, byte[] buffer, int index, int count)
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
                // ポインタ版関数を呼ぶ
                return Calculate(continusHash, p + index, count);
            }
        }


        /// <summary>
        /// 指定されたバッファの範囲を、CRC32の計算を行います
        /// </summary>
        /// <remarks>
        /// バッファが複数に分かれて、継続して計算する場合は、この関数が返したハッシュ値をそのまま continusHash パラメータに渡して計算を行って下さい。
        /// また、初回の計算をする前に continusHash へ uint.MaxValue をセットし、すべてのバッファ処理が終了後 uint.MaxValue の XOR 反転を行って下さい。
        /// </remarks>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のポインタ</param>
        /// <param name="count">計算するバイトの数</param>
        /// <returns>CRC32計算された結果を返します</returns>
        /// <exception cref="ArgumentNullException">buffer が null です</exception>
        public override unsafe uint Calculate(uint continusHash, byte* buffer, int count)
        {
            // もし buffer が null なら
            if (buffer == null)
            {
                // そもそも計算が出来ない
                throw new System.ArgumentNullException(nameof(buffer));
            }


            // 指定バッファ範囲分ループする
            for (int i = 0; i < count; ++i)
            {
                // CRC計算をする
                continusHash = table[(*buffer ^ continusHash) & 0xFF] ^ (continusHash >> 8);
                ++buffer;
            }


            // 計算結果を返す
            return continusHash;
        }
    }
}
