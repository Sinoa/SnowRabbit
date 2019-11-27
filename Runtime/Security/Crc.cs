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
    /// 巡回冗長検査アルゴリズムの基本となる実装を提供するクラスです。
    /// </summary>
    /// <remarks>
    /// CRCの実装については "https://en.wikipedia.org/wiki/Cyclic_redundancy_check" を参照して下さい
    /// </remarks>
    /// <typeparam name="T">CRCビットサイズに該当する符号なし整数型を指定します</typeparam>
    public abstract class Crc<T>
    {
        /// <summary>
        /// CRCテーブルを初期化します
        /// </summary>
        /// <param name="polynomial">CRCで使用する多項式の値</param>
        protected abstract void InitializeTable(T polynomial);


        /// <summary>
        /// 指定されたバッファ全体を、CRCの計算をします
        /// </summary>
        /// <remarks>
        /// この関数は、継続的にCRC計算をするのではなく、この関数の呼び出し一回で終了される事を想定します。
        /// </remarks>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <returns>計算された結果を返します</returns>
        public abstract T Calculate(byte[] buffer);


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
        public abstract T Calculate(byte[] buffer, int index, int count);


        /// <summary>
        /// 指定されたバッファ全体を、CRCの計算をします
        /// </summary>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <returns>計算された結果を返します</returns>
        public abstract T Calculate(T continusHash, byte[] buffer);


        /// <summary>
        /// 指定されたバッファの範囲を、CRCの計算を行います
        /// </summary>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のバッファ</param>
        /// <param name="index">バッファの開始位置</param>
        /// <param name="count">バッファから取り出す量</param>
        /// <returns>計算された結果を返します</returns>
        public abstract T Calculate(T continusHash, byte[] buffer, int index, int count);


        /// <summary>
        /// 指定されたバッファの、CRC計算を行います
        /// </summary>
        /// <param name="buffer">計算する対象のポインタ</param>
        /// <param name="count">計算するサイズ</param>
        /// <returns>計算された結果を返します</returns>
        unsafe public abstract T Calculate(byte* buffer, int count);


        /// <summary>
        /// 指定されたバッファの、CRC計算を行います
        /// </summary>
        /// <param name="continusHash">前回計算したハッシュ値、存在しない場合は既定値を指定</param>
        /// <param name="buffer">計算する対象のポインタ</param>
        /// <param name="count">計算するサイズ</param>
        /// <returns>計算された結果を返します</returns>
        unsafe public abstract T Calculate(T continusHash, byte* buffer, int count);
    }
}
