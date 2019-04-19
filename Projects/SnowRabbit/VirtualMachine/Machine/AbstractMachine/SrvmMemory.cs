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

using SnowRabbit.VirtualMachine.Runtime;

namespace SnowRabbit.VirtualMachine.Machine
{
    /// <summary>
    /// 仮想マシンが実装する仮想マシンメモリの抽象クラスです
    /// </summary>
    public abstract class SrvmMemory : SrvmMachineParts
    {
        /// <summary>
        /// 値型メモリを確保します
        /// </summary>
        /// <param name="size">確保するサイズ</param>
        /// <param name="type">確保するメモリの種類</param>
        /// <returns>確保された値型メモリのメモリブロックを返します</returns>
        public abstract MemoryBlock<SrValue> AllocateValue(int size, AllocationType type);


        /// <summary>
        /// 参照型メモリを確保します
        /// </summary>
        /// <param name="count">確保する参照数</param>
        /// <param name="type">確保するメモリの種類</param>
        /// <returns>確保された参照型メモリのメモリブロックを返します</returns>
        public abstract MemoryBlock<SrObject> AllocateObject(int count, AllocationType type);


        /// <summary>
        /// 値型メモリブロックを解放します
        /// </summary>
        /// <param name="memoryBlock">解放する値型メモリブロック</param>
        public abstract void DeallocateValue(MemoryBlock<SrValue> memoryBlock);


        /// <summary>
        /// 参照型メモリブロックを解放します
        /// </summary>
        /// <param name="memoryBlock">解放する参照型メモリブロック</param>
        public abstract void DeallocateObject(MemoryBlock<SrObject> memoryBlock);
    }
}