// zlib/libpng License
//
// Copyright(c) 2020 Sinoa
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

using System.IO;
using System.Threading.Tasks;
using SnowRabbit.Compiler;
using SnowRabbit.Compiler.Assembler;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;
using SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral;

namespace SampleApplication
{
    internal class ApplicationMain
    {
        private static void Main()
        {
            var outStream = new FileStream("sample.bin", FileMode.Create);
            var compiler = new SrCompiler();
            compiler.IsContainSymbolInfo = true;
            compiler.Compile("Sample.srs", outStream);
            outStream.Dispose();


            //var vm = new SrvmMachine(new MyFactory());
            //var process = vm.CreateProcess("sample.bin");
            //while (process.ProcessState != SrProcessStatus.Stopped)
            //{
            //    process.Run();
            //}
            //process.Dispose();



            var disassembleDumpText = new SrDisassembler().Disassemble(new FileStream("sample.bin", FileMode.Open));
            File.WriteAllText("dump.txt", disassembleDumpText);
        }
    }


    public class MyFactory : SrvmDefaultMachinePartsFactory
    {
        public override SrvmFirmware CreateFirmware()
        {
            var firmWare = base.CreateFirmware();
            firmWare.AttachPeripheral(new SamplePeripheral());
            return firmWare;
        }
    }


    [SrPeripheral("Sample")]
    public class SamplePeripheral
    {
        [SrHostFunction("MyAdd")]
        public string MyAdd(int a, int b)
        {
            return (a + b).ToString();
        }


        [SrHostFunction("Write")]
        public void Write(string text)
        {
            System.Console.WriteLine(text);
        }


        [SrHostFunction("Wait")]
        public Task Wait(int second)
        {
            return Task.Delay(second);
        }


        [SrHostFunction("Check")]
        public void ArgumentCheck(int a, int b, int c, [SrProcessID] int d)
        {
            return;
        }
    }
}
