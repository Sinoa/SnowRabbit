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
        public List<int> patchTargetAddressList;
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


            for (int i = 1; i < Children.Count; ++i)
            {
                var child = Children[i];
                if (child is ElseStatementSyntaxNode elseNode)
                {
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


            if (rootIfNode)
            {
                if (patchTargetAddressList.Count == 0)
                {
                    instruction.Set(OpCode.Br, SrvmProcessor.RegisterIPIndex, 0, 0, context.BodyCodeList.Count - updateTargetAddress);
                    context.UpdateBodyCode(updateTargetAddress, instruction, false);
                }
                else
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
}
