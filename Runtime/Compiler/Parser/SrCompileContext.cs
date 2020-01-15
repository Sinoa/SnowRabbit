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

using SnowRabbit.Compiler.Assembler;

namespace SnowRabbit.Compiler.Parser
{
    /// <summary>
    /// 構文ノードによるコンパイルコンテキストを持つクラスです
    /// </summary>
    public class SrCompileContext
    {
        /// <summary>
        /// 現在のコンテキストが持っているアセンブリデータ
        /// </summary>
        public SrAssemblyData AssemblyData { get; }


        /// <summary>
        /// 現在コンパイルをしている関数名
        /// </summary>
        public string CurrentCompileFunctionName { get; private set; }



        /// <summary>
        /// SrCompileContext クラスのインスタンスを初期化します
        /// </summary>
        public SrCompileContext()
        {
            // 諸々初期化
            AssemblyData = new SrAssemblyData();
        }
    }
}
