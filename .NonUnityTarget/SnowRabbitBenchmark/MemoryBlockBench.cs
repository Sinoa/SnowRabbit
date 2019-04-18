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
using BenchmarkDotNet.Attributes;
using SnowRabbit.VirtualMachine.Runtime;

namespace SnowRabbit.Benchmark
{
    /// <summary>
    /// MemoryBlock 構造体のベンチマークを取るクラスです
    /// </summary>
    [MemoryDiagnoser]
    public class MemoryBlockBench
    {
        // メンバ変数定義
        private MemoryBlock<SrValue> srvalueMemoryBlock;
        private MemoryBlock<object> objectMemoryBlock;
        private SrValue[] memoryPool = new SrValue[1 << 20];
        private object[] objectPool = new object[1 << 20];
        private ulong[] rawPool = new ulong[1 << 20];



        /// <summary>
        /// MemoryBlockBench のインスタンスを初期化します
        /// </summary>
        public MemoryBlockBench()
        {
            // メモリブロックの生成をする
            srvalueMemoryBlock = new MemoryBlock<SrValue>(memoryPool, 0, memoryPool.Length);
            objectMemoryBlock = new MemoryBlock<object>(objectPool, 0, objectPool.Length);
        }


        /// <summary>
        /// メモリプールの全体からメモリブロックを確保する性能を測定します
        /// </summary>
        [Benchmark]
        public void AllMemoryAllocBlock()
        {
            // プール全体を確保するメモリブロックを生成
            var memoryBlock = new MemoryBlock<SrValue>(memoryPool, 0, memoryPool.Length);
        }


        #region All memory write
        /// <summary>
        /// 単純な符号なし64bit整数配列への書き込み性能を測定します
        /// </summary>
        [Benchmark]
        public void AllMemoryWriteRaw()
        {
            // 配列全体を回る
            for (int i = 0; i < rawPool.Length; ++i)
            {
                // 要素に値を入れる
                rawPool[i] = (ulong)i;
            }
        }


        /// <summary>
        /// メモリブロックではなく直接メモリプールへの書き込み性能を測定します
        /// </summary>
        [Benchmark]
        public unsafe void AllMemoryWritePool()
        {
            // メモリプールの全体を回る
            for (int i = 0; i < memoryPool.Length; ++i)
            {
                // 要素に値を入れる
                memoryPool[i].Value.Ulong[0] = (ulong)i;
            }
        }


        /// <summary>
        /// メモリブロックに8MB分（正確には 8 byte * 1073741824 entry）の書き込み性能を測定します
        /// </summary>
        [Benchmark]
        public unsafe void AllMemoryWriteBlock()
        {
            // メモリブロック全体をループする
            var length = srvalueMemoryBlock.Length;
            for (int i = 0; i < length; ++i)
            {
                // 要素に値を入れる
                srvalueMemoryBlock[i].Value.Ulong[0] = (ulong)i;
            }
        }


        /// <summary>
        /// 直接オブジェクト配列への書き込み性能を測定します
        /// </summary>
        [Benchmark]
        public void AllObjectWritePool()
        {
            // メモリプールの全体を回る
            for (int i = 0; i < objectPool.Length; ++i)
            {
                // 要素に値を入れる
                objectPool[i] = i;
            }
        }


        /// <summary>
        /// メモリブロック経由のオブジェクトの書き込み性能を測定します
        /// </summary>
        [Benchmark]
        public void AllObjectWriteBlock()
        {
            // メモリブロック全体をループする
            var length = objectMemoryBlock.Length;
            for (int i = 0; i < length; ++i)
            {
                // 要素に値を入れる
                objectMemoryBlock[i] = (ulong)i;
            }
        }


        /// <summary>
        /// Span構造体による書き込み性能を測定します
        /// </summary>
        [Benchmark]
        public unsafe void AllMemoryWriteSpan()
        {
            // Spanのインスタンスを生成してループする
            var memorySpan = new Span<SrValue>(memoryPool, 0, memoryPool.Length);
            var length = memorySpan.Length;
            for (int i = 0; i < length; ++i)
            {
                // 要素に値を入れる
                memorySpan[i].Value.Ulong[0] = (ulong)i;
            }
        }
        #endregion


        #region All memory Read
        /// <summary>
        /// 生配列の読み込み性能を測定します
        /// </summary>
        [Benchmark]
        public void AllMemoryReadRaw()
        {
            // 全体を回る
            for (int i = 0; i < rawPool.Length; ++i)
            {
                // 読み取ってそのまま捨てる
                var result = rawPool[i];
            }
        }


        /// <summary>
        /// メモリプールの読み込み性能を測定します
        /// </summary>
        [Benchmark]
        public unsafe void AllMemoryReadPool()
        {
            // 全体を回る
            for (int i = 0; i < memoryPool.Length; ++i)
            {
                // 読み取ってそのまま捨てる
                var result = memoryPool[i].Value.Ulong[0];
            }
        }


        /// <summary>
        /// メモリブロックの読み込み性能を測定します
        /// </summary>
        [Benchmark]
        public unsafe void AllMemoryReadBlock()
        {
            // 全体を回る
            for (int i = 0; i < srvalueMemoryBlock.Length; ++i)
            {
                // 読み取ってそのまま捨てる
                var result = srvalueMemoryBlock[i].Value.Ulong[0];
            }
        }


        /// <summary>
        /// オブジェクトプールの読み込み性能を測定します
        /// </summary>
        [Benchmark]
        public void AllObjectReadPool()
        {
            // 全体を回る
            for (int i = 0; i < objectPool.Length; ++i)
            {
                // 読み取ってそのまま捨てる
                var result = objectPool[i];
            }
        }


        /// <summary>
        /// メモリブロックからのオブジェクト読み込み性能を測定します
        /// </summary>
        [Benchmark]
        public void AllObjectReadBlock()
        {
            // 全体を回る
            for (int i = 0; i < objectMemoryBlock.Length; ++i)
            {
                // 読み取ってそのまま捨てる
                var result = objectMemoryBlock[i];
            }
        }


        /// <summary>
        /// Span構造体の読み込み性能を測定します
        /// </summary>
        [Benchmark]
        public unsafe void AllMemoryReadSpan()
        {
            // 全体を回る
            var memorySpan = new Span<SrValue>(memoryPool, 0, memoryPool.Length);
            for (int i = 0; i < memorySpan.Length; ++i)
            {
                // 読み取ってそのまま捨てる
                var result = memorySpan[i].Value.Ulong[0];
            }
        }
        #endregion
    }
}