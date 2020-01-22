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

using SnowRabbit.RuntimeEngine;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    public class ForStatementSyntaxNode : SyntaxNode
    {
        public override void Compile(SrCompileContext context)
        {
            var initializeExpression = Children[0];
            var conditionalExpression = Children[1];
            var countingExpression = Children[2];
            var instruction = new SrInstruction();


            initializeExpression?.Compile(context);
            instruction.Set(OpCode.Br);
            context.AddBodyCode(instruction, false);

            var forCountingHeadAddress = context.BodyCodeList.Count;
            countingExpression?.Compile(context);


            var forConditionalHeadAddress = context.BodyCodeList.Count;
            conditionalExpression?.Compile(context);


            for (int i = 3; i < Children.Count; ++i)
            {
                Children[i].Compile(context);
            }


            var 
            var jumpOffsetAddress = 0;
        }
    }
}
