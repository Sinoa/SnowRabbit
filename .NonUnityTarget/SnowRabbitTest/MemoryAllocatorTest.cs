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
        /// MemoryAllocatorUtility クラスのサイズ計算ユーティリティ関数 ByteSizeToElementCount(int) のテストを行います
        /// </summary>
        /// <param name="expected">想定される要素数</param>
        /// <param name="byteSize">要求するバイト数</param>
        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(1, 8)]
        [TestCase(2, 10)]
        [TestCase(2, 16)]
        [TestCase(13, 100)]
        public void ByteSizeToElementCountTest(int expected, int byteSize)
        {
            // サイズ計算ユーティリティを叩いて想定した結果が返ってくればそれで良い
            Assert.AreEqual(expected, MemoryAllocatorUtility.ByteSizeToElementCount(byteSize));
        }


        /// <summary>
        /// MemoryAllocatorUtility クラスのサイズ計算ユーティリティ関数 ElementCountToByteSize(int) のテストを行います
        /// </summary>
        /// <param name="expected">想定されるバイト数</param>
        /// <param name="elementCount">要求する要素数</param>
        [TestCase(0, 0)]
        [TestCase(8, 1)]
        [TestCase(16, 2)]
        [TestCase(24, 3)]
        [TestCase(32, 4)]
        [TestCase(80, 10)]
        public void ElementCountToByteSizeTest(int expected, int elementCount)
        {
            // サイズ計算ユーティリティを叩いて想定した結果が返ってくればそれで良い
            Assert.AreEqual(expected, MemoryAllocatorUtility.ElementCountToByteSize(elementCount));
        }


        /// <summary>
        /// MemoryAllocatorUtility クラスのサイズ計算ユーティリティ関数 ByteSizeToElementCount(int, int) のテストを行います
        /// </summary>
        /// <param name="expected">想定される要素数</param>
        /// <param name="byteSize">要求するバイト数</param>
        /// <param name="blockSize">要求するメモリのブロックサイズ</param>
        [TestCase(0, 0, 10)]
        [TestCase(1, 1, 10)]
        [TestCase(1, 10, 10)]
        [TestCase(1, 20, 20)]
        [TestCase(2, 21, 20)]
        [TestCase(10, 100, 10)]
        public void ByteSizeToElementCountFlexibleTest(int expected, int byteSize, int blockSize)
        {
            // サイズ計算ユーティリティを叩いて想定した結果が返ってくればそれで良い
            Assert.AreEqual(expected, MemoryAllocatorUtility.ByteSizeToElementCount(byteSize, blockSize));
        }


        /// <summary>
        /// MemoryAllocatorUtility クラスのサイズ計算ユーティリティ関数 ElementCountToByteSize(int, int) のテストを行います
        /// </summary>
        /// <param name="expected">想定されるバイト数</param>
        /// <param name="elementCount">要求する要素数</param>
        /// <param name="blockSize">要求するメモリのブロックサイズ</param>
        [TestCase(0, 0, 20)]
        [TestCase(20, 1, 20)]
        [TestCase(40, 2, 20)]
        [TestCase(60, 3, 20)]
        [TestCase(200, 4, 50)]
        [TestCase(200, 10, 20)]
        public void ElementCountToByteSizeFlexibleTest(int expected, int elementCount, int blockSize)
        {
            // サイズ計算ユーティリティを叩いて想定した結果が返ってくればそれで良い
            Assert.AreEqual(expected, MemoryAllocatorUtility.ElementCountToByteSize(elementCount, blockSize));
        }


        /// <summary>
        /// 各種メモリアロケータ に Free タイプのメモリ確保をした場合のテストを行います
        /// </summary>
        [Test]
        public void FreeTypeAllocateTest()
        {
            // 全アロケータの生成関数リスト
            var allocatorCreateFunctionList = new List<Func<IMemoryAllocator<SrValue>>>()
            {
                new Func<IMemoryAllocator<SrValue>>(() => new DynamicManagedMemoryAllocator<SrValue>()),
                new Func<IMemoryAllocator<SrValue>>(() => new StandardSrValueAllocator(128)),
                new Func<IMemoryAllocator<SrValue>>(() => new StandardSrValueAllocator(new SrValue[128])),
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
            var allocator = new DynamicManagedMemoryAllocator<SrValue>();
            var memoryBlock = default(MemoryBlock<SrValue>);


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
            var allocatorA = new StandardSrValueAllocator(1024);
            var allocatorB = new StandardSrValueAllocator(new SrValue[1024]);
            var memoryBlock = default(MemoryBlock<SrValue>);


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


        /// <summary>
        /// StandardMemoryAllocator のメモリ管理情報制御が正しく動作するかテストを行います
        /// </summary>
        [Test]
        public unsafe void StandardAllocatorAllocateInfoTest()
        {
            // 元実装には以下の内容が定義されている想定
            // 指定されたインデックスの位置には [0]確保した要素の数(管理情報を含まない数) [1]メモリ管理情報(管理タイプ) が設定される
            // さらに、メモリ確保分サイズの末尾には [0]確保した要素の数(管理情報を含む全体の数) [1]次のフリーリストインデックス が設定される


            // メモリアロケータのアロケーション状況を生で覗くためのメモリプールを生成して、アロケータインスタンスを生成する
            var poolSize = 100;
            var memoryPool = new SrValue[poolSize];
            var allocator = new StandardSrValueAllocator(memoryPool);


            // 100要素分のプールを渡したので空きサイズが98（管理領域2要素分を引いた）であることと末尾のリンク情報が正しいか確認する
            Assert.AreEqual(poolSize - StandardSrValueAllocator.RequireMemoryInfoCount, memoryPool[0].Value.Int[0]);
            Assert.AreEqual((int)AllocationType.Free, memoryPool[0].Value.Int[1]);
            Assert.AreEqual(100, memoryPool[99].Value.Int[0]);
            Assert.AreEqual(-1, memoryPool[99].Value.Int[1]);


            // 80要素分のメモリ確保を行い、確保済み領域と空き領域の管理情報を確認をする
            var allocCount = 80;
            var allocSize = MemoryAllocatorUtility.ElementCountToByteSize(allocCount);
            var memoryBlockA = allocator.Allocate(allocSize, AllocationType.General);

            // test memoryblock info.
            Assert.AreEqual(StandardSrValueAllocator.HeadMemoryInfoCount, memoryBlockA.Offset);
            Assert.AreEqual(allocCount, memoryBlockA.Length);

            // test allocated info.
            Assert.AreEqual(allocCount, memoryPool[0].Value.Int[0]);
            Assert.AreEqual((int)AllocationType.General, memoryPool[0].Value.Int[1]);
            Assert.AreEqual(82, memoryPool[StandardSrValueAllocator.HeadMemoryInfoCount + allocCount].Value.Int[0]);
            Assert.AreEqual(-1, memoryPool[StandardSrValueAllocator.HeadMemoryInfoCount + allocCount].Value.Int[1]);

            // test new free area info.
            var availableFreeCount = poolSize - StandardSrValueAllocator.RequireMemoryInfoCount * 2 - allocCount;
            var newerHeadFreeIndex = StandardSrValueAllocator.RequireMemoryInfoCount + allocCount;
            Assert.AreEqual(availableFreeCount, memoryPool[newerHeadFreeIndex].Value.Int[0]);
            Assert.AreEqual((int)AllocationType.Free, memoryPool[newerHeadFreeIndex].Value.Int[1]);
            Assert.AreEqual(18, memoryPool[poolSize - 1].Value.Int[0]);
            Assert.AreEqual(-1, memoryPool[poolSize - 1].Value.Int[1]);


            // この段階で16要素数超過のメモリ確保は失敗するはず（プールサイズ - 前回のアロケーションサイズ(管理情報を含む) = 18(管理サイズを含まない残りの空きサイズ)）
            Assert.Throws<SrOutOfMemoryException>(() => allocator.Allocate(MemoryAllocatorUtility.ElementCountToByteSize(17), AllocationType.General));


            // 更に16要素分の確保をしようとして成功すればピッタリメモリを使用したことになる（1バイトでも確保しようとするとOutOfMemoryになるはず）
            var memoryBlockB = default(MemoryBlock<SrValue>);
            Assert.DoesNotThrow(() => memoryBlockB = allocator.Allocate(MemoryAllocatorUtility.ElementCountToByteSize(16), AllocationType.General));
            Assert.Throws<SrOutOfMemoryException>(() => allocator.Allocate(1, AllocationType.General));


            // 80要素分のメモリ解放を行う
            allocator.Deallocate(memoryBlockA);


            // 空き領域として生成されたか確認をする
            Assert.AreEqual(80, memoryPool[0].Value.Int[0]);
            Assert.AreEqual((int)AllocationType.Free, memoryPool[0].Value.Int[1]);


            // 解放された80要素分を2つに分けると最大38要素2つ分の確保が出来るはず（管理サイズを含むため）、その後1バイトも確保できない
            var memoryBlockC = default(MemoryBlock<SrValue>);
            var memoryBlockD = default(MemoryBlock<SrValue>);
            Assert.DoesNotThrow(() => memoryBlockC = allocator.Allocate(MemoryAllocatorUtility.ElementCountToByteSize(38), AllocationType.General));
            Assert.DoesNotThrow(() => memoryBlockD = allocator.Allocate(MemoryAllocatorUtility.ElementCountToByteSize(38), AllocationType.General));
            Assert.Throws<SrOutOfMemoryException>(() => allocator.Allocate(1, AllocationType.General));
        }
    }
}