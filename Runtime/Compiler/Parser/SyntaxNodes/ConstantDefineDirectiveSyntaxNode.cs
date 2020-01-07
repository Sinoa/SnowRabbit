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

using SnowRabbit.Compiler.Lexer;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 定数定義構文を表す構文ノードクラスです
    /// </summary>
    public class ConstantDefineDirectiveSyntaxNode : SyntaxNode
    {
        /// <summary>
        /// この構文ノードが対応する構文ノードを生成します
        /// </summary>
        /// <param name="context">コンパイルする対象となる翻訳単位コンテキスト</param>
        /// <returns>構文ノードを生成出来た場合は構文ノードのインスタンスを、生成出来ない場合は null を返します</returns>
        public static SyntaxNode Create(LocalCompileContext context)
        {
            // const トークンではないなら構文ノードは生成出来ない
            ref var token = ref context.Lexer.LastReadToken;
            if (token.Kind != SrTokenKind.Const) return null;


            // 定数型、定数名、定数値の構文ノードを生成する
            var constantType = ConstantTypesSyntaxNode.Create(context);
            var constantName = ConstantVarNameSyntaxNode.Create(context);
            var constantValue = ConstantValueSyntaxNode.Create(context);


            // 生成したノードを追加する
            var constantDefine = new ConstantDefineDirectiveSyntaxNode();
            constantDefine.Add(constantType);
            constantDefine.Add(constantName);
            constantDefine.Add(constantValue);


            // 自身を返す
            return constantDefine;
        }
    }
}
