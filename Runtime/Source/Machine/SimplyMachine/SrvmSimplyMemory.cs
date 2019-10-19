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

using SnowRabbit.Runtime;

namespace SnowRabbit.Machine
{
    /// <summary>
    /// 非常に単純な構成で実装された仮想マシンメモリクラスです
    /// </summary>
    public class SrvmSimplyMemory : SrvmMemory
    {
        // メンバ変数定義
        private StandardSrObjectAllocator objectAllocator;
        private StandardSrValueAllocator memoryAllocator;



        /// <summary>
        /// SrvmSimplyMemory クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="valueReservedSize">値型メモリの予約サイズ</param>
        /// <param name="objectReservedSize">オブジェクト型メモリの予約サイズ</param>
        public SrvmSimplyMemory(int valueReservedSize, int objectReservedSize)
        {
            // メモリの確保
            objectAllocator = new StandardSrObjectAllocator(objectReservedSize);
            memoryAllocator = new StandardSrValueAllocator(valueReservedSize);
        }


        /// <summary>
        /// オブジェクトメモリの確保をします
        /// </summary>
        /// <param name="count">確保するオブジェクト数</param>
        /// <param name="type">確保するオブジェクトの種類</param>
        /// <returns>確保したオブジェクトメモリを返します</returns>
        public override MemoryBlock<SrObject> AllocateObject(int count, AllocationType type)
        {
            // オブジェクトメモリアロケータからメモリを確保する
            return objectAllocator.Allocate(count, type);
        }


        /// <summary>
        /// メモリの確保をします
        /// </summary>
        /// <param name="size">確保するメモリサイズ</param>
        /// <param name="type">確保するメモリの種類</param>
        /// <returns>確保したメモリを返します</returns>
        public override MemoryBlock<SrValue> AllocateValue(int size, AllocationType type)
        {
            // メモリアロケータからメモリを確保する
            return memoryAllocator.Allocate(size, type);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="memoryBlock"></param>
        public override void DeallocateObject(MemoryBlock<SrObject> memoryBlock)
        {
            // オブジェクトメモリアロケータにメモリの解放を依頼する
            objectAllocator.Deallocate(memoryBlock);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="memoryBlock"></param>
        public override void DeallocateValue(MemoryBlock<SrValue> memoryBlock)
        {
            // メモリアロケータにメモリの解放を依頼する
            memoryAllocator.Deallocate(memoryBlock);
        }
    }
}