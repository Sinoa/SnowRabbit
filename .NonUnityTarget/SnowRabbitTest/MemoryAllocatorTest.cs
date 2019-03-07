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
using NUnit.Framework;
using SnowRabbit.VirtualMachine.Runtime;

namespace SnowRabbit.Test
{
    /// <summary>
    /// MemoryAllocator クラスに対するテストクラスです
    /// </summary>
    [TestFixture]
    public class MemoryAllocatorTest
    {
        /// <summary>
        /// DynamicManagedMemoryAllocator のメモリ確保テストを行います
        /// </summary>
        /// <param name="size">確保しようとしているメモリサイズ</param>
        /// <param name="expectedLength">確保されたはずの配列の長さ</param>
        /// <param name="exceptionType">例外が発生する場合の例外タイプ、発生しない場合は null</param>
        [TestCase(-1, 0, typeof(ArgumentOutOfRangeException))]
        [TestCase(0, 0, typeof(ArgumentOutOfRangeException))]
        [TestCase(1, 1, null)]
        [TestCase(8, 1, null)]
        [TestCase(9, 2, null)]
        [TestCase(16, 2, null)]
        [TestCase(100, 13, null)]
        public void DynamicAllocateTest(int size, int expectedLength, Type exceptionType)
        {
            // インスタンスを生成する
            var allocator = new DynamicManagedMemoryAllocator();
            var memoryBlock = default(MemoryBlock);


            // 例外タイプの指定がないなら
            if (exceptionType == null)
            {
                // メモリの確保は成功するテストを実行
                Assert.DoesNotThrow(() => memoryBlock = allocator.Allocate(size, AllocationType.General));
            }
            else
            {
                // メモリの確保時に例外が発生するテストを実行して終了する（そもそも確保に失敗したら何も出来ない）
                Assert.Throws(exceptionType, () => allocator.Allocate(size, AllocationType.General));
                return;
            }


            // ダイナミックメモリ確保の場合は 8の倍数になって確保されているので要求サイズの8の倍数になっていることを確認する
            Assert.AreEqual(expectedLength, memoryBlock.Length);
        }
    }
}