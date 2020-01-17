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

using System.Collections.Generic;
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
        private readonly Dictionary<string, SrExecutableData> executableDataTable = new Dictionary<string, SrExecutableData>();
        private int nextProcessID = 1;



        internal SrProcess CreateProcess(string path)
        {
            if (!executableDataTable.TryGetValue(path, out var executableData))
            {
                using (var reader = new SrExecutableDataReader(Machine.Storage.Open(path)))
                {
                    executableData = reader.Read();
                    executableDataTable[path] = executableData;
                }
            }


            var codeMemory = CreateCode(executableData);
            var globalMemory = CreateGlobalMemory(executableData);
            var heapMemory = CreateHeapMemory(executableData);
            var stackMemory = CreateStackMemory(executableData);
            var contextMemory = CreateContextMemory(executableData);
            return new SrProcess(nextProcessID++, codeMemory, globalMemory, heapMemory, stackMemory, contextMemory, Machine);
        }


        protected virtual MemoryBlock<SrValue> CreateCode(SrExecutableData data)
        {
            var codes = data.GetInstructionCodes();
            return new MemoryBlock<SrValue>(codes, 0, codes.Length);
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