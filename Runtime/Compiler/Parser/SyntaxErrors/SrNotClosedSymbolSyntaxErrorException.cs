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

using System;
using System.Runtime.Serialization;

namespace SnowRabbit.Compiler.Parser.SyntaxErrors
{
    /// <summary>
    /// SnowRabbit コンパイラの対になる記号で閉じていないエラー例外クラスです
    /// </summary>
    [Serializable]
    public class SrNotClosedSymbolSyntaxErrorException : SrSyntaxErrorException
    {
        // メンバ変数定義
        private readonly string message;


        /// <summary>
        /// 例外メッセージの内容を表します
        /// </summary>
        public override string Message => message;



        /// <summary>
        /// SrNotClosedSymbolSyntaxErrorException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="openSymbol">対になる記号のオープン記号文字列</param>
        /// <param name="closeSymbol">対になる記号のクローズ記号文字列</param>
        public SrNotClosedSymbolSyntaxErrorException(string openSymbol, string closeSymbol)
        {
            // メッセージを作る
            message = $"'{openSymbol}' に対応する '{closeSymbol}' がありません";
        }


        /// <summary>
        /// SrNotClosedSymbolSyntaxErrorException クラスのインスタンスを初期化します
        /// </summary>
        public SrNotClosedSymbolSyntaxErrorException()
        {
        }


        /// <summary>
        /// SrNotClosedSymbolSyntaxErrorException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="message">例外に設定するメッセージ</param>
        public SrNotClosedSymbolSyntaxErrorException(string message) : base(message)
        {
        }


        /// <summary>
        /// SrNotClosedSymbolSyntaxErrorException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="message">例外に設定するメッセージ</param>
        /// <param name="inner">この例外を発生させる原因となった例外</param>
        public SrNotClosedSymbolSyntaxErrorException(string message, Exception inner) : base(message, inner)
        {
        }


        /// <summary>
        /// シリアル化したデータを使用して SrNotClosedSymbolSyntaxErrorException クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">スローされている例外に関するシリアル化済みオブジェクトデータを保持している SerializationInfo</param>
        /// <param name="context">転送元または転送先についてのコンテキスト情報を含む StreamingContext</param>
        protected SrNotClosedSymbolSyntaxErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
