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
using System.Collections.Generic;
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
        /// MemoryAllocatorUtility クラスのサイズ計算ユーティリティ関数のテストを行います
        /// </summary>
        [Test]
        public void SizeUtilityTest()
        {
            // サイズ計算ユーティリティを叩いて想定した結果が返ってくればそれで良い
            // （負の値も受け入れるが原則想定された使われ方ではない上結果を保証しないので無視する）
            // Size => Element
            Assert.AreEqual(0, MemoryAllocatorUtility.ByteSizeToElementCount(0));
            Assert.AreEqual(1, MemoryAllocatorUtility.ByteSizeToElementCount(1));
            Assert.AreEqual(1, MemoryAllocatorUtility.ByteSizeToElementCount(8));
            Assert.AreEqual(2, MemoryAllocatorUtility.ByteSizeToElementCount(10));
            Assert.AreEqual(2, MemoryAllocatorUtility.ByteSizeToElementCount(16));
            Assert.AreEqual(13, MemoryAllocatorUtility.ByteSizeToElementCount(100));
            // Element => Size
            Assert.AreEqual(0, MemoryAllocatorUtility.ElementCountToByteSize(0));
            Assert.AreEqual(8, MemoryAllocatorUtility.ElementCountToByteSize(1));
            Assert.AreEqual(16, MemoryAllocatorUtility.ElementCountToByteSize(2));
            Assert.AreEqual(24, MemoryAllocatorUtility.ElementCountToByteSize(3));
            Assert.AreEqual(32, MemoryAllocatorUtility.ElementCountToByteSize(4));
            Assert.AreEqual(80, MemoryAllocatorUtility.ElementCountToByteSize(10));
        }


        /// <summary>
        /// 各種メモリアロケータ に Free タイプのメモリ確保をした場合のテストを行います
        /// </summary>
        [Test]
        public void FreeTypeAllocateTest()
        {
            // 全アロケータの生成関数リスト
            var allocatorCreateFunctionList = new List<Func<IMemoryAllocator>>()
            {
                new Func<IMemoryAllocator>(() => new DynamicManagedMemoryAllocator()),
                new Func<IMemoryAllocator>(() => new StandardMemoryAllocator(128)),
                new Func<IMemoryAllocator>(() => new StandardMemoryAllocator(new SrValue[128])),
            };



            // 全アロケータ生成関数分回る
            foreach (var create in allocatorCreateFunctionList)
            {
                // アロケータを生成してFreeTypeのメモリ確保をした場合例外が発生することを確認する
                var allocator = create();
                Assert.Throws<ArgumentException>(() => allocator.Allocate(10, AllocationType.Free));
            }
        }


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


        /// <summary>
        /// StandardMemoryAllocator のメモリ確保テストを行います
        /// </summary>
        /// <param name="size">確保しようとしているメモリサイズ</param>
        /// <param name="expectedLength">確保されたはずの配列の長さ（このテストケースではsize未満にならなければ良い）</param>
        /// <param name="exceptionType">例外が発生する場合の例外タイプ、発生しない場合は null</param>
        [TestCase(-1, 0, typeof(ArgumentOutOfRangeException))]
        [TestCase(0, 0, typeof(ArgumentOutOfRangeException))]
        [TestCase(1, 1, null)]
        [TestCase(8, 1, null)]
        [TestCase(9, 2, null)]
        [TestCase(16, 2, null)]
        [TestCase(100, 13, null)]
        public void StandardAllocateTest(int size, int expectedLength, Type exceptionType)
        {
            // 2パターン分のインスタンスを生成する
            var allocatorA = new StandardMemoryAllocator(1024);
            var allocatorB = new StandardMemoryAllocator(new SrValue[1024]);
            var memoryBlock = default(MemoryBlock);


            // 例外タイプの指定が無いなら
            if (exceptionType == null)
            {
                // メモリの確保は成功するテストを実行
                Assert.DoesNotThrow(() => memoryBlock = allocatorA.Allocate(size, AllocationType.General));
                Assert.DoesNotThrow(() => memoryBlock = allocatorB.Allocate(size, AllocationType.General));
            }
            else
            {
                // メモリ確保時に例外が発生するテストを実行して終了する（そもそも確保に失敗したら何も出来ない）
                Assert.Throws(exceptionType, () => allocatorA.Allocate(size, AllocationType.General));
                Assert.Throws(exceptionType, () => allocatorB.Allocate(size, AllocationType.General));
                return;
            }


            // 通常、メモリアロケータは絶対的なサイズで確保することは無いので、希望サイズ以上の確保サイズならよしとする
            Assert.GreaterOrEqual(expectedLength, memoryBlock.Length);
        }
    }
}