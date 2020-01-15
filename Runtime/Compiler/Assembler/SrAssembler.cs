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

using System;
using System.Collections.Generic;
using System.IO;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.Compiler.Assembler.Symbols;
using System.Linq;

namespace SnowRabbit.Compiler.Assembler
{
    /// <summary>
    /// SrBuilder によって生成されたアセンブリコードから、VMが実行できる実行コードを生成するクラスです
    /// </summary>
    public class SrAssembler
    {
        /// <summary>
        /// 指定されたアセンブリコードデータから、実行コードをアセンブルします
        /// </summary>
        /// <param name="data">アセンブルするためのアセンブリコードをもつデータ</param>
        /// <param name="outStream">実行コードの出力先ストリーム</param>
        /// <exception cref="ArgumentNullException">data が null です</exception>
        /// <exception cref="ArgumentNullException">outStream が null です</exception>
        /// <exception cref="ArgumentException">outStream に書き込みする能力がありません</exception>
        public void Assemble(SrAssemblyData data, Stream outStream)
        {
            MergeAllFunction(data);
            AllocateStringPool(data);
            AssignGlobalAddress(data);
            ResolveAddress(data);
        }


        private void MergeAllFunction(SrAssemblyData data)
        {
            var allCode = new SrAssemblyCode[data.CodeSize];
            var initCode = data.GetFunctionCode("__init");
            Array.Copy(initCode, 0, allCode, 0, initCode.Length);
            var currentIndex = initCode.Length;


            foreach (var function in data.functionCodeTable)
            {
                if (function.Key == "__init") continue;

                var name = function.Key;
                var code = function.Value;

                data.GetFunctionSymbol(name).Address = currentIndex;
                Array.Copy(code, 0, allCode, currentIndex, code.Length);
                currentIndex += code.Length;
            }


            data.functionCodeTable.Clear();
            data.functionCodeTable[""] = allCode;
        }


        private void AllocateStringPool(SrAssemblyData data)
        {
            var codeSize = data.CodeSize;
            var counter = 0;
            foreach (var stringSymbol in data.GetSymbolAll<SrStringSymbol>())
            {
                stringSymbol.Address = codeSize + counter++;
            }
        }


        private void AssignGlobalAddress(SrAssemblyData data)
        {
            var offsetAddress = SrVirtualMemory.GlobalOffset;
            var counter = 0;
            foreach (var symbol in data.GetSymbolAll<SrGlobalVariableSymbol>())
            {
                symbol.Address = (int)(offsetAddress + counter++);
            }
        }


        private void ResolveAddress(SrAssemblyData data)
        {
            Dictionary<int, int> addressTranslateTable = new Dictionary<int, int>();
            foreach (var symbol in data.GetSymbolAll<SrSymbol>())
            {
                addressTranslateTable[symbol.InitialAddress] = symbol.Address;
            }


            var code = data.functionCodeTable[""];
            for (int i = 0; i < code.Length; ++i)
            {
                if (!code[i].UnresolvedAddress) continue;
                code[i].Instruction.Int = addressTranslateTable[code[i].Instruction.Int];
                code[i].UnresolvedAddress = false;
            }
        }
    }
}