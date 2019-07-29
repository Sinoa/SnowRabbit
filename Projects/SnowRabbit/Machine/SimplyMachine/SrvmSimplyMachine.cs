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

namespace SnowRabbit.Machine
{
    /// <summary>
    /// 非常に単純な構成で実装された仮想マシンクラスです
    /// </summary>
    public class SrvmSimplyMachine : SrvmMachine
    {
        // 定数定義
        private const int DefaultValueMemoryReservedSize = 8 << 20;
        private const int DefaultObjectMemoryReservedSize = 8 << 20;



        /// <summary>
        /// SrvmSimplyMachine クラスのインスタンスを初期化します
        /// </summary>
        public SrvmSimplyMachine() : this(DefaultValueMemoryReservedSize, DefaultObjectMemoryReservedSize, string.Empty)
        {
        }


        /// <summary>
        /// SrvmSimplyMachine クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="valueMemoryReservedSize">仮想マシンの値型メモリの確保サイズ</param>
        /// <param name="objectMemoryReservedSize">仮想マシンの参照型メモリの確保サイズ</param>
        /// <param name="scriptBaseDirectoryPath">仮想マシンがロードするスクリプトのベースディレクトリパス</param>
        public SrvmSimplyMachine(int valueMemoryReservedSize, int objectMemoryReservedSize, string scriptBaseDirectoryPath)
            : base(new SrvmSimplyProcessor(), new SrvmSimplyFirmware(), new SrvmSimplyMemory(valueMemoryReservedSize, objectMemoryReservedSize), new SrvmSimplyStorage(scriptBaseDirectoryPath))
        {
        }
    }
}