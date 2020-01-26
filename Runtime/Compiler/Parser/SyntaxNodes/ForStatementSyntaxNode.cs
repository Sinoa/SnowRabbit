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

using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    public class ForStatementSyntaxNode : SyntaxNode
    {
        public override void Compile(SrCompileContext context)
        {
            context.EnterNestedBlock("for");


            var initializeExpression = Children[0];
            var conditionalExpression = Children[1];
            var countingExpression = Children[2];
            var instruction = new SrInstruction();


            // for initialize expression code
            initializeExpression?.Compile(context);


            // goto conditional temporary code
            var updateTargetAddress = context.BodyCodeList.Count;
            instruction.Set(OpCode.Br);
            context.AddBodyCode(instruction, false);


            // for counting expression code
            var forCountingHeadAddress = context.BodyCodeList.Count;
            countingExpression?.Compile(context);


            // update goto conditional code
            var forConditionalHeadAddress = context.BodyCodeList.Count;
            instruction.Set(OpCode.Br, SrvmProcessor.RegisterIPIndex, 0, 0, updateTargetAddress - forConditionalHeadAddress);
            context.UpdateBodyCode(updateTargetAddress, instruction, false);


            // for conditional code
            conditionalExpression?.Compile(context);


            // for body code
            for (int i = 3; i < Children.Count; ++i)
            {
                Children[i].Compile(context);
            }


            // goto for counting expression code
            var forTailAddress = context.BodyCodeList.Count;
            instruction.Set(OpCode.Br, SrvmProcessor.RegisterIPIndex, 0, 0, forCountingHeadAddress - forTailAddress);
            context.AddBodyCode(instruction, false);


            context.ExitNestedBlock();
        }
    }
}
