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
using static SnowRabbit.VirtualMachine.Runtime.MemoryAllocatorUtility;

namespace SnowRabbit.VirtualMachine.Runtime
{
    #region Utility
    /// <summary>
    /// メモリアロケータの実装で使用する機能をユーティリティ化したクラスです
    /// </summary>
    public static class MemoryAllocatorUtility
    {
        /// <summary>
        /// バイトサイズから配列の要素数を求めます
        /// </summary>
        /// <param name="size">計算するバイト数</param>
        /// <returns>バイト数から要素数に変換した値を返します</returns>
        public static int ByteSizeToElementCount(int size)
        {
            // パディング込みの計算式例（ビット演算ではなく汎用計算式の場合）
            // paddingSize = (blockSize - (dataSize % blockSize)) % blockSize
            // finalSize = dataSize + paddingSize
            // blockCount = finalSize / blockSize
            // サイズから確保するべき配列の要素数を求める（1ブロック8byteなので8byte倍数に収まるようにしてから求める）
            return (size + ((8 - (size & 7)) & 7)) >> 3;
        }


        /// <summary>
        /// 配列の要素数からバイトサイズを求めます
        /// </summary>
        /// <param name="count">計算する要素数</param>
        /// <returns>要素数からバイト数に変換した値を返します</returns>
        public static int ElementCountToByteSize(int count)
        {
            // ブロックサイズは8byteなのでそのまま8倍にして返す
            return count << 3;
        }
    }
    #endregion



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



    #region Standard MemoryAllocator
    /// <summary>
    /// SnowRabbit 仮想マシンが使用する標準メモリアロケータクラスです
    /// </summary>
    public class StandardMemoryAllocator : MemoryAllocator
    {
        // メンバ変数定義
        private SrValue[] memoryPool;
        private int poolElementSize;
        private int freePageStartIndex;



        #region Constructor and initializer
        /// <summary>
        /// メモリプールの容量を指定して StandardMemoryAllocator のインスタンスを初期化します
        /// </summary>
        /// <param name="memoryPoolSize"></param>
        public StandardMemoryAllocator(int memoryPoolSize)
        {
            // 事前の例外ハンドリングをする
            ThrowIfInvalidPoolSize(memoryPoolSize);


            // 要求サイズから確保するべき配列の要素数を求める
            var allocateSize = memoryPoolSize + ((8 - (memoryPoolSize & 7)) & 7);
            var allocateBlock = allocateSize >> 3;


            // 必要要素分配列を確保して初期化する
            Initialize(new SrValue[allocateBlock]);
        }


        /// <summary>
        /// メモリプールを直接渡して StandardMemoryAllocator のインスタンスを初期化します
        /// </summary>
        /// <param name="memoryPool">このアロケータに使ってもらうメモリプール</param>
        public StandardMemoryAllocator(SrValue[] memoryPool)
        {
            // 渡された引数を使って、そのまま初期化処理を呼ぶ
            Initialize(memoryPool);
        }


        /// <summary>
        /// 指定されたメモリプールのバッファでインスタンスの初期化を行います
        /// </summary>
        /// <param name="memoryPool">このアロケータが使用するメモリプール</param>
        private void Initialize(SrValue[] memoryPool)
        {
            // 事前例外ハンドリングをしてプールを覚える
            ThrowIfInvalidMemoryPool(memoryPool);
            this.memoryPool = memoryPool;
        }
        #endregion


        #region Allocation function
        /// <summary>
        /// メモリの確保をします。確保されるメモリのサイズは size が収まる様に確保されますが、実際の確保サイズは必ず一致しているかどうかを保証していません。
        /// </summary>
        /// <param name="size">確保するメモリのサイズ</param>
        /// <param name="type">確保するメモリの利用タイプ</param>
        /// <returns>確保したメモリのブロックを返します</returns>
        /// <exception cref="ArgumentOutOfRangeException">確保するサイズに 0 以下の指定は出来ません</exception>
        /// <exception cref="SrOutOfMemoryException">指定されたサイズのメモリを確保できませんでした</exception>
        public override MemoryBlock Allocate(int size, AllocationType type)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 指定されたメモリブロックを解放します。
        /// </summary>
        /// <param name="memoryBlock">解放するメモリブロック</param>
        public override void Deallocate(MemoryBlock memoryBlock)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region Exception builder function
        /// <summary>
        /// プールサイズが最低限確保するべきサイズを指定されなかった場合例外をスローします
        /// </summary>
        /// <param name="poolSize">確保するプールサイズ</param>
        protected void ThrowIfInvalidPoolSize(int poolSize)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// メモリプールの配列参照が null だった場合または容量が少ない場合に例外をスローします
        /// </summary>
        /// <param name="memoryPool">確認する配列</param>
        /// <exception cref="ArgumentNullException">memoryPool が null です</exception>
        protected void ThrowIfInvalidMemoryPool(SrValue[] memoryPool)
        {
            // null の場合は
            if (memoryPool == null)
            {
                // 例外を吐く
                throw new ArgumentNullException(nameof(memoryPool));
            }
        }
        #endregion
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
            // 事前の例外ハンドリングをしてから、サイズを計算後メモリブロックを生成して返す
            ThrowExceptionIfRequestInvalidSize(size);
            var allocateBlock = ByteSizeToElementCount(size);
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