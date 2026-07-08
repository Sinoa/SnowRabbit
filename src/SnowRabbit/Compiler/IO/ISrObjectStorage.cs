// zlib License
//
// Copyright (c) 2026 Sinoa
//
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software
// in a product, an acknowledgment in the product documentation would be
// appreciated but is not required.
//
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
//
// 3. This notice may not be removed or altered from any source
// distribution.

#nullable disable

using System.IO;

namespace SnowRabbit.Compiler.IO
{
    /// <summary>
    /// #link ディレクティブが参照するオブジェクトファイル (SROB) の解決を行うインターフェイスです
    /// </summary>
    public interface ISrObjectStorage
    {
        /// <summary>
        /// 指定されたパスのオブジェクトファイルをストリームとして開きます
        /// </summary>
        /// <param name="path">開くオブジェクトファイルのパス</param>
        /// <returns>正しくストリームを開けた場合は Stream のインスタンスを返しますが、開けなかった場合は null を返します</returns>
        Stream OpenRead(string path);
    }
}
