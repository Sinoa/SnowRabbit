﻿// zlib/libpng License
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
    #region interface
    /// <summary>
    /// 仮想マシンが使用するメモリを確保するメモリアロケータインターフェイスです
    /// </summary>
    public interface IMemoryAllocator
    {
        /// <summary>
        /// メモリの確保をします。確保されるメモリのサイズは size が収まる様に確保されますが、実際の確保サイズは必ず一致しているかどうかを保証していません。
        /// </summary>
        /// <param name="size">確保するメモリのサイズ</param>
        /// <param name="type">確保するメモリの利用タイプ</param>
        /// <returns>確保したメモリのブロックを返します</returns>
        /// <exception cref="ArgumentOutOfRangeException">確保するサイズに 0 以下の指定は出来ません</exception>
        /// <exception cref="SrOutOfMemoryException">指定されたサイズのメモリを確保できませんでした</exception>
        MemoryBlock Allocate(int size, AllocationType type);


        /// <summary>
        /// 指定されたメモリブロックを解放します。
        /// </summary>
        /// <param name="memoryBlock">解放するメモリブロック</param>
        void Deallocate(MemoryBlock memoryBlock);
    }
    #endregion



    #region abstract MemoryAllocator
    /// <summary>
    /// 仮想マシンが使用するメモリを確保するメモリアロケータの抽象クラスです
    /// </summary>
    public abstract class MemoryAllocator : IMemoryAllocator
    {
        /// <summary>
        /// メモリの確保をします。確保されるメモリのサイズは size が収まる様に確保されますが、実際の確保サイズは必ず一致しているかどうかを保証していません。
        /// </summary>
        /// <param name="size">確保するメモリのサイズ</param>
        /// <param name="type">確保するメモリの利用タイプ</param>
        /// <returns>確保したメモリのブロックを返します</returns>
        /// <exception cref="ArgumentOutOfRangeException">確保するサイズに 0 以下の指定は出来ません</exception>
        /// <exception cref="SrOutOfMemoryException">指定されたサイズのメモリを確保できませんでした</exception>
        public abstract MemoryBlock Allocate(int size, AllocationType type);


        /// <summary>
        /// 指定されたメモリブロックを解放します。
        /// </summary>
        /// <param name="memoryBlock">解放するメモリブロック</param>
        public abstract void Deallocate(MemoryBlock memoryBlock);


        /// <summary>
        /// 無効な確保サイズを要求された時に例外をスローします
        /// </summary>
        /// <param name="size">判断するための要求メモリサイズ</param>
        /// <exception cref="ArgumentOutOfRangeException">確保するサイズに 0 以下の指定は出来ません</exception>
        protected void ThrowExceptionIfRequestInvalidSize(int size)
        {
            // 0 以下のサイズ要求が有った場合は
            if (size <= 0)
            {
                // 例外を吐く
                throw new ArgumentOutOfRangeException(nameof(size), "確保するサイズに 0 以下の指定は出来ません");
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
    #endregion



    #region Dynamic MemoryAllocator
    /// <summary>
    /// 直接.NETランタイムからメモリを割り当てるメモリアロケータクラスです
    /// </summary>
    public class DynamicManagedMemoryAllocator : MemoryAllocator
    {
        /// <summary>
        /// メモリの確保をします。確保されるメモリのサイズは size が収まる様に最大8の倍数に調整された確保が行われます。
        /// </summary>
        /// <param name="size">確保するメモリのサイズ</param>
        /// <param name="type">確保するメモリの利用タイプ</param>
        /// <returns>確保したメモリのブロックを返します</returns>
        /// <exception cref="ArgumentOutOfRangeException">確保するサイズに 0 以下の指定は出来ません</exception>
        /// <exception cref="OutOfMemoryException">指定されたサイズのメモリを確保できませんでした</exception>
        public override MemoryBlock Allocate(int size, AllocationType type)
        {
            // 事前の例外ハンドリングを行う
            ThrowExceptionIfRequestInvalidSize(size);


            // 要求サイズから確保するべき配列の要素数を求める
            var allocateSize = size + ((8 - (size & 7)) & 7);
            var allocateBlock = allocateSize >> 3;


            // メモリブロックを生成後、必要配列数分確保して返す
            return new MemoryBlock(new SrValue[allocateBlock], 0, allocateBlock);
        }


        /// <summary>
        /// 指定されたメモリブロックを解放します。
        /// </summary>
        /// <param name="memoryBlock">解放するメモリブロック</param>
        public override void Deallocate(MemoryBlock memoryBlock)
        {
            // .NETランタイムへ返却するだけなので、このメモリアロケータは何もしません
        }
    }
    #endregion
}