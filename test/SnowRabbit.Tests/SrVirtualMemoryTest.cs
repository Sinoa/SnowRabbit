// zlib License
// 
// Copyright (c) 2019 Sinoa
// 
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
// 
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software
// in a product, an acknowledgment in the product documentation would be
// appreciated but is not required.
// 
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
// 
// 3. This notice may not be removed or altered from any source
// distribution.

using SnowRabbit.RuntimeEngine;

namespace SnowRabbit.Tests;

/// <summary>
/// SrVirtualMemory クラスに対するテストクラスです
/// </summary>
[TestFixture]
public class SrVirtualMemoryTest
{
    /// <summary>
    /// 仮想アドレスに対してアクセスを行い想定した値の設定取得が出来ているかのテストをします
    /// </summary>
    [Test]
    public void VirtualAddressAccessTest()
    {
        // まずは実体の配列を用意して仮想メモリのインスタンスを用意する
        SrValue[] rawMemory = new SrValue[40];
        MemoryBlock<SrValue> programMemory = new MemoryBlock<SrValue>(rawMemory, 0, 10);
        MemoryBlock<SrValue> globalMemory = new MemoryBlock<SrValue>(rawMemory, 10, 10);
        MemoryBlock<SrValue> heapMemory = new MemoryBlock<SrValue>(rawMemory, 20, 10);
        MemoryBlock<SrValue> stackMemory = new MemoryBlock<SrValue>(rawMemory, 30, 10);
        SrVirtualMemory virtualMemory = new SrVirtualMemory(programMemory, globalMemory, heapMemory, stackMemory);


        // プログラムメモリ領域に対して書き込んだら実体にも書き込まれているか確認
        virtualMemory[0x00000004] = 123456;
        Assert.That(programMemory[0x00000004].Primitive.Int, Is.EqualTo(123456));
        Assert.That(rawMemory[0x00000004].Primitive.Int, Is.EqualTo(123456));


        // グローバル領域に対して書き込んだら実体にも書き込まれているか確認
        virtualMemory[0x00100005] = 654321;
        Assert.That(globalMemory[0x00000005].Primitive.Int, Is.EqualTo(654321));
        Assert.That(rawMemory[0x0000000F].Primitive.Int, Is.EqualTo(654321));


        // ヒープ領域に対して書き込んだら実体にも書き込まれているか確認
        virtualMemory[0x00200009] = 112233;
        Assert.That(heapMemory[0x00000009].Primitive.Int, Is.EqualTo(112233));
        Assert.That(rawMemory[0x0000001D].Primitive.Int, Is.EqualTo(112233));


        // スタック領域に対して書き込んだら実体にも書き込まれているか確認
        virtualMemory[0x00300004] = 445566;
        Assert.That(stackMemory[0x00000004].Primitive.Int, Is.EqualTo(445566));
        Assert.That(rawMemory[0x00000022].Primitive.Int, Is.EqualTo(445566));
    }


    /// <summary>
    /// 定義されていないセグメント番号へのアクセスが例外になることをテストします
    /// </summary>
    [Test]
    public void InvalidSegmentAccessTest()
    {
        // 実体の配列を用意して仮想メモリのインスタンスを用意する
        SrValue[] rawMemory = new SrValue[40];
        MemoryBlock<SrValue> programMemory = new MemoryBlock<SrValue>(rawMemory, 0, 10);
        MemoryBlock<SrValue> globalMemory = new MemoryBlock<SrValue>(rawMemory, 10, 10);
        MemoryBlock<SrValue> heapMemory = new MemoryBlock<SrValue>(rawMemory, 20, 10);
        MemoryBlock<SrValue> stackMemory = new MemoryBlock<SrValue>(rawMemory, 30, 10);
        SrVirtualMemory virtualMemory = new SrVirtualMemory(programMemory, globalMemory, heapMemory, stackMemory);


        // セグメント番号4以上（スタックセグメントの次）へのアクセスは読み書きともに例外になることを確認する
        Assert.Throws<IndexOutOfRangeException>(() => _ = virtualMemory[0x00400000]);
        Assert.Throws<IndexOutOfRangeException>(() => virtualMemory[0x00400000] = 123);
        Assert.Throws<IndexOutOfRangeException>(() => _ = virtualMemory[0x7FF00000]);


        // 負の仮想アドレス（負のセグメント番号）へのアクセスも例外になることを確認する
        Assert.Throws<IndexOutOfRangeException>(() => _ = virtualMemory[-1]);
    }


#if DEBUG
    /// <summary>
    /// セグメント長を超えるオフセットへのアクセスが境界チェックで例外になることをテストします
    /// </summary>
    [Test]
    public void OffsetBoundaryAccessTest()
    {
        // 実体の配列を用意して仮想メモリのインスタンスを用意する（各セグメント長は10）
        SrValue[] rawMemory = new SrValue[40];
        MemoryBlock<SrValue> programMemory = new MemoryBlock<SrValue>(rawMemory, 0, 10);
        MemoryBlock<SrValue> globalMemory = new MemoryBlock<SrValue>(rawMemory, 10, 10);
        MemoryBlock<SrValue> heapMemory = new MemoryBlock<SrValue>(rawMemory, 20, 10);
        MemoryBlock<SrValue> stackMemory = new MemoryBlock<SrValue>(rawMemory, 30, 10);
        SrVirtualMemory virtualMemory = new SrVirtualMemory(programMemory, globalMemory, heapMemory, stackMemory);


        // セグメント内の最終要素へのアクセスは正常に動作することを確認する
        Assert.DoesNotThrow(() => virtualMemory[0x00100009] = 1);


        // セグメント長ちょうどのオフセット（境界外）へのアクセスは読み書きともに例外になることを確認する
        // （デバッグビルドの MemoryBlock 境界チェックによって検出される）
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = virtualMemory[0x0000000A]);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = virtualMemory[0x0010000A]);
        Assert.Throws<ArgumentOutOfRangeException>(() => virtualMemory[0x0020000A] = 123);
        Assert.Throws<ArgumentOutOfRangeException>(() => virtualMemory[0x0030000A] = 123);
    }
#endif


    /// <summary>
    /// セグメント長の上限（2^20要素）を超えるメモリブロックの指定が例外になることをテストします
    /// </summary>
    [Test]
    public void SegmentLengthLimitTest()
    {
        // 上限丁度の長さと上限を1要素超える長さのメモリブロックを用意する
        const int MaxSegmentLength = 1 << 20;
        SrValue[] rawMemory = new SrValue[MaxSegmentLength + 1];
        MemoryBlock<SrValue> maxLengthBlock = new MemoryBlock<SrValue>(rawMemory, 0, MaxSegmentLength);
        MemoryBlock<SrValue> tooLongBlock = new MemoryBlock<SrValue>(rawMemory, 0, MaxSegmentLength + 1);
        MemoryBlock<SrValue> smallBlock = new MemoryBlock<SrValue>(rawMemory, 0, 4);


        // 上限丁度の長さのセグメントは生成できることを確認する
        Assert.DoesNotThrow(() => _ = new SrVirtualMemory(maxLengthBlock, smallBlock, smallBlock, smallBlock));


        // どのセグメントであっても上限を超える長さの指定は例外になることを確認する
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new SrVirtualMemory(tooLongBlock, smallBlock, smallBlock, smallBlock));
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new SrVirtualMemory(smallBlock, tooLongBlock, smallBlock, smallBlock));
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new SrVirtualMemory(smallBlock, smallBlock, tooLongBlock, smallBlock));
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new SrVirtualMemory(smallBlock, smallBlock, smallBlock, tooLongBlock));
    }
}
