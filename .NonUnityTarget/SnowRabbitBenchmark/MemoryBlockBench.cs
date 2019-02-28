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
        private MemoryBlock memoryBlock;
        private SrValue[] memoryPool = new SrValue[1 << 20];
        private ulong[] rawPool = new ulong[1 << 20];



        /// <summary>
        /// MemoryBlockBench のインスタンスを初期化します
        /// </summary>
        public MemoryBlockBench()
        {
            // メモリブロックの生成をする
            memoryBlock = new MemoryBlock(memoryPool, 0, memoryPool.Length);
        }


        /// <summary>
        /// メモリプールの全体からメモリブロックを確保する性能を測定します
        /// </summary>
        [Benchmark]
        public void AllMemoryAllocBlock()
        {
            // プール全体を確保するメモリブロックを生成
            var memoryBlock = new MemoryBlock(memoryPool, 0, memoryPool.Length);
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
            var length = memoryBlock.Length;
            for (int i = 0; i < length; ++i)
            {
                // 要素に値を入れる
                memoryBlock[i].Value.Ulong[0] = (ulong)i;
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


        #region Unit memory write
        /// <summary>
        /// 単純な整数配列の単体要素への書き込み性能を測定します
        /// </summary>
        [Benchmark]
        public void UnitMemoryWriteRaw()
        {
            // 単体要素に何かを書く
            rawPool[100] = 12345UL;
        }


        /// <summary>
        /// メモリプールの単体要素への書き込み性能を測定します
        /// </summary>
        [Benchmark]
        public unsafe void UnitMemoryWritePool()
        {
            // 単体要素に何かを書く
            memoryPool[100].Value.Ulong[0] = 12345UL;
        }


        /// <summary>
        /// メモリブロックの単体要素への書き込み性能を測定します
        /// </summary>
        [Benchmark]
        public unsafe void UnitMemoryWriteBlock()
        {
            // 単体要素に何かを書く
            memoryBlock[100].Value.Ulong[0] = 12345UL;
        }


        /// <summary>
        /// メモリSpan構造体の単体要素への書き込み性能を測定します
        /// </summary>
        [Benchmark]
        public unsafe void UnitMemoryWriteSpan()
        {
            // 単体要素に何かを書く
            var memorySpan = new Span<SrValue>(memoryPool, 0, memoryPool.Length);
            memorySpan[100].Value.Ulong[0] = 12345UL;
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
            for (int i = 0; i < memoryBlock.Length; ++i)
            {
                // 読み取ってそのまま捨てる
                var result = memoryBlock[i].Value.Ulong[0];
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


        #region Unit memory read
        /// <summary>
        /// 単純な整数配列の単体要素への読み込み性能を測定します
        /// </summary>
        [Benchmark]
        public void UnitMemoryReadRaw()
        {
            // 適当に読み取って捨てる
            var result = rawPool[100];
        }


        /// <summary>
        /// メモリプールの単体要素への読み込み性能を測定します
        /// </summary>
        [Benchmark]
        public unsafe void UnitMemoryReadPool()
        {
            // 適当に読み取って捨てる
            var result = memoryPool[100].Value.Ulong[0];
        }


        /// <summary>
        /// メモリブロックの単体要素への読み込み性能を測定します
        /// </summary>
        [Benchmark]
        public unsafe void UnitMemoryReadBlock()
        {
            // 適当に読み取って捨てる
            var result = memoryBlock[100].Value.Ulong[0];
        }


        /// <summary>
        /// メモリSpan構造体の単体要素への読み込み性能を測定します
        /// </summary>
        [Benchmark]
        public unsafe void UnitMemoryReadSpan()
        {
            // 適当に読み取って捨てる
            var memorySpan = new Span<SrValue>(memoryPool, 0, memoryPool.Length);
            var result = memorySpan[100].Value.Ulong[0];
        }
        #endregion
    }
}