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
        /// また、この関数はコメント行を認識した場合は、読み捨てを行い代わりに半角空白スペースを返します。
        /// </summary>
        /// <returns>読み取られた文字を返します。読み取れなくなった場合は -1 を返します。</returns>
        private int ReadNextChara()
        {
            throw new NotImplementedException();
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