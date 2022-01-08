// zlib/libpng License
//
// Copyright(c) 2021 Sinoa
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
using SnowRabbit.Compiler.Parser.SyntaxNodes;

namespace SnowRabbit.Compiler.Parser.SyntaxTreePrinters
{
    /// <summary>
    /// コンソール（標準出力）に対して構文木の印字を行う機能を提供します
    /// </summary>
    public class ConsoleSyntaxTreePrinter : ISyntaxTreePrinter
    {
        /// <summary>
        /// 指定された構文ノードから構文木の印字を行います
        /// </summary>
        /// <param name="rootNode">印字する構文木のルートノード</param>
        /// <exception cref="ArgumentNullException">rootNode が null です</exception>
        public void Print(SyntaxNode rootNode)
        {
            if (rootNode == null)
            {
                throw new ArgumentNullException(nameof(rootNode));
            }

            Print(rootNode, 0);
        }


        private void Print(SyntaxNode node, int level)
        {
            // print indent
            for (int i = 0; i < level; ++i)
            {
                Console.Write("    ");
            }

            if (node == null)
            {
                Console.Write($"[NULLnode]\n");
                return;
            }

            Console.Write($"[{node.GetType().Name}] {node.Token.Text} ({node.Token.LineNumber}, {node.Token.ColumnNumber})\n");
            foreach (var child in node.Children)
            {
                Print(child, level + 1);
            }
        }
    }
}