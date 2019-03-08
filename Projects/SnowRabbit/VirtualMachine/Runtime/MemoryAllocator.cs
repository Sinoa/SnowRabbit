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
        // 定数定義
        public const int RequirePoolElementCount = 4;
        public const int RequirePoolSize = 8 * RequirePoolElementCount;

        // メンバ変数定義
        private SrValue[] memoryPool;
        private int freeElementIndex;



        #region Constructor and initializer
        /// <summary>
        /// メモリプールの容量を指定して StandardMemoryAllocator のインスタンスを初期化します
        /// </summary>
        /// <param name="memoryPoolSize"></param>
        /// <exception cref="ArgumentOutOfRangeException">poolSize が RequirePoolSize 未満です</exception>
        public StandardMemoryAllocator(int memoryPoolSize)
        {
            // 事前の例外ハンドリングをして、要求サイズから確保するサイズを求めて配列を生成後初期化する
            ThrowExceptionIfInvalidPoolSize(memoryPoolSize);
            Initialize(new SrValue[ByteSizeToElementCount(memoryPoolSize)]);
        }


        /// <summary>
        /// メモリプールを直接渡して StandardMemoryAllocator のインスタンスを初期化します
        /// </summary>
        /// <param name="memoryPool">このアロケータに使ってもらうメモリプール</param>
        /// <exception cref="ArgumentNullException">memoryPool が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">memoryPool の長さが RequirePoolElementCount 未満です</exception>
        public StandardMemoryAllocator(SrValue[] memoryPool)
        {
            // 渡された引数を使って、そのまま初期化処理を呼ぶ
            Initialize(memoryPool);
        }


        /// <summary>
        /// 指定されたメモリプールのバッファでインスタンスの初期化を行います
        /// </summary>
        /// <param name="memoryPool">このアロケータが使用するメモリプール</param>
        /// <exception cref="ArgumentNullException">memoryPool が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">memoryPool の長さが RequirePoolElementCount 未満です</exception>
        private unsafe void Initialize(SrValue[] memoryPool)
        {
            // 事前例外ハンドリングをする
            ThrowExceptionIfInvalidMemoryPool(memoryPool);


            // メモリプールの参照を覚えてインスタンス変数の初期化をする
            this.memoryPool = memoryPool;
            freeElementIndex = 0;


            // 最初のフリー領域の大きさを管理する
            SetAllocationInfo(freeElementIndex, -1, -1, memoryPool.Length - 2, AllocationType.Free);
        }
        #endregion


        #region AllocationInfo control function
        /// <summary>
        /// 指定したメモリプールのインデックスから、メモリ管理情報を設定します
        /// </summary>
        /// <param name="poolIndex">管理情報設定するメモリプールのインデックス位置</param>
        /// <param name="prevIndex">前のメモリブロック管理インデックス</param>
        /// <param name="nextIndex">次のメモリブロック管理インデックス</param>
        /// <param name="allocatedCount">該当メモリブロックの確保個数</param>
        /// <param name="type">該当メモリブロックの確保タイプ</param>
        private unsafe void SetAllocationInfo(int poolIndex, int prevIndex, int nextIndex, int allocatedCount, AllocationType type)
        {
            // 指定されたインデックスの位置は、前後のメモリブロックのインデックスを指す（双方向リストのインデックス）
            // 指定されたインデックスの次の位置は、該当管理メモリブロックの確保サイズと確保タイプを保持する
            // 指定されたインデックスから確保済みブロックの次の位置は、管理データを含む確保した全体サイズを保持する
            memoryPool[poolIndex + 0].Value.Int[0] = prevIndex;
            memoryPool[poolIndex + 0].Value.Int[1] = nextIndex;
            memoryPool[poolIndex + 1].Value.Int[0] = allocatedCount;
            memoryPool[poolIndex + 1].Value.Int[1] = (int)type;
            memoryPool[poolIndex + 2 + allocatedCount].Value.Int[0] = allocatedCount + 3;
        }


        /// <summary>
        /// 指定したメモリプールのインデックスから、メモリ管理情報を取得します
        /// </summary>
        /// <param name="poolIndex">管理情報設定するメモリプールのインデックス位置</param>
        /// <param name="prevIndex">前のメモリブロック管理インデックス</param>
        /// <param name="nextIndex">次のメモリブロック管理インデックス</param>
        /// <param name="allocatedCount">該当メモリブロックの確保個数</param>
        /// <param name="type">該当メモリブロックの確保タイプ</param>
        private unsafe void GetAllocationInfo(int poolIndex, out int prevIndex, out int nextIndex, out int allocatedCount, out AllocationType type)
        {
            // 指定されたインデックスの位置は、前後のメモリブロックのインデックスを指す（双方向リストのインデックス）
            // 指定されたインデックスの次の位置は、該当管理メモリブロックの確保サイズと確保タイプを保持する
            prevIndex = memoryPool[poolIndex + 0].Value.Int[0];
            nextIndex = memoryPool[poolIndex + 0].Value.Int[1];
            allocatedCount = memoryPool[poolIndex + 1].Value.Int[0];
            type = (AllocationType)memoryPool[poolIndex + 1].Value.Int[1];
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
            // 事前例外処理をして必要な確保要素数を求める
            ThrowExceptionIfRequestInvalidSize(size);
            var allocationCount = ByteSizeToElementCount(size);


            // そもそも空き要素インデックスが負の値なら
            if (freeElementIndex < 0)
            {
                // この時点で空きが足りない例外を吐く
                ThrowExceptionSrOutOfMemory();
            }
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
        /// <exception cref="ArgumentOutOfRangeException">poolSize が RequirePoolSize 未満です</exception>
        protected void ThrowExceptionIfInvalidPoolSize(int poolSize)
        {
            // RequirePoolSize 未満なら
            if (poolSize < RequirePoolSize)
            {
                // 例外を吐く
                throw new ArgumentOutOfRangeException(nameof(poolSize), $"{nameof(poolSize)} が {nameof(RequirePoolSize)} 未満です");
            }
        }


        /// <summary>
        /// メモリプールの配列参照が null だった場合に例外をスローします
        /// </summary>
        /// <param name="memoryPool">確認する配列</param>
        /// <exception cref="ArgumentNullException">memoryPool が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">memoryPool の長さが RequirePoolElementCount 未満です</exception>
        protected void ThrowExceptionIfInvalidMemoryPool(SrValue[] memoryPool)
        {
            // null の場合は
            if (memoryPool == null)
            {
                // 例外を吐く
                throw new ArgumentNullException(nameof(memoryPool));
            }


            // 長さが RequirePoolElementCount 未満なら
            if (memoryPool.Length < RequirePoolElementCount)
            {
                // 例外を吐く
                throw new ArgumentOutOfRangeException($"{nameof(memoryPool)} の長さが {nameof(RequirePoolElementCount)} 未満です", nameof(memoryPool));
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