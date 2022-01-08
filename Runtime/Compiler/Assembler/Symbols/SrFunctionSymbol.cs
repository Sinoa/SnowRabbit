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

namespace SnowRabbit.Compiler.Assembler.Symbols
{
    /// <summary>
    /// 関数シンボルを表現する抽象関数シンボルクラスです
    /// </summary>
    public abstract class SrFunctionSymbol : SrSymbol
    {
        /// <summary>
        /// この関数の戻り値型
        /// </summary>
        public SrRuntimeType ReturnType { get; set; }


        /// <summary>
        /// この関数のパラメータリスト
        /// </summary>
        public Dictionary<string, SrParameterVariableSymbol> ParameterTable { get; }


        /// <summary>
        /// この関数のローカル変数テーブル
        /// </summary>
        public Dictionary<string, SrLocalVariableSymbol> LocalVariableTable { get; }


        public HashSet<byte> UsedRegisterSet { get; }



        /// <summary>
        /// SrFunctionSymbol クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="name">シンボル名</param>
        /// <param name="initialAddress">初期アドレス</param>
        protected SrFunctionSymbol(string name, int initialAddress, SrSymbolKind kind) : base(name, initialAddress, SrScopeType.Global, kind)
        {
            // 関数の付随情報の初期化
            ParameterTable = new Dictionary<string, SrParameterVariableSymbol>();
            LocalVariableTable = new Dictionary<string, SrLocalVariableSymbol>();
            UsedRegisterSet = new HashSet<byte>();
        }


        /// <summary>
        /// 指定された名前と型でパラメータの追加をしますが、すでに同じ名前のパラメータが存在する場合は取得をします
        /// </summary>
        /// <param name="name">追加するパラメータ名</param>
        /// <param name="type">追加するパラメータの型</param>
        /// <returns>追加されたパラメータシンボルを返します</returns>
        public SrParameterVariableSymbol AddOrGetParameter(string name, SrRuntimeType type)
        {
            if (ParameterTable.TryGetValue(name, out var value)) return value;
            int position = ParameterTable.Count + 1;
            value = new SrParameterVariableSymbol(name, position, position);
            value.Type = type;
            return ParameterTable[name] = value;
        }


        /// <summary>
        /// 指定された位置のパラメータを取得します
        /// </summary>
        /// <param name="position">取得したい引数の位置（左から1, 2, 3,...）</param>
        /// <returns>指定された位置のパラメータを返します</returns>
        /// <exception cref="ArgumentOutOfRangeException">position の値が 0 以下または、引数の数超過です</exception>
        public SrParameterVariableSymbol GetParameter(int position)
        {
            if (position <= 0 || position > ParameterTable.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }


            foreach (var parameter in ParameterTable.Values)
            {
                if (parameter.Position == position)
                {
                    return parameter;
                }
            }


            throw new InvalidOperationException($"関数 '{Name}' の引数取得位置 '{position}' にパラメータが見つかりませんでした。");
        }


        /// <summary>
        /// 指定された名前と型でローカル変数の追加をしますが、既に同じ名前の変数が存在する場合は取得をします
        /// </summary>
        /// <param name="name">追加するローカル変数名</param>
        /// <param name="type">追加するローカル変数の型</param>
        /// <returns>追加されたローカル変数シンボルを返します</returns>
        public SrLocalVariableSymbol AddOrGetLocalVariable(string name, SrRuntimeType type)
        {
            if (LocalVariableTable.TryGetValue(name, out var value)) return value;
            value = new SrLocalVariableSymbol(name, LocalVariableTable.Count + 1);
            value.Type = type;
            return LocalVariableTable[name] = value;
        }
    }
}
