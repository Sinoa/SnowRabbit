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
using System.Runtime.CompilerServices;
using SnowRabbit.VirtualMachine.Runtime;

namespace SnowRabbit.VirtualMachine.Machine
{
    /// <summary>
    /// 仮想マシンが実装する仮想マシンプロセッサの抽象クラスです
    /// </summary>
    public abstract partial class SrvmProcessor : SrvmMachineParts
    {
        // Register Index
        public const int RegisterAIndex = 0; // General Accumulator Register
        public const int RegisterBIndex = 1; // General Base Register
        public const int RegisterCIndex = 2; // General Counter Register
        public const int RegisterDIndex = 3; // General Data Register
        public const int RegisterSIIndex = 4; // General SourceIndex Register
        public const int RegisterDIIndex = 5; // General DestinationIndex Register
        public const int RegisterBPIndex = 6; // General BasePointer Register
        public const int RegisterSPIndex = 7; // General StackPointer Register
        public const int RegisterRnBaseIndex = 8; // FullGeneral Register (x8)
        public const int RegisterIPIndex = 16; // InstructionPointer Register (SpecialRegister index 0)
        public const int RegisterFlagIndex = 17; // Flag Register (SpecialRegister index 1)
        public const int RegisterLinkIndex = 18; // Link Register (SpecialRegister index 2)
        // Register Information
        public const int RegisterTotalCount = RegisterLinkIndex + 1;
        public const int ProcessorContextSize = RegisterTotalCount * 8;



        /// <summary>
        /// 指定されたプロセスのプロセッサコンテキストを初期化します
        /// </summary>
        /// <param name="process">初期化するプロセス</param>
        internal unsafe void InitializeContext(ref SrProcess process)
        {
            // メモリモジュールからVMコンテキストとしてメモリを貰って全てゼロクリアする
            process.ProcessorContext = Machine.Memory.AllocateValue(ProcessorContextSize, AllocationType.VMContext);
            for (int i = 0; i < RegisterTotalCount; ++i)
            {
                // 値を0クリア
                process.ProcessorContext[i].Value.Ulong[0] = 0UL;
            }


            // スタックポインタはプロセスメモリの末尾へ（インデックス境界外なのは、デクリメントしてからのプッシュ、ポップしてからのインクリメント、の方式のため）
            process.ProcessorContext[RegisterSPIndex].Value.Long[0] = process.ProcessMemory.Length;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static void FetchInstruction(ref SrProcess process, out InstructionCode instruction, out long instructionPointer)
        {
            instructionPointer = process.ProcessorContext[RegisterIPIndex].Value.Long[0];
            instruction = process.ProcessMemory[(int)instructionPointer].Instruction;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static void UpdateInstructionPointer(ref SrProcess process, long instructionPointer)
        {
            process.ProcessorContext[RegisterIPIndex].Value.Long[0] = instructionPointer;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FetchRegisterInfo(ref InstructionCode instruction, out int regANumber, out int regBNumber, out int regCNumber, out SrRegisterType regAType, out SrRegisterType regBType, out SrRegisterType regCType)
        {
            regAType = (SrRegisterType)(instruction.Ra & 0xF0);
            regBType = (SrRegisterType)(instruction.Rb & 0xF0);
            regCType = (SrRegisterType)(instruction.Rc & 0xF0);
            regANumber = (instruction.Ra & 0x0F) + (regAType == SrRegisterType.SpecialRegister ? 16 : 0);
            regBNumber = (instruction.Rb & 0x0F) + (regBType == SrRegisterType.SpecialRegister ? 16 : 0);
            regCNumber = (instruction.Rc & 0x0F) + (regCType == SrRegisterType.SpecialRegister ? 16 : 0);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static bool IsFlagRegisterSet(ref SrProcess process, FlagRegisterMask flag)
        {
            return (process.ProcessorContext[RegisterFlagIndex].Value.Ulong[0] & (ulong)flag) == (ulong)flag;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static bool IsFlagRegisterClear(ref SrProcess process, FlagRegisterMask flag)
        {
            return (process.ProcessorContext[RegisterFlagIndex].Value.Ulong[0] & (ulong)flag) == 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static void SetFlagRegister(ref SrProcess process, FlagRegisterMask flag)
        {
            process.ProcessorContext[RegisterFlagIndex].Value.Ulong[0] |= (ulong)flag;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static void ClearFlagRegister(ref SrProcess process, FlagRegisterMask flag)
        {
            process.ProcessorContext[RegisterFlagIndex].Value.Ulong[0] &= ~(ulong)flag;
        }



        [Flags]
        public enum FlagRegisterMask : ulong
        {
            Z = 0x0000_0000_0000_0001,
            N = 0x0000_0000_0000_0002,
            P = 0x0000_0000_0000_0004,
        }
    }
}