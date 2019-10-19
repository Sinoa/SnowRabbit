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
    /// スクリプトをロードする実装を提供します
    /// </summary>
    public interface IScriptStorage
    {
        /// <summary>
        /// 指定したスクリプト名からスクリプトのテキストリーダーを開きます
        /// </summary>
        /// <param name="scriptName">開くスクリプトのスクリプト名</param>
        /// <returns>開いたテキストリーダーを返します</returns>
        /// <exception cref="ArgumentNullException">scriptName が null です</exception>
        /// <exception cref="ScriptNotFoundException">スクリプト 'scriptName' が見つかりませんでした。</exception>
        TextReader Open(string scriptName);

        /// <summary>
        /// 指定したテキストリーダーを閉じます
        /// </summary>
        /// <param name="reader">閉じるテキストリーダ</param>
        void Close(TextReader reader);
    }
}