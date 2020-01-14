// zlib/libpng License
//
// Copyright(c) 2020 Sinoa
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
using SnowRabbit.Compiler.IO;

namespace SnowRabbit.Compiler
{
    /// <summary>
    /// SnowRabbit が提供するコンパイラ機能を提供するクラスです
    /// </summary>
    public class SrCompiler : SrDisposable
    {
        // メンバ変数定義
        private ISrScriptStorage scriptStorage;



        /// <summary>
        /// SrCompiler クラスのインタンスを初期化します
        /// </summary>
        /// <param name="storage">スクリプトを保持しているストレージ</param>
        /// <exception cref="ArgumentNullException">storage が null です</exception>
        public SrCompiler(ISrScriptStorage storage)
        {
            // 参照を受け取る
            scriptStorage = storage ?? throw new ArgumentNullException(nameof(storage));
        }


        /// <summary>
        /// 指定されたパスのスクリプトをコンパイルします
        /// </summary>
        /// <param name="path">コンパイルするスクリプトのパス</param>
        /// <param name="outStream">コンパイルした結果の実行コードを出力するストリーム</param>
        /// <exception cref="ArgumentException">path が null または 空文字列 または 空白文字列 です</exception>
        /// <exception cref="ArgumentNullException">outStream が null です</exception>
        public void Compile(string path, Stream outStream)
        {
        }
    }
}
