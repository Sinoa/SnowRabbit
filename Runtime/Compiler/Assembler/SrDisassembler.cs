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

using System.Collections.Generic;
using System.IO;
using System.Text;
using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.IO;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.Data;

namespace SnowRabbit.Compiler.Assembler
{
    public class SrDisassembler
    {
        private static readonly string[] RegisterNameList = new string[] { "rax", "rbx", "rcx", "rdx", "rsi", "rdi", "rbp", "rsp", " r8", " r9", "r10", "r11", "r12", "r13", "r14", "r15", "r16", "r17", "r18", "r19", "r20", "r21", "r22", "r23", "r24", "r25", "r26", "r27", "r28", "r29", " ip", "zero" };
        private readonly Dictionary<int, string> labelTable = new Dictionary<int, string>();
        private readonly Dictionary<int, string> symbolTable = new Dictionary<int, string>();



        public string Disassemble(Stream stream)
        {
            var reader = new SrExecutableDataReader(stream);
            var executableData = reader.Read();
            reader.Dispose();


            var buffer = new StringBuilder();
            DumpSymbols(executableData, buffer);
            DumpStringTable(executableData, buffer);
            DumpProgramCode(executableData, buffer);


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


                if ((symbol.Kind == SrSymbolKind.Label || symbol.Kind == SrSymbolKind.ScriptFunction) && symbol.Name != "______init_LeaveLable___")
                {
                    labelTable[symbol.Address] = symbol.Name;
                }


                if (symbol.Name != "______init_LeaveLable___")
                {
                    symbolTable[symbol.Address] =
                        symbol.Kind == SrSymbolKind.String ?
                            $"string[0x{symbol.Address.ToString("X8")}({data.GetStringFromAddress(symbol.Address)})]" :
                            $"{symbol.Kind.ToString().ToLower()}[0x{symbol.Address.ToString("X8")}({symbol.Name})]";
                }
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


        private void DumpProgramCode(SrExecutableData data, StringBuilder buffer)
        {
            var codes = data.GetInstructionCodes();
            buffer.Append($"========== ProgramCode ==========\n");
            buffer.Append($"Length : {codes.Length}\n");
            buffer.Append($"\n");
            for (int i = 0; i < codes.Length; ++i)
            {
                var instruction = codes[i].Primitive.Instruction;


                var addressText = i.ToString("X8");
                var opCodeText = instruction.OpCode.ToString().ToLower().PadRight(8);
                instruction.GetRegisterNumber(out var r1, out var r2, out var r3);
                var operand1 = RegisterNameList[r1];
                var operand2 = RegisterNameList[r2];
                var operand3 = RegisterNameList[r3];
                var immediate = symbolTable.ContainsKey(instruction.Int) ? symbolTable[instruction.Int] : instruction.Int.ToString();


                if (labelTable.ContainsKey(i))
                {
                    buffer.Append($"{labelTable[i]}:\n");
                }


                buffer.Append(CodeToString(instruction.OpCode, addressText, opCodeText, operand1, operand2, operand3, immediate));
            }


            buffer.Append("\n\n\n");
        }


        private string CodeToString(OpCode opCode, string addressText, string opCodeText, string operand1, string operand2, string operand3, string immediate)
        {
            switch (opCode)
            {
                case OpCode.Halt: return $"    0x{addressText}  {opCodeText}\n";
                case OpCode.Mov: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}\n";
                case OpCode.Movl: return $"    0x{addressText}  {opCodeText}{operand1}, {immediate}\n";
                case OpCode.Ldr: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Ldrl: return $"    0x{addressText}  {opCodeText}{operand1}, {immediate}\n";
                case OpCode.Str: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Strl: return $"    0x{addressText}  {opCodeText}{operand1}, {immediate}\n";
                case OpCode.Push: return $"    0x{addressText}  {opCodeText}{operand1}\n";
                case OpCode.Pushl: return $"    0x{addressText}  {opCodeText}{immediate}\n";
                case OpCode.Pop: return $"    0x{addressText}  {opCodeText}{operand1}\n";
                case OpCode.Fmovl: return $"    0x{addressText}  {opCodeText}{immediate}\n";
                case OpCode.Fpushl: return $"    0x{addressText}  {opCodeText}{immediate}\n";
                case OpCode.Movfti: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}\n";
                case OpCode.Movitf: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}\n";
                case OpCode.Add: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Addl: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Sub: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Subl: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Mul: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Mull: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Div: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Divl: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Mod: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Modl: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Pow: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Powl: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Neg: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}\n";
                case OpCode.Negl: return $"    0x{addressText}  {opCodeText}{operand1}, {immediate}\n";
                case OpCode.Fadd: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Faddl: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Fsub: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Fsubl: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Fmul: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Fmull: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Fdiv: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Fdivl: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Fmod: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Fmodl: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Fpow: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Fpowl: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Fneg: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}\n";
                case OpCode.Fnegl: return $"    0x{addressText}  {opCodeText}{operand1}, {immediate}\n";
                case OpCode.Or: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Xor: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.And: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Not: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}\n";
                case OpCode.Shl: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Shr: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Teq: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Tne: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Tg: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Tge: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Tl: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Tle: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Toeq: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Tone: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Tonull: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}\n";
                case OpCode.Tonnull: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}\n";
                case OpCode.Br: return $"    0x{addressText}  {opCodeText}{operand1}, {immediate}\n";
                case OpCode.Brl: return $"    0x{addressText}  {opCodeText}{immediate}\n";
                case OpCode.Bnz: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Bnzl: return $"    0x{addressText}  {opCodeText}xxx, {operand2}, {immediate}\n";
                case OpCode.Call: return $"    0x{addressText}  {opCodeText}{operand1}, {immediate}\n";
                case OpCode.Calll: return $"    0x{addressText}  {opCodeText}{immediate}\n";
                case OpCode.Callnz: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {immediate}\n";
                case OpCode.Callnzl: return $"    0x{addressText}  {opCodeText}xxx, {operand2}, {immediate}\n";
                case OpCode.Ret: return $"    0x{addressText}  {opCodeText}\n";
                case OpCode.Gpf: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
                case OpCode.Cpf: return $"    0x{addressText}  {opCodeText}{operand1}, {operand2}, {operand3}\n";
            }


            return $"    0x{addressText}  {opCodeText}\n";
        }
    }
}
