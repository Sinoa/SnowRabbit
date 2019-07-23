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
using SnowRabbit.Runtime;

namespace SnowRabbit.Machine
{
    partial class SrvmProcessor
    {
        /// <summary>
        /// 指定されたプロセスを実行します
        /// </summary>
        /// <param name="process">実行するプロセスへの参照</param>
        internal unsafe void Execute(ref SrProcess process)
        {
            // プロセスの各種フィールドへの参照を取得しておく
            ref var context = ref process.ProcessorContext;
            ref var memory = ref process.ProcessMemory;
            ref var objMem = ref process.ObjectMemory;


            // 実行フラグを設定してフラグが降りるまでループ
            var execution = true;
            while (execution)
            {
                FetchInstruction(ref process, out var instruction, out var instructionPointer);
                FetchRegisterInfo(ref instruction, out var regANumber, out var regBNumber, out var regCNumber, out var regAType, out var regBType, out var regCType);
                var nextInstructionPointer = instructionPointer + 1;
                var immediate = instruction.Immediate.Int;
                var immediateF = instruction.Immediate.Float;


                switch (instruction.OpCode)
                {
                    #region CPU Control
                    case OpCode.Halt:
                        execution = false;
                        break;
                    #endregion


                    #region Data Transfer
                    case OpCode.Mov:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0];
                        break;


                    case OpCode.Movl:
                        context[regANumber].Value.Long[0] = immediate;
                        break;


                    case OpCode.Ldr:
                        var offsetAddress = (int)context[regBNumber].Value.Long[0];
                        context[regANumber].Value.Long[0] = memory[offsetAddress + immediate].Value.Long[0];
                        break;


                    case OpCode.Ldrl:
                        context[regANumber].Value.Long[0] = memory[immediate].Value.Long[0];
                        break;


                    case OpCode.Str:
                        offsetAddress = (int)context[regBNumber].Value.Long[0];
                        memory[offsetAddress + immediate].Value.Long[0] = context[regANumber].Value.Long[0];
                        break;


                    case OpCode.Strl:
                        memory[immediate].Value.Long[0] = context[regANumber].Value.Long[0];
                        break;


                    case OpCode.Push:
                        var sp = context[RegisterSPIndex].Value.Long[0] - 1;
                        memory[(int)sp].Value.Long[0] = context[regANumber].Value.Long[0];
                        context[RegisterSPIndex].Value.Long[0] = sp;
                        break;


                    case OpCode.Pushl:
                        sp = context[RegisterSPIndex].Value.Long[0] - 1;
                        memory[(int)sp].Value.Long[0] = immediate;
                        context[RegisterSPIndex].Value.Long[0] = sp;
                        break;


                    case OpCode.Pop:
                        sp = context[RegisterSPIndex].Value.Long[0];
                        context[regANumber].Value.Long[0] = memory[(int)sp].Value.Long[0];
                        context[RegisterSPIndex].Value.Long[0] = sp - 1;
                        break;


                    case OpCode.Fmovl:
                        context[regANumber].Value.Float[0] = immediateF;
                        break;


                    case OpCode.Fpushl:
                        sp = context[RegisterSPIndex].Value.Long[0] - 1;
                        memory[(int)sp].Value.Float[0] = immediateF;
                        context[RegisterSPIndex].Value.Long[0] = sp;
                        break;


                    case OpCode.Movfti:
                        context[regANumber].Value.Long[0] = (long)context[regBNumber].Value.Float[0];
                        break;


                    case OpCode.Movitf:
                        context[regANumber].Value.Float[0] = context[regBNumber].Value.Long[0];
                        break;
                    #endregion


                    #region Arithmetic
                    case OpCode.Add:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] + context[regCNumber].Value.Long[0];
                        break;


                    case OpCode.Addl:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] + immediate;
                        break;


                    case OpCode.Sub:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] - context[regCNumber].Value.Long[0];
                        break;


                    case OpCode.Subl:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] - immediate;
                        break;


                    case OpCode.Mul:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] * context[regCNumber].Value.Long[0];
                        break;


                    case OpCode.Mull:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] * immediate;
                        break;


                    case OpCode.Div:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] / context[regCNumber].Value.Long[0];
                        break;


                    case OpCode.Divl:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] / immediate;
                        break;


                    case OpCode.Mod:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] % context[regCNumber].Value.Long[0];
                        break;


                    case OpCode.Modl:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] % immediate;
                        break;


                    case OpCode.Pow:
                        context[regANumber].Value.Long[0] = (long)Math.Pow(context[regBNumber].Value.Long[0], context[regCNumber].Value.Long[0]);
                        break;


                    case OpCode.Powl:
                        context[regANumber].Value.Long[0] = (long)Math.Pow(context[regBNumber].Value.Long[0], immediate);
                        break;


                    case OpCode.Neg:
                        context[regANumber].Value.Long[0] = -context[regBNumber].Value.Long[0];
                        break;


                    case OpCode.Negl:
                        context[regANumber].Value.Long[0] = -immediate;
                        break;


                    case OpCode.Fadd:
                        context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] + context[regCNumber].Value.Float[0];
                        break;


                    case OpCode.Faddl:
                        context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] + immediateF;
                        break;


                    case OpCode.Fsub:
                        context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] - context[regCNumber].Value.Float[0];
                        break;


                    case OpCode.Fsubl:
                        context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] - immediateF;
                        break;


                    case OpCode.Fmul:
                        context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] * context[regCNumber].Value.Float[0];
                        break;


                    case OpCode.Fmull:
                        context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] * immediateF;
                        break;


                    case OpCode.Fdiv:
                        context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] / context[regCNumber].Value.Float[0];
                        break;


                    case OpCode.Fdivl:
                        context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] / immediateF;
                        break;


                    case OpCode.Fmod:
                        context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] % context[regCNumber].Value.Float[0];
                        break;


                    case OpCode.Fmodl:
                        context[regANumber].Value.Float[0] = context[regBNumber].Value.Float[0] % immediateF;
                        break;


                    case OpCode.Fpow:
                        context[regANumber].Value.Float[0] = (float)Math.Pow(context[regBNumber].Value.Float[0], context[regCNumber].Value.Float[0]);
                        break;


                    case OpCode.Fpowl:
                        context[regANumber].Value.Float[0] = (float)Math.Pow(context[regBNumber].Value.Float[0], immediateF);
                        break;


                    case OpCode.Fneg:
                        context[regANumber].Value.Float[0] = -context[regBNumber].Value.Float[0];
                        break;


                    case OpCode.Fnegl:
                        context[regANumber].Value.Float[0] = -immediateF;
                        break;
                    #endregion


                    #region Logic
                    case OpCode.Or:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] | context[regCNumber].Value.Long[0];
                        break;


                    case OpCode.Xor:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] ^ context[regCNumber].Value.Long[0];
                        break;


                    case OpCode.And:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] & context[regCNumber].Value.Long[0];
                        break;


                    case OpCode.Not:
                        context[regANumber].Value.Long[0] = ~context[regANumber].Value.Long[0];
                        break;


                    case OpCode.Shl:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] << (int)(context[regCNumber].Value.Long[0]);
                        break;


                    case OpCode.Shr:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] >> (int)(context[regCNumber].Value.Long[0]);
                        break;


                    case OpCode.Teq:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] == context[regCNumber].Value.Long[0] ? 1L : 0L;
                        break;


                    case OpCode.Tne:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] != context[regCNumber].Value.Long[0] ? 1L : 0L;
                        break;


                    case OpCode.Tg:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] > context[regCNumber].Value.Long[0] ? 1L : 0L;
                        break;


                    case OpCode.Tge:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] >= context[regCNumber].Value.Long[0] ? 1L : 0L;
                        break;


                    case OpCode.Tl:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] < context[regCNumber].Value.Long[0] ? 1L : 0L;
                        break;


                    case OpCode.Tle:
                        context[regANumber].Value.Long[0] = context[regBNumber].Value.Long[0] <= context[regCNumber].Value.Long[0] ? 1L : 0L;
                        break;
                    #endregion


                    #region Flow Control
                    case OpCode.Br:
                        nextInstructionPointer = context[regANumber].Value.Long[0] + immediate;
                        break;


                    case OpCode.Brl:
                        nextInstructionPointer = immediate;
                        break;


                    case OpCode.Bnz:
                        nextInstructionPointer = context[regBNumber].Value.Long[0] != 0 ? context[regANumber].Value.Long[0] + immediate : nextInstructionPointer;
                        break;


                    case OpCode.Bnzl:
                        nextInstructionPointer = context[regBNumber].Value.Long[0] != 0 ? immediate : nextInstructionPointer;
                        break;


                    case OpCode.Call:
                        sp = context[RegisterSPIndex].Value.Long[0] - 1;
                        memory[(int)sp].Value.Long[0] = nextInstructionPointer;
                        context[RegisterSPIndex].Value.Long[0] = sp;
                        nextInstructionPointer = context[regANumber].Value.Long[0] + immediate;
                        break;


                    case OpCode.Calll:
                        sp = context[RegisterSPIndex].Value.Long[0] - 1;
                        memory[(int)sp].Value.Long[0] = nextInstructionPointer;
                        context[RegisterSPIndex].Value.Long[0] = sp;
                        nextInstructionPointer = immediate;
                        break;


                    case OpCode.Callnz:
                        if (context[regBNumber].Value.Long[0] != 0)
                        {
                            sp = context[RegisterSPIndex].Value.Long[0] - 1;
                            memory[(int)sp].Value.Long[0] = nextInstructionPointer;
                            context[RegisterSPIndex].Value.Long[0] = sp;
                            nextInstructionPointer = context[regANumber].Value.Long[0] + immediate;
                        }
                        break;


                    case OpCode.Callnzl:
                        if (context[regBNumber].Value.Long[0] != 0)
                        {
                            sp = context[RegisterSPIndex].Value.Long[0] - 1;
                            memory[(int)sp].Value.Long[0] = nextInstructionPointer;
                            context[RegisterSPIndex].Value.Long[0] = sp;
                            nextInstructionPointer = immediate;
                        }
                        break;


                    case OpCode.Ret:
                        sp = context[RegisterSPIndex].Value.Long[0];
                        nextInstructionPointer = memory[(int)sp].Value.Long[0];
                        context[RegisterSPIndex].Value.Long[0] = sp - 1;
                        break;
                    #endregion


                    #region CSharp Host Control
                    case OpCode.Cpf:
                        var peripheral = Machine.Firmware.GetPeripheral((int)context[regANumber].Value.Long[0]);
                        var function = peripheral.GetFunction((int)context[regBNumber].Value.Long[0]);
                        var stackFrame = memory.Slice((int)context[RegisterSPIndex].Value.Long[0], (int)context[regCNumber].Value.Long[0]);
                        function(new SrStackFrame(stackFrame, process.ObjectMemory));
                        break;


                    case OpCode.Cpfl:
                        peripheral = Machine.Firmware.GetPeripheral((int)context[regANumber].Value.Long[0]);
                        function = peripheral.GetFunction((int)context[regBNumber].Value.Long[0]);
                        stackFrame = memory.Slice((int)context[RegisterSPIndex].Value.Long[0], immediate);
                        function(new SrStackFrame(stackFrame, process.ObjectMemory));
                        break;


                    case OpCode.Gpid:
                        context[regANumber].Value.Long[0] = Machine.Firmware.GetPeripheralID((string)process.ObjectMemory[(int)context[regBNumber].Value.Long[0]].Value);
                        break;


                    case OpCode.Gpidl:
                        context[regANumber].Value.Long[0] = Machine.Firmware.GetPeripheralID((string)process.ObjectMemory[immediate].Value);
                        break;


                    case OpCode.Gpfid:
                        peripheral = Machine.Firmware.GetPeripheral((int)context[regBNumber].Value.Long[0]);
                        context[regANumber].Value.Long[0] = peripheral.GetFunctionID((string)process.ObjectMemory[(int)context[regCNumber].Value.Long[0]].Value);
                        break;


                    case OpCode.Gpfidl:
                        peripheral = Machine.Firmware.GetPeripheral((int)context[regBNumber].Value.Long[0]);
                        context[regANumber].Value.Long[0] = peripheral.GetFunctionID((string)process.ObjectMemory[immediate].Value);
                        break;
                    #endregion


                    #region Undefined
                    default: throw new NotImplementedException($"Unknown machine code {(byte)instruction.OpCode}");
                        #endregion
                }


                UpdateInstructionPointer(ref process, nextInstructionPointer);
            }
        }
    }
}