﻿// zlib/libpng License
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
using CarrotAssemblerLib.Common;

namespace CarrotAssemblerLib.IO
{
    /// <summary>
    /// ストリームからトークンを読み込むリーダークラスです
    /// </summary>
    public class TokenReader : IDisposable
    {
        // 定数定義
        public const int DefaultBufferSize = (4 << 10);
        public const int EndOfStream = -1;

        // 静的メンバ変数定義
        private static readonly Dictionary<string, TokenKind> KeywordTable;

        // メンバ変数定義
        private bool disposed;
        private StreamReader reader;
        private StringBuilder tokenReadBuffer;
        private int lastReadChara;
        private int currentLineNumber;
        private int currentColumnNumber;



        /// <summary>
        /// TokenReader クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="inputStream">トークンを読み出す為のストリーム</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時 inputStream を開いたままにする場合は true を、一緒に閉じる場合は false を指定</param>
        /// <exception cref="ArgumentNullException">inputStream が null です</exception>
        public TokenReader(Stream inputStream, bool leaveOpen)
        {
            // リーダーインスタンスを生成して書くメンバ変数の初期化
            reader = new StreamReader(inputStream ?? throw new ArgumentNullException(nameof(inputStream)), Encoding.UTF8, true, DefaultBufferSize, leaveOpen);
            tokenReadBuffer = new StringBuilder();
            lastReadChara = ' ';
            currentLineNumber = 1;
            currentColumnNumber = 0;
        }


        /// <summary>
        /// TokenReader クラスの初期化をします
        /// </summary>
        static TokenReader()
        {
            // キーワードテーブルを構築する
            KeywordTable = new Dictionary<string, TokenKind>()
            {
                { "const", TokenKind.Const }
            };
        }


        #region Dispose pattern
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
                // 様々なDisposeを呼ぶ
                reader.Dispose();
            }


            // 破棄済みをマーク
            disposed = true;
        }
        #endregion


        #region Read functions
        /// <summary>
        /// 次に読み取れるトークンを読み込みます
        /// </summary>
        /// <param name="token">読み出されたトークンを設定するトークンへの参照</param>
        /// <returns>トークンが正しく読み出された場合は true を、読みだせなかった場合は false を返します</returns>
        /// <exception cref="ObjectDisposedException">インスタンスが既に破棄されています</exception>
        public bool ReadNextToken(out Token token)
        {
            // 事前の例外判定を行う
            ThrowIfDisposed();


            // 最後に読み込んだ文字を取得する
            var readChara = lastReadChara;


            // もし空白文字または改行コードなら、有効な文字がくるまで読み飛ばす
            while (char.IsWhiteSpace((char)readChara) || readChara == '\n')
            {
                // 次の文字を読み込む
                readChara = ReadNextChara();
            }


            // もしストリームの最後なら
            if (readChara == EndOfStream)
            {
                // 読み切ったトークンを設定してtrueを返す
                token = new Token(TokenKind.EndOfToken, string.Empty, 0, currentLineNumber, currentColumnNumber);
                return true;
            }


            // 読み取った最初の文字によってトークン読み込み関数を呼び分ける
            switch ((char)readChara)
            {
                // コロンなら
                case char c when c == ':':
                    // コロンのトークンとして設定してから一文字読み進める
                    token = new Token(TokenKind.Coron, ":", 0, currentLineNumber, currentColumnNumber);
                    ReadNextChara();
                    return true;


                // シャープなら
                case char c when c == '#':
                    // シャープのトークンとして設定してから一文字読み進める
                    token = new Token(TokenKind.Sharp, "#", 0, currentLineNumber, currentColumnNumber);
                    ReadNextChara();
                    return true;


                // カンマなら
                case char c when c == ',':
                    // カンマのトークンとして設定してから一文字読み進める
                    token = new Token(TokenKind.Comma, ",", 0, currentLineNumber, currentColumnNumber);
                    ReadNextChara();
                    return true;


                // ダブルクォートまたはシングルクォートなら
                case char c when c == '"' || c == '\'':
                    // 文字列トークンとして読み込む
                    ReadStringToken(readChara, out token);
                    return true;


                // 数字なら
                case char n when char.IsDigit(n):
                    // 数値トークンとして読み込む
                    ReadIntegerToken(readChara, out token);
                    return true;


                // アンダーバーまたはレター文字なら
                case char c when char.IsLetter(c) || c == '_':
                    // 識別子またはキーワードトークンとして読み込む
                    ReadIdentifierOrKeywordToken(readChara, out token);
                    return true;


                // 上記どれでもないなら
                default:
                    // 無効なトークンとして設定して読み取りに失敗したことを返す
                    token = new Token(TokenKind.Unknown, new string(new char[] { (char)readChara }), 0, currentLineNumber, currentColumnNumber);
                    return false;
            }
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
            if (lastReadChara == EndOfStream || reader.EndOfStream)
            {
                // これ以上の読み込みはないことを返す
                return EndOfStream;
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


            // もしキャリッジリターンなら
            if (readChara == '\r')
            {
                // キャリッジリターンは空白スペースとして認知させる
                lastReadChara = ' ';
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


        #region TokenRead functions
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


            // もしキーワードテーブルに存在するトークンなら
            if (KeywordTable.TryGetValue(text, out var kind))
            {
                // キーワードトークンであることを設定する
                token = new Token(kind, text, 0, startLineNumber, startColumnNumber);
                return;
            }


            // キーワードではないのなら識別子としてトークンを初期化する
            token = new Token(TokenKind.Identifier, text, 0, startLineNumber, startColumnNumber);
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
                    // 次の文字を読み込む
                    readChara = ReadNextChara();


                    // nなら
                    if (readChara == 'n')
                    {
                        // 改行コードをバッファに入れる
                        tokenReadBuffer.Append('\n');
                    }
                    // tなら
                    else if (readChara == 't')
                    {
                        // タブをバッファに入れる
                        tokenReadBuffer.Append('\t');
                    }
                    // 再びバックスラッシュなら
                    else if (readChara == '\\')
                    {
                        // バックスラッシュをバッファに入れる
                        tokenReadBuffer.Append('\\');
                    }
                    // ダブルクォートなら
                    else if (readChara == '"')
                    {
                        // ダブルクォートをバッファに入れる
                        tokenReadBuffer.Append('"');
                    }
                    // シングルクォートなら
                    else if (readChara == '\'')
                    {
                        // シングルクォートをバッファに入れる
                        tokenReadBuffer.Append('\'');
                    }
                    else
                    {
                        // それ以外の場合は無効なエスケープシーケンスとしてトークンを設定して終了
                        token = new Token(TokenKind.Unknown, $"無効なエスケープ文字 '{(char)readChara}' です", 0, currentLineNumber, currentColumnNumber);
                        return;
                    }


                    // ループを継続する
                    continue;
                }


                // もしストリームが終了していたら
                if (readChara == EndOfStream)
                {
                    // 無効な文字列設定として終了
                    token = new Token(TokenKind.Unknown, "文字列が正しく終了していません", 0, currentLineNumber, currentColumnNumber);
                    return;
                }


                // 文字をそのままバッファに詰める
                tokenReadBuffer.Append((char)readChara);


                // 次の文字を読み込む
                readChara = ReadNextChara();
            }


            // 文字列トークンを生成して次の文字を読み取っておく
            token = new Token(TokenKind.String, tokenReadBuffer.ToString(), 0, startLineNumber, startColumnNumber);
            ReadNextChara();
        }


        /// <summary>
        /// ストリームから整数数値トークンとして読み込みトークンを形成します
        /// </summary>
        /// <param name="firstChara">最初に読み取られた文字</param>
        /// <param name="token">形成したトークンを設定する参照</param>
        private void ReadIntegerToken(int firstChara, out Token token)
        {
            // 数字が読み込まれる間はループ
            var readChara = firstChara;
            long result = 0L;
            while (char.IsDigit((char)readChara))
            {
                // 数字から数値へ変換して次の文字を読み取る
                result = result * 10 + (firstChara - 0x30);
                readChara = ReadNextChara();
            }


            // トークンを初期化する
            token = new Token(TokenKind.Integer, result.ToString(), result, currentLineNumber, currentColumnNumber);

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
    }
}