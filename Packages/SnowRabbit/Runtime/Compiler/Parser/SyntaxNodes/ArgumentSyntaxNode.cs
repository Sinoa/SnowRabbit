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

using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 引数構文を表す構文ノードクラスです
    /// </summary>
    public class ArgumentSyntaxNode : SyntaxNode
    {
        public SrRuntimeType Type { get; private set; }



        public ArgumentSyntaxNode(in Token token) : base(token)
        {
        }


        public override void Compile(SrCompileContext context)
        {
            var expression = Children[0];
            expression.Compile(context);


            SrInstruction instruction = default;
            if (expression is FunctionCallSyntaxNode functionCallSyntaxNode)
            {
                Type = context.AssemblyData.GetFunctionSymbol(functionCallSyntaxNode.FunctionName).ReturnType;
                instruction.Set(OpCode.Mov, SrvmProcessor.RegisterAIndex, SrvmProcessor.RegisterR29Index);
                context.AddBodyCode(instruction, false);
            }
            else
            {
                Type = ((ExpressionSyntaxNode)expression).ResultType;
            }


            instruction.Set(OpCode.Push, SrvmProcessor.RegisterAIndex);
            context.AddBodyCode(instruction, false);
        }
    }
}