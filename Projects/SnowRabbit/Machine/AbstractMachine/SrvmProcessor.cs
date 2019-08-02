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
using SnowRabbit.Runtime;

namespace SnowRabbit.Machine
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
        public const int RegisterR8Index = 8; // FullGeneral Register (x8)[Rx => R8 - R15]
        public const int RegisterR9Index = 9; // FullGeneral Register (x8)[Rx => R8 - R15]
        public const int RegisterR10Index = 10; // FullGeneral Register (x8)[Rx => R8 - R15]
        public const int RegisterR11Index = 11; // FullGeneral Register (x8)[Rx => R8 - R15]
        public const int RegisterR12Index = 12; // FullGeneral Register (x8)[Rx => R8 - R15]
        public const int RegisterR13Index = 13; // FullGeneral Register (x8)[Rx => R8 - R15]
        public const int RegisterR14Index = 14; // FullGeneral Register (x8)[Rx => R8 - R15]
        public const int RegisterR15Index = 15; // FullGeneral Register (x8)[Rx => R8 - R15]
        public const int RegisterIPIndex = 16; // InstructionPointer Register (SpecialRegister index 0)[IP]
        // Register Information
        private const int RegisterTotalCount = RegisterIPIndex + 1;
        private const int ProcessorContextSize = RegisterTotalCount * 8;



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
                // 無条件で0を設定する
                process.ProcessorContext[i].Value.Long[0] = 0;
            }


            // スタックおよびベースポインタはプロセスメモリの末尾へ（インデックス境界外なのは、デクリメントしてからのプッシュ、ポップしてからのインクリメント、の方式のため）
            process.ProcessorContext[RegisterSPIndex].Value.Long[0] = process.ProcessMemory.Length;
            process.ProcessorContext[RegisterBPIndex].Value.Long[0] = process.ProcessMemory.Length;
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
        private static void FetchRegisterInfo(ref InstructionCode instruction, out int regANumber, out int regBNumber, out int regCNumber)
        {
            // レジスタの構造タイプを指定する意味が殆どなくなったためそのまま受け取る
            regANumber = instruction.Ra;
            regBNumber = instruction.Rb;
            regCNumber = instruction.Rc;
        }
    }
}