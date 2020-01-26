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
    /// グローバル変数宣言構文を表す構文ノードクラスです
    /// </summary>
    public class GlobalVariableDeclareSyntaxNode : SyntaxNode
    {
        public override void Compile(SrCompileContext context)
        {
            var type = context.ToRuntimeType(Children[0].Token.Kind);
            var name = Children[1].Token.Text;
            var literal = default(Token);


            if (Children.Count > 2)
            {
                literal = Children[2].Token;
                var literalType = context.ToRuntimeType(literal.Kind);
                if (type != literalType)
                {
                    // 宣言の型とリテラルの型が一致しない
                    throw new System.Exception();
                }
            }


            if (context.AssemblyData.GetGlobalSymbol(name) != null)
            {
                // 既に定義済みの名前
                throw new System.Exception();
            }


            context.CreateGlobalVariableSymbol(type, name, literal);
        }
    }
}
