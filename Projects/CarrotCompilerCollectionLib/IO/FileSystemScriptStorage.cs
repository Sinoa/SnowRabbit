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

namespace CarrotCompilerCollection.IO
{
    /// <summary>
    /// ファイルシステムによるスクリプトストレージクラスです
    /// </summary>
    public class FileSystemScriptStorage : IScriptStorage
    {
        /// <summary>
        /// 指定されたスクリプト名のスクリプトを開きます
        /// </summary>
        /// <param name="scriptFilePath">スクリプトが存在するスクリプトファイルパス</param>
        /// <returns>正しくスクリプトを開けた場合はテキストリーダーを返しますが、失敗した場合は null を返します</returns>
        public TextReader Open(string scriptFilePath)
        {
            // 文字列が無効または、指定されたファイルが存在しないなら
            if (string.IsNullOrWhiteSpace(scriptFilePath) || !File.Exists(scriptFilePath))
            {
                // 開けないことを返す
                return null;
            }


            // ストリームリーダーとして返す
            return new StreamReader(scriptFilePath);
        }


        /// <summary>
        /// 指定されたリーダーを閉じます
        /// </summary>
        /// <param name="reader">閉じるテキストリーダー</param>
        public void Close(TextReader reader)
        {
            // 単純にDisposeを呼ぶだけ
            reader.Dispose();
        }
    }
}