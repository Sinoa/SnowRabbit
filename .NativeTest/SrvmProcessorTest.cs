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
using NUnit.Framework;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbitTest
{
    /// <summary>
    /// SrvmProcessor クラスに対するテストクラスです
    /// </summary>
    [TestFixture]
    public class SrvmProcessorTest
    {
        // メンバ変数定義
        private TestProcessor processor;
        private SrValue[] rawMemory;
        private MemoryBlock<SrValue> globalMemory;
        private MemoryBlock<SrValue> heapMemory;
        private MemoryBlock<SrValue> stackMemory;
        private MemoryBlock<SrValue> processorContext;



        /// <summary>
        /// テストのセットアップをします
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            // テスト用のプロセッサのインスタンスを生成する
            processor = new TestProcessor();


            // 実行用メモリ領域を確保してメモリブロックを割り当てる
            rawMemory = new SrValue[30 + SrvmProcessor.TotalRegisterCount];
            globalMemory = new MemoryBlock<SrValue>(rawMemory, 0, 10);
            heapMemory = new MemoryBlock<SrValue>(rawMemory, 10, 10);
            stackMemory = new MemoryBlock<SrValue>(rawMemory, 20, 10);
            processorContext = new MemoryBlock<SrValue>(rawMemory, 30, SrvmProcessor.TotalRegisterCount);


            // 生メモリをクリア
            Array.Clear(rawMemory, 0, rawMemory.Length);
        }


        /// <summary>
        /// 指定されたプロセスIDとプログラムコードからプロセスを生成します
        /// </summary>
        /// <param name="processID">プロセスに割り当てるプロセスID</param>
        /// <param name="programCode">プロセスが実行するプログラムコード</param>
        /// <returns>生成されたプロセスを返します</returns>
        private SrProcess CreateProcess(int processID, SrValue[] programCode)
        {
            // メモリをクリアして、事前に割り当てたメモリブロックを使ってプロセスを生成後、コンテキストの初期化
            Array.Clear(rawMemory, 0, rawMemory.Length);
            var programMemory = new MemoryBlock<SrValue>(programCode, 0, programCode.Length);
            var process = new SrProcess(processID, programMemory, globalMemory, heapMemory, stackMemory, processorContext);
            SrvmProcessor.InitializeProcessorContext(process);
            return process;
        }


        public void OpHaltTest()
        {
        }
    }



    /// <summary>
    /// 実行コードを正しく実行しているか確認するためのプロセッサクラスです
    /// </summary>
    public class TestProcessor : SrvmProcessor
    {
    }
}
