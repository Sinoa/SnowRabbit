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



        /// <summary>
        /// CccBinaryCoder クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="outputStream">出力ストリーム</param>
        /// <exception cref="ArgumentNullException">outputStream が null です</exception>
        public CccBinaryCoder(Stream outputStream)
        {
            // SnowRabbit向けバイナリIOのインスタンスを生成する
            binaryIO = new SrBinaryIO(outputStream ?? throw new ArgumentNullException(nameof(outputStream)));
        }


        /// <summary>
        /// 現在の状態から実行コードを出力します
        /// </summary>
        public void OutputExecuteCode()
        {
        }



        private enum FunctionType
        {
            Standard,
            Peripheral,
        }



        private class FunctionInfo
        {
            private List<Action<int, int>> addressResolverList;



            public string Name { get; set; }
            public int Address { get; set; }
            public bool Unresolve { get; set; }
            public FunctionType Type { get; set; }



            public FunctionInfo()
            {
                addressResolverList = new List<Action<int, int>>();
            }


            public void AddAddressResolver(Action<int, int> resolver)
            {
                addressResolverList.Add(resolver ?? throw new ArgumentNullException(nameof(resolver)));
            }
        }
    }
}