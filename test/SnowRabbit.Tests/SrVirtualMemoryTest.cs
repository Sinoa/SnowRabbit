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
}
