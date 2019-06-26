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
    /// 仮想マシンが使用するメモリを確保するメモリアロケータの抽象クラスです
    /// </summary>
    /// <typeparam name="T">メモリ管理をする対象の型</typeparam>
    public abstract class MemoryAllocator<T> : IMemoryAllocator<T>
    {
        /// <summary>
        /// メモリの確保をします。確保されるメモリのサイズは size が収まる様に確保されますが、実際の確保サイズは必ず一致しているかどうかを保証していません。
        /// </summary>
        /// <param name="size">確保するメモリのサイズ</param>
        /// <param name="type">確保するメモリの利用タイプ</param>
        /// <returns>確保したメモリのブロックを返します</returns>
        /// <exception cref="ArgumentOutOfRangeException">確保するサイズに 0 以下の指定は出来ません</exception>
        /// <exception cref="SrOutOfMemoryException">指定されたサイズのメモリを確保できませんでした</exception>
        /// <exception cref="ArgumentException">メモリ確保の種類に 'Free' の指定は出来ません</exception>
        public abstract MemoryBlock<T> Allocate(int size, AllocationType type);


        /// <summary>
        /// 指定されたメモリブロックを解放します。
        /// </summary>
        /// <param name="memoryBlock">解放するメモリブロック</param>
        public abstract void Deallocate(MemoryBlock<T> memoryBlock);


        /// <summary>
        /// 無効な確保サイズ または 空き領域タイプとして 要求された時に例外をスローします
        /// </summary>
        /// <param name="size">判断するための要求メモリサイズ</param>
        /// <exception cref="ArgumentOutOfRangeException">確保するサイズに 0 以下の指定は出来ません</exception>
        /// <exception cref="ArgumentException">メモリ確保の種類に 'Free' の指定は出来ません</exception>
        protected void ThrowExceptionIfRequestInvalidSizeOrType(int size, AllocationType type)
        {
            // 0 以下のサイズ要求が有った場合は
            if (size <= 0)
            {
                // 例外を吐く
                throw new ArgumentOutOfRangeException(nameof(size), "確保するサイズに 0 以下の指定は出来ません");
            }


            // 空き領域タイプの要求は無効
            if (type == AllocationType.Free)
            {
                // 例外を吐く
                throw new ArgumentException($"メモリ確保の種類に '{nameof(AllocationType.Free)}' の指定は出来ません", nameof(type));
            }
        }


        /// <summary>
        /// 要求メモリサイズの確保が出来ない例外をスローします
        /// </summary>
        protected void ThrowExceptionSrOutOfMemory()
        {
            // 無条件で例外をスローする
            throw new SrOutOfMemoryException("指定されたサイズのメモリを確保できませんでした");
        }
    }
}