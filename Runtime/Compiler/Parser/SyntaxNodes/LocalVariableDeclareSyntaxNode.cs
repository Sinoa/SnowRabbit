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
    public class LocalVariableDeclareSyntaxNode : SyntaxNode
    {
        public override void Compile(SrCompileContext context)
        {
            var type = context.ToRuntimeType(Children[0].Token.Kind);
            var name = Children[1].Token.Text;
            var expression = Children.Count > 2 ? Children[2] : null;
            if (context.AssemblyData.GetVariableSymbol(name, context.CurrentCompileFunctionName) != null)
            {
                // すでに定義済みローカル変数
                throw new System.Exception();
            }


            var function = context.AssemblyData.GetFunctionSymbol(context.CurrentCompileFunctionName);
            var localSymbol = function.AddOrGetLocalVariable(name, type);


            if (expression == null) return;


            expression.Compile(context);
            SrInstruction instruction = default;
            instruction.Set(OpCode.Str, SrvmProcessor.RegisterAIndex, SrvmProcessor.RegisterBPIndex, 0, -localSymbol.Address);
            context.AddBodyCode(instruction, false);
        }
    }
}
