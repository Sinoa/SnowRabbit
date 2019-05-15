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

/*
asm-script-unit
    : {'#' directive} {operation}

directive
    : 'const' const-string-define
    | 'global' global-var-define
    <EndOfLine>

const-string-define
    : integer identifier

global-var-define
    : identifier

operation
    : op-code [argument-list] <EndOfLine>

op-code
    : identifier

argument-list
    : argument {',' argument}

argument
    : identifier
    | integer
*/

using System;

namespace CarrotAssemblerLib
{
    /// <summary>
    /// Carrot アセンブリの構文を解析するクラスです
    /// </summary>
    public class CarrotAssemblyParser
    {
        // メンバ変数定義
        private TokenReader lexer;
        private BinaryCodeBuilder builder;



        /// <summary>
        /// CarrotAssemblyParser クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="lexer">トークンを取り出すためのレキサ</param>
        /// <param name="builder">実行コードを構築するためのビルダ</param>
        /// <exception cref="ArgumentNullException">lexer が null です</exception>
        /// <exception cref="ArgumentNullException">builder が null です</exception>
        public CarrotAssemblyParser(TokenReader lexer, BinaryCodeBuilder builder)
        {
            // トークンリーダーの参照を覚える
            this.lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
            this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }


        /// <summary>
        /// このアセンブリパーサに設定されている情報を元にアセンブルして実行コードを生成します。
        /// </summary>
        public void Assemble()
        {
            throw new NotImplementedException();
        }
    }



    /// <summary>
    /// SnowRabbit 仮想マシン用の実行コードを構築するクラスです
    /// </summary>
    public class BinaryCodeBuilder
    {
    }
}