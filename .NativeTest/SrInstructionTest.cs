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

using NUnit.Framework;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbitTest
{
    /// <summary>
    /// SrInstruction 構造体のテストクラスです
    /// </summary>
    [TestFixture]
    public class SrInstructionTest
    {
        /// <summary>
        /// Set関数による値を設定したときに意図したビットパターンになっているかをテストします
        /// </summary>
        /// <param name="opCode">設定するオペコード</param>
        /// <param name="r1">指定レジスタインデックス1番</param>
        /// <param name="r2">指定レジスタインデックス2番</param>
        /// <param name="r3">指定レジスタインデックス3番</param>
        /// <param name="imm">即値</param>
        /// <param name="expected">想定されるビットパターン</param>
        [TestCase(OpCode.Mov, SrvmProcessor.RegisterBPIndex, SrvmProcessor.RegisterSPIndex, (byte)0, 0U, (0b_0001_0000_00000000_0_00110_00111_00000UL << 32) | 0x00000000UL)]
        [TestCase(OpCode.Addl, SrvmProcessor.RegisterBIndex, SrvmProcessor.RegisterIPIndex, (byte)0, 0xA5A5A5A5U, (0b_0010_0001_00000000_0_00001_11110_00000UL << 32) | 0xA5A5A5A5UL)]
        [TestCase(OpCode.Gpfid, SrvmProcessor.RegisterR10Index, SrvmProcessor.RegisterR13Index, SrvmProcessor.RegisterR9Index, 0x123U, (0b_1111_0100_00000000_0_01010_01101_01001UL << 32) | 0x123UL)]
        public void BitPatternTest(OpCode opCode, byte r1, byte r2, byte r3, uint imm, ulong expected)
        {
            // 値をセットして想定したビットパターンかを確認
            var instruction = new SrInstruction();
            instruction.Set(opCode, r1, r2, r3, imm);
            Assert.AreEqual(expected, instruction.Raw);
        }
    }
}