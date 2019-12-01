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

namespace SnowRabbit.RuntimeEngine
{
    /// <summary>
    /// SnowRabbit のスクリプト実行単位を表すプロセスクラスです
    /// </summary>
    public class SrProcess : SrDisposable
    {
        // メンバ変数定義
        private readonly int processID;
        private readonly MemoryBlock<SrValue> programCode;
        private readonly MemoryBlock<SrValue> processMemory;
        private readonly MemoryBlock<SrValue> processorContext;



        /// <summary>
        /// SrProcess クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="processID">このプロセスのプロセスID</param>
        /// <param name="programCode">このプロセスが使用するプログラムコードのメモリブロック</param>
        /// <param name="processMemory">このプロセスが使用するメモリのメモリブロック</param>
        /// <param name="processorContext">このプロセスが使用するプロセッサコンテキストのメモリブロック</param>
        public SrProcess(int processID, MemoryBlock<SrValue> programCode, MemoryBlock<SrValue> processMemory, MemoryBlock<SrValue> processorContext)
        {
            // すべて受け取る
            this.processID = processID;
            this.programCode = programCode;
            this.processMemory = processMemory;
            this.processorContext = processorContext;
        }
    }
}
