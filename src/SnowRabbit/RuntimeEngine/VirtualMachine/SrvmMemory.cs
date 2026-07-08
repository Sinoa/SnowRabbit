// zlib License
// 
// Copyright (c) 2019 Sinoa
// 
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
// 
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software
// in a product, an acknowledgment in the product documentation would be
// appreciated but is not required.
// 
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
// 
// 3. This notice may not be removed or altered from any source
// distribution.

#nullable disable

using System;
using System.Threading;
using SnowRabbit.IO;
using SnowRabbit.RuntimeEngine.Data;

namespace SnowRabbit.RuntimeEngine.VirtualMachine
{
    /// <summary>
    /// SnowRabbit が実装する仮想マシンメモリクラスです
    /// </summary>
    public class SrvmMemory : SrvmMachineParts
    {
        // 以下メンバ変数定義
        private int nextProcessID = 0;



        internal SrProcess CreateProcess(string path)
        {
            var dataStream = Machine.Storage.Open(path) ?? throw new ExecutableDataNotFoundException(null, path);
            SrExecutableData executableData;
            using (var reader = new SrExecutableDataReader(dataStream))
            {
                // 読み取りが失敗してもリーダー（および元ストリーム）が確実に破棄されるようにする
                executableData = reader.Read();
            }


            var codeMemory = CreateCodeMemory(executableData);
            var globalMemory = CreateGlobalMemory(executableData);
            var heapMemory = CreateHeapMemory(executableData);
            var stackMemory = CreateStackMemory(executableData);
            var contextMemory = CreateContextMemory(executableData);
            // プロセスIDの採番は複数スレッドからの生成でも重複しないようにアトミックに行う
            var process = new SrProcess(Interlocked.Increment(ref nextProcessID), codeMemory, globalMemory, heapMemory, stackMemory, contextMemory, Machine);
            SrvmProcessor.InitializeProcessorContext(process);
            return process;
        }


        protected virtual MemoryBlock<SrValue> CreateCodeMemory(SrExecutableData data)
        {
            var instructionCodes = data.GetInstructionCodes();
            var codeMemory = new SrValue[instructionCodes.Length + data.StringRecordCount];
            Array.Copy(instructionCodes, 0, codeMemory, 0, instructionCodes.Length);
            for (int i = 0; i < data.StringRecordCount; ++i)
            {
                // 文字列レコードのアドレスは命令領域の直後の文字列領域を指していなければならない
                var record = data.GetString(i);
                if (record.Address < instructionCodes.Length || record.Address >= codeMemory.Length)
                {
                    throw new SrMalformedExecutableDataException($"文字列レコードのアドレス '{record.Address}' が不正です");
                }
                codeMemory[record.Address].Object = record.String;
            }


            return new MemoryBlock<SrValue>(codeMemory, 0, codeMemory.Length);
        }


        protected virtual MemoryBlock<SrValue> CreateGlobalMemory(SrExecutableData data)
        {
            return new MemoryBlock<SrValue>(new SrValue[512], 0, 512);
        }


        protected virtual MemoryBlock<SrValue> CreateHeapMemory(SrExecutableData data)
        {
            return new MemoryBlock<SrValue>(new SrValue[512], 0, 512);
        }


        protected virtual MemoryBlock<SrValue> CreateStackMemory(SrExecutableData data)
        {
            return new MemoryBlock<SrValue>(new SrValue[512], 0, 512);
        }


        protected virtual MemoryBlock<SrValue> CreateContextMemory(SrExecutableData data)
        {
            return new MemoryBlock<SrValue>(new SrValue[SrvmProcessor.TotalRegisterCount], 0, SrvmProcessor.TotalRegisterCount);
        }
    }
}