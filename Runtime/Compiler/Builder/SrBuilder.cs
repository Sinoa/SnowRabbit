﻿// zlib/libpng License
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
using SnowRabbit.Compiler.Parser.SyntaxNodes;

namespace SnowRabbit.Compiler.Builder
{
    /// <summary>
    /// SrParser によって生成された構文木から、アセンブリコードに対応するコード生成を行うビルダークラスです
    /// </summary>
    public class SrBuilder
    {
        /// <summary>
        /// 指定されたルート構文ノードからアセンブリコードを構築します
        /// </summary>
        /// <param name="rootNode">構文解析されたルート構文ノード</param>
        /// <returns>正しく構文の意味をなして構築が可能な場合は生成されたアセンブリコードを返しますが、意味をなしていない場合は null を返します</returns>
        public SrAssemblyCode Build(SyntaxNode rootNode)
        {
            throw new System.NotImplementedException();
        }
    }
}
