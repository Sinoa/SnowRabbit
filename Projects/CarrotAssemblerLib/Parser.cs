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
    : {'#' directive} {program-body}

directive
    : 'const' const-string-define
    | 'global' global-var-define

const-string-define
    : <integer> <string>

global-var-define
    : <identifier>

program-body
    : ':' label-name
    | op-code argument-list

label-name
    : <identifier>

operation
    : op-code argument-list

op-code
    : 'halt'
    | 'mov' | 'ldr' | 'str' | 'push' | 'pop'
    | 'add' | 'sub' | 'mul' | 'div' | 'mod' | 'pow'
    | 'or' | 'xor' | 'and' | 'not' | 'shl' | 'shr'
    | 'teq' | 'tne' | 'tg' | 'tge' | 'tl' | 'tle'
    | 'br' | 'bnz' | 'call' | 'callnz' | 'ret'
    | 'cpf' | 'gpid' | 'gpfid'

argument-list
    : [argument] {',' argument}

argument
    : <identifier>
    | <integer>
    | register-name

register-name
    : 'rax' | 'rbx' | 'rcx' | 'rdx' | 'rsi' | 'rdi' | 'rbp' | 'rsp'
    | 'r8' | 'r9' | 'r10' | 'r11' | 'r12' | 'r13' | 'r14' | 'r15'
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TextProcessorLib;

namespace CarrotAssemblerLib
{
    #region Parser
    /// <summary>
    /// Carrot アセンブリの構文を解析するクラスです
    /// </summary>
    public class CarrotAssemblyParser
    {
        // メンバ変数定義
        private TokenReader lexer;
        private BinaryCodeBuilder builder;
        private ParserLogger logger;
        private Token lastReadToken;



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


            // 改行コードをトークンとして認めない
            lexer.AllowEndOfLineToken = false;
        }


        /// <summary>
        /// このアセンブリ構文解析に設定されている情報を元にアセンブルして実行コードを生成します。
        /// </summary>
        /// <exception cref="CarrotParseException">構文解析中に問題が発生しました</exception>
        public void Assemble()
        {
            // 構文解析を開始する
            ParseAsmScriptUnit();
        }


        #region Parse functions
        /// <summary>
        /// アセンブリスクリプト単位の構文を解析します
        /// </summary>
        private void ParseAsmScriptUnit()
        {
            // 最初のトークンを取り出す
            lexer.ReadNextToken(out lastReadToken);


            // シャープトークンが来る間はループ
            while (lastReadToken.Kind == TokenKind.Sharp)
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
            // キーワードトークンを取り出す
            lexer.ReadNextToken(out lastReadToken);


            // 取り出したキーワードに応じて呼び出すべきディレクティブを切り替える
            switch (lastReadToken.Kind)
            {
                // const なら文字列定数定義の構文解析を呼ぶ
                case CarrotAsmTokenKind.Const:
                    ParseConstStringDefine();
                    break;


                // global ならグローバル変数定義構文解析を呼ぶ
                case CarrotAsmTokenKind.Global:
                    ParseGlobalVarDefine();
                    break;


                // ここに来てしまったということは未定義の構文に出会ってしまった（不明なディレクティブキーワード）
                default:
                    OccurParseError(ParserLogCode.ErrorUnsupportedDirectiveKeyword, $"不明なディレクティブキーワード '{lastReadToken.Text}' です");
                    return;
            }
        }


        /// <summary>
        /// 文字列定数定義の構文を解析します
        /// </summary>
        private void ParseConstStringDefine()
        {
            // トークンを取り出すが、数値トークンでなければ
            lexer.ReadNextToken(out lastReadToken);
            if (lastReadToken.Kind != TokenKind.Integer)
            {
                // 数値以外のインデックス指定は禁止である構文エラーを発生
                OccurParseError(ParserLogCode.ErrorUnspecifiedConstStringIndex, $"文字列定数のインデックスが未指定です");
            }


            // 指定された数値を覚えて、32bit整数に収まるか確認
            var index = lastReadToken.Integer;
            if (index > uint.MaxValue)
            {
                // 収まらないなら収まらない構文エラーを発生
                OccurParseError(ParserLogCode.ErrorBiggerConstStringIndex, $"文字列定数のインデックス値が大きすぎます '{index}'");
            }


            // トークンを取り出して、文字列トークンでなければ
            lexer.ReadNextToken(out lastReadToken);
            if (lastReadToken.Kind != TokenKind.String)
            {
                // 文字列以外の定数は定義できない構文エラーを発生
                OccurParseError(ParserLogCode.ErrorUnsupportedConstStringOtherType, $"文字列定数で定義が出来るのは文字列のみです");
            }


            // ビルダーに文字列定数を登録するが重複エラーが発生した場合は
            if (!builder.RegisterConstString((uint)index, lastReadToken.Text))
            {
                // 文字列定数のインデックスが重複指定された構文エラーを発生
                OccurParseError(ParserLogCode.ErrorDuplicatedConstStringIndex, $"指定された文字列定数のインデックスが既に定義済みです '{index}'");
            }
        }


        /// <summary>
        /// グローバル変数定義の構文を解析します
        /// </summary>
        private void ParseGlobalVarDefine()
        {
            // トークンを取り出すが、識別子でなければ
            lexer.ReadNextToken(out lastReadToken);
            if (lastReadToken.Kind != TokenKind.Identifier)
            {
                // グローバル変数名が未指定である構文エラーを発生
                OccurParseError(ParserLogCode.ErrorUnspecifiedGlobalVariableName, $"グローバル変数名が有効な識別子ではありません");
            }


            // ビルダーにグローバル変数名を登録するが重複エラーが発生した場合は
            if (builder.RegisterGlobalVariable(lastReadToken.Text) == 0)
            {
                // 既に定義済みである構文エラーを発生
                OccurParseError(ParserLogCode.ErrorDuplicatedGlobalVariableName, $"既に定義済みのグローバル変数名です '{lastReadToken.Text}'");
            }
        }
        #endregion


        #region Utility function
        /// <summary>
        /// 構文エラーを発生させます
        /// </summary>
        /// <param name="code">構文エラーのコード</param>
        /// <param name="message">構文エラーメッセージ</param>
        private void OccurParseError(ParserLogCode code, string message)
        {
            // ロガーに構文エラーが発生したことを出力して例外を発生させる
            logger.WriteError(lastReadToken.LineNumber, lastReadToken.ColumnNumber, code, message);
            throw new CarrotParseException(message);
        }
        #endregion
    }
    #endregion



    #region Builder
    /// <summary>
    /// SnowRabbit 仮想マシン用の実行コードを構築するクラスです
    /// </summary>
    public class BinaryCodeBuilder
    {
        // メンバ変数定義
        private Dictionary<uint, string> constStringTable;
        private Dictionary<string, int> globalVariableTable;
        private int nextGlobalVariableIndex;



        /// <summary>
        /// BinaryCodeBuilder クラスのインスタンスを初期化します
        /// </summary>
        public BinaryCodeBuilder()
        {
            // メンバ変数の初期化をする
            constStringTable = new Dictionary<uint, string>();
            globalVariableTable = new Dictionary<string, int>();
            nextGlobalVariableIndex = -1;
        }


        /// <summary>
        /// 文字列定数を登録します
        /// </summary>
        /// <param name="index">登録する文字列のインデックス値</param>
        /// <param name="text">登録する文字列</param>
        /// <returns>正常に登録された場合は true を、指定されたインデックスが既に登録されている場合は false を返します</returns>
        internal bool RegisterConstString(uint index, string text)
        {
            // 既に指定されたインデックスが登録済みなら
            if (constStringTable.ContainsKey(index))
            {
                // 登録済みであることを返す
                return false;
            }


            // 登録して成功を返す
            constStringTable[index] = text;
            return true;
        }


        /// <summary>
        /// 指定されたグローバル変数名を登録します
        /// </summary>
        /// <param name="name">登録する変数名</param>
        /// <returns>正常に変数名が登録された場合は登録した変数インデックスを、既に登録済みの場合は 0 を返します</returns>
        internal int RegisterGlobalVariable(string name)
        {
            // 既に同じ名前の変数が登録済みなら
            if (globalVariableTable.ContainsKey(name))
            {
                // 登録済みであることを返す
                return 0;
            }


            // グローバル変数名を登録して登録したインデックスを返す（次に登録するインデックスも更新する）
            var registerdIndex = nextGlobalVariableIndex--;
            globalVariableTable[name] = registerdIndex;
            return registerdIndex;
        }
    }
    #endregion



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

        /// <summary>
        /// サポートしていないディレクティブキーワードを指定されています
        /// </summary>
        ErrorUnsupportedDirectiveKeyword = 0x8000_0001,

        /// <summary>
        /// 文字列定数のインデックスが未指定です
        /// </summary>
        ErrorUnspecifiedConstStringIndex = 0x8000_0002,

        /// <summary>
        /// 文字列定数のインデックスが重複指定されています
        /// </summary>
        ErrorDuplicatedConstStringIndex = 0x8000_0003,

        /// <summary>
        /// 文字列定数のインデックスが大きすぎます
        /// </summary>
        ErrorBiggerConstStringIndex = 0x8000_0004,

        /// <summary>
        /// 文字列定数の定義が可能な定数は文字列のみです
        /// </summary>
        ErrorUnsupportedConstStringOtherType = 0x8000_0005,

        /// <summary>
        /// グローバル変数名が未指定です
        /// </summary>
        ErrorUnspecifiedGlobalVariableName = 0x8000_0006,

        /// <summary>
        /// 既に定義済みのグローバル変数名です
        /// </summary>
        ErrorDuplicatedGlobalVariableName = 0x8000_0007,
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