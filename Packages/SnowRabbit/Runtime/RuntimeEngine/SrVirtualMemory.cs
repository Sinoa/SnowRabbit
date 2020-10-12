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
    public readonly struct SrVirtualMemory : IEquatable<SrVirtualMemory>
    {
        // 定数定義
        public const uint GlobalOffset = 0x00100000;
        public const uint HeapOffset = 0x00200000;
        public const uint StackOffset = 0x00300000;
        private const uint SegmentBitMask = 0x000FFFFF;

        // 以下メンバ変数定義
        private readonly MemoryBlock<SrValue>[] Memory;



        /// <summary>
        /// プログラムコードのサイズ
        /// </summary>
        public int ProgramCodeSize => Memory[0].Length;


        /// <summary>
        /// グローバルメモリのサイズ
        /// </summary>
        public int GlobalMemorySize => Memory[1].Length;


        /// <summary>
        /// ヒープメモリのサイズ
        /// </summary>
        public int HeapMemorySize => Memory[2].Length;


        /// <summary>
        /// スタックメモリのサイズ
        /// </summary>
        public int StackMemorySize => Memory[3].Length;



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
        /// <param name="globalMemory">仮想メモリが持つグローバルメモリのメモリブロック</param>
        /// <param name="heapMemory">仮想メモリが持つヒープメモリのメモリブロック</param>
        /// <param name="stackMemory">仮想メモリが持つスタックメモリのメモリブロック</param>
        public SrVirtualMemory(MemoryBlock<SrValue> programCode, MemoryBlock<SrValue> globalMemory, MemoryBlock<SrValue> heapMemory, MemoryBlock<SrValue> stackMemory)
        {
            // メモリセグメント範囲ごとに参照としてもたせる
            Memory = new MemoryBlock<SrValue>[]
            {
                programCode, // Segment 0
                globalMemory, // Segment 1
                heapMemory, // Segment 2
                stackMemory, // Segment 3
            };
        }


        /// <summary>
        /// SrVirtualMemory の等価確認をします
        /// </summary>
        /// <param name="other">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public bool Equals(SrVirtualMemory other) => Memory == other.Memory;


        /// <summary>
        /// object の等価オーバーロードです
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public override bool Equals(object obj) => obj is SrVirtualMemory ? Equals((SrVirtualMemory)obj) : false;


        /// <summary>
        /// ハッシュコードを取得します
        /// </summary>
        /// <returns>ハッシュコードを返します</returns>
        public override int GetHashCode() => base.GetHashCode();


        /// <summary>
        /// 等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>等価の結果を返します</returns>
        public static bool operator ==(SrVirtualMemory left, SrVirtualMemory right) => left.Equals(right);


        /// <summary>
        /// 非等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>非等価の結果を返します</returns>
        public static bool operator !=(SrVirtualMemory left, SrVirtualMemory right) => !left.Equals(right);
    }
}