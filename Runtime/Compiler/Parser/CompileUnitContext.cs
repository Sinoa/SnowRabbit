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

using System;
using SnowRabbit.Compiler.Lexer;

namespace SnowRabbit.Compiler.Parser
{
    /// <summary>
    /// 翻訳単位毎のコンテキストです
    /// </summary>
    public class CompileUnitContext
    {
        /// <summary>
        /// このコンテキストを持つアナライザコンテキスト
        /// </summary>
        public AnalyzerContext AnalyzerContext { get; }


        /// <summary>
        /// 使用しているレキサ
        /// </summary>
        public TokenReader Lexer { get; }



        /// <summary>
        /// CompileUnitContext クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="lexer">この翻訳で使用するレキサ</param>
        /// <param name="analyzerContext">この翻訳単位を持つアナライザコンテキスト</param>
        /// <exception cref="ArgumentNullException">lexer が null です</exception>
        /// <exception cref="ArgumentNullException">analyzerContext が null です</exception>
        public CompileUnitContext(TokenReader lexer, AnalyzerContext analyzerContext)
        {
            // 参照を受け取る
            AnalyzerContext = analyzerContext ?? throw new ArgumentNullException(nameof(analyzerContext));
            Lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
        }
    }
}