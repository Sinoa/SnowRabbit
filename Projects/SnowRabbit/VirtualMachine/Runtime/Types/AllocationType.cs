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

namespace SnowRabbit.VirtualMachine.Runtime
{
    /// <summary>
    /// 仮想マシンがメモリ確保する際のメモリの種別を表します
    /// </summary>
    public enum AllocationType : byte
    {
        /// <summary>
        /// 何も割り当てられていない未使用メモリ領域です
        /// </summary>
        Free = 0,

        /// <summary>
        /// 汎用的な理由によるメモリ確保です
        /// </summary>
        General = 1,

        /// <summary>
        /// 仮想マシンのコンテキストメモリ領域です
        /// </summary>
        VMContext = 2,

        /// <summary>
        /// 実行コードが格納されているメモリ領域です
        /// </summary>
        Program = 3,

        /// <summary>
        /// 仮想マシンが実行するプロセスのメモリ領域です
        /// </summary>
        Process = 4,
    }
}