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
using static SnowRabbit.Runtime.MemoryAllocatorUtility;

namespace SnowRabbit.Runtime
{
    /// <summary>
    /// 直接.NETランタイムからメモリを割り当てるメモリアロケータクラスです
    /// </summary>
    /// <typeparam name="T">メモリ管理をする対象の型</typeparam>
    public class DynamicManagedMemoryAllocator<T> : MemoryAllocator<T>
    {
        /// <summary>
        /// メモリの確保をします。確保されるメモリのサイズは size が収まる様に最大8の倍数に調整された確保が行われます。
        /// </summary>
        /// <param name="size">確保するメモリのサイズ</param>
        /// <param name="type">確保するメモリの利用タイプ</param>
        /// <returns>確保したメモリのブロックを返します</returns>
        /// <exception cref="ArgumentOutOfRangeException">確保するサイズに 0 以下の指定は出来ません</exception>
        /// <exception cref="OutOfMemoryException">指定されたサイズのメモリを確保できませんでした</exception>
        /// <exception cref="ArgumentException">メモリ確保の種類に 'Free' の指定は出来ません</exception>
        public override MemoryBlock<T> Allocate(int size, AllocationType type)
        {
            // 事前の例外ハンドリングをしてから、サイズを計算後メモリブロックを生成して返す
            ThrowExceptionIfRequestInvalidSizeOrType(size, type);
            var allocateBlock = ByteSizeToElementCount(size);
            return new MemoryBlock<T>(new T[allocateBlock], 0, allocateBlock);
        }


        /// <summary>
        /// 指定されたメモリブロックを解放します。
        /// </summary>
        /// <param name="memoryBlock">解放するメモリブロック</param>
        public override void Deallocate(MemoryBlock<T> memoryBlock)
        {
            // .NETランタイムへ返却するだけなので、このメモリアロケータは何もしません
        }
    }
}