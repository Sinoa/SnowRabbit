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
using SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral;

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
        public const byte RegisterR16Index = 16; // FullGeneral Register[R16]
        public const byte RegisterR17Index = 17; // FullGeneral Register[R17]
        public const byte RegisterR18Index = 18; // FullGeneral Register[R18]
        public const byte RegisterR19Index = 19; // FullGeneral Register[R19]
        public const byte RegisterR20Index = 20; // FullGeneral Register[R20]
        public const byte RegisterR21Index = 21; // FullGeneral Register[R21]
        public const byte RegisterR22Index = 22; // FullGeneral Register[R22]
        public const byte RegisterR23Index = 23; // FullGeneral Register[R23]
        public const byte RegisterR24Index = 24; // FullGeneral Register[R24]
        public const byte RegisterR25Index = 25; // FullGeneral Register[R25]
        public const byte RegisterR26Index = 26; // FullGeneral Register[R26]
        public const byte RegisterR27Index = 27; // FullGeneral Register[R27]
        public const byte RegisterR28Index = 28; // FullGeneral Register[R28]
        public const byte RegisterR29Index = 29; // FullGeneral Register[R29]
        public const byte RegisterIPIndex = 30; // InstructionPointer Register[IP]
        public const byte RegisterZeroIndex = 31; // Zero Register[ZERO]
        public const byte TotalRegisterCount = RegisterZeroIndex + 1;



        #region Initialize
        /// <summary>
        /// 対象プロセスのプロセッサコンテキストを初期化します
        /// </summary>
        /// <param name="process">初期化するプロセス</param>
        /// <exception cref="ArgumentNullException">process が null です</exception>
        internal static unsafe void InitializeProcessorContext(SrProcess process)
        {
            // null を渡されたらむり
            if (process == null) throw new ArgumentNullException(nameof(process));


            // 末尾のレジスタインデックスまでループ
            for (var i = 0; i < TotalRegisterCount; ++i)
            {
                // プロセッサコンテキストの値を初期化する
                process.ProcessorContext[i] = default;
            }


            // スタックポインタとベースポインタの位置をプロセスメモリの末尾へ
            // （プッシュ時はデクリメントされたから値がセットされるので、配列の長さそのままで初期化）
            var memoryLength = process.VirtualMemory.StackMemorySize;
            process.ProcessorContext[RegisterSPIndex].Primitive.Long = SrVirtualMemory.StackOffset + memoryLength;
            process.ProcessorContext[RegisterBPIndex].Primitive.Long = SrVirtualMemory.StackOffset + memoryLength;
        }
        #endregion


        #region Event handler
        /// <summary>
        /// プロセスが起動を開始した時の処理をします
        /// </summary>
        /// <param name="process">起動を開始するプロセス</param>
        protected virtual void OnProcessStartup(SrProcess process)
        {
        }


        /// <summary>
        /// プロセスが動作を再開をした時の処理をします
        /// </summary>
        /// <param name="process">動作を再開するプロセス</param>
        protected virtual void OnProcessResume(SrProcess process)
        {
        }


        /// <summary>
        /// プロセスが動作を一時停止した時の処理をします
        /// </summary>
        /// <param name="process">一時停止したプロセス</param>
        protected virtual void OnProcessSuspended(SrProcess process)
        {
        }


        /// <summary>
        /// プロセスが動作を停止した時の処理をします
        /// </summary>
        /// <param name="process">停止したプロセス</param>
        protected virtual void OnProcessStopped(SrProcess process)
        {
        }


        /// <summary>
        /// プロセスが無限ループしていると思われる時の処理をします
        /// </summary>
        /// <param name="process">無限ループしている疑いのあるプロセス</param>
        protected virtual void OnProcessInfinityLoopingTriggered(SrProcess process, out bool isForceStop)
        {
            // 既定動作は停止させない
            isForceStop = false;
        }


        /// <summary>
        /// プロセスの実行中に発生した例外を処理します
        /// </summary>
        /// <param name="process">例外が発生したプロセス</param>
        /// <param name="exception">発生した例外</param>
        protected virtual void OnExceptionOccurrenced(SrProcess process, Exception exception)
        {
        }


        /// <summary>
        /// デバッグ時のみ動作するCPUが命令を実行する直前に処理をします
        /// </summary>
        /// <param name="process">処理するプロセス</param>
        /// <param name="instruction">実行しようとしている命令コード</param>
        [Conditional("DEBUG")]
        protected virtual void OnPreProcessInstruction_Debug(SrProcess process, SrInstruction instruction)
        {
        }


        /// <summary>
        /// デバッグ時のみ動作するCPUが命令を実行した直後に処理をします
        /// </summary>
        /// <param name="process">処理するプロセス</param>
        /// <param name="instruction">実行した命令コード</param>
        [Conditional("DEBUG")]
        protected virtual void OnPostProcessInstruction_Debug(SrProcess process, SrInstruction instruction)
        {
        }


        /// <summary>
        /// デバッグ時のみ動作するCPUが不明な命令を実行した直後に処理をします
        /// </summary>
        /// <param name="process">不明な命令を実行したプロセス</param>
        /// <param name="instruction">不明な命令とされた命令</param>
        [Conditional("DEBUG")]
        protected virtual void OnUnknownInstructionExecution_Debug(SrProcess process, SrInstruction instruction)
        {
        }
        #endregion


        #region Execution main code
        /// <summary>
        /// 指定されたプロセスを実行します
        /// </summary>
        /// <param name="process"></param>
        /// <exception cref="ArgumentNullException">process が null です</exception>
        internal unsafe void Execute(SrProcess process)
        {
            // null を渡されたらむり
            if (process == null) throw new ArgumentNullException(nameof(process));


            // もしプロセスが 停止 または パニックなら
            if ((process.ProcessState & (SrProcessStatus.Stopped | SrProcessStatus.Panic)) != 0)
            {
                // 何もせず終了
                return;
            }
            else if (process.ProcessState == SrProcessStatus.Suspended)
            {
                // プロセスが一時停止中なら、プロセスのタスクの状態を確認して未完了なら
                if (process.Task != null && !process.Task.IsCompleted)
                {
                    // まだプロセスは再開できない
                    return;
                }


                // タスクが完了しているのなら結果を受け取ってタスクや関連情報をクリアしつつ開始状態にする
                process.ProcessorContext[process.ResultReceiveRegisterNumber] = process.PeripheralFunction.GetResult();
                process.PeripheralFunction = null;
                process.Task = null;
                process.ProcessState = SrProcessStatus.Running;


                // 再開イベントを呼ぶ
                OnProcessResume(process);
            }
            else if (process.ProcessState == SrProcessStatus.Ready)
            {
                // プロセスが準備完了状態なら、プロセスの動作開始イベントを実行して開始状態にする
                process.ProcessState = SrProcessStatus.Running;
                OnProcessStartup(process);
            }
            else if (process.ProcessState == SrProcessStatus.ResumeRequested)
            {
                // プロセスが再開要求状態なら、プロセスの動作再開イベントを実行して開始状態にする
                process.ProcessState = SrProcessStatus.Running;
                OnProcessResume(process);
            }


            try
            {
                // プロセスを実行する
                ExecuteCore(process);
            }
            catch (Exception exception)
            {
                // プロセスをパニック状態にして動作計測を停止する
                process.ProcessState = SrProcessStatus.Panic;
                process.RunningStopwatch.Stop();


                // 例外発生イベントを起こしてから再スロー
                OnExceptionOccurrenced(process, exception);
                throw;
            }
        }


        /// <summary>
        /// プロセスを実際に処理する実行関数です
        /// </summary>
        /// <param name="process">実行するプロセス</param>
        /// <exception cref="SrUnknownInstructionException">不明な命令 'Op={instruction.OpCode}' を実行しようとしました</exception>
        private unsafe void ExecuteCore(SrProcess process)
        {
            // プロセスに紐付いている情報をローカル変数に持ってくる
            var context = process.ProcessorContext;
            var memory = process.VirtualMemory;


            // プロセスの動作計測ストップウォッチを開始
            var startElapsedTime = process.RunningStopwatch.ElapsedMilliseconds;
            process.RunningStopwatch.Start();


            // 実行フラグを立てて、降りるまでループ
            var running = true;
            while (running)
            {
                // ゼロレジスタは常にゼロ（null）
                context[RegisterZeroIndex] = default;


                // 現在の命令ポインタが指している命令を取り出して、実行の準備をしてデバッグイベントを呼ぶ
                var instructionPointer = context[RegisterIPIndex].Primitive.Int;
                var nextInstructionPointer = instructionPointer + 1;
                var instruction = memory[instructionPointer].Primitive.Instruction;
                instruction.GetRegisterNumber(out var r1, out var r2, out var r3);
                OnPreProcessInstruction_Debug(process, instruction);


                // 実行する命令ごとに切り替える
                switch (instruction.OpCode)
                {
                    #region CPU Control
                    case OpCode.Halt:
                        running = false;
                        process.ProcessState = SrProcessStatus.Stopped;
                        OnProcessStopped(process);
                        break;
                    #endregion


                    #region Data Transfer
                    case OpCode.Mov:
                        context[r1] = context[r2];
                        break;


                    case OpCode.Movl:
                        context[r1] = instruction.Uint;
                        break;


                    case OpCode.Ldr:
                        var offsetAddress = context[r2].Primitive.Int;
                        context[r1] = memory[offsetAddress + instruction.Int];
                        break;


                    case OpCode.Ldrl:
                        context[r1] = memory[instruction.Int];
                        break;


                    case OpCode.Str:
                        offsetAddress = context[r2].Primitive.Int;
                        memory[offsetAddress + instruction.Int] = context[r1];
                        break;


                    case OpCode.Strl:
                        memory[instruction.Int] = context[r1];
                        break;


                    case OpCode.Push:
                        var sp = context[RegisterSPIndex] - 1;
                        memory[sp] = context[r1];
                        context[RegisterSPIndex] = sp;
                        break;


                    case OpCode.Pushl:
                        sp = context[RegisterSPIndex] - 1;
                        memory[sp] = instruction.Uint;
                        context[RegisterSPIndex] = sp;
                        break;


                    case OpCode.Pop:
                        sp = context[RegisterSPIndex];
                        context[r1] = memory[sp];
                        context[RegisterSPIndex] = sp + 1;
                        break;


                    case OpCode.Fmovl:
                        context[r1] = instruction.Float;
                        break;


                    case OpCode.Fpushl:
                        sp = context[RegisterSPIndex] - 1;
                        memory[sp] = instruction.Float;
                        context[RegisterSPIndex] = sp;
                        break;


                    case OpCode.Movfti:
                        context[r1].Primitive.Long = (long)context[r2].Primitive.Float;
                        break;


                    case OpCode.Movitf:
                        context[r1].Primitive.Float = (float)context[r1].Primitive.Long;
                        break;
                    #endregion


                    #region Arithmetic
                    case OpCode.Add:
                        context[r1].Primitive.Long = context[r2].Primitive.Long + context[r3].Primitive.Long;
                        break;


                    case OpCode.Addl:
                        context[r1].Primitive.Long = context[r2].Primitive.Long + instruction.Int;
                        break;


                    case OpCode.Sub:
                        context[r1].Primitive.Long = context[r2].Primitive.Long - context[r3].Primitive.Long;
                        break;


                    case OpCode.Subl:
                        context[r1].Primitive.Long = context[r2].Primitive.Long - instruction.Int;
                        break;


                    case OpCode.Mul:
                        context[r1].Primitive.Long = context[r2].Primitive.Long * context[r3].Primitive.Long;
                        break;


                    case OpCode.Mull:
                        context[r1].Primitive.Long = context[r2].Primitive.Long * instruction.Int;
                        break;


                    case OpCode.Div:
                        context[r1].Primitive.Long = context[r2].Primitive.Long / context[r3].Primitive.Long;
                        break;


                    case OpCode.Divl:
                        context[r1].Primitive.Long = context[r2].Primitive.Long / instruction.Int;
                        break;


                    case OpCode.Mod:
                        context[r1].Primitive.Long = context[r2].Primitive.Long % context[r3].Primitive.Long;
                        break;


                    case OpCode.Modl:
                        context[r1].Primitive.Long = context[r2].Primitive.Long % instruction.Int;
                        break;


                    case OpCode.Pow:
                        context[r1].Primitive.Long = (long)Math.Pow(context[r2].Primitive.Long, context[r3].Primitive.Long);
                        break;


                    case OpCode.Powl:
                        context[r1].Primitive.Long = (long)Math.Pow(context[r2].Primitive.Long, instruction.Int);
                        break;


                    case OpCode.Neg:
                        context[r1].Primitive.Long = -context[r2].Primitive.Long;
                        break;


                    case OpCode.Negl:
                        context[r1].Primitive.Long = -instruction.Int;
                        break;


                    case OpCode.Fadd:
                        context[r1].Primitive.Float = context[r2].Primitive.Float + context[r3].Primitive.Float;
                        break;


                    case OpCode.Faddl:
                        context[r1].Primitive.Float = context[r2].Primitive.Float + instruction.Float;
                        break;


                    case OpCode.Fsub:
                        context[r1].Primitive.Float = context[r2].Primitive.Float - context[r3].Primitive.Float;
                        break;


                    case OpCode.Fsubl:
                        context[r1].Primitive.Float = context[r2].Primitive.Float - instruction.Float;
                        break;


                    case OpCode.Fmul:
                        context[r1].Primitive.Float = context[r2].Primitive.Float * context[r3].Primitive.Float;
                        break;


                    case OpCode.Fmull:
                        context[r1].Primitive.Float = context[r2].Primitive.Float * instruction.Float;
                        break;


                    case OpCode.Fdiv:
                        context[r1].Primitive.Float = context[r2].Primitive.Float / context[r3].Primitive.Float;
                        break;


                    case OpCode.Fdivl:
                        context[r1].Primitive.Float = context[r2].Primitive.Float / instruction.Float;
                        break;


                    case OpCode.Fmod:
                        context[r1].Primitive.Float = context[r2].Primitive.Float % context[r3].Primitive.Float;
                        break;


                    case OpCode.Fmodl:
                        context[r1].Primitive.Float = context[r2].Primitive.Float % instruction.Float;
                        break;


                    case OpCode.Fpow:
                        context[r1].Primitive.Float = (float)Math.Pow(context[r2].Primitive.Float, context[r3].Primitive.Float);
                        break;


                    case OpCode.Fpowl:
                        context[r1].Primitive.Float = (float)Math.Pow(context[r2].Primitive.Float, instruction.Float);
                        break;


                    case OpCode.Fneg:
                        context[r1].Primitive.Float = -context[r2].Primitive.Float;
                        break;


                    case OpCode.Fnegl:
                        context[r1].Primitive.Float = -instruction.Float;
                        break;
                    #endregion


                    #region Logic
                    case OpCode.Or:
                        context[r1].Primitive.Ulong = context[r2].Primitive.Ulong | context[r3].Primitive.Ulong;
                        break;


                    case OpCode.Xor:
                        context[r1].Primitive.Ulong = context[r2].Primitive.Ulong ^ context[r3].Primitive.Ulong;
                        break;


                    case OpCode.And:
                        context[r1].Primitive.Ulong = context[r2].Primitive.Ulong & context[r3].Primitive.Ulong;
                        break;


                    case OpCode.Not:
                        context[r1].Primitive.Ulong = ~context[r1].Primitive.Ulong;
                        break;


                    case OpCode.Shl:
                        context[r1].Primitive.Long = context[r2].Primitive.Long << (int)(context[r3]);
                        break;


                    case OpCode.Shr:
                        context[r1].Primitive.Long = context[r2].Primitive.Long >> (int)(context[r3]);
                        break;


                    case OpCode.Teq:
                        context[r1].Primitive.Long = context[r2].Primitive.Ulong == context[r3].Primitive.Ulong ? 1L : 0L;
                        break;


                    case OpCode.Tne:
                        context[r1].Primitive.Long = context[r2].Primitive.Ulong != context[r3].Primitive.Ulong ? 1L : 0L;
                        break;


                    case OpCode.Tg:
                        context[r1].Primitive.Long = context[r2].Primitive.Ulong > context[r3].Primitive.Ulong ? 1L : 0L;
                        break;


                    case OpCode.Tge:
                        context[r1].Primitive.Long = context[r2].Primitive.Ulong >= context[r3].Primitive.Ulong ? 1L : 0L;
                        break;


                    case OpCode.Tl:
                        context[r1].Primitive.Long = context[r2].Primitive.Ulong < context[r3].Primitive.Ulong ? 1L : 0L;
                        break;


                    case OpCode.Tle:
                        context[r1].Primitive.Long = context[r2].Primitive.Ulong <= context[r3].Primitive.Ulong ? 1L : 0L;
                        break;


                    case OpCode.Ftg:
                        context[r1].Primitive.Long = context[r2].Primitive.Float > context[r3].Primitive.Float ? 1L : 0L;
                        break;


                    case OpCode.Ftge:
                        context[r1].Primitive.Long = context[r2].Primitive.Float >= context[r3].Primitive.Float ? 1L : 0L;
                        break;


                    case OpCode.Ftl:
                        context[r1].Primitive.Long = context[r2].Primitive.Float < context[r3].Primitive.Float ? 1L : 0L;
                        break;


                    case OpCode.Ftle:
                        context[r1].Primitive.Long = context[r2].Primitive.Float <= context[r3].Primitive.Float ? 1L : 0L;
                        break;


                    case OpCode.Toeq:
                        context[r1].Primitive.Long = context[r2].Object == context[r3].Object ? 1L : 0L;
                        break;


                    case OpCode.Tone:
                        context[r1].Primitive.Long = context[r2].Object != context[r3].Object ? 1L : 0L;
                        break;


                    case OpCode.Tonull:
                        context[r1].Primitive.Long = context[r2].Object == null ? 1L : 0L;
                        break;


                    case OpCode.Tonnull:
                        context[r1].Primitive.Long = context[r2].Object != null ? 1L : 0L;
                        break;
                    #endregion


                    #region Flow Control
                    case OpCode.Br:
                        nextInstructionPointer = context[r1].Primitive.Int + instruction.Int;
                        break;


                    case OpCode.Brl:
                        nextInstructionPointer = instruction.Int;
                        break;


                    case OpCode.Bnz:
                        nextInstructionPointer = context[r2].Primitive.Long != 0 ? context[r1].Primitive.Int + instruction.Int : nextInstructionPointer;
                        break;


                    case OpCode.Bnzl:
                        nextInstructionPointer = context[r2].Primitive.Long != 0 ? instruction.Int : nextInstructionPointer;
                        break;


                    case OpCode.Call:
                        sp = context[RegisterSPIndex] - 1;
                        memory[sp] = nextInstructionPointer;
                        context[RegisterSPIndex] = sp;
                        nextInstructionPointer = context[r1].Primitive.Int + instruction.Int;
                        break;


                    case OpCode.Calll:
                        sp = context[RegisterSPIndex] - 1;
                        memory[sp] = nextInstructionPointer;
                        context[RegisterSPIndex] = sp;
                        nextInstructionPointer = instruction.Int;
                        break;


                    case OpCode.Callnz:
                        if (context[r2].Primitive.Long != 0)
                        {
                            sp = context[RegisterSPIndex] - 1;
                            memory[sp] = nextInstructionPointer;
                            context[RegisterSPIndex] = sp;
                            nextInstructionPointer = context[r1].Primitive.Int + instruction.Int;
                        }
                        break;


                    case OpCode.Callnzl:
                        if (context[r2].Primitive.Long != 0)
                        {
                            sp = context[RegisterSPIndex] - 1;
                            memory[sp] = nextInstructionPointer;
                            context[RegisterSPIndex] = sp;
                            nextInstructionPointer = instruction.Int;
                        }
                        break;


                    case OpCode.Ret:
                        sp = context[RegisterSPIndex];
                        nextInstructionPointer = memory[sp];
                        context[RegisterSPIndex] = sp + 1;
                        break;
                    #endregion


                    #region CSharp Host Control
                    case OpCode.Gpf:
                        var peripheral = Machine.Firmware.GetPeripheral(context[r2].Object as string);
                        context[r1].Object = peripheral.GetPeripheralFunction(context[r3].Object as string);
                        context[r1].Primitive.Ulong = 0;
                        break;


                    case OpCode.Cpf:
                        var function = context[r2].Object as SrPeripheralFunction;
                        var task = function.Call(memory, context[RegisterSPIndex].Primitive.Int, process.ProcessID);
                        if (task.IsCompleted)
                        {
                            context[r1] = function.GetResult();
                        }
                        else
                        {
                            running = false;
                            process.PeripheralFunction = function;
                            process.Task = task;
                            process.ResultReceiveRegisterNumber = r1;
                            process.ProcessState = SrProcessStatus.Suspended;
                            OnProcessSuspended(process);
                        }
                        break;
                    #endregion


                    // オペコードが一致しない命令が来た場合は例外を投げる
                    default:
                        OnUnknownInstructionExecution_Debug(process, instruction);
                        throw new SrUnknownInstructionException($"不明な命令 '0x{instruction.Raw.ToString("X16")}' を実行しようとしました");
                }


                // 最終的な次に実行する命令位置をもどして実行後イベントも呼ぶ
                context[RegisterIPIndex].Primitive.Int = nextInstructionPointer;
                OnPostProcessInstruction_Debug(process, instruction);


                // 現在の単位実行時間を確認して、もし無限ループ経過時間の閾値を超過していたら
                var unitRunningTime = process.RunningStopwatch.ElapsedMilliseconds - startElapsedTime;
                if (unitRunningTime > process.InfinityLoopElapseTimeThreshold)
                {
                    // 無限ループイベントを呼んで強制停止するかどうかも判断を委ねて、停止するなら
                    OnProcessInfinityLoopingTriggered(process, out var isForceStop);
                    if (isForceStop)
                    {
                        // 実行を停止してプロセス実行状態も停止にする
                        running = false;
                        process.ProcessState = SrProcessStatus.Stopped;
                    }
                }
            }


            // プロセスの動作計測ストップウォッチを停止
            process.RunningStopwatch.Stop();
        }
        #endregion
    }
}