// zlib/libpng License
//
// Copyright(c) 2019 Sinoa
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
    /// 定数構文を表す構文ノードクラスです
    /// </summary>
    public class LiteralSyntaxNode : SyntaxNode
    {
        public byte StoreRegisterIndex { get; set; } = SrvmProcessor.RegisterAIndex;


        /// <summary>
        /// LiteralSyntaxNode クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="token">対応するトークン</param>
        public LiteralSyntaxNode(in Token token) : base(token)
        {
        }


        public override void Compile(SrCompileContext context)
        {
            SrInstruction instruction = default;
            switch (context.ToRuntimeType(Token.Kind))
            {
                case SrRuntimeType.Integer:
                    instruction.Set(OpCode.Movl, StoreRegisterIndex, 0, 0, (int)Token.Integer);
                    context.AddBodyCode(instruction, false);
                    break;


                case SrRuntimeType.Number:
                    instruction.Set(OpCode.Movl, StoreRegisterIndex, 0, 0, (float)Token.Number);
                    context.AddBodyCode(instruction, false);
                    break;


                case SrRuntimeType.Boolean:
                    var boolValue = Token.Text == "true" ? 1 : 0;
                    instruction.Set(OpCode.Movl, StoreRegisterIndex, 0, 0, boolValue);
                    context.AddBodyCode(instruction, false);
                    break;


                case SrRuntimeType.String:
                    var symbol = context.CreateOrGetStringSymbol(Token.Text);
                    instruction.Set(OpCode.Ldrl, StoreRegisterIndex, 0, 0, symbol.InitialAddress);
                    context.AddBodyCode(instruction, true);
                    break;


                case SrRuntimeType.Object:
                    instruction.Set(OpCode.Movl, StoreRegisterIndex, 0, 0, 0);
                    context.AddBodyCode(instruction, false);
                    break;
            }
        }
    }
}
