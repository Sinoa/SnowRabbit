// zlib License
// 
// Copyright (c) 2020 Sinoa
// 
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
// 
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software
// in a product, an acknowledgment in the product documentation would be
// appreciated but is not required.
// 
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
// 
// 3. This notice may not be removed or altered from any source
// distribution.

#nullable disable

using System.Collections.Generic;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// else文またはelse if文を表す構文ノードクラスです。
    /// if文の代替分岐として機能し、IfStatementSyntaxNodeと連携します。
    /// </summary>
    /// <remarks>
    /// 子ノード構造:
    /// - else if の場合: Children[0] = IfStatementSyntaxNode
    /// - else の場合: Children[0...n] = else節の文
    /// </remarks>
    public class ElseStatementSyntaxNode : SyntaxNode
    {
        /// <summary>
        /// 分岐終了位置のパッチ対象アドレスリスト。
        /// 親のIfStatementSyntaxNodeから渡され、else if チェインで共有されます。
        /// </summary>
        public List<int> patchTargetAddressList;



        public override void Compile(SrCompileContext context)
        {
            if (Children.Count == 0) return;


            if (Children[0] is IfStatementSyntaxNode ifNode)
            {
                ifNode.patchTargetAddressList = patchTargetAddressList;
            }


            foreach (var node in Children)
            {
                node.Compile(context);
            }
        }
    }
}
