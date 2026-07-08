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


        /// <summary>
        /// この引数が渡されるパラメータの型。ArgumentListSyntaxNode からコンパイル前に注入されます。
        /// </summary>
        internal SrRuntimeType ExpectedParameterType { get; set; }



        public ArgumentSyntaxNode(in Token token) : base(token)
        {
        }


        public override void Compile(SrCompileContext context)
        {
            var expression = Children[0];
            var valueRegisterIndex = ExpressionSyntaxNode.CompileExpressionValue(expression, context, out var valueType);


            // パラメータの型へ暗黙変換できない引数は型エラー
            if (!ExpressionSyntaxNode.TryEmitImplicitConversion(valueRegisterIndex, valueType, ExpectedParameterType, context))
            {
                throw context.ErrorReporter.InvalidParameterStoreType(expression.Token, valueType, ExpectedParameterType);
            }


            Type = ExpectedParameterType;


            SrInstruction instruction = default;
            instruction.Set(OpCode.Push, valueRegisterIndex);
            context.AddBodyCode(instruction, false);


            // 値はスタックへ退避済みのため、ネストした呼び出しでレジスタを溜め込まないように即時返却する
            context.ReleaseRegisterIndex(valueRegisterIndex);
        }
    }
}