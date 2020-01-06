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

using SnowRabbit.Compiler.Lexer;
using SnowRabbit.Diagnostics.Logging;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 1つの構文を表す構文ノード抽象クラスです
    /// </summary>
    public abstract class SyntaxNode
    {
        // メンバ変数定義
        protected Token token;



        /// <summary>
        /// 現れたトークンへの参照
        /// </summary>
        public ref Token Token => ref token;



        /// <summary>
        /// SyntaxNode クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="token">適応するトークンへの参照</param>
        protected SyntaxNode(in Token token)
        {
            // トークンを受け取る
            this.token = token;
        }


        /// <summary>
        /// この構文ノードに対応する構文を解釈します
        /// </summary>
        /// <param name="context">構文解釈する対象となっている翻訳単位コンテキスト</param>
        /// <returns>解釈した結果の構文ノードを返しますが、正しく解釈出来なかった場合は null を返します。</returns>
        public virtual SyntaxNode Compile(LocalCompileContext context)
        {
            // 既定は null を返す
            SrLogger.Trace(nameof(SyntaxNode), $"'{GetType().Name}' is not implemented 'Compile' method.");
            return null;
        }
    }
}
