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

using System.Collections.Generic;

namespace SnowRabbit.Compiler.Assembler.Symbols
{
    /// <summary>
    /// 周辺機器関数シンボルを表す関数シンボルクラスです
    /// </summary>
    public class SrPeripheralFunctionSymbol : SrFunctionSymbol
    {
        /// <summary>
        /// この周辺機器関数が所属する周辺機器名
        /// </summary>
        public string PeripheralName { get; set; } = string.Empty;


        /// <summary>
        /// この周辺機器関数が提供する関数名
        /// </summary>
        public string PeripheralFunctionName { get; set; } = string.Empty;


        /// <summary>
        /// この周辺機器関数の参照を格納する変数の名前
        /// </summary>
        public string PeripheralGlobalVariableName { get; }



        /// <summary>
        /// SrPeripheralFunctionSymbol クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="name">シンボル名</param>
        /// <param name="initialAddress">初期アドレス</param>
        public SrPeripheralFunctionSymbol(string name, int initialAddress) : base(name, initialAddress, SrSymbolKind.PeripheralFunction)
        {
            PeripheralGlobalVariableName = $"___PF__{name}___";
        }


        /// <summary>
        /// この周辺機器関数のシグネチャ（戻り値型・周辺機器名・関数名・パラメータ型の並び）が一致するか確認します
        /// </summary>
        /// <param name="returnType">比較する戻り値型</param>
        /// <param name="peripheralName">比較する周辺機器名</param>
        /// <param name="peripheralFunctionName">比較する周辺機器関数名</param>
        /// <param name="parameterTypes">比較するパラメータ型の並び</param>
        /// <returns>シグネチャが一致する場合は true を、一致しない場合は false を返します</returns>
        public bool SignatureEquals(SrRuntimeType returnType, string peripheralName, string peripheralFunctionName, IReadOnlyList<SrRuntimeType> parameterTypes)
        {
            if (ReturnType != returnType || PeripheralName != peripheralName || PeripheralFunctionName != peripheralFunctionName)
            {
                return false;
            }


            if (ParameterTable.Count != parameterTypes.Count)
            {
                return false;
            }


            for (int i = 0; i < parameterTypes.Count; ++i)
            {
                if (GetParameter(i + 1).Type != parameterTypes[i])
                {
                    return false;
                }
            }


            return true;
        }
    }
}
