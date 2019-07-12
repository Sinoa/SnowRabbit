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

namespace CarrotCompilerCollection.IO
{
    /// <summary>
    /// 文字列をそのままストレージとして取り扱うスクリプトストレージクラスです
    /// </summary>
    public class StringScriptStorage : IScriptStorage
    {
        // メンバ変数定義
        private Dictionary<string, string> scriptTable;



        /// <summary>
        /// StringScriptStorage クラスのインスタンスを初期化します
        /// </summary>
        public StringScriptStorage()
        {
            // スクリプトテーブルを生成
            scriptTable = new Dictionary<string, string>();
        }


        /// <summary>
        /// ストレージに指定したスクリプト名でスクリプトコードを設定します
        /// </summary>
        /// <param name="scriptName">設定するスクリプト名</param>
        /// <param name="code">設定するスクリプトコード文字列</param>
        public void SetScriptCode(string scriptName, string code)
        {
            // 無条件で上書き
            scriptTable[scriptName] = code;
        }


        /// <summary>
        /// 指定されたスクリプト名からスクリプトコードを取得します
        /// </summary>
        /// <param name="scriptName">取得するスクリプト名</param>
        /// <returns>取得に成功した場合はコード文字列を返しますが、失敗した場合は null を返します</returns>
        public string GetScriptCode(string scriptName)
        {
            // 取得に試みた結果をそのまま返す
            scriptTable.TryGetValue(scriptName, out var code);
            return code;
        }


        /// <summary>
        /// 指定されたスクリプト名のスクリプトコードを削除します
        /// </summary>
        /// <param name="scriptName">削除するスクリプト名</param>
        public void Remove(string scriptName)
        {
            // 削除する
            scriptTable.Remove(scriptName);
        }


        /// <summary>
        /// 指定されたスクリプト名のスクリプトが存在するか確認します
        /// </summary>
        /// <param name="scriptName">確認するスクリプト名</param>
        public bool Exists(string scriptName)
        {
            // キーが存在するかどうかの結果をそのまま返す
            return scriptTable.ContainsKey(scriptName);
        }


        /// <summary>
        /// 指定したスクリプト名からスクリプトのテキストリーダーを開きます
        /// </summary>
        /// <param name="scriptName">開くスクリプトのスクリプト名</param>
        /// <returns>正しくスクリプトを開けた場合はテキストリーダーを返しますが、失敗した場合は null を返します</returns>
        public TextReader Open(string scriptName)
        {
            // 文字列が無効または、指定されたキーが存在しないなら
            if (string.IsNullOrWhiteSpace(scriptName) || !Exists(scriptName))
            {
                // 開けないことを返す
                return null;
            }


            // 文字列リーダーとして返す
            return new StringReader(scriptTable[scriptName]);
        }


        /// <summary>
        /// 指定されたリーダーを閉じます
        /// </summary>
        /// <param name="reader">閉じるテキストリーダー</param>
        public void Close(TextReader reader)
        {
            // そのままDisposeを呼ぶだけ
            reader.Dispose();
        }
    }
}