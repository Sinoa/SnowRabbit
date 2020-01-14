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

namespace SnowRabbit.Compiler.Assembler.Symbols
{
    /// <summary>
    /// ローカル変数シンボルを表す抽象シンボルクラスです
    /// </summary>
    public abstract class SrLocalVariableSymbol : SrSymbol
    {
        // メンバ変数定義
        private string mangledName;



        /// <summary>
        /// SrLocalVariableSymbol クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="name">シンボル名</param>
        /// <param name="initialAddress">初期アドレス</param>
        protected SrLocalVariableSymbol(string name, int initialAddress) : base(name, initialAddress)
        {
        }


        /// <summary>
        /// ローカル変数シンボル名をマングリングします
        /// </summary>
        /// <returns>マングリングした名前を返します</returns>
        protected override string Mangling()
        {
            // マングリング済みならそのまま返して、まだならローカル変数固有のマングリンク結果をそのまま返す
            return mangledName ?? (mangledName = $"___SR_LV_{Name}___");
        }
    }



    /// <summary>
    /// ローカル変数シンボルを表すローカル変数シンボルクラスです
    /// </summary>
    /// <typeparam name="T">ローカル変数に定義された型 int, double, string, bool, object のいずれか</typeparam>
    public class SrLocalVariableSymbol<T> : SrLocalVariableSymbol
    {
        /// <summary>
        /// SrLocalVariableSymbol クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="name">シンボル名</param>
        /// <param name="initialAddress">初期アドレス</param>
        public SrLocalVariableSymbol(string name, int initialAddress) : base(name, initialAddress)
        {
        }
    }
}
