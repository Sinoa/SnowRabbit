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
using System.Runtime.Serialization;

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
        private ParserLogger logger;



        /// <summary>
        /// CarrotAssemblyParser クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="lexer">トークンを取り出すためのレキサ</param>
        /// <param name="builder">実行コードを構築するためのビルダ</param>
        /// <param name="logger">構文解析中のログを出力するロガー</param>
        /// <exception cref="ArgumentNullException">lexer が null です</exception>
        /// <exception cref="ArgumentNullException">builder が null です</exception>
        /// <exception cref="ArgumentNullException">logger が null です</exception>
        public CarrotAssemblyParser(TokenReader lexer, BinaryCodeBuilder builder, ParserLogger logger)
        {
            // 必要なサブシステムの参照を覚える
            this.lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
            this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));


            // 改行コードをトークンとして認める
            lexer.AllowEndOfLineToken = true;
        }


        /// <summary>
        /// このアセンブリ構文解析に設定されている情報を元にアセンブルして実行コードを生成します。
        /// </summary>
        /// <exception cref="CarrotParseException">構文解析中に問題が発生しました</exception>
        public void Assemble()
        {
            // 最初のトークンを取り出す
            lexer.ReadNextToken(out var token);


            // シャープトークンが来る間はループ
            while (token.Kind == TokenKind.Sharp)
            {
                // ディレクティブの構文を解析する
                ParseDirective();
            }
        }


        /// <summary>
        /// ディレクティブの構文を解析します
        /// </summary>
        private void ParseDirective()
        {
        }
    }



    /// <summary>
    /// SnowRabbit 仮想マシン用の実行コードを構築するクラスです
    /// </summary>
    public class BinaryCodeBuilder
    {
    }



    #region ParserLogCode
    /// <summary>
    /// 構文解析中に出力されるログコードを表します
    /// </summary>
    public enum ParserLogCode : ulong
    {
        /// <summary>
        /// 通常のログ出力コード
        /// </summary>
        InfoLog = 0,
    }
    #endregion



    #region ParserLogger
    /// <summary>
    /// 構文解析ロガー抽象クラスです
    /// </summary>
    public abstract class ParserLogger
    {
        /// <summary>
        /// 通常のログを書き込みます
        /// </summary>
        /// <param name="lineNumber">ログを出すタイミングになった行番号。ただし、必ず正確な位置を示すことはありません。</param>
        /// <param name="columnNumber">ログを出すタイミングになった列番号。ただし、必ず正確な位置を示すことはありません。</param>
        /// <param name="code">構文解析がログを出力する要因となったコード</param>
        /// <param name="message">構文解析が出力するメッセージ</param>
        public abstract void WriteLog(int lineNumber, int columnNumber, ParserLogCode code, string message);


        /// <summary>
        /// 警告のログを書き込みます
        /// </summary>
        /// <param name="lineNumber">ログを出すタイミングになった行番号。ただし、必ず正確な位置を示すことはありません。</param>
        /// <param name="columnNumber">ログを出すタイミングになった列番号。ただし、必ず正確な位置を示すことはありません。</param>
        /// <param name="code">構文解析がログを出力する要因となったコード</param>
        /// <param name="message">構文解析が出力するメッセージ</param>
        public abstract void WriteWarning(int lineNumber, int columnNumber, ParserLogCode code, string message);


        /// <summary>
        /// エラーのログを書き込みます
        /// </summary>
        /// <param name="lineNumber">ログを出すタイミングになった行番号。ただし、必ず正確な位置を示すことはありません。</param>
        /// <param name="columnNumber">ログを出すタイミングになった列番号。ただし、必ず正確な位置を示すことはありません。</param>
        /// <param name="code">構文解析がログを出力する要因となったコード</param>
        /// <param name="message">構文解析が出力するメッセージ</param>
        public abstract void WriteError(int lineNumber, int columnNumber, ParserLogCode code, string message);
    }



    /// <summary>
    /// コンソール向け構文解析ロガー抽象クラスです
    /// </summary>
    public class ConsoleParserLogger : ParserLogger
    {
        /// <summary>
        /// 通常のログを書き込みます
        /// </summary>
        /// <param name="lineNumber">ログを出すタイミングになった行番号。ただし、必ず正確な位置を示すことはありません。</param>
        /// <param name="columnNumber">ログを出すタイミングになった列番号。ただし、必ず正確な位置を示すことはありません。</param>
        /// <param name="code">構文解析がログを出力する要因となったコード</param>
        /// <param name="message">構文解析が出力するメッセージ</param>
        public override void WriteLog(int lineNumber, int columnNumber, ParserLogCode code, string message)
        {
            // カラーをリセットしてログ出力
            Console.ResetColor();
            Console.WriteLine($"({lineNumber}, {columnNumber}) info C{(ulong)code}: {message}");
        }


        /// <summary>
        /// 警告のログを書き込みます
        /// </summary>
        /// <param name="lineNumber">ログを出すタイミングになった行番号。ただし、必ず正確な位置を示すことはありません。</param>
        /// <param name="columnNumber">ログを出すタイミングになった列番号。ただし、必ず正確な位置を示すことはありません。</param>
        /// <param name="code">構文解析がログを出力する要因となったコード</param>
        /// <param name="message">構文解析が出力するメッセージ</param>
        public override void WriteWarning(int lineNumber, int columnNumber, ParserLogCode code, string message)
        {
            // 警告色に設定してログ出力
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"({lineNumber}, {columnNumber}) warning C{(ulong)code}: {message}");
        }


        /// <summary>
        /// エラーのログを書き込みます
        /// </summary>
        /// <param name="lineNumber">ログを出すタイミングになった行番号。ただし、必ず正確な位置を示すことはありません。</param>
        /// <param name="columnNumber">ログを出すタイミングになった列番号。ただし、必ず正確な位置を示すことはありません。</param>
        /// <param name="code">構文解析がログを出力する要因となったコード</param>
        /// <param name="message">構文解析が出力するメッセージ</param>
        public override void WriteError(int lineNumber, int columnNumber, ParserLogCode code, string message)
        {
            // エラー色に設定してログ出力
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"({lineNumber}, {columnNumber}) warning C{(ulong)code}: {message}");
        }
    }
    #endregion



    #region Exception
    /// <summary>
    /// 構文解析例外が発生した時の例外クラスです
    /// </summary>
    [Serializable]
    public class CarrotParseException : Exception
    {
        /// <summary>
        /// CarrotParseException のインスタンスを初期化します
        /// </summary>
        public CarrotParseException() : base()
        {
        }


        /// <summary>
        /// CarrotParseException のインスタンスを初期化します
        /// </summary>
        /// <param name="message">発生した例外のメッセージ</param>
        public CarrotParseException(string message) : base(message)
        {
        }


        /// <summary>
        /// CarrotParseException のインスタンスを初期化します
        /// </summary>
        /// <param name="message">発生した例外のメッセージ</param>
        /// <param name="innerException">この例外を発生させた原因の例外</param>
        public CarrotParseException(string message, Exception innerException) : base(message, innerException)
        {
        }


        /// <summary>
        /// シリアル化したデータから CarrotParseException のインスタンスを初期化します
        /// </summary>
        /// <param name="info">シリアル化されたオブジェクト情報</param>
        /// <param name="context">シリアルデータの転送コンテキスト</param>
        protected CarrotParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
    #endregion
}