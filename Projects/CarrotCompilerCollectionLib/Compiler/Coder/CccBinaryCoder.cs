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
        private int nextGlobalVariableAddress;



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
            nextGlobalVariableAddress = 0;
        }


        /// <summary>
        /// 現在の状態から実行コードを出力します
        /// </summary>
        public void OutputExecuteCode()
        {
        }


        public string ManglingPeripheralVariableName(string peripheralName)
        {
            return $"___CCC_GPVN_{peripheralName}___";
        }


        public string ManglingPeripheralFunctionVariableName(string functionName)
        {
            return $"___CCC_GPFVN_{functionName}___";
        }


        public void RegisterPeripheralFunction(string functionName, int returnType, IList<int> argumentList, string peripheralName, string peripheralFunctionName)
        {
            var info = new FunctionInfo();
            info.Name = functionName;
            info.PeripheralName = peripheralName;
            info.PeripheralFunctionName = peripheralFunctionName;
            info.Address = -1;
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


            var peripheralVariableName = ManglingPeripheralVariableName(peripheralName);
            var peripheralFunctionVariableName = ManglingPeripheralFunctionVariableName(peripheralFunctionName);
            if (!variableTable.ContainsKey(peripheralVariableName))
            {
                RegisterVariable(peripheralVariableName, CccTokenKind.TypeInt, VariableType.Global);
            }
            RegisterVariable(peripheralFunctionVariableName, CccTokenKind.TypeInt, VariableType.Global);
        }


        public void RegisterFunction(string functionName, int returnType, List<CccArgumentInfo> argumentList)
        {
            var info = new FunctionInfo();
            info.Name = functionName;
            info.PeripheralName = string.Empty;
            info.PeripheralFunctionName = string.Empty;
            info.Address = -1;
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
        }


        public FunctionInfo GetFunction(string functionName)
        {
            return functionTable.TryGetValue(functionName, out var function) ? function : null;
        }


        public void RegisterVariable(string name, int typeKind, VariableType varType)
        {
            var info = new VariableInfo();
            info.Name = name;
            info.StorageType = varType;
            info.Address = varType == VariableType.Global ? nextGlobalVariableAddress++ : -1;
            info.Unresolve = varType != VariableType.Global;
            info.Type =
                typeKind == CccTokenKind.TypeInt ? CccType.Int :
                typeKind == CccTokenKind.TypeNumber ? CccType.Number :
                CccType.String;


            variableTable[name] = info;
        }



        #region common
        internal enum CccType
        {
            Void,
            Int,
            Number,
            String,
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
            private List<Action<int, int>> addressResolverList;



            public string Name { get; set; }
            public string PeripheralName { get; set; }
            public string PeripheralFunctionName { get; set; }
            public int Address { get; set; }
            public bool Unresolve { get; set; }
            public FunctionType Type { get; set; }
            public CccType ReturnType { get; set; }
            public Dictionary<string, CccArgumentInfo> ArgumentTable { get; set; }
            public Dictionary<string, VariableInfo> LocalVariableTable { get; set; }


            private int nextLocalVariableIndex = 0;



            public FunctionInfo()
            {
                addressResolverList = new List<Action<int, int>>();
                ArgumentTable = new Dictionary<string, CccArgumentInfo>();
                LocalVariableTable = new Dictionary<string, VariableInfo>();
            }


            public void AddAddressResolver(Action<int, int> resolver)
            {
                addressResolverList.Add(resolver ?? throw new ArgumentNullException(nameof(resolver)));
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
        }
        #endregion
    }
}