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
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using SnowRabbit.Diagnostics.Logging;

namespace SnowRabbit.RuntimeEngine.VirtualMachine
{
    /// <summary>
    /// SnowRabbit が実装する仮想マシンプロセッサクラスです
    /// </summary>
    public class SrvmProcessor : SrvmMachineParts
    {
        // 定数定義
        public const byte RegisterAIndex = 0; // General Accumulator Register[A]
        public const byte RegisterBIndex = 1; // General Base Register[B]
        public const byte RegisterCIndex = 2; // General Counter Register[C]
        public const byte RegisterDIndex = 3; // General Data Register[D]
        public const byte RegisterSIIndex = 4; // General SourceIndex Register[SI]
        public const byte RegisterDIIndex = 5; // General DestinationIndex Register[DI]
        public const byte RegisterBPIndex = 6; // General BasePointer Register[BP]
        public const byte RegisterSPIndex = 7; // General StackPointer Register[SP]
        public const byte RegisterR8Index = 8; // FullGeneral Register[R8]
        public const byte RegisterR9Index = 9; // FullGeneral Register[R9]
        public const byte RegisterR10Index = 10; // FullGeneral Register[R10]
        public const byte RegisterR11Index = 11; // FullGeneral Register[R11]
        public const byte RegisterR12Index = 12; // FullGeneral Register[R12]
        public const byte RegisterR13Index = 13; // FullGeneral Register[R13]
        public const byte RegisterR14Index = 14; // FullGeneral Register[R14]
        public const byte RegisterR15Index = 15; // FullGeneral Register[R15]
        public const byte RegisterIPIndex = 30; // InstructionPointer Register[IP]
        public const byte RegisterZeroIndex = 31; // Zero Register[ZERO]
        public const byte RegisterInvalidIndex = 31; // Invalid Register[INVALID]



        /// <summary>
        /// 対象プロセスのプロセッサコンテキストを初期化します
        /// </summary>
        /// <param name="process">初期化するプロセス</param>
        /// <exception cref="ArgumentNullException">process が null です</exception>
        public unsafe virtual void InitializeProcessorContext(SrProcess process)
        {
            // null を渡されたらむり
            if (process == null) throw new ArgumentNullException(nameof(process));


            // 末尾のレジスタインデックスまでループ
            SrLogger.Trace(SharedString.LogTag.SR_VM_PROCESSOR, "InitializeProcessorContext");
            for (var i = 0; i < RegisterZeroIndex; ++i)
            {
                // プロセッサコンテキストの値を初期化する
                process.ProcessorContext[i] = default;
            }


            // スタックポインタとベースポインタの位置をプロセスメモリの末尾へ
            // （プッシュ時はデクリメントされたから値がセットされるので、配列の長さそのままで初期化）
            var memoryLength = process.ProcessMemory.Length;
            process.ProcessorContext[RegisterSPIndex].Primitive.Long = memoryLength;
            process.ProcessorContext[RegisterBPIndex].Primitive.Long = memoryLength;
        }


        /// <summary>
        /// 指定されたプロセスを実行します
        /// </summary>
        /// <param name="process"></param>
        /// <exception cref="ArgumentNullException">process が null です</exception>
        internal unsafe void Execute(SrProcess process)
        {
            // null を渡されたらむり
            if (process == null) throw new ArgumentNullException(nameof(process));
            SrLogger.Trace(SharedString.LogTag.SR_VM_PROCESSOR, $"Execute process ID={process.ProcessID}");


            // もしプロセスが 停止 または 一時停止 または パニックなら
            if ((process.ProcessState & (SrProcessStatus.Stopped | SrProcessStatus.Suspended | SrProcessStatus.Panic)) != 0)
            {
                // 何もせず終了
                SrLogger.Trace(SharedString.LogTag.SR_VM_PROCESSOR, $"Execute stopped, for Process status is '{process.ProcessState}'.");
                return;
            }


            // プロセスが準備完了状態なのなら
            if (process.ProcessState == SrProcessStatus.Ready)
            {
                // プロセスの動作開始イベントを実行して開始状態にする
                SrLogger.Trace(SharedString.LogTag.SR_VM_PROCESSOR, $"Startup process ID={process.ProcessID}");
                OnProcessStartup(process);
                process.ProcessState = SrProcessStatus.Running;
            }


            try
            {
                // プロセスを実行する
                SrLogger.Trace(SharedString.LogTag.SR_VM_PROCESSOR, $"Execute Core process ID={process.ProcessID}");
                ExecuteCore(process);
            }
            catch (Exception exception)
            {
                // プロセスをパニック状態と動作計測を停止にする
                SrLogger.Fatal(SharedString.LogTag.SR_VM_PROCESSOR, exception.Message, exception);
                process.ProcessState = SrProcessStatus.Panic;
                process.RunningStopwatch.Stop();


                // 動作モードがそのままスローなら
                if (process.UnhandledExceptionMode == UnhandledExceptionMode.ThrowException)
                {
                    // 何も言わず再スロー
                    SrLogger.Debug(SharedString.LogTag.SR_VM_PROCESSOR, $"UnhandledExceptionMode is ThrowException. ID={process.ProcessID}");
                    throw;
                }


                // 発生した例外をキャプチャして例外を処理するイベントを起こす
                SrLogger.Debug(SharedString.LogTag.SR_VM_PROCESSOR, $"Exception capture process ID={process.ProcessID}");
                process.ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
                OnExceptionOccurrenced(process, exception);
            }
        }


        /// <summary>
        /// プロセスを実際に処理する実行関数です
        /// </summary>
        /// <param name="process">実行するプロセス</param>
        private unsafe void ExecuteCore(SrProcess process)
        {
            // プロセスの動作計測ストップウォッチを開始
            process.RunningStopwatch.Start();


            // 実行フラグを立てて、降りるまでループ
            var running = true;
            while (running)
            {
                // 現在の命令ポインタが指している命令を取り出して、実行の準備をしてデバッグイベントを呼ぶ
                var instructionPointer = process.ProcessorContext[RegisterIPIndex].Primitive.Int;
                var nextInstructionPointer = instructionPointer + 1;
                var instruction = process.ProgramCode[instructionPointer].Primitive.Instruction;
                instruction.GetRegisterNumber(out var r1, out var r2, out var r3);
                OnPreProcessInstruction_Debug(process, instruction);


                // 実行する命令ごとに切り替える
                switch (instruction.OpCode)
                {
                    #region CPU Control
                    //case OpCode.Halt:
                    //    execution = false;
                    //    process.ProcessStatus = SrProcessStatus.Stopped;
                    //    break;
                    #endregion


                    #region Data Transfer
                    //case OpCode.Mov:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0];
                    //    break;


                    //case OpCode.Movl:
                    //    context[regANumber].Value.Long[0] = immediate;
                    //    break;


                    //case OpCode.Ldr:
                    //    var offsetAddress = (int)context[regBNumber].Value.Long[0];
                    //    context[regANumber].Value.Long[0] = memory[offsetAddress + immediate].Value.Long[0];
                    //    break;


                    //case OpCode.Ldrl:
                    //    context[regANumber].Value.Long[0] = memory[immediate].Value.Long[0];
                    //    break;


                    //case OpCode.Str:
                    //    offsetAddress = (int)context[regBNumber].Value.Long[0];
                    //    memory[offsetAddress + immediate].Value.Long[0] = context[regANumber].Value.Long[0];
                    //    break;


                    //case OpCode.Strl:
                    //    memory[immediate].Value.Long[0] = context[regANumber].Value.Long[0];
                    //    break;


                    //case OpCode.Push:
                    //    var sp = context[RegisterSPIndex].Value.Long[0] - 1;
                    //    memory[(int)sp].Value.Long[0] = context[regANumber].Value.Long[0];
                    //    context[RegisterSPIndex].Value.Long[0] = sp;
                    //    break;


                    //case OpCode.Pushl:
                    //    sp = context[RegisterSPIndex].Value.Long[0] - 1;
                    //    memory[(int)sp].Value.Long[0] = immediate;
                    //    context[RegisterSPIndex].Value.Long[0] = sp;
                    //    break;


                    //case OpCode.Pop:
                    //    sp = context[RegisterSPIndex].Value.Long[0];
                    //    context[regANumber].Value.Long[0] = memory[(int)sp].Value.Long[0];
                    //    context[RegisterSPIndex].Value.Long[0] = sp + 1;
                    //    break;


                    //case OpCode.Fmovl:
                    //    context[regANumber].Value.Float[0] = immediateF;
                    //    break;


                    //case OpCode.Fpushl:
                    //    sp = context[RegisterSPIndex].Value.Long[0] - 1;
                    //    memory[(int)sp].Value.Float[0] = immediateF;
                    //    context[RegisterSPIndex].Value.Long[0] = sp;
                    //    break;


                    //case OpCode.Movfti:
                    //    context[regANumber].Value.Long[0] = (long)context[regBNumber].Value.Float[0];
                    //    break;


                    //case OpCode.Movitf:
                    //    context[regANumber].Value.Float[0] = context[regBNumber].Value.Long[0];
                    //    break;
                    #endregion


                    #region Arithmetic
                    //case OpCode.Add:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] + context[regCNumber].Value.Long[0];
                    //    break;


                    //case OpCode.Addl:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] + immediate;
                    //    break;


                    //case OpCode.Sub:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] - context[regCNumber].Value.Long[0];
                    //    break;


                    //case OpCode.Subl:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] - immediate;
                    //    break;


                    //case OpCode.Mul:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] * context[regCNumber].Value.Long[0];
                    //    break;


                    //case OpCode.Mull:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] * immediate;
                    //    break;


                    //case OpCode.Div:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] / context[regCNumber].Value.Long[0];
                    //    break;


                    //case OpCode.Divl:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] / immediate;
                    //    break;


                    //case OpCode.Mod:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] % context[regCNumber].Value.Long[0];
                    //    break;


                    //case OpCode.Modl:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] % immediate;
                    //    break;


                    //case OpCode.Pow:
                    //    context[regANumber].Value.Long[0] = (long)Math.Pow(context[regBNumber].Value.Long[0], context[regCNumber].Value.Long[0]);
                    //    break;


                    //case OpCode.Powl:
                    //    context[regANumber].Value.Long[0] = (long)Math.Pow(context[regBNumber].Value.Long[0], immediate);
                    //    break;


                    //case OpCode.Neg:
                    //    context[regANumber].Value.Long[0] = -context[regBNumber].Value.Long[0];
                    //    break;


                    //case OpCode.Negl:
                    //    context[regANumber].Value.Long[0] = -immediate;
                    //    break;


                    //case OpCode.Fadd:
                    //    context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] + context[regCNumber].Value.Float[0];
                    //    break;


                    //case OpCode.Faddl:
                    //    context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] + immediateF;
                    //    break;


                    //case OpCode.Fsub:
                    //    context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] - context[regCNumber].Value.Float[0];
                    //    break;


                    //case OpCode.Fsubl:
                    //    context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] - immediateF;
                    //    break;


                    //case OpCode.Fmul:
                    //    context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] * context[regCNumber].Value.Float[0];
                    //    break;


                    //case OpCode.Fmull:
                    //    context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] * immediateF;
                    //    break;


                    //case OpCode.Fdiv:
                    //    context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] / context[regCNumber].Value.Float[0];
                    //    break;


                    //case OpCode.Fdivl:
                    //    context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] / immediateF;
                    //    break;


                    //case OpCode.Fmod:
                    //    context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] % context[regCNumber].Value.Float[0];
                    //    break;


                    //case OpCode.Fmodl:
                    //    context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] % immediateF;
                    //    break;


                    //case OpCode.Fpow:
                    //    context[regANumber].Value.Float[0] = (float)Math.Pow(context[regBNumber].Value.Float[0], context[regCNumber].Value.Float[0]);
                    //    break;


                    //case OpCode.Fpowl:
                    //    context[regANumber].Value.Float[0] = (float)Math.Pow(context[regBNumber].Value.Float[0], immediateF);
                    //    break;


                    //case OpCode.Fneg:
                    //    context[regANumber].Value.Float[0] = -context[regBNumber].Value.Float[0];
                    //    break;


                    //case OpCode.Fnegl:
                    //    context[regANumber].Value.Float[0] = -immediateF;
                    //    break;
                    #endregion


                    #region Logic
                    //case OpCode.Or:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] | context[regCNumber].Value.Long[0];
                    //    break;


                    //case OpCode.Xor:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] ^ context[regCNumber].Value.Long[0];
                    //    break;


                    //case OpCode.And:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] & context[regCNumber].Value.Long[0];
                    //    break;


                    //case OpCode.Not:
                    //    context[regANumber].Value.Long[0] = ~context[regANumber].Value.Long[0];
                    //    break;


                    //case OpCode.Shl:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] << (int)(context[regCNumber].Value.Long[0]);
                    //    break;


                    //case OpCode.Shr:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] >> (int)(context[regCNumber].Value.Long[0]);
                    //    break;


                    //case OpCode.Teq:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] == context[regCNumber].Value.Long[0] ? 1L : 0L;
                    //    break;


                    //case OpCode.Tne:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] != context[regCNumber].Value.Long[0] ? 1L : 0L;
                    //    break;


                    //case OpCode.Tg:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] > context[regCNumber].Value.Long[0] ? 1L : 0L;
                    //    break;


                    //case OpCode.Tge:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] >= context[regCNumber].Value.Long[0] ? 1L : 0L;
                    //    break;


                    //case OpCode.Tl:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] < context[regCNumber].Value.Long[0] ? 1L : 0L;
                    //    break;


                    //case OpCode.Tle:
                    //    context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] <= context[regCNumber].Value.Long[0] ? 1L : 0L;
                    //    break;
                    #endregion


                    #region Flow Control
                    //case OpCode.Br:
                    //    nextInstructionPointer = context[regANumber].Value.Long[0] + immediate;
                    //    break;


                    //case OpCode.Brl:
                    //    nextInstructionPointer = immediate;
                    //    break;


                    //case OpCode.Bnz:
                    //    nextInstructionPointer = context[regBNumber].Value.Long[0] != 0 ? context[regANumber].Value.Long[0] + immediate : nextInstructionPointer;
                    //    break;


                    //case OpCode.Bnzl:
                    //    nextInstructionPointer = context[regBNumber].Value.Long[0] != 0 ? immediate : nextInstructionPointer;
                    //    break;


                    //case OpCode.Call:
                    //    sp = context[RegisterSPIndex].Value.Long[0] - 1;
                    //    memory[(int)sp].Value.Long[0] = nextInstructionPointer;
                    //    context[RegisterSPIndex].Value.Long[0] = sp;
                    //    nextInstructionPointer = context[regANumber].Value.Long[0] + immediate;
                    //    break;


                    //case OpCode.Calll:
                    //    sp = context[RegisterSPIndex].Value.Long[0] - 1;
                    //    memory[(int)sp].Value.Long[0] = nextInstructionPointer;
                    //    context[RegisterSPIndex].Value.Long[0] = sp;
                    //    nextInstructionPointer = immediate;
                    //    break;


                    //case OpCode.Callnz:
                    //    if (context[regBNumber].Value.Long[0] != 0)
                    //    {
                    //        sp = context[RegisterSPIndex].Value.Long[0] - 1;
                    //        memory[(int)sp].Value.Long[0] = nextInstructionPointer;
                    //        context[RegisterSPIndex].Value.Long[0] = sp;
                    //        nextInstructionPointer = context[regANumber].Value.Long[0] + immediate;
                    //    }
                    //    break;


                    //case OpCode.Callnzl:
                    //    if (context[regBNumber].Value.Long[0] != 0)
                    //    {
                    //        sp = context[RegisterSPIndex].Value.Long[0] - 1;
                    //        memory[(int)sp].Value.Long[0] = nextInstructionPointer;
                    //        context[RegisterSPIndex].Value.Long[0] = sp;
                    //        nextInstructionPointer = immediate;
                    //    }
                    //    break;


                    //case OpCode.Ret:
                    //    sp = context[RegisterSPIndex].Value.Long[0];
                    //    nextInstructionPointer = memory[(int)sp].Value.Long[0];
                    //    context[RegisterSPIndex].Value.Long[0] = sp + 1;
                    //    break;
                    #endregion


                    #region CSharp Host Control
                    //case OpCode.Cpf:
                    //    var peripheral = Machine.Firmware.GetPeripheral((int)context[regANumber].Value.Long[0]);
                    //    var function = peripheral.GetFunction((int)context[regBNumber].Value.Long[0]);
                    //    var stackFrame = memory.Slice((int)context[RegisterSPIndex].Value.Long[0], (int)context[regCNumber].Value.Long[0]);
                    //    var result = function(new SrStackFrame(process.ID, context, stackFrame, process.ObjectMemory));
                    //    if (result == HostFunctionResult.Pause)
                    //    {
                    //        execution = false;
                    //        process.ProcessStatus = SrProcessStatus.Suspended;
                    //    }
                    //    break;


                    //case OpCode.Cpfl:
                    //    peripheral = Machine.Firmware.GetPeripheral((int)context[regANumber].Value.Long[0]);
                    //    function = peripheral.GetFunction((int)context[regBNumber].Value.Long[0]);
                    //    stackFrame = memory.Slice((int)context[RegisterSPIndex].Value.Long[0], immediate);
                    //    result = function(new SrStackFrame(process.ID, context, stackFrame, process.ObjectMemory));
                    //    if (result == HostFunctionResult.Pause)
                    //    {
                    //        execution = false;
                    //        process.ProcessStatus = SrProcessStatus.Suspended;
                    //    }
                    //    break;


                    //case OpCode.Gpid:
                    //    context[regANumber].Value.Long[0] = Machine.Firmware.GetPeripheralID((string)process.ObjectMemory[(int)context[regBNumber].Value.Long[0]].Value);
                    //    break;


                    //case OpCode.Gpidl:
                    //    context[regANumber].Value.Long[0] = Machine.Firmware.GetPeripheralID((string)process.ObjectMemory[immediate].Value);
                    //    break;


                    //case OpCode.Gpfid:
                    //    peripheral = Machine.Firmware.GetPeripheral((int)context[regBNumber].Value.Long[0]);
                    //    context[regANumber].Value.Long[0] = peripheral.GetFunctionID((string)process.ObjectMemory[(int)context[regCNumber].Value.Long[0]].Value);
                    //    break;


                    //case OpCode.Gpfidl:
                    //    peripheral = Machine.Firmware.GetPeripheral((int)context[regBNumber].Value.Long[0]);
                    //    context[regANumber].Value.Long[0] = peripheral.GetFunctionID((string)process.ObjectMemory[immediate].Value);
                    //    break;
                    #endregion


                    //default: throw new NotImplementedException($"Unknown machine code {(byte)instruction.OpCode}");
                }


                // 最終的な次に実行する命令位置をもどして実行後イベントも呼ぶ
                process.ProcessorContext[RegisterIPIndex].Primitive.Int = nextInstructionPointer;
                OnPostProcessInstruction_Debug(process, instruction);
            }


            // プロセスの動作計測ストップウォッチを停止
            process.RunningStopwatch.Stop();
        }


        /// <summary>
        /// プロセスが起動を開始した時の処理をします
        /// </summary>
        /// <param name="process">起動を開始するプロセス</param>
        protected virtual void OnProcessStartup(SrProcess process)
        {
            // トレースログくらいは出す
            SrLogger.Trace(SharedString.LogTag.SR_VM_PROCESSOR, $"OnProcessStartup ID={process.ProcessID}");
        }


        /// <summary>
        /// プロセスの実行中に発生した例外を処理します
        /// </summary>
        /// <param name="process">例外が発生したプロセス</param>
        /// <param name="exception">発生した例外</param>
        protected virtual void OnExceptionOccurrenced(SrProcess process, Exception exception)
        {
            // トレースログくらいは出す
            SrLogger.Trace(SharedString.LogTag.SR_VM_PROCESSOR, $"OnExceptionOccurrenced message={exception.Message}, ID={process.ProcessID}");
        }


        /// <summary>
        /// デバッグ時のみ動作するCPUが命令を実行する直前に処理をします
        /// </summary>
        /// <param name="process">処理するプロセス</param>
        /// <param name="instruction">実行しようとしている命令コード</param>
        [Conditional(SharedString.Conditional.DEBUG)]
        protected virtual void OnPreProcessInstruction_Debug(SrProcess process, SrInstruction instruction)
        {
            // トレースログくらいは出す
            SrLogger.Trace(SharedString.LogTag.SR_VM_PROCESSOR, $"OnPreProcessInstruction_Debug OpCode={instruction.OpCode} ID={process.ProcessID}");
        }


        /// <summary>
        /// デバッグ時のみ動作するCPUが命令を実行した直後に処理をします
        /// </summary>
        /// <param name="process">処理するプロセス</param>
        /// <param name="instruction">実行した命令コード</param>
        [Conditional(SharedString.Conditional.DEBUG)]
        protected virtual void OnPostProcessInstruction_Debug(SrProcess process, SrInstruction instruction)
        {
            // トレースログくらいは出す
            SrLogger.Trace(SharedString.LogTag.SR_VM_PROCESSOR, $"OnPostProcessInstruction_Debug OpCode={instruction.OpCode} ID={process.ProcessID}");
        }
    }
}