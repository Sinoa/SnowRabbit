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
    /// UnityのTextAssetを元に実行コードを実行するランナーコンポーネントクラスです
    /// </summary>
    [AddComponentMenu("SrUnityFramework/Runner/TextAsset")]
    public class SrTextAssetRunner : SrRunnerBase
    {
        // インスペクタ公開用メンバ変数定義
        [SerializeField]
        private TextAsset textAsset = null;



        /// <summary>
        /// プログラムコードを取得します
        /// </summary>
        /// <param name="programCode">取得したプログラムコードを設定するバイト配列の参照</param>
        /// <returns>取得に成功した場合は true を、失敗した場合は false を返します</returns>
        protected override bool GetProgramCode(out byte[] programCode)
        {
            // 何も設定されていなかったら
            if (textAsset == null)
            {
                // 何も出来ないので失敗を返す
                programCode = null;
                return false;
            }


            // TextAssetのバイト配列を設定する
            programCode = textAsset.bytes;
            return true;
        }
    }
}