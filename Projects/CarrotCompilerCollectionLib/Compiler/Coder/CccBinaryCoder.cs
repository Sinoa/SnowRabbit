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
        public int GenerateJumpTest(FunctionInfo function, int offsetAddress)
        {
            // movl ra = r8, imm = 0
            // teq ra = r15, rb = r15, rc = r8
            // bnz ra = ip, rb = r15, imm = endlabel
            var r8 = (byte)SrvmProcessor.RegisterR8Index;
            var r15 = (byte)SrvmProcessor.RegisterR15Index;
            var ip = (byte)SrvmProcessor.RegisterIPIndex;
            function.CreateInstruction(OpCode.Movl, r8, 0, 0, 0, false);
            function.CreateInstruction(OpCode.Teq, r15, r15, r8, 0, false);
            function.CreateInstruction(OpCode.Bnz, ip, r15, 0, offsetAddress, true);
            return function.InstructionInfoList.Count - 1;
        }


        public int GenerateOffsetJump(FunctionInfo function, int offsetAddress)
        {
            // br ra = ip, imm = offsetAddress
            var ip = (byte)SrvmProcessor.RegisterIPIndex;
            function.CreateInstruction(OpCode.Br, ip, 0, 0, offsetAddress, false);
            return function.CurrentInstructionCount - 1;
        }


        public void UpdateJumpAddress(FunctionInfo function, int instructionIndex, int jumpIndex)
        {
            function.InstructionInfoList[instructionIndex].Code.Immediate.Int = jumpIndex;
            function.InstructionInfoList[instructionIndex].UnresolveAddress = false;
        }


        public int GenerateFunctionEnter(FunctionInfo function)
        {
            // push ra = rbp
            // mov ra = rbp, rb = rsp
            // subl ra = rsp, rb = rsp, imm = localVarCount
            // push ra = r15
            var rsp = (byte)SrvmProcessor.RegisterSPIndex;
            var rbp = (byte)SrvmProcessor.RegisterBPIndex;
            var r15 = (byte)SrvmProcessor.RegisterR15Index;
            function.CreateInstruction(OpCode.Push, rbp, 0, 0, 0, false);
            function.CreateInstruction(OpCode.Mov, rbp, rsp, 0, 0, false);
            function.CreateInstruction(OpCode.Subl, rsp, rsp, 0, 0, false);
            var patchIndex = function.CurrentInstructionCount - 1;
            function.CreateInstruction(OpCode.Push, r15, 0, 0, 0, false);
            return patchIndex;
        }


        public void UpdateFunctionLocalVariableCount(FunctionInfo function, int patchIndex, int localVarCount)
        {
            function.InstructionInfoList[patchIndex].Code.Immediate.Int = localVarCount;
        }


        public void GenerateFunctionLeave(FunctionInfo function)
        {
            // pop ra = r15
            // mov ra = rsp, rb = rbp
            // pop ra = rbp
            // ret
            var rsp = (byte)SrvmProcessor.RegisterSPIndex;
            var rbp = (byte)SrvmProcessor.RegisterBPIndex;
            var r15 = (byte)SrvmProcessor.RegisterR15Index;
            function.CreateInstruction(OpCode.Pop, r15, 0, 0, 0, false);
            function.CreateInstruction(OpCode.Mov, rsp, rbp, 0, 0, false);
            function.CreateInstruction(OpCode.Pop, rbp, 0, 0, 0, false);
            function.CreateInstruction(OpCode.Ret, 0, 0, 0, 0, false);
        }


        public void GenerateFunctionReturnCode(FunctionInfo function)
        {
            var rax = (byte)SrvmProcessor.RegisterAIndex;
            var r15 = (byte)SrvmProcessor.RegisterR15Index;
            function.CreateInstruction(OpCode.Mov, rax, r15, 0, 0, false);
            function.CreateInstruction(OpCode.Brl, 0, 0, 0, int.MaxValue, true);
        }


        public void GeneratePushRax(FunctionInfo function)
        {
            function.CreateInstruction(OpCode.Push, SrvmProcessor.RegisterAIndex, 0, 0, 0, false);
        }


        public void GeneratePushR15(FunctionInfo function)
        {
            function.CreateInstruction(OpCode.Push, SrvmProcessor.RegisterR15Index, 0, 0, 0, false);
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


        public void GenerateSetSingleExpressionValue(FunctionInfo function, ref ExpressionValue value)
        {
            var r15 = (byte)SrvmProcessor.RegisterR15Index;
            GenerateMovFromExpressionValue(function, r15, ref value);
        }


        public void GenerateOperationCode(FunctionInfo function, int operationKind, ref ExpressionValue left, ref ExpressionValue right)
        {
            var r15 = (byte)SrvmProcessor.RegisterR15Index;
            var r14 = (byte)SrvmProcessor.RegisterR14Index;
            var r13 = (byte)SrvmProcessor.RegisterR13Index;


            if (left.FirstGenerate)
            {
                GenerateMovFromExpressionValue(function, r15, ref left);
                left.FirstGenerate = false;
            }


            if (operationKind != CccTokenKind.Equal)
            {
                GenerateMovFromExpressionValue(function, r14, ref right);
            }


            if (left.Type == CccType.Number || right.Type == CccType.Number)
            {
                if (left.Type != CccType.Number)
                {
                    left.Type = CccType.Number;
                    left.Number = left.Integer;
                    function.CreateInstruction(OpCode.Movitf, r15, r15, 0, 0, false);
                }


                if (right.Type != CccType.Number)
                {
                    right.Type = CccType.Number;
                    right.Number = right.Integer;
                    function.CreateInstruction(OpCode.Movitf, r14, r14, 0, 0, false);
                }
            }


            OpCode opCode;
            switch (operationKind)
            {
                case CccTokenKind.Asterisk: // *
                    opCode = left.Type == CccType.Int ? OpCode.Mul : OpCode.Fmul;
                    function.CreateInstruction(opCode, r15, r15, r14, 0, false);
                    break;


                case CccTokenKind.Slash: // /
                    opCode = left.Type == CccType.Int ? OpCode.Div : OpCode.Fdiv;
                    function.CreateInstruction(opCode, r15, r15, r14, 0, false);
                    break;


                case CccTokenKind.Percent: // %
                    opCode = left.Type == CccType.Int ? OpCode.Mod : OpCode.Fmod;
                    function.CreateInstruction(opCode, r15, r15, r14, 0, false);
                    break;


                case CccTokenKind.PlusEqual: // +=
                case CccTokenKind.Plus: // +
                    opCode = left.Type == CccType.Int ? OpCode.Add : OpCode.Fadd;
                    function.CreateInstruction(opCode, r15, r15, r14, 0, false);
                    r14 = r15;
                    break;


                case CccTokenKind.MinusEqual: // +=
                case CccTokenKind.Minus: // -
                    opCode = left.Type == CccType.Int ? OpCode.Sub : OpCode.Fsub;
                    function.CreateInstruction(opCode, r15, r15, r14, 0, false);
                    r14 = r15;
                    break;


                case CccTokenKind.DoubleAnd: // &&
                    function.CreateInstruction(OpCode.Modl, r13, 0, 0, 0, false);
                    function.CreateInstruction(OpCode.Tne, r15, r15, r13, 0, false);
                    function.CreateInstruction(OpCode.Tne, r14, r14, r13, 0, false);
                    function.CreateInstruction(OpCode.And, r15, r15, r14, 0, false);
                    break;


                case CccTokenKind.DoubleVerticalbar: // ||
                    function.CreateInstruction(OpCode.Modl, r13, 0, 0, 0, false);
                    function.CreateInstruction(OpCode.Tne, r15, r15, r13, 0, false);
                    function.CreateInstruction(OpCode.Tne, r14, r14, r13, 0, false);
                    function.CreateInstruction(OpCode.Or, r15, r15, r14, 0, false);
                    break;


                case CccTokenKind.DoubleCloseAngle: // >>
                    function.CreateInstruction(OpCode.Shr, r15, r15, r14, 0, false);
                    break;


                case CccTokenKind.DoubleOpenAngle: // <<
                    function.CreateInstruction(OpCode.Shl, r15, r15, r14, 0, false);
                    break;


                case CccTokenKind.DoubleEqual: // ==
                    function.CreateInstruction(OpCode.Teq, r15, r15, r14, 0, false);
                    break;


                case CccTokenKind.CloseAngle: // >
                    function.CreateInstruction(OpCode.Tg, r15, r15, r14, 0, false);
                    break;


                case CccTokenKind.OpenAngle: // <
                    function.CreateInstruction(OpCode.Tl, r15, r15, r14, 0, false);
                    break;


                case CccTokenKind.GreaterEqual: // >=
                    function.CreateInstruction(OpCode.Tge, r15, r15, r14, 0, false);
                    break;


                case CccTokenKind.LesserEqual: // <=
                    function.CreateInstruction(OpCode.Tle, r15, r15, r14, 0, false);
                    break;
            }


            if (operationKind == CccTokenKind.Equal)
            {
                //if (left.Type != right.Type)
                //{
                //    if (left.Type == CccType.Int)
                //    {
                //        function.CreateInstruction(OpCode.Movfti, r15, r14, 0, 0, false);
                //    }


                //    if (left.Type == CccType.Number)
                //    {
                //        function.CreateInstruction(OpCode.Movitf, r15, r14, 0, 0, false);
                //    }
                //}
                //else
                //{
                //    function.CreateInstruction(OpCode.Mov, r15, r14, 0, 0, false);
                //}


                GenerateStr(function, r15, ref left);
            }
        }


        public void GenerateMovFromExpressionValue(FunctionInfo function, byte registerNum, ref ExpressionValue value)
        {
            switch (value.IdentifierKind)
            {
                case IdentifierKind.ConstantValue:
                    switch (value.Type)
                    {
                        case CccType.Int:
                            function.CreateInstruction(OpCode.Movl, registerNum, 0, 0, value.Integer, false);
                            return;


                        case CccType.Number:
                            function.CreateInstruction(OpCode.Fmovl, registerNum, 0, 0, (float)value.Number, false);
                            return;


                        case CccType.String:
                            var hashCode = value.Text.GetHashCode().ToString();
                            RegisterConstantValue(hashCode, CccType.String, 0, 0.0f, value.Text);
                            function.CreateInstruction(OpCode.Movl, registerNum, 0, 0, GetConstant(hashCode).Address, true);
                            return;
                    }
                    break;


                case IdentifierKind.GlobalVariable:
                    var globalVarInfo = GetVariable(value.Text);
                    function.CreateInstruction(OpCode.Ldrl, registerNum, 0, 0, globalVarInfo.Address, true);
                    break;


                case IdentifierKind.ArgumentVariable:
                    var argumentInfo = function.ArgumentTable[value.Text];
                    var offsetAddress = function.ArgumentTable.Count - 1 - argumentInfo.Index;
                    function.CreateInstruction(OpCode.Ldr, registerNum, SrvmProcessor.RegisterBPIndex, 0, offsetAddress, false);
                    break;


                case IdentifierKind.LocalVariable:
                    var localVariableInfo = function.LocalVariableTable[value.Text];
                    function.CreateInstruction(OpCode.Ldr, registerNum, SrvmProcessor.RegisterBPIndex, 0, localVariableInfo.Address, false);
                    break;


                case IdentifierKind.PeripheralFunction:
                case IdentifierKind.StandardFunction:
                    function.CreateInstruction(OpCode.Mov, registerNum, SrvmProcessor.RegisterAIndex, 0, 0, false);
                    break;
            }
        }


        public void GenerateStr(FunctionInfo function, byte registerNum, ref ExpressionValue value)
        {
            switch (value.IdentifierKind)
            {
                case IdentifierKind.GlobalVariable:
                    var globalVarInfo = GetVariable(value.Text);
                    function.CreateInstruction(OpCode.Strl, registerNum, 0, 0, globalVarInfo.Address, true);
                    break;


                case IdentifierKind.ArgumentVariable:
                    var argumentInfo = function.ArgumentTable[value.Text];
                    var offsetAddress = function.ArgumentTable.Count - 1 - argumentInfo.Index;
                    function.CreateInstruction(OpCode.Str, registerNum, SrvmProcessor.RegisterBPIndex, 0, offsetAddress, false);
                    break;


                case IdentifierKind.LocalVariable:
                    var localVariableInfo = function.LocalVariableTable[value.Text];
                    function.CreateInstruction(OpCode.Str, registerNum, SrvmProcessor.RegisterBPIndex, 0, localVariableInfo.Address, false);
                    break;
            }
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
            info.Address = nextVirtualAddress--;


            constantTable[name] = info;


            symbolTable[info.Address] = new SymbolInfo()
            {
                Type = SymbolType.Constant,
                Constant = info,
                Name = info.Name,
                VirtualAddress = info.Address,
            };
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
            public int Address { get; set; }
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
            public int CurrentInstructionCount => InstructionInfoList.Count;


            private int nextLocalVariableIndex;



            public FunctionInfo()
            {
                ArgumentTable = new Dictionary<string, CccArgumentInfo>();
                LocalVariableTable = new Dictionary<string, VariableInfo>();
                InstructionOffsetAddressTable = new Dictionary<string, int>();
                InstructionInfoList = new List<InstructionInfo>();
                nextLocalVariableIndex = -1;
            }


            public void RegisterVariable(string name, int typeKind)
            {
                var info = new VariableInfo();
                info.Name = name;
                info.StorageType = VariableType.Local;
                info.Address = nextLocalVariableIndex--;
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
        }
        #endregion



        #region symbol
        internal enum SymbolType
        {
            Unknown,
            GlobalVariable,
            GlobalFunction,
            Constant,
        }



        internal class SymbolInfo
        {
            public int VirtualAddress { get; set; }
            public SymbolType Type { get; set; }
            public string Name { get; set; }
            public FunctionInfo Function { get; set; }
            public VariableInfo Variable { get; set; }
            public ConstantInfo Constant { get; set; }
        }
        #endregion



        #region expression
        internal struct ExpressionValue
        {
            public IdentifierKind IdentifierKind;
            public CccType Type;
            public long Integer;
            public double Number;
            public string Text;
            public bool FirstGenerate;
        }
        #endregion
    }
}