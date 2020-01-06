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

namespace SnowRabbit.Compiler.Parser.Symbols
{
    /// <summary>
    /// 宣言されているシンボルを表現する抽象シンボルクラスです
    /// </summary>
    public abstract class SrSymbol
    {
        // メンバ変数定義
        private int symbolAddress;



        /// <summary>
        /// シンボルのアドレス
        /// </summary>
        public int Address { get => symbolAddress; set => UpdateAddress(value); }


        /// <summary>
        /// シンボルの名前
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// マングリングされたシンボルの名前
        /// </summary>
        public string MangledName => Mangling();



        /// <summary>
        /// SrSymbol のインスタンスを初期化します
        /// </summary>
        /// <param name="name">シンボル名</param>
        /// <param name="initialAddress">初期アドレス</param>
        protected SrSymbol(string name, int initialAddress)
        {
            // 種別と名前はそのまま覚える
            Name = name;
            symbolAddress = initialAddress;
        }


        /// <summary>
        /// このシンボルを表すユニークな名前を作ります
        /// </summary>
        /// <returns>マングリングされた名前を返します</returns>
        protected abstract string Mangling();


        /// <summary>
        /// シンボルアドレスを更新します
        /// </summary>
        /// <param name="newAddress">更新する新しいアドレス</param>
        private void UpdateAddress(int newAddress)
        {
            // 古いアドレスを覚えて新しいアドレスへ更新後、イベント呼び出し
            var oldAddress = symbolAddress;
            symbolAddress = newAddress;
            OnAddressUpdated(oldAddress, newAddress);
        }


        /// <summary>
        /// シンボルアドレスが更新された時の処理をします
        /// </summary>
        /// <param name="oldAddress">更新前のアドレス</param>
        /// <param name="newAddress">更新後のアドレス</param>
        protected virtual void OnAddressUpdated(int oldAddress, int newAddress)
        {
        }
    }
}
