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

using System.Collections.Generic;
using System.IO;

namespace SnowRabbit.Compiler.IO
{
    /// <summary>
    /// SnowRabbit コンパイラが使用するスクリプトストレージを文字列として実装したクラスです
    /// </summary>
    public class SrStringScriptStorage : ISrScriptStorage
    {
        // メンバ変数定義
        private readonly Dictionary<string, string> scriptTable = new Dictionary<string, string>();



        /// <summary>
        /// スクリプトコードにスクリプト名を割り当ててスクリプトを設定します
        /// </summary>
        /// <param name="name">割り当てるスクリプト名</param>
        /// <param name="scriptCode">設定するスクリプトコード</param>
        public void SetScript(string name, string scriptCode)
        {
            // そのまま設定する
            scriptTable[name] = scriptCode;
        }


        /// <summary>
        /// 指定されたスクリプト名のスクリプトを削除します
        /// </summary>
        /// <param name="name">削除するスクリプト名</param>
        public void RemoveScript(string name)
        {
            // そのまま削除をする
            scriptTable.Remove(name);
        }


        /// <summary>
        /// 対象のスクリプト名からテキストリーダを開きます
        /// </summary>
        /// <param name="name">開く対象となるスクリプト名</param>
        /// <returns>対象のスクリプト名からテキストリーダを開けた場合は参照を、開けなかった場合は null を返します</returns>
        public TextReader OpenRead(string name)
        {
            // 対象のスクリプト名があればそのままインスタンスを生成して返す
            return scriptTable.TryGetValue(name, out var scriptCode) ? new StringReader(scriptCode) : null;
        }
    }
}