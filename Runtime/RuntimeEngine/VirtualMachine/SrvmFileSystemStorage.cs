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

using System;
using System.IO;
using SnowRabbit.Diagnostics.Logging;

namespace SnowRabbit.RuntimeEngine.VirtualMachine
{
    /// <summary>
    /// ホストマシンのファイルシステムを使ったストレージクラスです
    /// </summary>
    public class SrvmFileSystemStorage : SrvmStorage
    {
        /// <summary>
        /// 指定されたパスのファイルをストリームとして開きます
        /// </summary>
        /// <param name="path">開くファイルへのパス</param>
        /// <returns>正しくストリームを開けた場合は Stream インスタンスを返しますが、開けなかった場合は null を返します</returns>
        /// <exception cref="ArgumentException">path が null または 空文字列 または 空白文字列 です</exception>
        public override Stream Open(string path)
        {
            // 取り扱えない文字列を渡されたら
            if (string.IsNullOrWhiteSpace(path))
            {
                // 取り扱えない例外を吐く
                throw new ArgumentException("path が null または 空文字列 または 空白文字列 です");
            }


            // 指定されたファイルパスにファイルが存在しないなら
            if (!File.Exists(path))
            {
                // エラーログ吐いてnullを返す
                SrLogger.Error(SharedString.LogTag.SR_VM_FS_STORAGE, $"File '{path}' not found.");
                return null;
            }


            // 存在しているなら16KBバッファでファイルを開く（16KBバッファは iOS 向けの小さく僅かな対策）
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 16 << 10);
        }
    }
}
