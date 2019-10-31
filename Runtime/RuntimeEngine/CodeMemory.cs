﻿// zlib/libpng License
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

namespace SnowRabbit.RuntimeEngine
{
    /// <summary>
    /// SnowRabbit のランタイムエンジンが実行するコードを保持するメモリクラスです
    /// </summary>
    internal class CodeMemory
    {
        // メンバ変数定義
        private SrValue[] values;



        public ref SrValue this[int index] => ref values[index];



        /// <summary>
        /// CodeMemory クラスのインスタンスを初期化子ます
        /// </summary>
        /// <param name="values">ランタイム実行コードへの参照</param>
        /// <exception cref="ArgumentNullException">values が null です</exception>
        public CodeMemory(SrValue[] values)
        {
            // 参照を覚えるだけ
            this.values = values ?? throw new ArgumentNullException(nameof(values));
        }
    }
}