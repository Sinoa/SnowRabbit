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

using System.Collections.Generic;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    public class IfStatementSyntaxNode : SyntaxNode
    {
        private List<int> patchTargetAddressList;



        public override void Compile(SrCompileContext context)
        {
            patchTargetAddressList = (Parent is ElseStatementSyntaxNode elseNode) ? elseNode.patchTargetAddressList : new List<int>();
            var instruction = new SrInstruction();


            var condition = Children[0];
            condition.Compile(context);
            instruction.Set(OpCode.Bnz, SrvmProcessor.RegisterIPIndex, 0, 0, 2);
            context.AddBodyCode(instruction, false);


            patchTargetAddressList.Add(context.BodyCodeList.Count);
            instruction.Set(OpCode.Brl, 0, 0, 0, -1);
            context.AddBodyCode(instruction, false);


            for (int i = 1; i < Children.Count; ++i)
            {
                var child = Children[i];
                if (child is ElseStatementSyntaxNode childElseNode)
                {
                    patchTargetAddressList.Add(context.BodyCodeList.Count);
                    instruction.Set(OpCode.Brl, 0, 0, 0, -1);
                    context.AddBodyCode(instruction, false);


                    childElseNode.patchTargetAddressList = patchTargetAddressList;
                    childElseNode.Compile(context);
                    break; ;
                }


                child.Compile(context);
            }


            PatchAddress(context);
        }


        private void PatchAddress(SrCompileContext context)
        {
            if (Parent is ElseStatementSyntaxNode) return;
            var instruction = new SrInstruction();


            foreach (var targetAddress in patchTargetAddressList)
            {
                instruction.Set(OpCode.Br, SrvmProcessor.RegisterIPIndex, 0, 0, context.BodyCodeList.Count - targetAddress);
                context.UpdateBodyCode(targetAddress, instruction, false);
            }
        }
    }
}
