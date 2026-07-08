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

using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// while文を表す構文ノードクラスです。
    /// 条件が真の間繰り返すループ構造を表現します。
    /// </summary>
    /// <remarks>
    /// 子ノード構造:
    /// - Children[0]: 条件式
    /// - Children[1...n]: ループ本体の文
    /// 
    /// 生成されるコード構造:
    /// 1. 条件式（falseならループ終了へジャンプ）
    /// 2. ループ本体
    /// 3. 条件式へジャンプ
    /// </remarks>
    public class WhileStatementSyntaxNode : SyntaxNode
    {
        public override void Compile(SrCompileContext context)
        {
            context.EnterNestedBlock("while");


            var whileHeadAddress = context.BodyCodeList.Count;
            var conditionRegisterIndex = ExpressionSyntaxNode.CompileConditionExpressionValue(Children[0], context);
            var instruction = new SrInstruction();
            instruction.Set(OpCode.Bnz, SrvmProcessor.RegisterIPIndex, conditionRegisterIndex, 0, 2);
            context.AddBodyCode(instruction, false);
            instruction.Set(OpCode.Br, SrvmProcessor.RegisterIPIndex, 0, 0, 0);
            context.AddBodyCode(instruction, false);
            var updateTargetAddress = context.BodyCodeList.Count - 1;


            for (int i = 1; i < Children.Count; ++i)
            {
                Children[i].CompileAsStatement(context);
            }


            var whileHeadOffsetAddress = context.BodyCodeList.Count - whileHeadAddress;
            instruction.Set(OpCode.Br, SrvmProcessor.RegisterIPIndex, 0, 0, -whileHeadOffsetAddress);
            context.AddBodyCode(instruction, false);


            var whileTailAddress = context.BodyCodeList.Count;
            instruction.Set(OpCode.Br, SrvmProcessor.RegisterIPIndex, 0, 0, whileTailAddress - updateTargetAddress);
            context.UpdateBodyCode(updateTargetAddress, instruction, false);


            context.ExitNestedBlock();
        }
    }
}
