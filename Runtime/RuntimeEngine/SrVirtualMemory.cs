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
    /// SnowRabbit のプロセスが動作する上で使用するメモリを仮想化した表現を提供する構造体です
    /// </summary>
    public readonly struct SrVirtualMemory
    {
        // 定数定義
        private const uint SegmentBitMask = 0x000FFFFF;

        // 以下メンバ変数定義
        private readonly MemoryBlock<SrValue>[] Memory;



        /// <summary>
        /// 指定された仮想アドレスにメモリアクセスをします
        /// </summary>
        /// <param name="virtualAddress">アクセスする仮想アドレス</param>
        /// <returns>指定されたアドレスの値を返します</returns>
        /// <exception cref="IndexOutOfRangeException">指定された仮想アドレスは境界外です</exception>
        public SrValue this[int virtualAddress]
        {
            get
            {
                // 上位12bitはセグメント番号、下位20bitはオフセットアドレス
                return Memory[virtualAddress >> 20][(int)(virtualAddress & SegmentBitMask)];
            }
            set
            {
                // 上位12bitはセグメント番号、下位20bitはオフセットアドレス
                Memory[virtualAddress >> 20][(int)(virtualAddress & SegmentBitMask)] = value;
            }
        }



        /// <summary>
        /// SrVirtualMemory 構造体のインスタンスを初期化します
        /// </summary>
        /// <param name="programCode">仮想メモリが持つプログラムコードのメモリブロック</param>
        /// <param name="processMemory">仮想メモリが持つプロセスメモリのメモリブロック</param>
        public SrVirtualMemory(MemoryBlock<SrValue> programCode, MemoryBlock<SrValue> processMemory)
        {
            // メモリセグメント範囲ごとに参照としてもたせる
            Memory = new MemoryBlock<SrValue>[]
            {
                programCode, // Segment 0
                processMemory, // Segment 1
            };
        }
    }
}