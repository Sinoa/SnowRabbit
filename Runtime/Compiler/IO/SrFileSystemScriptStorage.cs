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
using SnowRabbit.Diagnostics.Logging;

namespace SnowRabbit.Compiler.IO
{
    /// <summary>
    /// SnowRabbit コンパイラが使用するスクリプトストレージをファイルシステムとして実装したクラスです
    /// </summary>
    public class SrFileSystemScriptStorage : ISrScriptStorage
    {
        /// <summary>
        /// 対象のパスからテキストリーダを開きます
        /// </summary>
        /// <param name="path">開く対象となるパス</param>
        /// <returns>対象のパスからテキストリーダを開けた場合は参照を、開けなかった場合は null を返します</returns>
        public TextReader OpenRead(string path)
        {
            // 対象のファイルパスが存在する場合はテキストリーダとして開いて返す
            SrLogger.Trace(nameof(SrFileSystemScriptStorage), $"OpenRead:{path}");
            return File.Exists(path) ? new StreamReader(path) : null;
        }
    }
}