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

using BenchmarkDotNet.Attributes;
using SnowRabbit.VirtualMachine.Runtime;

namespace SnowRabbit.Benchmark
{
    /// <summary>
    /// メモリアロケータのベンチマークを取るクラスです
    /// </summary>
    [MemoryDiagnoser]
    public class MemoryAllocatorBench
    {
        // 定数定義
        private const int LoopCount = 1000000;
        private const int AllocSize = 100;

        // メンバ変数定義
        private IMemoryAllocator dynamicAllocator;
        private IMemoryAllocator standardAllocator;
        private MemoryBlock[] memoryBlocks;



        /// <summary>
        /// MemoryAllocatorBench のインスタンスを初期化します
        /// </summary>
        public MemoryAllocatorBench()
        {
            // メモリアロケータを生成する
            dynamicAllocator = new DynamicManagedMemoryAllocator();
            standardAllocator = new StandardMemoryAllocator(1 << 30);
            memoryBlocks = new MemoryBlock[LoopCount];
        }


        /// <summary>
        /// DynamicManagedMemoryAllocator の確保性能を計測します
        /// </summary>
        [Benchmark]
        public void AllocateDynamicMemory()
        {
            // 規定回数回る
            for (int i = 0; i < LoopCount; ++i)
            {
                // メモリ確保を行う
                dynamicAllocator.Allocate(AllocSize, AllocationType.General);
            }
        }


        /// <summary>
        /// StandardMemoryAllocator の確保性能を計測します
        /// </summary>
        [Benchmark]
        public void AllocateStandardMemory()
        {
            // 規定回数回る
            for (int i = 0; i < LoopCount; ++i)
            {
                // メモリ確保を行う
                memoryBlocks[i] = standardAllocator.Allocate(AllocSize, AllocationType.General);
            }
        }


        /// <summary>
        /// StandardMemoryAlloc によって確保したメモリを解放します
        /// </summary>
        [IterationCleanup(Target = nameof(AllocateStandardMemory))]
        public void CleanupStandardMemory()
        {
            // 規定回数回る
            for (int i = 0; i < LoopCount; ++i)
            {
                // メモリ解放を行う
                standardAllocator.Deallocate(memoryBlocks[i]);
            }
        }


        /// <summary>
        /// StandardMemoryAlloc によるメモリ確保をします
        /// </summary>
        [IterationSetup(Target = nameof(DeallocateStandardMemory))]
        public void SetupStandardMemory()
        {
            // 規定回数回る
            for (int i = 0; i < LoopCount; ++i)
            {
                // メモリ確保を行う
                memoryBlocks[i] = standardAllocator.Allocate(AllocSize, AllocationType.General);
            }
        }


        /// <summary>
        /// StandardMemoryAllocator の解放性能を計測します
        /// </summary>
        [Benchmark]
        public void DeallocateStandardMemory()
        {
            // 規定回数回る
            for (int i = 0; i < LoopCount; ++i)
            {
                // メモリ解放を行う
                standardAllocator.Deallocate(memoryBlocks[i]);
            }
        }
    }
}