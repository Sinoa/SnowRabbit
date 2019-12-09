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

using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace SnowRabbit.RuntimeEngine
{
    /// <summary>
    /// SnowRabbit のスクリプト実行単位を表すプロセスクラスです
    /// </summary>
    public class SrProcess : SrDisposable
    {
        // メンバ変数定義
        internal readonly MemoryBlock<SrValue> ProcessorContext;
        internal readonly SrVirtualMemory VirtualMemory;
        internal readonly Stopwatch RunningStopwatch;
        internal ExceptionDispatchInfo ExceptionDispatchInfo;



        /// <summary>
        /// 現在のプロセスID
        /// </summary>
        public int ProcessID { get; internal set; }


        /// <summary>
        /// プロセスの動作状態を取得します
        /// </summary>
        public SrProcessStatus ProcessState { get; internal set; }


        /// <summary>
        /// 例外発生時の動作モードを取得設定します
        /// </summary>
        public UnhandledExceptionMode UnhandledExceptionMode { get; set; }



        /// <summary>
        /// SrProcess クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="processID">このプロセスのプロセスID</param>
        /// <param name="programCode">このプロセスが使用するプログラムコードのメモリブロック</param>
        /// <param name="processMemory">このプロセスが使用するメモリのメモリブロック</param>
        /// <param name="processorContext">このプロセスが使用するプロセッサコンテキストのメモリブロック</param>
        internal SrProcess(int processID, MemoryBlock<SrValue> programCode, MemoryBlock<SrValue> processMemory, MemoryBlock<SrValue> processorContext)
        {
            // すべて受け取る
            ProcessID = processID;
            VirtualMemory = new SrVirtualMemory(programCode, processMemory);
            ProcessorContext = processorContext;


            // 他初期化処理
            ProcessState = SrProcessStatus.Ready;
            RunningStopwatch = new Stopwatch();
            UnhandledExceptionMode = UnhandledExceptionMode.CatchException;
        }


        /// <summary>
        /// 一時停止しているプロセスを再開します。ただし、停止したプロセスは呼び出しが無視されます。
        /// さらに UnhandledExceptionMode の CatchException によってキャッチされた例外があった場合は、例外が破棄されることに注意してください。
        /// </summary>
        public void Resume()
        {
            // プロセスが 一時停止中 または パニック 以外なら何もしない
            var stateMask = SrProcessStatus.Suspended | SrProcessStatus.Panic;
            if ((ProcessState & stateMask) == 0) return;


            // プロセスの状態を再開要求にして例外発行情報もクリア
            ProcessState = SrProcessStatus.ResumeRequested;
            ExceptionDispatchInfo = null;
        }


        /// <summary>
        /// プロセスの実行中に発生した例外で UnhandledExceptionMode にて CatchException の場合にキャッチした例外をスローします。
        /// 何も例外が発生していないか ThrowException モードの時に発生した例外は何もしません。また、パニック状態になったプロセスは再開要求状態になります。
        /// </summary>
        public void ResumeAndThrowIfOccurrencedException()
        {
            // プロセスが停止中 または 未起動 または 動作中 または 例外自体発生していないなら何もせず終了
            if ((ProcessState & (SrProcessStatus.Stopped | SrProcessStatus.Ready | SrProcessStatus.Running)) != 0 || ExceptionDispatchInfo == null) return;


            // プロセスの状態を再開要求状態にして例外発行情報をスタック変数に参照を移す
            ProcessState = SrProcessStatus.ResumeRequested;
            var dispatchInfo = ExceptionDispatchInfo;
            ExceptionDispatchInfo = null;


            // 例外発行情報の例外をスローする
            dispatchInfo.Throw();
        }
    }
}
