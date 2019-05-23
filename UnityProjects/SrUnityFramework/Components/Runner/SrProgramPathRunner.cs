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

using UnityEngine;

namespace SrUnityFramework.Components
{
    /// <summary>
    /// パス指定された実行コードを実行するランナーコンポーネントクラスです
    /// </summary>
    [AddComponentMenu("SrUnityFramework/Runner/ProgramPath")]
    public class SrProgramPathRunner : SrRunnerBase
    {
        // インスペクタ公開用メンバ変数定義
        [SerializeField]
        private string programPath = string.Empty;



        /// <summary>
        /// プログラムパスを取得します
        /// </summary>
        /// <param name="programPath">取得したプログラムパスを設定する文字列の参照</param>
        /// <returns>取得に成功した場合は true を、失敗した場合は false を返します</returns>
        protected override bool GetProgramPath(out string programPath)
        {
            // インスペクタの値をそのまま設定して成功を返す
            programPath = this.programPath;
            return true;
        }
    }
}