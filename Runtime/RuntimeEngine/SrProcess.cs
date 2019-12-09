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



        /// <summary>
        /// 現在のプロセスID
        /// </summary>
        public int ProcessID { get; internal set; }


        /// <summary>
        /// プロセスの動作状態を取得します
        /// </summary>
        public SrProcessStatus ProcessState { get; internal set; }



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
        }
    }
}
