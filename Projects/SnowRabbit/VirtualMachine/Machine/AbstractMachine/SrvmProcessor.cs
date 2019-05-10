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
        public const int RegisterAIndex = 0; // General Accumulator Register[RAX]
        public const int RegisterBIndex = 1; // General Base Register[RBX]
        public const int RegisterCIndex = 2; // General Counter Register[RCX]
        public const int RegisterDIndex = 3; // General Data Register[RDX]
        public const int RegisterSIIndex = 4; // General SourceIndex Register[RSI]
        public const int RegisterDIIndex = 5; // General DestinationIndex Register[RDI]
        public const int RegisterBPIndex = 6; // General BasePointer Register[RBP]
        public const int RegisterSPIndex = 7; // General StackPointer Register[RSP]
        public const int RegisterRnBaseIndex = 8; // FullGeneral Register (x8)[Rx => R8 - R15]
        public const int RegisterIPIndex = 16; // InstructionPointer Register (SpecialRegister index 0)[IP]
        // Register Information
        public const int RegisterTotalCount = RegisterIPIndex + 1;
        public const int ProcessorContextSize = RegisterTotalCount * 8;



        #region Register Control
        /// <summary>
        /// 指定されたプロセスのプロセッサコンテキストのレジスタに値を設定します
        /// </summary>
        /// <param name="process">設定する対象のコンテキストを持っているプロセス</param>
        /// <param name="registerIndex">設定するレジスタインデックス</param>
        /// <param name="value">設定する値</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static void SetRegisterValue(ref SrProcess process, int registerIndex, long value)
        {
            // 指定されたインデックスに無条件で値を設定する
            process.ProcessorContext[registerIndex].Value.Long[0] = value;
        }


        /// <summary>
        /// 指定されたプロセスのプロセッサコンテキストのレジスタの値を取得します
        /// </summary>
        /// <param name="process">取得する対象のコンテキストを持っているプロセス</param>
        /// <param name="registerIndex">取得するレジスタインデックス</param>
        /// <returns>取得された値を返します</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static long GetRegisterValue(ref SrProcess process, int registerIndex)
        {
            // 指定されたインデックスの値を無条件で返す
            return process.ProcessorContext[registerIndex].Value.Long[0];
        }
        #endregion


        #region Context Control
        /// <summary>
        /// 指定されたプロセスのプロセッサコンテキストを初期化します
        /// </summary>
        /// <remarks>
        /// コンテキストを初期化する前に必ずプロセスメモリが初期化されている必要があります
        /// </remarks>
        /// <param name="process">コンテキストを初期化するプロセス</param>
        internal unsafe void InitializeContext(ref SrProcess process)
        {
            // メモリモジュールからVMコンテキストとしてメモリを貰って全てゼロクリアする
            process.ProcessorContext = Machine.Memory.AllocateValue(ProcessorContextSize, AllocationType.VMContext);
            for (int i = 0; i < RegisterTotalCount; ++i)
            {
                // 値を0クリア
                SetRegisterValue(ref process, i, 0);
            }


            // スタックおよびベースポインタはプロセスメモリの末尾へ（インデックス境界外なのは、デクリメントしてからのプッシュ、ポップしてからのインクリメント、の方式のため）
            SetRegisterValue(ref process, RegisterBPIndex, process.ProcessMemory.Length);
            SetRegisterValue(ref process, RegisterSPIndex, process.ProcessMemory.Length);
        }


        /// <summary>
        /// プロセッサコンテキストの命令ポインタを更新します
        /// </summary>
        /// <param name="process">更新するプロセッサコンテキストを持っているプロセス</param>
        /// <param name="instructionPointer">更新する命令ポインタの値</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static void UpdateInstructionPointer(ref SrProcess process, long instructionPointer)
        {
            // 命令ポインタ
            process.ProcessorContext[RegisterIPIndex].Value.Long[0] = instructionPointer;
        }
        #endregion


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static void FetchInstruction(ref SrProcess process, out InstructionCode instruction, out long instructionPointer)
        {
            instructionPointer = process.ProcessorContext[RegisterIPIndex].Value.Long[0];
            instruction = process.ProcessMemory[(int)instructionPointer].Instruction;
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
    }
}