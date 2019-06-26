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

using System.IO;

namespace SnowRabbit.Machine
{
    /// <summary>
    /// 仮想マシンが実装する仮想マシンストレージの抽象クラスです
    /// </summary>
    public abstract class SrvmStorage : SrvmMachineParts
    {
        /// <summary>
        /// 指定されたパスにあるデータリソースを開きます
        /// </summary>
        /// <param name="path">開くデータリソースへのパス</param>
        /// <returns>データリソースを開けた場合は Stream のインスタンスを、開けなかった場合は null を返します</returns>
        protected internal abstract Stream Open(string path);


        /// <summary>
        /// 指定されたストリームを閉じます
        /// </summary>
        /// <param name="stream">閉じるストリーム</param>
        protected internal abstract void Close(Stream stream);
    }
}