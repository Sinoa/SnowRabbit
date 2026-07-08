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
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// if文を表す構文ノードクラスです。
    /// 条件分岐（if-else if-else）の構造を保持し、分岐命令を生成します。
    /// </summary>
    /// <remarks>
    /// 子ノード構造:
    /// - Children[0]: 条件式
    /// - Children[1...n-1]: then節の文
    /// - Children[n]: else節（ElseStatementSyntaxNode、存在する場合）
    /// </remarks>
    public class IfStatementSyntaxNode : SyntaxNode
    {
        /// <summary>
        /// 分岐終了位置のパッチ対象アドレスリスト。
        /// if-else if-else チェインで共有され、最終的な分岐先アドレスをバックパッチするために使用されます。
        /// </summary>
        public List<int> patchTargetAddressList;

        /// <summary>
        /// このノードがif-elseチェインのルート（最初のif）かどうかを示します
        /// </summary>
        private bool rootIfNode;



        public override void Compile(SrCompileContext context)
        {
            if (patchTargetAddressList == null)
            {
                patchTargetAddressList = new List<int>();
                rootIfNode = true;
            }


            var instruction = new SrInstruction();


            var condition = Children[0];
            condition.Compile(context);
            if (condition is FunctionCallSyntaxNode)
            {
                instruction.Set(OpCode.Mov, SrvmProcessor.RegisterAIndex, SrvmProcessor.RegisterR29Index);
                context.AddBodyCode(instruction, false);
            }

            instruction.Set(OpCode.Bnz, SrvmProcessor.RegisterIPIndex, 0, 0, 2);
            context.AddBodyCode(instruction, false);
            var updateTargetAddress = context.BodyCodeList.Count;
            instruction.Set(OpCode.Br);
            context.AddBodyCode(instruction, false);


            var hasElse = false;
            for (int i = 1; i < Children.Count; ++i)
            {
                var child = Children[i];
                if (child is ElseStatementSyntaxNode elseNode)
                {
                    hasElse = true;
                    patchTargetAddressList.Add(context.BodyCodeList.Count);
                    instruction.Set(OpCode.Br);
                    context.AddBodyCode(instruction, false);


                    instruction.Set(OpCode.Br, SrvmProcessor.RegisterIPIndex, 0, 0, context.BodyCodeList.Count - updateTargetAddress);
                    context.UpdateBodyCode(updateTargetAddress, instruction, false);


                    elseNode.patchTargetAddressList = patchTargetAddressList;
                    elseNode.Compile(context);
                    break;
                }


                child.Compile(context);
            }


            // else 節を持たない場合は、条件偽時の分岐先をチェイン終端の一括パッチに委ねる
            // （else if チェイン途中の if でも未パッチの分岐命令が残らないようにするため）
            if (!hasElse)
            {
                patchTargetAddressList.Add(updateTargetAddress);
            }


            if (rootIfNode)
            {
                foreach (var targetAddress in patchTargetAddressList)
                {
                    instruction.Set(OpCode.Br, SrvmProcessor.RegisterIPIndex, 0, 0, context.BodyCodeList.Count - targetAddress);
                    context.UpdateBodyCode(targetAddress, instruction, false);
                }
            }
        }
    }
}
