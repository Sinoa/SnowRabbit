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
using SnowRabbit.Compiler.Parser.SyntaxErrors;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// SnowRabbit の翻訳単位の大元となる構文ノードクラスです
    /// </summary>
    public class CompileUnitSyntaxNode : SyntaxNode
    {
        /// <summary>
        /// この構文ノードが対応する構文ノードを生成します
        /// </summary>
        /// <param name="context">コンパイルする対象となる翻訳単位コンテキスト</param>
        /// <returns>構文ノードを生成出来た場合は構文ノードのインスタンスを、生成出来ない場合は null を返します</returns>
        public static SyntaxNode Create(LocalCompileContext context)
        {
            // 自身の構文ノードを生成する
            var compileUnit = new CompileUnitSyntaxNode();


            // 終端トークンが返されるまでループ
            ref var token = ref context.Lexer.LastReadToken;
            while (token.Kind != TokenKind.EndOfToken)
            {
                // ディレクティブ構文ノードの生成に成功したら
                var directive = DirectiveSyntaxNode.Create(context);
                if (directive != null)
                {
                    // ディレクティブ構文ノードを追加して継続ループ
                    compileUnit.Add(directive);
                    continue;
                }


                // 周辺機器関数宣言構文ノードの生成に成功したら
                var peripheralDeclare = PeripheralDeclareSyntaxNode.Create(context);
                if (peripheralDeclare != null)
                {
                    // 周辺機器関数構文ノードを追加して継続ループ
                    compileUnit.Add(peripheralDeclare);
                    continue;
                }


                // グローバル変数宣言構文ノードの生成に成功したら
                var globalVariableDeclare = GlobalVariableDeclareSyntaxNode.Create(context);
                if (globalVariableDeclare != null)
                {
                    // グローバル変数宣言構文ノードを追加して継続ループ
                    compileUnit.Add(globalVariableDeclare);
                    continue;
                }


                // 関数定義構文ノードの生成に成功したら
                var functionDeclare = FunctionDeclareSyntaxNode.Create(context);
                if (functionDeclare != null)
                {
                    // 関数宣言構文ノードを追加して継続ループ
                    compileUnit.Add(functionDeclare);
                    continue;
                }


                // ここまで到達してしまったら構文的に誤りがあるということになるので構文エラーとして処理する
                context.ThrowSyntaxError(new SrUnknownTokenSyntaxErrorException(ref token));
            }


            // 生成された自身のノードを返す
            return compileUnit;
        }
    }
}
