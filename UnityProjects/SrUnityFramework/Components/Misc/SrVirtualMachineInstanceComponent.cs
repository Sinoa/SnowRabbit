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

using SrUnityFramework.Base;
using UnityEngine;

namespace SrUnityFramework.Components
{
    /// <summary>
    /// Unityのコンポーネントとして雪兎仮想マシンのインスタンスを持つコンポーネントクラスです
    /// </summary>
    [AddComponentMenu("SrUnityFramework/Misc/Instance")]
    public class SrVirtualMachineInstanceComponent : SrVirtualMachineInstanceComponentBase
    {
        // インスペクタ公開用メンバ変数定義
        [SerializeField]
        private int objectMemorySize = 4 << 20;
        [SerializeField]
        private int valueMemorySize = 4 << 20;
        [SerializeField]
        private bool dontDestroy = false;



        /// <summary>
        /// コンポーネントの初期化をします
        /// </summary>
        private void Awake()
        {
            // 仮想マシンのインスタンスを生成して起動する
            Instance = new SrvmUnityMachine(objectMemorySize, valueMemorySize);


            // もし破棄禁止設定をされていたら
            if (dontDestroy)
            {
                // 破棄されないように設定をする
                DontDestroyOnLoad(this);
            }
        }
    }
}