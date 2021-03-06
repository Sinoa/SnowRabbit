﻿// zlib/libpng License
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

namespace SnowRabbit.Compiler.Assembler.Symbols
{
    /// <summary>
    /// 引数変数シンボルを表すンボルクラスです
    /// </summary>
    public class SrParameterVariableSymbol : SrVariableSymbol
    {
        public int Position { get; }


        /// <summary>
        /// SrArgumentVariableSymbol クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="name">シンボル名</param>
        /// <param name="initialAddress">初期アドレス</param>
        public SrParameterVariableSymbol(string name, int initialAddress, int position) : base(name, initialAddress, SrScopeType.Parameter, SrSymbolKind.ParameterVariable)
        {
            Position = position;
        }
    }
}
