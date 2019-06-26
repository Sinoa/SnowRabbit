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

namespace SnowRabbit.VirtualMachine.Runtime
{
    /// <summary>
    /// T の固定長配列から特定の範囲を切り出したブロックを参照として読み書きするための構造体です。
    /// </summary>
    /// <typeparam name="T">固定長配列の宣言をしている型</typeparam>
    /// <remarks>
    /// UnityGameEngineにSystem.Memory.dllのSpan<T>が入った場合は、差し替わる予定になっています。
    /// </remarks>
    public readonly struct MemoryBlock<T>
    {
        // メンバ変数定義
        private readonly T[] memoryPool;

        /// <summary>
        /// 内部メモリプールの参照を開始するオフセット
        /// </summary>
        public readonly int Offset;

        /// <summary>
        /// 内部メモリプールから利用する長さ
        /// </summary>
        public readonly int Length;


        /// <summary>
        /// このメモリブロックにおける範囲のインデックスで参照アクセスをします
        /// </summary>
        /// <param name="index">アクセスするインデックス</param>
        /// <returns>インデックスの場所にある T の参照を取得します</returns>
        /// <exception cref="ArgumentOutOfRangeException">index が MemoryBlock コンストラクタで指定された範囲を超えています</exception>
        public ref T this[int index]
        {
            get
            {
#if DEBUG
                // デバッグビルド時のみ境界チェックを行うようにする
                if (index < 0 || index >= Length)
                {
                    // 境界外アクセスは禁止
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
#endif


                // 自身が参照するオフセット位置から指定されたインデックスの参照を返す
                return ref memoryPool[Offset + index];
            }
        }



        /// <summary>
        /// MemoryBlock インスタンスの初期化を行います
        /// </summary>
        /// <param name="values">メモリブロックとして利用する T の配列</param>
        /// <param name="start">values の参照を開始するインデックス位置</param>
        /// <param name="length">values から利用する長さ</param>
        /// <exception cref="ArgumentNullException">values が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">start が 0 未満 または length が 0 未満 または start と length の合計が values の境界を超えています</exception>
        public MemoryBlock(T[] values, int start, int length)
        {
            // null を渡されたら
            if (values == null)
            {
                // 何を参照すればよいのか
                throw new ArgumentNullException(nameof(values));
            }


            // 指示された範囲を超える参照位置を渡されたら、例外を吐く
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (values.Length - start < length) throw new ArgumentOutOfRangeException("start, length", $"start({start}) + length({length}) = {start + length}");


            // メンバ変数の初期化をする
            memoryPool = values;
            Offset = start;
            Length = length;
        }


        /// <summary>
        /// このメモリブロックの範囲内から指定された、位置と長さで新しいメモリブロックを切り出します
        /// </summary>
        /// <param name="offset">このメモリブロックにおける開始位置</param>
        /// <param name="count">切り出すメモリブロックの数</param>
        /// <returns>このメモリブロックから指定された範囲の新しく切り出した MemoryBlock のインスタンスを返します</returns>
        public MemoryBlock<T> Slice(int offset, int count)
        {
            // 指定された範囲で新しいメモリブロックを生成する
            return new MemoryBlock<T>(memoryPool, Offset + offset, count);
        }
    }
}