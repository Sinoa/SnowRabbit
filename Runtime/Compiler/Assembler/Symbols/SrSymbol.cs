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

namespace SnowRabbit.Compiler.Assembler.Symbols
{
    /// <summary>
    /// 宣言されているシンボルを表現する抽象シンボルクラスです
    /// </summary>
    public abstract class SrSymbol
    {
        /// <summary>
        /// シンボルの初期アドレス
        /// </summary>
        public int InitialAddress { get; }


        /// <summary>
        /// シンボルのアドレス
        /// </summary>
        public int Address { get; set; }


        /// <summary>
        /// シンボルの名前
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// シンボルのスコープ
        /// </summary>
        public SrScopeType Scope { get; }


        /// <summary>
        /// シンボルの種類
        /// </summary>
        public SrSymbolKind Kind { get; }



        /// <summary>
        /// SrSymbol のインスタンスを初期化します
        /// </summary>
        /// <param name="name">シンボル名</param>
        /// <param name="initialAddress">初期アドレス</param>
        /// <exception cref="ArgumentNullException">name が null です</exception>
        protected SrSymbol(string name, int initialAddress) : this(name, initialAddress, SrScopeType.Global)
        {
        }


        /// <summary>
        /// SrSymbol のインスタンスを初期化します
        /// </summary>
        /// <param name="name">シンボル名</param>
        /// <param name="initialAddress">初期アドレス</param>
        /// <param name="scope">シンボルのスコープ範囲、既定はグローバルです</param>
        /// <exception cref="ArgumentNullException">name が null です</exception>
        protected SrSymbol(string name, int initialAddress, SrScopeType scope) : this(name, initialAddress, scope, SrSymbolKind.Unknown)
        {
        }


        protected SrSymbol(string name, int initialAddress, SrScopeType scope, SrSymbolKind kind)
        {
            // 種別と名前はそのまま覚える
            Name = name ?? throw new ArgumentNullException(nameof(name));
            InitialAddress = initialAddress;
            Address = initialAddress;
            Scope = scope;
            Kind = kind;
        }
    }
}
