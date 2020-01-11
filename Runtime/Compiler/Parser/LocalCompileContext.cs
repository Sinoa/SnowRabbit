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
using SnowRabbit.Compiler.Parser.SyntaxErrors;

namespace SnowRabbit.Compiler.Parser
{
    /// <summary>
    /// 実際に翻訳する単位となる、ローカルコンパイルコンテキストクラスです
    /// </summary>
    public class LocalCompileContext
    {
        /// <summary>
        /// 使用しているレキサ
        /// </summary>
        public TokenReader Lexer { get; }



        /// <summary>
        /// LocalCompileContext クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="lexer">この翻訳で使用するレキサ</param>
        /// <exception cref="ArgumentNullException">lexer が null です</exception>
        /// <exception cref="ArgumentNullException">globalCompileContext が null です</exception>
        public LocalCompileContext(TokenReader lexer)
        {
            // 参照を受け取る
            Lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
        }


        /// <summary>
        /// 構文エラー例外をスローします
        /// </summary>
        /// <param name="exception">スローする構文エラー例外</param>
        public void ThrowSyntaxError(SrSyntaxErrorException exception)
        {
            // 渡された例外を投げる
            throw exception;
        }
    }
}