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
    /// 定数定義されたシンボルを表す抽象定数シンボルクラスです
    /// </summary>
    public abstract class SrConstantSymbol : SrSymbol
    {
        // メンバ変数定義
        private string mangledName;



        /// <summary>
        /// SrConstantSymbol クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="name">シンボル名</param>
        /// <param name="initialAddress">初期アドレス</param>
        protected SrConstantSymbol(string name, int initialAddress) : base(name, initialAddress)
        {
        }


        /// <summary>
        /// 定数定義されたシンボル名のマングリングをします
        /// </summary>
        /// <returns>マングリングした名前を返します</returns>
        protected override string Mangling()
        {
            // マングリング済みならそのまま返して、まだなら定数定義固有のマングリンク結果をそのまま返す
            return mangledName ?? (mangledName = $"___SR_CONST_{Name}___");
        }
    }



    /// <summary>
    /// 定数定義されたシンボルを表す定数シンボルクラスです
    /// </summary>
    /// <typeparam name="T">定数の型 int, double, string のいずれか</typeparam>
    public class SrConstantSymbol<T> : SrConstantSymbol
    {
        // メンバ変数定義
        private T value;



        /// <summary>
        /// この定数定義された値が確定しているか否か
        /// </summary>
        public bool FixedValue { get; private set; }



        /// <summary>
        /// 定数定義された整数の値
        /// </summary>
        public T Value { get => value; set => UpdateValue(value); }



        /// <summary>
        /// SrConstantSymbol クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="name">シンボル名</param>
        /// <param name="initialAddress">初期アドレス</param>
        public SrConstantSymbol(string name, int initialAddress) : base(name, initialAddress)
        {
        }


        /// <summary>
        /// 定数値をその更新します。この更新関数は内部で値の確定をします。
        /// </summary>
        /// <param name="value">更新する値</param>
        private void UpdateValue(T value)
        {
            // 値を設定して確定したことを示す
            this.value = value;
            FixedValue = true;
        }
    }
}
