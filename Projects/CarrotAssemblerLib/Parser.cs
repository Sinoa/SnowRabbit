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

::::: Syntax :::::

asm-script-unit
    : {'#' directive} '{' {program-body} '}'

directive
    : 'const' const-string-define
    | 'global' global-var-define

const-string-define
    : <integer> <string>

global-var-define
    : <identifier>

program-body
    : ':' label-name
    | op-code [argument-list] ';'

label-name
    : <identifier>

op-code
    : 'halt'
    | 'mov' | 'ldr' | 'str' | 'push' | 'pop'
    | 'add' | 'sub' | 'mul' | 'div' | 'mod' | 'pow'
    | 'or' | 'xor' | 'and' | 'not' | 'shl' | 'shr'
    | 'teq' | 'tne' | 'tg' | 'tge' | 'tl' | 'tle'
    | 'br' | 'bnz' | 'call' | 'callnz' | 'ret'
    | 'cpf' | 'gpid' | 'gpfid'

argument-list
    : argument {',' argument}

argument
    : <integer>
    | label-name
    | register-name
    | global-var-define

register-name
    : 'rax' | 'rbx' | 'rcx' | 'rdx' | 'rsi' | 'rdi' | 'rbp' | 'rsp'
    | 'r8' | 'r9' | 'r10' | 'r11' | 'r12' | 'r13' | 'r14' | 'r15'

*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SnowRabbit.VirtualMachine.Runtime;
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
        private CarrotAsmLexer lexer;
        private CarrotBinaryCoder coder;
        private ParserLogger logger;



        /// <summary>
        /// CarrotAssemblyParser クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="lexer">トークンを取り出すためのレキサ</param>
        /// <param name="coder">実行コードを構築するためのビルダ</param>
        /// <param name="logger">構文解析中のログを出力するロガー</param>
        /// <exception cref="ArgumentNullException">lexer が null です</exception>
        /// <exception cref="ArgumentNullException">coder が null です</exception>
        /// <exception cref="ArgumentNullException">logger が null です</exception>
        public CarrotAssemblyParser(CarrotAsmLexer lexer, CarrotBinaryCoder coder, ParserLogger logger)
        {
            // 必要なサブシステムの参照を覚える
            this.lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
            this.coder = coder ?? throw new ArgumentNullException(nameof(coder));
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
            // 構文解析を開始して実行コードの出力をする
            ParseAsmScriptUnit();
            coder.OutputExecuteCode();
        }


        #region Parse functions
        /// <summary>
        /// アセンブリスクリプト単位の構文を解析します
        /// </summary>
        private void ParseAsmScriptUnit()
        {
            // 最初のトークンを取り出す
            lexer.ReadNextToken(out var token);


            // シャープトークンが来る間はループ
            while (token.Kind == CarrotAsmTokenKind.Sharp)
            {
                // ディレクティブの構文を解析して次のトークンを読む
                ParseDirective();
                lexer.ReadNextToken(out token);
            }


            // 読み込んだトークンがオープンブレスでは無いのなら
            if (token.Kind != CarrotAsmTokenKind.OpenBrace)
            {
                // プログラムコード開始キーワードが必要である構文エラーを発生
                OccurParseError(ParserLogCode.ErrorNeedProgramBeginKeyword, $"プログラムコードの開始キーワード '{{' が必要です");
            }


            // ひたすらループ
            while (true)
            {
                // トークンを読むが最後まで読み切っていたら
                if (!lexer.ReadNextToken(out token))
                {
                    // プログラムの終了キーワードが必要である構文エラーを発生
                    OccurParseError(ParserLogCode.ErrorNeedProgramEndKeyword, $"プログラムコードの終了キーワード '}}' が必要です");
                }


                // もしシャープトークンが来ていたら
                if (token.Kind == CarrotAsmTokenKind.Sharp)
                {
                    // プログラムコード内でディレクティブを定義出来ない構文エラーを発生
                    OccurParseError(ParserLogCode.ErrorUnsupportedDefineDirectiveInProgramCode, $"プログラムコード内でディレクティブの定義は出来ません");
                }


                // クローズブレスが来ているのなら
                if (token.Kind == CarrotAsmTokenKind.CloseBrace)
                {
                    // 抜ける
                    break;
                }


                // プログラム本体の構文解析をする
                ParseProgramBody();
            }
        }


        /// <summary>
        /// ディレクティブの構文を解析します
        /// </summary>
        private void ParseDirective()
        {
            // キーワードトークンを取り出す
            lexer.ReadNextToken(out var token);


            // 取り出したキーワードに応じて呼び出すべきディレクティブを切り替える
            switch (token.Kind)
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
                    OccurParseError(ParserLogCode.ErrorUnsupportedDirectiveKeyword, $"不明なディレクティブキーワード '{token.Text}' です");
                    return;
            }
        }


        /// <summary>
        /// 文字列定数定義の構文を解析します
        /// </summary>
        private void ParseConstStringDefine()
        {
            // トークンを取り出すが、数値トークンでなければ
            lexer.ReadNextToken(out var token);
            if (token.Kind != CarrotAsmTokenKind.Integer)
            {
                // 数値以外のインデックス指定は禁止である構文エラーを発生
                OccurParseError(ParserLogCode.ErrorUnspecifiedConstStringIndex, $"文字列定数のインデックスが未指定です");
            }


            // 指定された数値を覚えて、32bit整数に収まるか確認
            var index = token.Integer;
            if (index > uint.MaxValue)
            {
                // 収まらないなら収まらない構文エラーを発生
                OccurParseError(ParserLogCode.ErrorBiggerConstStringIndex, $"文字列定数のインデックス値が大きすぎます '{index}'");
            }


            // トークンを取り出して、文字列トークンでなければ
            lexer.ReadNextToken(out token);
            if (token.Kind != CarrotAsmTokenKind.String)
            {
                // 文字列以外の定数は定義できない構文エラーを発生
                OccurParseError(ParserLogCode.ErrorUnsupportedConstStringOtherType, $"文字列定数で定義が出来るのは文字列のみです");
            }


            // ビルダーに文字列定数を登録するが重複エラーが発生した場合は
            if (!coder.RegisterConstString((uint)index, token.Text))
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
            lexer.ReadNextToken(out var token);
            if (token.Kind != CarrotAsmTokenKind.Identifier)
            {
                // グローバル変数名が未指定である構文エラーを発生
                OccurParseError(ParserLogCode.ErrorUnspecifiedGlobalVariableName, $"グローバル変数名が有効な識別子ではありません");
            }


            // ビルダーにグローバル変数名を登録するが重複エラーが発生した場合は
            if (coder.RegisterGlobalVariable(token.Text) == 0)
            {
                // 既に定義済みである構文エラーを発生
                OccurParseError(ParserLogCode.ErrorDuplicatedGlobalVariableName, $"既に定義済みのグローバル変数名です '{token.Text}'");
            }
        }


        /// <summary>
        /// プログラムコードの構文を解析します
        /// </summary>
        private void ParseProgramBody()
        {
            // 最後に読み出したトークンを取り出す
            var token = lexer.LastReadToken;


            // もしコロントークンなら
            if (token.Kind == CarrotAsmTokenKind.Colon)
            {
                // ラベル名の構文を解析する
                ParseLabelName();
                return;
            }


            // もしOpCodeトークンなら
            if (CarrotAsmTokenKind.IsOpCodeKind(token.Kind))
            {
                // ビルダーにOpCodeトークンを設定する
                coder.SetOpCodeTokenKind(token.Kind);


                // 次のトークンを取り出してセミコロンでは無いのなら
                lexer.ReadNextToken(out token);
                if (token.Kind != CarrotAsmTokenKind.Semicolon)
                {
                    // 引数リストの構文を解析する
                    ParseArgumentList();


                    // 最後に読み取られたトークンがセミコロンでは無いのなら
                    if (lexer.LastReadToken.Kind != CarrotAsmTokenKind.Semicolon)
                    {
                        // 命令コードは最後にセミコロンが必要である構文エラーを発生
                        OccurParseError(ParserLogCode.ErrorOperationNotEndSemicolon, $"オペレーションコードは必ずセミコロンで終了している必要があります '{lexer.LastReadToken.Text}'");
                    }
                }


                // ビルダーに命令コードの生成をしてもらうが失敗したのなら
                if (!coder.GenerateCode(out var message))
                {
                    // エラー理由をそのまま構文エラーとして発生
                    OccurParseError(ParserLogCode.ErrorInvalidOperationCode, message);
                }


                // 解析終了
                return;
            }


            // コロンの開始でもなくOpCodeトークンでも無いなら、不明なプログラムコードである構文エラーを発生
            OccurParseError(ParserLogCode.ErrorInvalidProgramCode, $"プログラムコードが不正です '{token.Text}'");
        }


        /// <summary>
        /// ラベル名の構文を解析します
        /// </summary>
        private void ParseLabelName()
        {
            // トークンを取り出す
            lexer.ReadNextToken(out var token);


            // もし識別子以外のトークンが来たら
            if (token.Kind != CarrotAsmTokenKind.Identifier)
            {
                // ラベル名は有効な識別子でなければならない構文エラーを発生
                OccurParseError(ParserLogCode.ErrorInvalidLabelName, $"ラベル名は有効な識別子でなければなりません '{token.Text}'");
            }


            // ビルダーにラベル名を登録するが重複エラーが発生した場合は
            if (coder.RegisterLable(token.Text) == -1)
            {
                // 既にラベル名が定義済みである構文エラーを発生
                OccurParseError(ParserLogCode.ErrorDuplicatedLabelName, $"既にラベル名が定義済みです '{token.Text}'");
            }
        }


        /// <summary>
        /// 引数リストの構文を解析します
        /// </summary>
        private void ParseArgumentList()
        {
            // 最後に読み取ったトークンを知る
            var token = lexer.LastReadToken;


            // ひたすらループ
            while (true)
            {
                // もし整数トークンなら
                if (token.Kind == CarrotAsmTokenKind.Integer)
                {
                    // ビルダー引数の追加をする
                    coder.AddArgumentToken(ref token);
                }


                // もし識別子かつラベル名として定義済み
                else if (token.Kind == CarrotAsmTokenKind.Identifier && coder.ContainLable(token.Text))
                {
                    // ビルダー引数の追加をする
                    coder.AddArgumentToken(ref token);
                }


                // もしレジスタ名としてなら
                else if (CarrotAsmTokenKind.IsRegisterNameKind(token.Kind))
                {
                    // ビルダー引数の追加をする
                    coder.AddArgumentToken(ref token);
                }


                // もしグローバル変数名としてなら
                else if (token.Kind == CarrotAsmTokenKind.Identifier && coder.ContainGlobalVariable(token.Text))
                {
                    // ビルダー引数の追加をする
                    coder.AddArgumentToken(ref token);
                }


                // どれでもないのなら
                else
                {
                    // 引数として使えない構文エラーを発生
                    OccurParseError(ParserLogCode.ErrorInvalidArgumentIdentifier, $"引数として利用できない識別子です '{token.Text}'");
                }


                // 次のトークンを取得してカンマでは無いのなら
                lexer.ReadNextToken(out token);
                if (token.Kind != CarrotAsmTokenKind.Comma)
                {
                    // ループから脱出する
                    break;
                }


                // 次のトークンを読み込む
                lexer.ReadNextToken(out token);
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
            ref var lastReadToken = ref lexer.LastReadToken;
            logger.WriteError(lastReadToken.LineNumber, lastReadToken.ColumnNumber, code, message);
            throw new CarrotParseException(message);
        }
        #endregion
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

        /// <summary>
        /// プログラム開始キーワードが必要です
        /// </summary>
        ErrorNeedProgramBeginKeyword = 0x8000_0008,

        /// <summary>
        /// プログラム終了キーワードが必要です
        /// </summary>
        ErrorNeedProgramEndKeyword = 0x8000_0009,

        /// <summary>
        /// プログラムコード内にディレクティブの定義は出来ません
        /// </summary>
        ErrorUnsupportedDefineDirectiveInProgramCode = 0x8000_0010,

        /// <summary>
        /// ラベル名が有効な識別子ではありません
        /// </summary>
        ErrorInvalidLabelName = 0x8000_0011,

        /// <summary>
        /// ラベル名が重複定義されています
        /// </summary>
        ErrorDuplicatedLabelName = 0x8000_0012,

        /// <summary>
        /// 命令コードは最後にセミコロンで終了している必要があります
        /// </summary>
        ErrorOperationNotEndSemicolon = 0x8000_0013,

        /// <summary>
        /// 無効な引数識別子です
        /// </summary>
        ErrorInvalidArgumentIdentifier = 0x8000_0014,

        /// <summary>
        /// 無効なプログラムコードです
        /// </summary>
        ErrorInvalidProgramCode = 0x8000_0015,

        /// <summary>
        /// 無効なオペレーションコードです
        /// </summary>
        ErrorInvalidOperationCode = 0x8000_0016,
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
            // 警告色に設定してログ出力後カラーをリセット
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"({lineNumber}, {columnNumber}) warning C{(ulong)code}: {message}");
            Console.ResetColor();
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
            // エラー色に設定してログ出力後カラーをリセット
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"({lineNumber}, {columnNumber}) error C{(ulong)code}: {message}");
            Console.ResetColor();
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