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

        // メンバ変数定義
        private bool disposed;
        private StreamReader reader;
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
            lastReadChara = 0;
            currentLineNumber = 1;
            currentColumnNumber = 0;
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


            throw new NotImplementedException();
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