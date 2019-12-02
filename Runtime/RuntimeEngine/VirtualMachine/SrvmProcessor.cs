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