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
using System.Text;
using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.IO;
using SnowRabbit.RuntimeEngine.Data;

namespace SnowRabbit.Compiler.Assembler
{
    public class SrDisassembler
    {
        private static readonly string[] RegisterNameList = new string[] { "rax", "rbx", "rcx", "rdx", "rsi", "rdi", "rbp", "rsp", "r8", "r9", "r10", "r11", "r12", "r13", "r14", "r15", "ip" };



        public string Disassemble(Stream stream)
        {
            var reader = new SrExecutableDataReader(stream);
            var executableData = reader.Read();
            reader.Dispose();


            var buffer = new StringBuilder();
            DumpSymbols(executableData, buffer);
            DumpStringTable(executableData, buffer);


            return buffer.ToString();
        }


        private void DumpSymbols(SrExecutableData data, StringBuilder buffer)
        {
            var symbols = data.GetSymbols();
            if (symbols.Length == 0) return;


            buffer.Append($"========== SymbolInfo ==========\n");
            buffer.Append($"TotalSymbolCount : {symbols.Length} (peripheral and constant is not dumped.)\n");
            buffer.Append($"\n");
            buffer.Append($"| Address    | Scope   | Type           | Name\n");
            foreach (var symbol in symbols)
            {
                if (symbol.Kind == SrSymbolKind.PeripheralFunction || symbol.Kind == SrSymbolKind.Constant) continue;


                var addressText = symbol.Address.ToString("X8");
                var scopeText = symbol.Scope.ToString().PadRight(8);
                var kindText = symbol.Kind.ToString().PadRight(15);


                buffer.Append($"| 0x{addressText} ");
                buffer.Append($"| {scopeText}");
                buffer.Append($"| {kindText}");
                buffer.Append($"| {(symbol.Kind == SrSymbolKind.String ? data.GetStringFromAddress(symbol.Address) : symbol.Name)}\n");
            }


            buffer.Append("\n\n\n");
        }


        private void DumpStringTable(SrExecutableData data, StringBuilder buffer)
        {
            var stringCount = data.StringRecordCount;
            if (stringCount == 0) return;


            buffer.Append($"========== StringTableInfo ==========\n");
            buffer.Append($"TotalStringCount : {stringCount}\n");
            buffer.Append($"\n");
            buffer.Append($"| Address    | Text\n");
            for (int i = 0; i < stringCount; ++i)
            {
                var stringData = data.GetString(i);
                var addressText = stringData.Address.ToString("X8");
                buffer.Append($"| 0x{addressText} | {stringData.String}\n");
            }


            buffer.Append("\n\n\n");
        }
    }
}
