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
    /// MemoryBlock クラスに対するテストクラスです
    /// </summary>
    [TestFixture]
    public class MemoryBlockTest
    {
        /// <summary>
        /// コンストラクタのテストを行います
        /// </summary>
        /// <param name="arraySize">用意するメモリプール配列の長さ。-1でnullになります。</param>
        /// <param name="start">メモリブロックコンストラクタのstartパラメータ</param>
        /// <param name="length">メモリブロックコンストラクタのlengthパラメータ</param>
        /// <param name="exceptionType">例外が発生する場合の例外の型、例外が発生しない場合はnull</param>
        [TestCase(-1, 0, 0, typeof(ArgumentNullException))]
        [TestCase(0, 0, 1, typeof(ArgumentOutOfRangeException))]
        [TestCase(100, -1, 0, typeof(ArgumentOutOfRangeException))]
        [TestCase(100, 0, -1, typeof(ArgumentOutOfRangeException))]
        [TestCase(100, 0, 101, typeof(ArgumentOutOfRangeException))]
        [TestCase(100, 100, 1, typeof(ArgumentOutOfRangeException))]
        [TestCase(0, 0, 0, null)]
        [TestCase(100, 100, 0, null)]
        [TestCase(100, 0, 100, null)]
        [TestCase(100, 0, 0, null)]
        [TestCase(100, 49, 50, null)]
        public void ConstructorTest(int arraySize, int start, int length, Type exceptionType)
        {
            // 0以上の時だけ配列を生成し、それ以外は null で初期化をする
            SrValue[] values = arraySize < 0 ? null : new SrValue[arraySize];


            // 例外タイプが指定されていないなら
            if (exceptionType == null)
            {
                // 受け取ったパラメータで例外は発生しないテストをする
                Assert.DoesNotThrow(() => new MemoryBlock<SrValue>(values, start, length));
            }
            else
            {
                // 受け取った型の例外が発生するテストをする
                Assert.Throws(exceptionType, () => new MemoryBlock<SrValue>(values, start, length));
            }
        }


        /// <summary>
        /// インデクサのインデックスアクセスのテストを行います
        /// </summary>
        /// <param name="arraySize">用意するメモリプール配列の長さ。-1でnullになります。</param>
        /// <param name="start">メモリブロックコンストラクタのstartパラメータ</param>
        /// <param name="length">メモリブロックコンストラクタのlengthパラメータ</param>
        /// <param name="accessIndex">インデクサに指定するインデックス</param>
        /// <param name="exceptionType">例外が発生する場合の例外の型、例外が発生しない場合はnull</param>
#if DEBUG
        [TestCase(10, 0, 5, 5, typeof(ArgumentOutOfRangeException))]
        [TestCase(10, 0, 5, 6, typeof(ArgumentOutOfRangeException))]
        [TestCase(10, 0, 5, -1, typeof(ArgumentOutOfRangeException))]
        [TestCase(10, 4, 5, 5, typeof(ArgumentOutOfRangeException))]
        [TestCase(10, 4, 5, 6, typeof(ArgumentOutOfRangeException))]
        [TestCase(10, 4, 5, -1, typeof(ArgumentOutOfRangeException))]
#else
        [TestCase(10, 0, 5, 5, null)] // But memory break.
        [TestCase(10, 0, 5, 6, null)] // But memory break.
        [TestCase(10, 0, 5, -1, typeof(IndexOutOfRangeException))]
        [TestCase(10, 4, 5, 5, null)] // But memory break.
        [TestCase(10, 4, 5, 6, typeof(IndexOutOfRangeException))]
        [TestCase(10, 4, 5, -1, null)] // But memory break.
#endif
        [TestCase(10, 0, 5, 0, null)]
        [TestCase(10, 0, 5, 4, null)]
        [TestCase(10, 4, 5, 0, null)]
        [TestCase(10, 4, 5, 4, null)]
        public void IndexerTest(int arraySize, int start, int length, int accessIndex, Type exceptionType)
        {
            // 0以上の時だけ配列を生成し、それ以外は null で初期化をする
            SrValue[] values = arraySize < 0 ? null : new SrValue[arraySize];
            var memoryBlock = new MemoryBlock<SrValue>(values, start, length);


            // 例外タイプが指定されていないなら
            if (exceptionType == null)
            {
                // 受け取ったパラメータで例外は発生しないテストをする
                Assert.DoesNotThrow(() => { ref var value = ref memoryBlock[accessIndex]; });
            }
            else
            {
                // 受け取った型の例外が発生するテストをする
                Assert.Throws(exceptionType, () => { ref var value = ref memoryBlock[accessIndex]; });
            }
        }


        /// <summary>
        /// インデクサアクセスを行い、値の読み書きが想定通りに動作するかテストを行います
        /// </summary>
        [Test]
        public unsafe void ItemAccessTest()
        {
            // メモリプールといくつかのメモリブロックを用意
            var memoryPool = new SrValue[100];
            var memoryBlockA = new MemoryBlock<SrValue>(memoryPool, 0, 50);
            var memoryBlockB = new MemoryBlock<SrValue>(memoryPool, 50, 50);
            var memoryBlockC = new MemoryBlock<SrValue>(memoryPool, 0, 100);
            var memoryBlockD = new MemoryBlock<SrValue>(memoryPool, 25, 50);


            // メモリブロック適当入力値の用意
            var valueA = 123;
            var valueB = 456;
            var valueD = 789;


            // A,B,Dのメモリブロックの先頭に適当な値を入れる
            memoryBlockA[0].Value.Int[0] = valueA;
            memoryBlockB[0].Value.Int[0] = valueB;
            memoryBlockD[0].Value.Int[0] = valueD;


            // メモリプールの中がメモリブロックによって書き換えがちゃんと行われているかチェック
            Assert.AreEqual(valueA, memoryPool[0].Value.Int[0]);
            Assert.AreEqual(valueB, memoryPool[50].Value.Int[0]);
            Assert.AreEqual(valueD, memoryPool[25].Value.Int[0]);
            Assert.AreEqual(valueA, memoryBlockC[0].Value.Int[0]);
            Assert.AreEqual(valueB, memoryBlockC[50].Value.Int[0]);
            Assert.AreEqual(valueD, memoryBlockC[25].Value.Int[0]);
        }


        /// <summary>
        /// メモリブロックから新しいメモリブロックを切り出す Slice 関数のテストをします
        /// </summary>
        [Test]
        public void SliceTest()
        {
            // 簡単なint型メモリブロックを用意する
            var memoryPool = new int[100];
            var memoryBlock = new MemoryBlock<int>(memoryPool, 0, memoryPool.Length);


            // オリジナルメモリブロックに値を入れる
            for (int i = 0; i < memoryBlock.Length; ++i)
            {
                // ただひたすら代入
                memoryBlock[i] = i;
            }


            // まずは適当な範囲を切り出して値がインデックスの範囲の値通りか確認
            var memoryBlockB = memoryBlock.Slice(50, 10);
            Assert.AreEqual(50, memoryBlockB.Offset);
            Assert.AreEqual(10, memoryBlockB.Length);
            var memoryBlockBB = memoryBlockB.Slice(10, 5);
            Assert.AreEqual(60, memoryBlockBB.Offset);
            Assert.AreEqual(5, memoryBlockBB.Length);
            for (int i = 0; i < memoryBlockB.Length; ++i)
            {
                Assert.AreEqual(i + memoryBlockB.Offset, memoryBlockB[i]);
            }
            for (int i = 0; i < memoryBlockBB.Length; ++i)
            {
                Assert.AreEqual(i + memoryBlockBB.Offset, memoryBlockBB[i]);
            }
        }
    }
}
