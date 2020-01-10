// zlib/libpng License
//
// Copyright(c) 2019 - 2020 Sinoa
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
    /// SnowRabbit が提供する既定マシンパーツ生成クラスです
    /// </summary>
    internal class SrvmDefaultMachinePartsFactory : SrvmMachinePartsFactory
    {
        /// <summary>
        /// ファームウェアを生成します
        /// </summary>
        /// <returns>生成したファームウェアを返します</returns>
        public override SrvmFirmware CreateFirmware()
        {
            // 何も装飾しないファームウェアインスタンスを生成して返す
            return new SrvmFirmware();
        }


        /// <summary>
        /// メモリを生成します
        /// </summary>
        /// <returns>生成したメモリを返します</returns>
        public override SrvmMemory CreateMemory()
        {
            // 何も装飾しないメモリインスタンスを生成して返す
            return new SrvmMemory();
        }


        /// <summary>
        /// プロセッサを生成します
        /// </summary>
        /// <returns>生成したプロセッサを返します</returns>
        public override SrvmProcessor CreateProcessor()
        {
            // 何も装飾しないプロセッサインスタンスを生成して返す
            return new SrvmProcessor();
        }


        /// <summary>
        /// ストレージを生成します
        /// </summary>
        /// <returns>生成したストレージを返します</returns>
        public override SrvmStorage CreateStorage()
        {
            // ファイルシステムストレージを生成して返す
            return new SrvmFileSystemStorage();
        }
    }
}
