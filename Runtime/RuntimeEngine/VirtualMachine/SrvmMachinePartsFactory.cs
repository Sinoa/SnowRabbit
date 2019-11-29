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

namespace SnowRabbit.RuntimeEngine.VirtualMachine
{
    /// <summary>
    /// 仮想マシンが使用する各種マシンパーツクラスを生成するファクトリ抽象クラスです
    /// </summary>
    public abstract class SrvmMachinePartsFactory
    {
        /// <summary>
        /// プロセッサを生成します
        /// </summary>
        /// <returns>生成したプロセッサを返します</returns>
        public abstract SrvmProcessor CreateProcessor();


        /// <summary>
        /// メモリを生成します
        /// </summary>
        /// <returns>生成したメモリを返します</returns>
        public abstract SrvmMemory CreateMemory();


        /// <summary>
        /// ファームウェアを生成します
        /// </summary>
        /// <returns>生成したファームウェアを返します</returns>
        public abstract SrvmFirmware CreateFirmware();


        /// <summary>
        /// ストレージを生成します
        /// </summary>
        /// <returns>生成したストレージを返します</returns>
        public abstract SrvmStorage CreateStorage();
    }
}
