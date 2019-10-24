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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextProcessorLib
{
    #region Token struct
    /// <summary>
    /// トークンの表現を定義している構造体です
    /// </summary>
    public readonly struct Token
    {
        /// <summary>
        /// トークンの種別
        /// </summary>
        public readonly int Kind;

        /// <summary>
        /// トークンの文字列本体
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// トークンが整数で表現される時の整数値
        /// </summary>
        public readonly long Integer;

        /// <summary>
        /// トークンが実数で表現される時の実数値
        /// </summary>
        public readonly double Number;

        /// <summary>
        /// トークンが現れた行番号
        /// </summary>
        public readonly int LineNumber;

        /// <summary>
        /// トークンが現れた最初の列番号
        /// </summary>
        public readonly int ColumnNumber;



        /// <summary>
        /// Token 構造体のインスタンスを初期化します
        /// </summary>
        /// <param name="kind">トークンの種別</param>
        /// <param name="text">トークン文字列</param>
        /// <param name="integer">トークン整数</param>
        /// <param name="number">トークン実数</param>
        /// <param name="lineNumber">出現行番号</param>
        /// <param name="columnNumber">出現列番号</param>
        public Token(int kind, string text, long integer, double number, int lineNumber, int columnNumber)
        {
            // 各種メンバ変数の初期化
            Kind = kind;
            Text = text;
            Integer = integer;
            Number = number;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }
    }
    #endregion



    #region TokenKind
    /// <summary>
    /// トークンの種別を表現した定数を保持しているクラスです。
    /// </summary>
    /// <remarks>
    /// 独自の追加トークンを定義する場合は、このクラスを継承して追加の定義を行って下さい。
    /// ただし、必ず UserDefineOffset 以上の正の整数を用いて下さい
    /// </remarks>
    public class TokenKind
    {
        #region Special
        /// <summary>
        /// 追加のトークン定義を行うためのオフセット
        /// </summary>
        public const int UserDefineOffset = 1000;

        /// <summary>
        /// 不明なトークン(通常は無効値として扱われ無効な識別子としても扱います)
        /// invalid token kind and invalid identifier.
        /// </summary>
        public const int Unknown = 0;

        /// <summary>
        /// これ以上のトークンは存在しないトークン
        /// </summary>
        public const int EndOfToken = -1;

        /// <summary>
        /// 行末を示すトークン
        /// </summary>
        public const int EndOfLine = -2;
        #endregion

        #region Typical
        /// <summary>
        /// 識別子
        /// identifier | hogemoge | _identifier | hoge123
        /// </summary>
        public const int Identifier = -100;

        /// <summary>
        /// 文字列
        /// "a" or "abc" or 'a' or 'abc'
        /// </summary>
        public const int String = -101;

        /// <summary>
        /// 整数
        /// 1234 | 0xABCD
        /// </summary>
        public const int Integer = -102;

        /// <summary>
        /// 実数
        /// 1234.0 | 1234.56
        /// </summary>
        public const int Number = -103;
        #endregion

        #region SingleSymbol
        /// <summary>
        /// オープンパーレン
        /// (
        /// </summary>
        public const int OpenParen = -500;

        /// <summary>
        /// クローズパーレン
        /// )
        /// </summary>
        public const int CloseParen = -501;

        /// <summary>
        /// オープンアングル
        /// <
        /// </summary>
        public const int OpenAngle = -502;

        /// <summary>
        /// クローズアングル
        /// >
        /// </summary>
        public const int CloseAngle = -503;

        /// <summary>
        /// オープンブラケット
        /// [
        /// </summary>
        public const int OpenBracket = -504;

        /// <summary>
        /// クローズブラケット
        /// ]
        /// </summary>
        public const int CloseBracket = -505;

        /// <summary>
        /// オープンブレス
        /// {
        /// </summary>
        public const int OpenBrace = -506;

        /// <summary>
        /// クローズブレス
        /// }
        /// </summary>
        public const int CloseBrace = -507;

        /// <summary>
        /// コロン
        /// :
        /// </summary>
        public const int Colon = -508;

        /// <summary>
        /// セミコロン
        /// ;
        /// </summary>
        public const int Semicolon = -509;

        /// <summary>
        /// シャープ
        /// #
        /// </summary>
        public const int Sharp = -510;

        /// <summary>
        /// カンマ
        /// ,
        /// </summary>
        public const int Comma = -511;

        /// <summary>
        /// ピリオド
        /// .
        /// </summary>
        public const int Period = -512;

        /// <summary>
        /// イコール
        /// =
        /// </summary>
        public const int Equal = -513;

        /// <summary>
        /// プラス
        /// +
        /// </summary>
        public const int Plus = -514;

        /// <summary>
        /// マイナス
        /// -
        /// </summary>
        public const int Minus = -515;

        /// <summary>
        /// アスタリスク
        /// *
        /// </summary>
        public const int Asterisk = -516;

        /// <summary>
        /// スラッシュ
        /// /
        /// </summary>
        public const int Slash = -517;

        /// <summary>
        /// パーセント
        /// %
        /// </summary>
        public const int Percent = -518;

        /// <summary>
        /// エクスクラメーション
        /// !
        /// </summary>
        public const int Exclamation = -519;

        /// <summary>
        /// クエスチョン
        /// ?
        /// </summary>
        public const int Question = -520;

        /// <summary>
        /// バーティカルバー
        /// |
        /// </summary>
        public const int Verticalbar = -521;

        /// <summary>
        /// アンド
        /// &
        /// </summary>
        public const int And = -522;

        /// <summary>
        /// ドル
        /// $
        /// </summary>
        public const int Dollar = -523;

        /// <summary>
        /// サーカムフレックス
        /// ^
        /// </summary>
        public const int Circumflex = -524;

        /// <summary>
        /// チルダ
        /// ~
        /// </summary>
        public const int Tilde = -525;

        /// <summary>
        /// アットサイン
        /// @
        /// </summary>
        public const int AtSign = -526;
        #endregion

        #region DoubleSymbol
        /// <summary>
        /// ダブルイコール
        /// ==
        /// </summary>
        public const int DoubleEqual = -600;

        /// <summary>
        /// ノットイコール
        /// !=
        /// </summary>
        public const int NotEqual = -601;

        /// <summary>
        /// レッサーイコール
        /// <=
        /// </summary>
        public const int LesserEqual = -602;

        /// <summary>
        /// グレイターイコール
        /// >=
        /// </summary>
        public const int GreaterEqual = -603;

        /// <summary>
        /// プラスイコール
        /// +=
        /// </summary>
        public const int PlusEqual = -604;

        /// <summary>
        /// マイナスイコール
        /// -=
        /// </summary>
        public const int MinusEqual = -605;

        /// <summary>
        /// アスタリスクイコール
        /// *=
        /// </summary>
        public const int AsteriskEqual = -606;

        /// <summary>
        /// スラッシュイコール
        /// /=
        /// </summary>
        public const int SlashEqual = -607;

        /// <summary>
        /// 右矢印
        /// ->
        /// </summary>
        public const int RightArrow = -607;

        /// <summary>
        /// 左矢印
        /// <-
        /// </summary>
        public const int LeftArrow = -608;

        /// <summary>
        /// ダブルアンド
        /// &&
        /// </summary>
        public const int DoubleAnd = -609;

        /// <summary>
        /// ダブルバーティカルバー
        /// ||
        /// </summary>
        public const int DoubleVerticalbar = -610;

        /// <summary>
        /// ダブルオープンアングル
        /// <<
        /// </summary>
        public const int DoubleOpenAngle = -611;

        /// <summary>
        /// ダブルクローズアングル
        /// >>
        /// </summary>
        public const int DoubleCloseAngle = -612;

        /// <summary>
        /// ダブルプラス
        /// ++
        /// </summary>
        public const int DoublePlus = -613;

        /// <summary>
        /// ダブルマイナス
        /// --
        /// </summary>
        public const int DoubleMinus = -614;
        #endregion
    }
    #endregion



    #region TokenReader
    /// <summary>
    /// ストリームからトークンを読み込むリーダークラスです
    /// </summary>
    public abstract class TokenReader : IDisposable
    {
        // 定数定義
        public const int EndOfStream = -1;

        // クラス変数定義
        public static readonly TokenReader EmptyReader;
        private static readonly Dictionary<Type, Dictionary<string, int>> KeywordTableTable;

        // メンバ変数定義
        private bool disposed;
        private bool leaveOpen;
        private Dictionary<string, int> keywordTable;
        private TextReader reader;
        private StringBuilder tokenReadBuffer;
        private int lastReadChara;
        private int currentLineNumber;
        private int currentColumnNumber;
        private Token lastReadToken;



        #region property
        /// <summary>
        /// ラインフィールド改行コードをトークンとして認めるかどうか
        /// </summary>
        public bool AllowEndOfLineToken { get; set; }


        /// <summary>
        /// トークンを読み切った場合は true を、まだ続く可能性がある場合は false を返します
        /// </summary>
        public bool EndOfToken => lastReadToken.Kind == TokenKind.EndOfToken;


        /// <summary>
        /// 最後に読み込んだトークンの参照を取得します
        /// </summary>
        public ref Token LastReadToken
        {
            get
            {
                // 最後に読み込んだトークンの参照を返す
                return ref lastReadToken;
            }
        }
        #endregion



        #region Constructor and Dispose and Initializer
        /// <summary>
        /// TokenReader クラスの初期化をします
        /// </summary>
        static TokenReader()
        {
            // キーワードテーブルのテーブルを生成する
            KeywordTableTable = new Dictionary<Type, Dictionary<string, int>>();
            EmptyReader = new EmptyTokenReader();
        }


        /// <summary>
        /// TokenReader クラスのインスタンスを初期化します
        /// </summary>
        public TokenReader() : this(TextReader.Null, true)
        {
        }


        /// <summary>
        /// TokenReader クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="textReader">トークンを読み出す為のテキストリーダー</param>
        /// <exception cref="ArgumentNullException">textReader が null です</exception>
        public TokenReader(TextReader textReader) : this(textReader, false)
        {
        }


        /// <summary>
        /// TokenReader クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="textReader">トークンを読み出す為のテキストリーダー</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時に textReader を開いたままにする場合は true を、閉じる場合は false</param>
        /// <exception cref="ArgumentNullException">textReader が null です</exception>
        public TokenReader(TextReader textReader, bool leaveOpen)
        {
            // 状態リセットを呼ぶ
            Reset(textReader, leaveOpen);
        }


        /// <summary>
        /// TokenReader クラスのデストラクタです
        /// </summary>
        ~TokenReader()
        {
            // デストラクタからのDispose呼び出し
            Dispose(false);
        }


        /// <summary>
        /// TokenReader クラスによって確保されたリソースを解放します
        /// </summary>
        public void Dispose()
        {
            // DisposeからのDispose呼び出しをしてファイナライザを呼び出さないようにしてもらう
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// TokenReader の Dispose 実装です
        /// </summary>
        /// <param name="disposing">Disposeからの呼び出しなら true を、ファイナライザからの呼び出しなら false を指定</param>
        protected virtual void Dispose(bool disposing)
        {
            // 既に破棄済みなら
            if (disposed)
            {
                // 何もせず終了
                return;
            }


            // マネージド解放なら
            if (disposing)
            {
                // leaveOpen が false なら
                if (!leaveOpen)
                {
                    // リーダーの解放をする
                    reader.Dispose();
                }
            }


            // 破棄済みをマーク
            disposed = true;
        }


        /// <summary>
        /// TokenReader のインスタンス状態をリセットします。
        /// これにより、新たにインスタンスを生成しなくても、トークンの再取得が可能になります。
        /// </summary>
        /// <param name="textReader">トークンを読み出す為のテキストリーダー</param>
        /// <exception cref="ArgumentNullException">textReader が null です</exception>
        public void Reset(TextReader textReader)
        {
            // textReaderを閉じるようにリセットをする
            Reset(textReader, false);
        }


        /// <summary>
        /// TokenReader のインスタンス状態をリセットします。
        /// これにより、新たにインスタンスを生成しなくても、トークンの再取得が可能になります。
        /// </summary>
        /// <param name="textReader">トークンを読み出す為のテキストリーダー</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時に textReader を開いたままにする場合は true を、閉じる場合は false</param>
        /// <exception cref="ArgumentNullException">textReader が null です</exception>
        public void Reset(TextReader textReader, bool leaveOpen)
        {
            // 既に以前のテキストリーダーが存在しかつ現在の leaveOpen が false なら
            if (reader != null && !this.leaveOpen)
            {
                // 現在持っているリーダーのDisposeを呼ぶ
                reader.Dispose();
            }


            // リーダーインスタンスを生成して書くメンバ変数の初期化
            reader = textReader ?? throw new ArgumentNullException(nameof(textReader));
            tokenReadBuffer = tokenReadBuffer ?? new StringBuilder();
            lastReadChara = ' ';
            currentLineNumber = 1;
            currentColumnNumber = 0;
            lastReadToken = new Token(TokenKind.Unknown, string.Empty, 0, 0.0, 0, 0);
            this.leaveOpen = leaveOpen;


            // バッファのクリア
            tokenReadBuffer.Clear();


            // キーワードテーブルを取得するが、失敗したら
            var myType = GetType();
            if (!KeywordTableTable.TryGetValue(myType, out keywordTable))
            {
                // 新しくトークンテーブルを生成してトークンの追加を行う
                keywordTable = CreateDefaultTokenTable();
                SetupToken(keywordTable);


                // トークンテーブルの登録
                KeywordTableTable[myType] = keywordTable;
            }
        }


        /// <summary>
        /// 必要最低限の標準トークンテーブルを生成します
        /// </summary>
        /// <returns>標準のトークンテーブルを返します</returns>
        protected Dictionary<string, int> CreateDefaultTokenTable()
        {
            // 標準のキーワードテーブルを生成して返す
            return new Dictionary<string, int>()
            {
                // 単独記号
                { "(", TokenKind.OpenParen }, { ")", TokenKind.CloseParen }, { "<", TokenKind.OpenAngle }, { ">", TokenKind.CloseAngle },
                { "[", TokenKind.OpenBracket }, { "]", TokenKind.CloseBracket }, { "{", TokenKind.OpenBrace }, { "}", TokenKind.CloseBrace },
                { ":", TokenKind.Colon }, { ";", TokenKind.Semicolon }, { "#", TokenKind.Sharp }, { ",", TokenKind.Comma }, { ".", TokenKind.Period },
                { "=", TokenKind.Equal }, { "+", TokenKind.Plus }, { "-", TokenKind.Minus }, { "*", TokenKind.Asterisk }, { "/", TokenKind.Slash },
                { "%", TokenKind.Percent }, { "!", TokenKind.Exclamation }, { "?", TokenKind.Question }, { "|", TokenKind.Verticalbar },
                { "&", TokenKind.And }, { "$", TokenKind.Dollar }, { "^", TokenKind.Circumflex }, { "~", TokenKind.Tilde }, { "@", TokenKind.AtSign },

                // 二重記号
                { "==", TokenKind.DoubleEqual }, { "!=", TokenKind.NotEqual },
                { "<=", TokenKind.LesserEqual }, { ">=", TokenKind.GreaterEqual },
                { "+=", TokenKind.PlusEqual }, { "-=", TokenKind.MinusEqual },
                { "*=", TokenKind.AsteriskEqual }, { "/=", TokenKind.SlashEqual },
                { "->", TokenKind.RightArrow }, { "<-", TokenKind.LeftArrow },
                { "&&", TokenKind.DoubleAnd }, { "||", TokenKind.DoubleVerticalbar },
                { "<<", TokenKind.DoubleOpenAngle }, { ">>", TokenKind.DoubleCloseAngle },
                { "++", TokenKind.DoublePlus }, { "--", TokenKind.DoubleMinus },
            };
        }


        /// <summary>
        /// 既定トークンテーブルに対して、更に実装クラス側で追加が必要なトークンを追加します。
        /// </summary>
        /// <param name="tokenTable">追加するトークンを受け取るテーブル</param>
        protected virtual void SetupToken(Dictionary<string, int> tokenTable)
        {
        }
        #endregion


        #region Read functions
        /// <summary>
        /// 次に読み取れるトークンを読み込みます。
        /// 読み込まれたトークンは LastReadToken プロパティからアクセスする事が出来ます。
        /// </summary>
        /// <returns>トークンを読み込んだ場合は true を、読み出すべきトークンがない（EndOfToken）場合は false を返します</returns>
        /// <exception cref="ObjectDisposedException">インスタンスが既に破棄されています</exception>
        public bool ReadNextToken()
        {
            // 読み込まれたトークンパラメータを破棄するだけの呼び出しを行う
            return ReadNextToken(out var dummy);
        }


        /// <summary>
        /// 次に読み取れるトークンを読み込みます
        /// </summary>
        /// <param name="token">読み出されたトークンを設定するトークンへの参照</param>
        /// <returns>トークンを読み込んだ場合は true を、読み出すべきトークンがない（EndOfToken）場合は false を返します</returns>
        /// <exception cref="ObjectDisposedException">インスタンスが既に破棄されています</exception>
        public bool ReadNextToken(out Token token)
        {
            // 事前の例外判定を行う
            ThrowIfDisposed();


            // 最後に読み込んだ文字を取得して'\n'を除く空白文字なら、有効な文字がくるまで読み飛ばす
            // ただしAllowEndOfLineTokenがfalseならラインフィールドもトークンとして認めない
            var readChara = lastReadChara;
            while ((!AllowEndOfLineToken || readChara != '\n') && char.IsWhiteSpace((char)readChara))
            {
                // 次の文字を読み込む
                readChara = ReadNextChara();
            }


            // もしストリームの最後なら
            if (readChara == EndOfStream)
            {
                // 読み切ったトークンを設定してfalseを返す
                token = new Token(TokenKind.EndOfToken, string.Empty, 0, 0.0, currentLineNumber, currentColumnNumber);
                lastReadToken = token;
                return false;
            }


            // 読み取った最初の文字によってトークン読み込み関数を呼び分ける
            switch ((char)readChara)
            {
                // 数字なら、数値トークンとして読み込む
                case char n when char.IsDigit(n):
                    ReadIntegerOrNumberToken(readChara, out token);
                    break;


                // アンダーバーまたはレター文字なら、識別子またはキーワードトークンとして読み込む
                case char c when char.IsLetter(c) || c == '_':
                    ReadIdentifierOrKeywordToken(readChara, out token);
                    break;


                // ダブルクォートまたはシングルクォートなら、文字列トークンとして読み込む
                case char c when c == '"' || c == '\'':
                    ReadStringToken(readChara, out token);
                    break;


                // 上記どれでもないなら、記号または他のトークンとして読み込む
                default:
                    ReadSymbolOrOtherToken(readChara, out token);
                    break;
            }


            // 最後に読み取ったトークンを覚えて読み込めたことを返す
            lastReadToken = token;
            return true;
        }


        /// <summary>
        /// ストリームから次に読み取れる文字を読み込みます。
        /// また、この関数はコメント行を認識した場合は、読み捨てを行い代わりにラインフィールドを返します。
        /// さらに、この関数は文字を読み進めるたびに状況に応じて、行番号及び列番号の更新も行います。
        /// </summary>
        /// <returns>読み取られた文字を返します。読み取れなくなった場合は EndOfStream を返します。</returns>
        private int ReadNextChara()
        {
            // 最後に読み込んだ文字が最終ストリームか既にストリームが最終位置なら
            if (lastReadChara == EndOfStream)
            {
                // これ以上の読み込みはないことを返す
                lastReadChara = EndOfStream;
                return lastReadChara;
            }


            // 前回に読み込んだ文字が改行コードなら
            if (lastReadChara == '\n')
            {
                // 行番号を増やして列番号を先頭に移動する
                ++currentLineNumber;
                currentColumnNumber = 0;
            }


            // 文字を読み込んで列番号を進める
            var readChara = reader.Read();
            ++currentColumnNumber;


            // もし読み取り結果が -1 なら
            if (readChara == -1)
            {
                // 最終読み込みであることを返す
                lastReadChara = EndOfStream;
                return lastReadChara;
            }


            // もしスラッシュなら
            if (readChara == '/')
            {
                // 次の文字もスラッシュなら
                if (reader.Peek() == '/')
                {
                    // 行コメントとして認知して行末または文字として扱えない文字までループ
                    while (readChara != '\n' && readChara != -1)
                    {
                        // 次の文字を読み込んで列番号も増やす
                        readChara = reader.Read();
                        ++currentColumnNumber;
                    }


                    // ストリーム最後であったとしても行コメントは改行コードとして返す
                    lastReadChara = '\n';
                    return lastReadChara;
                }
            }


            // 最後に読み込んだ文字として覚えて文字を返す
            lastReadChara = readChara;
            return lastReadChara;
        }
        #endregion


        #region Token builder functions
        /// <summary>
        /// ストリームから整数または実数数値トークンとして読み込みトークンを形成します
        /// </summary>
        /// <param name="firstChara">最初に読み取られた文字</param>
        /// <param name="token">形成したトークンを設定する参照</param>
        private void ReadIntegerOrNumberToken(int firstChara, out Token token)
        {
            // バッファのクリアをして、行番号と列番号を覚える
            tokenReadBuffer.Clear();
            var startLineNumber = currentLineNumber;
            var startColumnNumber = currentColumnNumber;


            // 数字が読み込まれる間はループ
            var readChara = firstChara;
            long result = 0L;
            while (char.IsDigit((char)readChara))
            {
                // 文字列バッファには詰めていく
                tokenReadBuffer.Append((char)readChara);


                // 数字から数値へ変換して次の文字を読み取る
                result = result * 10 + (readChara - '0');
                readChara = ReadNextChara();
            }


            // もし結果が0かつ次に読み込まれた文字が'x | X'なら
            if (result == 0 && (readChara == 'x' || readChara == 'X'))
            {
                // 次の文字を読み込む
                tokenReadBuffer.Append((char)readChara);
                readChara = ReadNextChara();


                // 次の文字が16進数の範囲の文字でないなら
                if (!(char.IsDigit((char)readChara) || ('A' <= readChara && readChara <= 'F') || ('a' <= readChara && readChara <= 'f')))
                {
                    // 何をしたいのか整数トークンとして解釈出来ない事を初期化する
                    token = new Token(TokenKind.Unknown, $"16進数は最低でも1桁以上の入力が必要です '{tokenReadBuffer.ToString()}'", 0, 0.0, startLineNumber, startColumnNumber);
                    return;
                }


                // 数字またはa-fA-Fの文字の間はループ
                while (char.IsDigit((char)readChara) || ('A' <= readChara && readChara <= 'F') || ('a' <= readChara && readChara <= 'f'))
                {
                    // 文字バッファに文字を詰めて単体数字の数値を得る
                    tokenReadBuffer.Append((char)readChara);
                    int unitValue;


                    // 小文字のa-fなら
                    if (readChara >= 'a')
                    {
                        // 小文字から数値を作る
                        unitValue = readChara - 'a' + 10;
                    }


                    // 大文字のA-Fなら
                    else if (readChara >= 'A')
                    {
                        // 大文字から数値を作る
                        unitValue = readChara - 'A' + 10;
                    }


                    // 数字なら
                    else
                    {
                        // 数字から数値を作る
                        unitValue = readChara - '0';
                    }


                    // 単体数値から結果変数へ詰めて次の文字へ
                    result = result * 16 + unitValue;
                    readChara = ReadNextChara();
                }


                // 整数トークンとして初期化をする（トークン文字列は16進数の文字列を入れる）
                token = new Token(TokenKind.Integer, tokenReadBuffer.ToString(), result, 0.0, startLineNumber, startColumnNumber);
                return;
            }


            // もし次に読み取られた文字がピリオドでは無いのなら
            if (readChara != '.')
            {
                // そのまま整数トークンとして初期化をする
                token = new Token(TokenKind.Integer, result.ToString(), result, 0.0, startLineNumber, startColumnNumber);
                return;
            }


            // ピリオドが付いているということは実数の可能性のため次の文字を読み込む
            tokenReadBuffer.Append((char)readChara);
            readChara = ReadNextChara();


            // もし数字では無いのなら
            if (!char.IsDigit((char)readChara))
            {
                // どういうトークンなのかが不明になった
                token = new Token(TokenKind.Unknown, $"実数はピリオドのみで終了することは出来ません '{tokenReadBuffer.ToString()}'", 0, 0.0, startLineNumber, startColumnNumber);
                return;
            }


            // 数字が続くまでループ
            while (char.IsDigit((char)readChara))
            {
                // 文字列バッファには詰めていく
                tokenReadBuffer.Append((char)readChara);
                readChara = ReadNextChara();
            }


            // 浮動小数点は素直にパースして実数トークンとして初期化をする（整数部は整数値を入れておく）
            var number = double.Parse(tokenReadBuffer.ToString());
            token = new Token(TokenKind.Number, number.ToString(), result, number, startLineNumber, startColumnNumber);
        }


        /// <summary>
        /// ストリームから識別子またはキーワードトークンとして読み込みトークンを形成します
        /// </summary>
        /// <param name="firstChara">最初に読み取られた文字</param>
        /// <param name="token">形成したトークンを設定する参照</param>
        private void ReadIdentifierOrKeywordToken(int firstChara, out Token token)
        {
            // バッファのクリアをして、行番号と列番号を覚える
            tokenReadBuffer.Clear();
            var startLineNumber = currentLineNumber;
            var startColumnNumber = currentColumnNumber;


            // 次の文字を読み込んで識別子として有効な文字の間はループ（レター文字, 数字, アンダーバー）
            var readChar = firstChara;
            while (char.IsLetterOrDigit((char)readChar) || readChar == '_')
            {
                // 有効な文字をバッファに入れて次の文字を読み込む
                tokenReadBuffer.Append((char)readChar);
                readChar = ReadNextChara();
            }


            // 読み込みバッファから文字列化する
            var text = tokenReadBuffer.ToString();


            // もしキーワードテーブルに存在しないトークンなら
            if (!keywordTable.TryGetValue(text, out var kind))
            {
                // 識別子としての種類とする
                kind = TokenKind.Identifier;
            }


            // 得られたトークン種別を用いて初期化する
            token = new Token(kind, text, 0, 0.0, startLineNumber, startColumnNumber);
        }


        /// <summary>
        /// ストリームから文字列トークンとして読み込みトークンを形成します
        /// </summary>
        /// <param name="firstChara">最初に読み取られた文字</param>
        /// <param name="token">形成したトークンを設定する参照</param>
        private void ReadStringToken(int firstChara, out Token token)
        {
            // バッファのクリアをして、行番号と列番号を覚える
            tokenReadBuffer.Clear();
            var startLineNumber = currentLineNumber;
            var startColumnNumber = currentColumnNumber;


            // 次の文字を読み込んでダブルクォートまたはシングルクォート（最初に読み込んだ文字）がくるまでループ
            var readChara = ReadNextChara();
            while (readChara != firstChara)
            {
                // もしバックスラッシュが読み込まれていたら
                if (readChara == '\\')
                {
                    // 次の文字を読み込んで文字によって判定する
                    readChara = ReadNextChara();
                    switch ((char)readChara)
                    {
                        // 各文字に相当する文字をバッファに入れる
                        case 'n': tokenReadBuffer.Append('\n'); break;
                        case 't': tokenReadBuffer.Append('\t'); break;
                        case '\\': tokenReadBuffer.Append('\\'); break;
                        case '"': tokenReadBuffer.Append('"'); break;
                        case '\'': tokenReadBuffer.Append('\''); break;


                        // 上記以外の文字が来たら無効なエスケープ文字としてトークンを設定して終了
                        default:
                            token = new Token(TokenKind.Unknown, $"無効なエスケープ文字 '{(char)readChara}' です", 0, 0.0, currentLineNumber, currentColumnNumber);
                            return;
                    }


                    // 次の文字を読み込んでループを継続する
                    readChara = ReadNextChara();
                    continue;
                }


                // もしストリームが終了していたら
                if (readChara == EndOfStream)
                {
                    // 無効な文字列設定として終了
                    token = new Token(TokenKind.Unknown, "文字列が正しく終了していません", 0, 0.0, currentLineNumber, currentColumnNumber);
                    return;
                }


                // 文字をそのままバッファに詰めて次の文字を読み込む
                tokenReadBuffer.Append((char)readChara);
                readChara = ReadNextChara();
            }


            // 文字列トークンを生成して次の文字を読み取っておく
            token = new Token(TokenKind.String, tokenReadBuffer.ToString(), 0, 0.0, startLineNumber, startColumnNumber);
            ReadNextChara();
        }


        /// <summary>
        /// ストリームから記号トークンとして読み込みトークンを形成します
        /// </summary>
        /// <param name="firstChara">最初に読み取られた文字</param>
        /// <param name="token">形成したトークンを設定する参照</param>
        private void ReadSymbolOrOtherToken(int firstChara, out Token token)
        {
            // もしラインフィールド文字なら
            if (firstChara == '\n')
            {
                // ラインフィールドは無条件で行末トークンとして生成して次の文字を読み込む
                token = new Token(TokenKind.EndOfLine, string.Empty, 0, 0.0, currentLineNumber, currentColumnNumber);
                ReadNextChara();
                return;
            }


            // バッファのクリアをして、行番号と列番号を覚える
            tokenReadBuffer.Clear();
            var startLineNumber = currentLineNumber;
            var startColumnNumber = currentColumnNumber;


            // まずは最初の一文字をバッファに入れて文字列を生成してから次の文字を読み込む
            tokenReadBuffer.Append((char)firstChara);
            var tokenText = tokenReadBuffer.ToString();
            var readChara = ReadNextChara();


            // キーワードテーブルからトークンの種類を取得するが、できなかったら
            if (!keywordTable.TryGetValue(tokenText, out var kind))
            {
                // 無効なトークン種別に設定する
                kind = TokenKind.Unknown;
            }


            // 次に読み込んだ文字がストリーム末尾では無いのなら
            if (readChara != EndOfStream)
            {
                // バッファに入れて二重記号用文字列を生成
                tokenReadBuffer.Append((char)readChara);
                var doubleSymbolText = tokenReadBuffer.ToString();


                // もし二重記号用文字列がキーワードテーブルから取得出来るのなら
                if (keywordTable.TryGetValue(doubleSymbolText, out var newKind))
                {
                    // トークン種別と文字列を更新して次の文字を読み込む
                    kind = newKind;
                    tokenText = doubleSymbolText;
                    ReadNextChara();
                }
            }


            // 取得された種類と文字列でトークンを生成する
            token = new Token(kind, tokenText, 0, 0.0, startLineNumber, startColumnNumber);
        }
        #endregion


        #region Exception thrower
        /// <summary>
        /// 既にオブジェクトが破棄済みの場合に例外を送出します
        /// </summary>
        /// <exception cref="ObjectDisposedException">インスタンスが既に破棄されています</exception>
        private void ThrowIfDisposed()
        {
            // 既に破棄済みなら
            if (disposed)
            {
                // オブジェクト破棄済み例外を投げる
                throw new ObjectDisposedException(null);
            }
        }
        #endregion



        #region EmptyTokenReader
        /// <summary>
        /// 追加のトークン解析が無いトークンリーダークラスです
        /// </summary>
        private class EmptyTokenReader : TokenReader
        {
        }
        #endregion
    }
    #endregion
}