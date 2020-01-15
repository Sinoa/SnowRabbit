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
using SnowRabbit.Compiler.Assembler.Symbols;

namespace SnowRabbit.Compiler.Assembler
{
    /// <summary>
    /// アセンブラがアセンブルする対象となるアセンブリデータを持ったクラスです
    /// </summary>
    public class SrAssemblyData
    {
        // メンバ変数定義
        private readonly Dictionary<string, SrSymbol> globalSymbolTable = new Dictionary<string, SrSymbol>();
        private readonly Dictionary<string, SrAssemblyCode[]> functionCodeTable = new Dictionary<string, SrAssemblyCode[]>();



        /// <summary>
        /// 指定されたシンボルを追加します
        /// </summary>
        /// <param name="symbol">追加するシンボル</param>
        /// <returns>追加に成功した場合は true を、重複または追加出来なかった場合は false を返します</returns>
        /// <exception cref="ArgumentNullException">symbol が null です</exception>
        public bool AddSymbol(SrSymbol symbol)
        {
            // シンボル名を取得してテーブルに存在していれば false を返す
            var name = (symbol ?? throw new ArgumentNullException(nameof(symbol))).Name;
            if (globalSymbolTable.ContainsKey(name)) return false;


            // シンボルを追加して成功を返す
            globalSymbolTable[name] = symbol;
            return true;
        }


        /// <summary>
        /// 指定された名前のグローバルシンボルを取得します
        /// </summary>
        /// <param name="name">取得するシンボル名</param>
        /// <returns>取得されたシンボルを返しますが、見つからなかった場合は null を返します</returns>
        public SrSymbol GetGlobalSymbol(string name)
        {
            // グローバルシンボルテーブルに指定された名前のシンボルがあれば返す
            return globalSymbolTable.TryGetValue(name, out var symbol) ? symbol : null;
        }


        /// <summary>
        /// 指定された名前の関数シンボルを取得します
        /// </summary>
        /// <param name="name">取得する関数シンボル名</param>
        /// <returns>指定された関数シンボルがある場合は関数シンボルを返しますが、見つけられなかった場合は null を返します</returns>
        public SrFunctionSymbol GetFunctionSymbol(string name)
        {
            // グローバルシンボルテーブルにまず指定された名前のシンボルがあるかを取得して、関数シンボルなら返す
            return (globalSymbolTable.TryGetValue(name, out var symbol) && symbol is SrFunctionSymbol) ? (SrFunctionSymbol)symbol : null;
        }


        /// <summary>
        /// 指定された名前の定数シンボルを取得します
        /// </summary>
        /// <param name="name">取得する定数シンボル名</param>
        /// <returns>指定された定数シンボルがある場合は定数シンボルを返しますが、見つけられなかった場合は null を返します</returns>
        public SrConstantSymbol GetConstantSymbol(string name)
        {
            // グローバルシンボルテーブルにまず指定された名前のシンボルがあるかを取得して、定数シンボルなら返す
            return (globalSymbolTable.TryGetValue(name, out var symbol) && symbol is SrConstantSymbol) ? (SrConstantSymbol)symbol : null;
        }


        /// <summary>
        /// 指定された文字列のシンボルを取得します
        /// </summary>
        /// <param name="text">取得する文字列シンボルの文字列</param>
        /// <returns>指定された文字列シンボルがある場合は文字列シンボルを返しますが、見つけられなかった場合は null を返します</returns>
        public SrStringSymbol GetStringSymbol(string text)
        {
            // グローバルシンボルテーブルにまず指定された名前のシンボルがあるかを取得して、定数シンボルなら返す
            return (globalSymbolTable.TryGetValue(text.GetHashCode().ToString(), out var symbol) && symbol is SrStringSymbol) ? (SrStringSymbol)symbol : null;
        }


        /// <summary>
        /// 指定された名前の変数および定数シンボルを取得します。また検索する範囲は、ローカル > パラメータ > グローバル の順になります。
        /// </summary>
        /// <param name="name">取得するする変数及び定数シンボル名</param>
        /// <param name="functionName">関数ローカルを含む場合の関数名、グローバルのみの検索の場合は null</param>
        /// <returns>指定された名前のシンボルが見つかった場合はシンボルを返しますが、見つけられなかった場合は null を返します</returns>
        public SrVariableSymbol GetVariableSymbol(string name, string functionName)
        {
            // 関数名が指定されていたら
            if (functionName != null)
            {
                // まずは関数シンボルを取得して見つけた場合は
                var functionSymbol = GetFunctionSymbol(functionName);
                if (functionName != null)
                {
                    // 関数シンボルに該当の変数シンボルが存在するかチェック
                    var result =
                        functionSymbol.LocalVariableTable.TryGetValue(name, out var localSymbol) ? (SrVariableSymbol)localSymbol :
                        functionSymbol.ParameterTable.TryGetValue(name, out var paramSymbol) ? (SrVariableSymbol)paramSymbol :
                        null;


                    // 存在すれば結果を返す
                    if (result != null) return result;
                }
            }


            // なければグローバルシンボルテーブルから調べる
            return (globalSymbolTable.TryGetValue(name, out var symbol) && symbol is SrVariableSymbol) ? (SrVariableSymbol)symbol : null;
        }


        /// <summary>
        /// 指定された名前として関数コードを設定します
        /// </summary>
        /// <param name="name">設定する関数名</param>
        /// <param name="codes">設定するコード</param>
        /// <exception cref="ArgumentNullException">name が null です</exception>
        /// <exception cref="ArgumentNullException">codes が null です</exception>
        public void SetFunctionCode(string name, SrAssemblyCode[] codes)
        {
            // 既に存在していても関係なく上書き設定
            functionCodeTable[name] = codes;
        }


        /// <summary>
        /// 関数コードを列挙する列挙オブジェクトを取得します
        /// </summary>
        /// <returns>関数コードを列挙するオブジェクトを返します</returns>
        public Dictionary<string, SrAssemblyCode[]>.Enumerator GetFunctionCodeEnumerator()
        {
            // そのままEnumeratorを返す
            return functionCodeTable.GetEnumerator();
        }
    }
}
