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
    /// スクリプト関数定義構文を表す構文ノードクラスです
    /// </summary>
    public class FunctionDeclareSyntaxNode : SyntaxNode
    {
        public override void Compile(SrCompileContext context)
        {
            var returnType = context.ToRuntimeType(Children[0].Token.Kind);
            var functionName = Children[1].Token.Text;
            var parameterList = Children[2];
            if (context.AssemblyData.GetGlobalSymbol(functionName) != null)
            {
                // 既に定義済みの名前
                throw new System.Exception();
            }


            var symbol = context.EnterFunctionCompile(returnType, functionName);
            AddParameter(symbol, parameterList, context);


            for (int i = 3; i < Children.Count; ++i)
            {
                Children[i].Compile(context);
            }


            CompileFunctionEnterCode(context, symbol);
            CompileFunctionLeaveCode(context, symbol);


            context.ExitFunctionCompile();
        }


        private void AddParameter(SrScriptFunctionSymbol symbol, SyntaxNode parameterList, SrCompileContext context)
        {
            if (parameterList == null) return;
            foreach (var parameter in parameterList.Children)
            {
                var type = context.ToRuntimeType(parameter.Children[0].Token.Kind);
                var name = parameter.Children[1].Token.Text;
                symbol.AddOrGetParameter(name, type);
            }
        }


        private void CompileFunctionEnterCode(SrCompileContext context, SrScriptFunctionSymbol symbol)
        {
            // push rbp
            // mov rbp, rsp
            // subl rsp, rsp, LocalVarCount + UsedRegisterCount
            // UsedRegister backup code
            // str rax, rbp[LocalVarCount + ...]
            // ...
            var rsp = SrvmProcessor.RegisterSPIndex;
            var rbp = SrvmProcessor.RegisterBPIndex;
            var instruction = default(SrInstruction);
            instruction.Set(OpCode.Push, rbp);
            context.AddHeadCode(instruction, false);
            instruction.Set(OpCode.Mov, rbp, rsp);
            context.AddHeadCode(instruction, false);
            instruction.Set(OpCode.Subl, rsp, rsp, 0, symbol.LocalVariableTable.Count + symbol.UsedRegisterSet.Count);
            context.AddHeadCode(instruction, false);

            int count = 0;
            foreach (var targetRegister in symbol.UsedRegisterSet)
            {
                var backupAddressOffset = -symbol.LocalVariableTable.Count - count - 1;
                instruction.Set(OpCode.Str, targetRegister, rbp, 0, backupAddressOffset);
                context.AddHeadCode(instruction, false);
                count++;
            }
        }


        private void CompileFunctionLeaveCode(SrCompileContext context, SrScriptFunctionSymbol symbol)
        {
            // mov r29, rax (if non void return function)
            // UsedRegister restore code
            // addl rsp, rsp, LocalVarCount + UsedRegisterCount
            // mov rsp, rbp
            // pop rbp
            // ret
            var rax = SrvmProcessor.RegisterAIndex;
            var rsp = SrvmProcessor.RegisterSPIndex;
            var rbp = SrvmProcessor.RegisterBPIndex;
            var instruction = default(SrInstruction);
            if (symbol.ReturnType != SrRuntimeType.Void)
            {
                var r29 = SrvmProcessor.RegisterR29Index;
                instruction.Set(OpCode.Mov, r29, rax);
                context.AddTailCode(instruction, false);
            }


            int count = 0;
            foreach (var targetRegister in symbol.UsedRegisterSet)
            {
                var backupAddressOffset = -symbol.LocalVariableTable.Count - count - 1;
                instruction.Set(OpCode.Ldr, targetRegister, rbp, 0, backupAddressOffset);
                context.AddTailCode(instruction, false);
                count++;
            }


            instruction.Set(OpCode.Addl, rsp, rsp, 0, symbol.LocalVariableTable.Count + symbol.UsedRegisterSet.Count);
            context.AddTailCode(instruction, false);
            instruction.Set(OpCode.Mov, rsp, rsp);
            context.AddTailCode(instruction, false);
            instruction.Set(OpCode.Pop, rbp);
            context.AddTailCode(instruction, false);
            instruction.Set(OpCode.Ret);
            context.AddTailCode(instruction, false);

        }
    }
}
