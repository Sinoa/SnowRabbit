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

using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 関数呼び出し構文の構文ノードクラスです
    /// </summary>
    public class FunctionCallSyntaxNode : SyntaxNode
    {
        public string FunctionName { get; private set; }



        public override void Compile(SrCompileContext context)
        {
            FunctionName = Children[0].Token.Text;
            var argumentList = Children[1];

            var functionSymbol = context.AssemblyData.GetFunctionSymbol(FunctionName);
            if (functionSymbol == null)
            {
                throw context.ErrorReporter.UnknownSymbol(Children[0].Token);
            }

            if (argumentList != null)
            {
                argumentList.Compile(context);
            }

            var instruction = default(SrInstruction);
            if (functionSymbol is SrScriptFunctionSymbol)
            {
                instruction.Set(OpCode.Calll, 0, 0, 0, functionSymbol.InitialAddress);
                context.AddBodyCode(instruction, true);
            }
            else if (functionSymbol is SrPeripheralFunctionSymbol peripheralFunction)
            {
                var globalVariableAddress = context.AssemblyData.GetVariableSymbol(peripheralFunction.PeripheralGlobalVariableName, null).InitialAddress;
                instruction.Set(OpCode.Ldrl, SrvmProcessor.RegisterR29Index, 0, 0, globalVariableAddress);
                context.AddBodyCode(instruction, true);
                instruction.Set(OpCode.Cpf, SrvmProcessor.RegisterR29Index, SrvmProcessor.RegisterR29Index);
                context.AddBodyCode(instruction, false);
            }
            else
            {
                throw context.ErrorReporter.UnknownSymbol(Children[0].Token);
            }


            if (argumentList != null)
            {
                instruction.Set(OpCode.Addl, SrvmProcessor.RegisterSPIndex, SrvmProcessor.RegisterSPIndex, 0, argumentList.Children.Count);
                context.AddBodyCode(instruction, false);
            }
        }
    }
}
