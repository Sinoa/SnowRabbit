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

using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// SnowRabbit の翻訳単位の大元となる構文ノードクラスです
    /// </summary>
    public class CompileUnitSyntaxNode : SyntaxNode
    {
        public override void Compile(SrCompileContext context)
        {
            foreach (var child in Children)
            {
                child.Compile(context);
            }


            if (Parent == null)
            {
                CompileStartupCode(context);
            }
        }


        private void CompileStartupCode(SrCompileContext context)
        {
            context.EnterFunctionCompile(SrRuntimeType.Void, "___init");
            CompilePeripheralLoadCode(context);
            CompileGlobalVariableInitializeCode(context);
            CompileCallMainFunctionCode(context);
            CompileStopCode(context);
            context.ExitFunctionCompile();
        }


        private void CompileGlobalVariableInitializeCode(SrCompileContext context)
        {
            foreach (var symbol in context.AssemblyData.GetSymbolAll<SrGlobalVariableSymbol>())
            {
                // 初期化リテラルを持たないグローバル変数（ペリフェラル関数用変数を含む）は既定値のままにする
                var literal = symbol.InitializeLiteral;
                if (literal.Text == null) continue;


                var instruction = default(SrInstruction);
                switch (symbol.Type)
                {
                    case SrRuntimeType.Integer:
                        instruction.Set(OpCode.Movl, SrvmProcessor.RegisterAIndex, 0, 0, (int)literal.Integer);
                        context.AddBodyCode(instruction, false);
                        break;


                    case SrRuntimeType.Number:
                        instruction.Set(OpCode.Fmovl, SrvmProcessor.RegisterAIndex, 0, 0, (float)literal.Number);
                        context.AddBodyCode(instruction, false);
                        break;


                    case SrRuntimeType.Boolean:
                        instruction.Set(OpCode.Movl, SrvmProcessor.RegisterAIndex, 0, 0, literal.Text == "true" ? 1 : 0);
                        context.AddBodyCode(instruction, false);
                        break;


                    case SrRuntimeType.String:
                        var stringSymbol = context.CreateOrGetStringSymbol(literal.Text);
                        instruction.Set(OpCode.Ldrl, SrvmProcessor.RegisterAIndex, 0, 0, stringSymbol.InitialAddress);
                        context.AddBodyCode(instruction, true);
                        break;


                    default:
                        // object 型の初期化子は null のみなので、既定値のまま何もしない
                        continue;
                }


                instruction.Set(OpCode.Strl, SrvmProcessor.RegisterAIndex, 0, 0, symbol.InitialAddress);
                context.AddBodyCode(instruction, true);
            }
        }


        private void CompilePeripheralLoadCode(SrCompileContext context)
        {
            foreach (var peripheral in context.AssemblyData.GetSymbolAll<SrPeripheralFunctionSymbol>())
            {
                var globalVariableSymbol = context.AssemblyData.GetVariableSymbol(peripheral.PeripheralGlobalVariableName, null);
                var peripheralNameSymbol = context.CreateOrGetStringSymbol(peripheral.PeripheralName);
                var peripheralFunctionNameSymbol = context.CreateOrGetStringSymbol(peripheral.PeripheralFunctionName);


                var instruction = default(SrInstruction);
                instruction.Set(OpCode.Ldrl, SrvmProcessor.RegisterBIndex, 0, 0, peripheralNameSymbol.InitialAddress);
                context.AddBodyCode(instruction, true);
                instruction.Set(OpCode.Ldrl, SrvmProcessor.RegisterCIndex, 0, 0, peripheralFunctionNameSymbol.InitialAddress);
                context.AddBodyCode(instruction, true);
                instruction.Set(OpCode.Gpf, SrvmProcessor.RegisterAIndex, SrvmProcessor.RegisterBIndex, SrvmProcessor.RegisterCIndex);
                context.AddBodyCode(instruction, false);
                instruction.Set(OpCode.Strl, SrvmProcessor.RegisterAIndex, 0, 0, globalVariableSymbol.InitialAddress);
                context.AddBodyCode(instruction, true);
            }
        }


        private void CompileCallMainFunctionCode(SrCompileContext context)
        {
            var mainFunctionSymbol = context.AssemblyData.GetFunctionSymbol("main");
            if (mainFunctionSymbol == null)
            {
                // メイン関数がない
                throw context.ErrorReporter.MainFunctionNotFound(Token);
            }


            var instruction = default(SrInstruction);
            instruction.Set(OpCode.Calll, 0, 0, 0, mainFunctionSymbol.InitialAddress);
            context.AddBodyCode(instruction, true);
        }


        private void CompileStopCode(SrCompileContext context)
        {
            var instruction = default(SrInstruction);
            instruction.Set(OpCode.Halt);
            context.AddBodyCode(instruction, false);
            instruction.Set(OpCode.Br, SrvmProcessor.RegisterIPIndex, 0, 0, -1);
            context.AddBodyCode(instruction, false);
        }
    }
}
