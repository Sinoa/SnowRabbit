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
using System.Collections.Generic;
using System.IO;
using SnowRabbit.Machine;
using SnowRabbit.Runtime;

namespace CarrotCompilerCollection.Compiler
{
    /// <summary>
    /// SnowRabbit 仮想マシン向け実行コードを生成するコーダークラスです
    /// </summary>
    internal class CccBinaryCoder
    {
        // メンバ変数定義
        private SrBinaryIO binaryIO;
        private Dictionary<string, FunctionInfo> functionTable;
        private Dictionary<string, VariableInfo> variableTable;
        private Dictionary<string, ConstantInfo> constantTable;
        private Dictionary<int, SymbolInfo> symbolTable;
        private int nextVirtualAddress;



        /// <summary>
        /// CccBinaryCoder クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="outputStream">出力ストリーム</param>
        /// <exception cref="ArgumentNullException">outputStream が null です</exception>
        public CccBinaryCoder(Stream outputStream)
        {
            // SnowRabbit向けバイナリIOのインスタンスを生成する
            binaryIO = new SrBinaryIO(outputStream ?? throw new ArgumentNullException(nameof(outputStream)));
            functionTable = new Dictionary<string, FunctionInfo>();
            variableTable = new Dictionary<string, VariableInfo>();
            constantTable = new Dictionary<string, ConstantInfo>();
            symbolTable = new Dictionary<int, SymbolInfo>();
            nextVirtualAddress = -1;
        }


        /// <summary>
        /// 現在の状態から実行コードを出力します
        /// </summary>
        public void OutputExecuteCode()
        {
        }


        #region code generate function
        public int GenerateJumpTest(FunctionInfo function, int endLabelIndex)
        {
            // movl ra = r8, imm = 0
            // Teq ra = rax, rb = rax, rc = r8
            // Bnzl rb = rax, imm = endlabel
            function.CreateInstruction(OpCode.Movl, SrvmProcessor.RegisterR8Index, 0, 0, 0, false);
            function.CreateInstruction(OpCode.Teq, SrvmProcessor.RegisterAIndex, SrvmProcessor.RegisterAIndex, SrvmProcessor.RegisterR8Index, 0, false);
            function.CreateInstruction(OpCode.Bnzl, 0, SrvmProcessor.RegisterAIndex, 0, endLabelIndex, true);
            return function.InstructionInfoList.Count - 1;
        }


        public void UpdateJumpAddress(FunctionInfo function, int instructionIndex, int jumpIndex)
        {
            function.InstructionInfoList[instructionIndex].Code.Immediate.Int = jumpIndex;
            function.InstructionInfoList[instructionIndex].UnresolveAddress = false;
        }


        public void GenerateFunctionEnter(FunctionInfo function, int argumentCount)
        {
            // push ra = rbp
            // mov ra = rbp, rb = rsp
            // subl ra = rsp, rb = rsp, imm = argumentCount
            var rsp = (byte)SrvmProcessor.RegisterSPIndex;
            var rbp = (byte)SrvmProcessor.RegisterBPIndex;
            function.CreateInstruction(OpCode.Push, rbp, 0, 0, 0, false);
            function.CreateInstruction(OpCode.Mov, rbp, rsp, 0, 0, false);
            function.CreateInstruction(OpCode.Subl, rsp, rsp, 0, argumentCount, false);
        }


        public void GenerateFunctionLeave(FunctionInfo function)
        {
            // mov ra = rsp, rb = rbp
            // pop ra = rbp
            // ret
            var rsp = (byte)SrvmProcessor.RegisterSPIndex;
            var rbp = (byte)SrvmProcessor.RegisterBPIndex;
            function.CreateInstruction(OpCode.Mov, rsp, rbp, 0, 0, false);
            function.CreateInstruction(OpCode.Pop, rbp, 0, 0, 0, false);
            function.CreateInstruction(OpCode.Ret, 0, 0, 0, 0, false);
        }


        public void GeneratePushRax(FunctionInfo function)
        {
            function.CreateInstruction(OpCode.Push, SrvmProcessor.RegisterAIndex, 0, 0, 0, false);
        }


        public void GenerateStackPointerAdd(FunctionInfo function, int value)
        {
            // addl ra = sp, rb = sp, imm = value
            var sp = (byte)SrvmProcessor.RegisterSPIndex;
            function.CreateInstruction(OpCode.Addl, sp, sp, 0, value, false);
        }


        public void GeneratePeripheralFunctionCall(FunctionInfo function, string functionName)
        {
            var peripheralFunction = GetFunction(functionName);
            if (peripheralFunction == null)
            {
                return;
            }


            var peripheralVarName = ManglingPeripheralVariableName(peripheralFunction.PeripheralName);
            var peripheralFunctionVarName = ManglingPeripheralFunctionVariableName(peripheralFunction.PeripheralFunctionName);
            var peripheralVarAddress = GetVariable(peripheralVarName).Address;
            var peripheralFunctionVarAddress = GetVariable(peripheralFunctionVarName).Address;
            var argumentCount = peripheralFunction.ArgumentTable.Count;


            // ldrl ra = r8, imm = peripheralID
            // ldrl ra = r9, imm = peripheralFunctionID
            // movl ra = r10, imm = argumentCount
            // cpf ra = r8, rb = r9, rc = r10
            var r8 = (byte)SrvmProcessor.RegisterR8Index;
            var r9 = (byte)SrvmProcessor.RegisterR9Index;
            var r10 = (byte)SrvmProcessor.RegisterR10Index;
            function.CreateInstruction(OpCode.Ldrl, r8, 0, 0, peripheralVarAddress, true);
            function.CreateInstruction(OpCode.Ldrl, r9, 0, 0, peripheralFunctionVarAddress, true);
            function.CreateInstruction(OpCode.Movl, r10, 0, 0, argumentCount, false);
            function.CreateInstruction(OpCode.Cpf, r8, r9, r10, 0, false);
        }


        public void GenerateFunctionCall(FunctionInfo function, string functionName)
        {
            var targetFunction = GetFunction(functionName);

            // calll imm = targetFunctionAddress
            function.CreateInstruction(OpCode.Calll, 0, 0, 0, targetFunction.Address, true);
        }
        #endregion


        #region constant and function and variable control
        public string ManglingPeripheralVariableName(string peripheralName)
        {
            return $"___CCC_GPVN_{peripheralName}___";
        }


        public string ManglingPeripheralFunctionVariableName(string functionName)
        {
            return $"___CCC_GPFVN_{functionName}___";
        }


        public void RegisterConstantValue(string name, CccType type, long integer, float number, string text)
        {
            var info = new ConstantInfo();
            info.Name = name;
            info.Type = type;
            info.IntegerValue = integer;
            info.NumberValue = number;
            info.TextValue = text;


            constantTable[name] = info;
        }


        public ConstantInfo GetConstant(string name)
        {
            return constantTable.TryGetValue(name, out var constant) ? constant : null;
        }


        public bool ContainConstant(string name)
        {
            return constantTable.ContainsKey(name);
        }


        public void RegisterPeripheralFunction(string functionName, int returnType, IList<int> argumentList, string peripheralName, string peripheralFunctionName)
        {
            var info = new FunctionInfo();
            info.Name = functionName;
            info.PeripheralName = peripheralName;
            info.PeripheralFunctionName = peripheralFunctionName;
            info.Address = nextVirtualAddress--;
            info.Unresolve = false;
            info.Type = FunctionType.Peripheral;
            info.ReturnType =
                returnType == CccTokenKind.TypeInt ? CccType.Int :
                returnType == CccTokenKind.TypeNumber ? CccType.Number :
                returnType == CccTokenKind.TypeString ? CccType.String :
                CccType.Void;


            for (int i = 0; i < argumentList.Count; ++i)
            {
                var argumentInfo = new CccArgumentInfo();
                argumentInfo.Index = i;
                argumentInfo.Name = i.ToString();
                argumentInfo.Type =
                    argumentList[i] == CccTokenKind.TypeInt ? CccType.Int :
                    returnType == CccTokenKind.TypeNumber ? CccType.Number :
                    CccType.String;


                info.ArgumentTable[argumentInfo.Name] = argumentInfo;
            }


            functionTable[functionName] = info;
            symbolTable[info.Address] = new SymbolInfo()
            {
                Type = SymbolType.GlobalFunction,
                Function = info,
                Name = functionName,
                VirtualAddress = info.Address,
            };


            var peripheralVariableName = ManglingPeripheralVariableName(peripheralName);
            var peripheralFunctionVariableName = ManglingPeripheralFunctionVariableName(peripheralFunctionName);
            if (!variableTable.ContainsKey(peripheralVariableName))
            {
                RegisterVariable(peripheralVariableName, CccTokenKind.TypeInt);
            }
            RegisterVariable(peripheralFunctionVariableName, CccTokenKind.TypeInt);
        }


        public void RegisterFunction(string functionName, int returnType, List<CccArgumentInfo> argumentList)
        {
            var info = new FunctionInfo();
            info.Name = functionName;
            info.PeripheralName = string.Empty;
            info.PeripheralFunctionName = string.Empty;
            info.Address = nextVirtualAddress--;
            info.Unresolve = true;
            info.Type = FunctionType.Standard;
            info.ReturnType =
                returnType == CccTokenKind.TypeInt ? CccType.Int :
                returnType == CccTokenKind.TypeNumber ? CccType.Number :
                returnType == CccTokenKind.TypeString ? CccType.String :
                CccType.Void;


            int index = 0;
            foreach (var argument in argumentList)
            {
                argument.Index = index++;
                info.ArgumentTable[argument.Name] = argument;
            }


            functionTable[functionName] = info;
            symbolTable[info.Address] = new SymbolInfo()
            {
                Type = SymbolType.GlobalFunction,
                Function = info,
                Name = functionName,
                VirtualAddress = info.Address,
            };
        }


        public FunctionInfo GetFunction(string functionName)
        {
            return functionTable.TryGetValue(functionName, out var function) ? function : null;
        }


        public bool ContainFunction(string functionName)
        {
            return functionTable.ContainsKey(functionName);
        }


        public void RegisterVariable(string name, int typeKind)
        {
            var info = new VariableInfo();
            info.Name = name;
            info.StorageType = VariableType.Global;
            info.Address = nextVirtualAddress--;
            info.Unresolve = false;
            info.Type =
                typeKind == CccTokenKind.TypeInt ? CccType.Int :
                typeKind == CccTokenKind.TypeNumber ? CccType.Number :
                CccType.String;


            variableTable[name] = info;
            symbolTable[info.Address] = new SymbolInfo()
            {
                Type = SymbolType.GlobalVariable,
                Variable = info,
                Name = name,
                VirtualAddress = info.Address,
            };
        }


        public VariableInfo GetVariable(string name)
        {
            return variableTable.TryGetValue(name, out var variable) ? variable : null;
        }


        public bool ContainVariable(string name)
        {
            return variableTable.ContainsKey(name);
        }


        public IdentifierKind GetIdentifierKind(string identifier, string functionName)
        {
            if (!string.IsNullOrWhiteSpace(functionName))
            {
                var function = GetFunction(functionName);
                if (function != null)
                {
                    if (function.ContainVariable(identifier))
                    {
                        return IdentifierKind.LocalVariable;
                    }
                    else if (function.ContainArgument(identifier))
                    {
                        return IdentifierKind.ArgumentVariable;
                    }
                }
            }


            if (ContainVariable(identifier))
            {
                return IdentifierKind.GlobalVariable;
            }
            else if (ContainConstant(identifier))
            {
                return IdentifierKind.ConstantValue;
            }
            else if (ContainFunction(identifier))
            {
                var info = GetFunction(identifier);
                if (info.Type == FunctionType.Standard)
                {
                    return IdentifierKind.StandardFunction;
                }
                else if (info.Type == FunctionType.Peripheral)
                {
                    return IdentifierKind.PeripheralFunction;
                }
            }


            return IdentifierKind.Unknown;
        }
        #endregion



        #region common
        internal enum IdentifierKind
        {
            Unknown,
            LocalVariable,
            ArgumentVariable,
            GlobalVariable,
            ConstantValue,
            StandardFunction,
            PeripheralFunction,
        }



        internal enum CccType
        {
            Void,
            Int,
            Number,
            String,
        }



        public SymbolInfo GetSymbolFromVirtualAddress(int virtualAddress)
        {
            return symbolTable.TryGetValue(virtualAddress, out var symbol) ? symbol : null;
        }



        public class InstructionInfo
        {
            public InstructionCode Code;
            public bool UnresolveAddress;
        }
        #endregion



        #region const
        internal class ConstantInfo
        {
            public string Name { get; set; }
            public CccType Type { get; set; }
            public long IntegerValue { get; set; }
            public float NumberValue { get; set; }
            public string TextValue { get; set; }
        }
        #endregion



        #region variable
        internal enum VariableType
        {
            Global,
            Local,
        }



        internal class VariableInfo
        {
            private List<Action<int, int>> addressResolverList;



            public string Name { get; set; }
            public CccType Type { get; set; }
            public VariableType StorageType { get; set; }
            public int Address { get; set; }
            public bool Unresolve { get; set; }



            public VariableInfo()
            {
                addressResolverList = new List<Action<int, int>>();
            }


            public void AddAddressResolver(Action<int, int> resolver)
            {
                addressResolverList.Add(resolver ?? throw new ArgumentNullException(nameof(resolver)));
            }
        }
        #endregion



        #region function
        internal enum FunctionType
        {
            Standard,
            Peripheral,
        }



        internal class CccArgumentInfo
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public CccType Type { get; set; }
        }



        internal class FunctionInfo
        {
            public string Name { get; set; }
            public string PeripheralName { get; set; }
            public string PeripheralFunctionName { get; set; }
            public int Address { get; set; }
            public bool Unresolve { get; set; }
            public FunctionType Type { get; set; }
            public CccType ReturnType { get; set; }
            public Dictionary<string, CccArgumentInfo> ArgumentTable { get; set; }
            public Dictionary<string, VariableInfo> LocalVariableTable { get; set; }
            public Dictionary<string, int> InstructionOffsetAddressTable { get; set; }
            public List<InstructionInfo> InstructionInfoList { get; set; }
            public Stack<int> WhileNumberStack { get; set; }


            private int nextLocalVariableIndex;
            private int nextWhileNumber;



            public FunctionInfo()
            {
                ArgumentTable = new Dictionary<string, CccArgumentInfo>();
                LocalVariableTable = new Dictionary<string, VariableInfo>();
                InstructionOffsetAddressTable = new Dictionary<string, int>();
                InstructionInfoList = new List<InstructionInfo>();
                WhileNumberStack = new Stack<int>();
                nextLocalVariableIndex = 0;
                nextWhileNumber = 0;
            }


            public void RegisterVariable(string name, int typeKind)
            {
                var info = new VariableInfo();
                info.Name = name;
                info.StorageType = VariableType.Local;
                info.Address = nextLocalVariableIndex++;
                info.Unresolve = true;
                info.Type =
                    typeKind == CccTokenKind.TypeInt ? CccType.Int :
                    typeKind == CccTokenKind.TypeNumber ? CccType.Number :
                    CccType.String;


                LocalVariableTable[name] = info;
            }


            public bool ContainArgument(string name)
            {
                return ArgumentTable.ContainsKey(name);
            }


            public bool ContainVariable(string name)
            {
                return LocalVariableTable.ContainsKey(name);
            }


            public int CreateInstruction(OpCode opCode, byte ra, byte rb, byte rc, int imm, bool unresolveAddress)
            {
                var result = CommonCreateInstruction(opCode, ra, rb, rc, unresolveAddress);
                InstructionInfoList[result].Code.Immediate.Int = imm;
                return result;
            }


            public int CreateInstruction(OpCode opCode, byte ra, byte rb, byte rc, float imm, bool unresolveAddress)
            {
                var result = CommonCreateInstruction(opCode, ra, rb, rc, unresolveAddress);
                InstructionInfoList[result].Code.Immediate.Float = imm;
                return result;
            }


            private int CommonCreateInstruction(OpCode opCode, byte ra, byte rb, byte rc, bool unresolveAddress)
            {
                var instructionInfo = new InstructionInfo();
                instructionInfo.Code = new InstructionCode()
                {
                    OpCode = opCode,
                    Ra = ra,
                    Rb = rb,
                    Rc = rc,
                };

                instructionInfo.UnresolveAddress = unresolveAddress;

                var currentAddress = InstructionInfoList.Count;
                InstructionInfoList.Add(instructionInfo);
                return currentAddress;
            }


            public int SetInstructionCurrentAddress(string name)
            {
                var currentInstructionIndex = InstructionInfoList.Count;
                InstructionOffsetAddressTable[name] = currentInstructionIndex;
                return currentInstructionIndex;
            }


            public int GetInstructionOffsetAddress(string name)
            {
                return InstructionOffsetAddressTable.TryGetValue(name, out var address) ? address : 0;
            }


            public int PushWhileNumber()
            {
                var number = nextWhileNumber++;
                WhileNumberStack.Push(number);
                return number;
            }


            public int PeekWhileNumber()
            {
                return WhileNumberStack.Peek();
            }


            public string GetCurrentWhileBeginLabelName()
            {
                return $"__begin_while_{PeekWhileNumber()}";
            }


            public string GetCurrentWhileEndLabelName()
            {
                return $"__end_while_{PeekWhileNumber()}";
            }


            public int PopWhileNumber()
            {
                return WhileNumberStack.Pop();
            }
        }
        #endregion



        #region symbol
        internal enum SymbolType
        {
            Unknown,
            GlobalVariable,
            GlobalFunction,
        }



        internal class SymbolInfo
        {
            public int VirtualAddress { get; set; }
            public SymbolType Type { get; set; }
            public string Name { get; set; }
            public FunctionInfo Function { get; set; }
            public VariableInfo Variable { get; set; }
        }
        #endregion
    }
}