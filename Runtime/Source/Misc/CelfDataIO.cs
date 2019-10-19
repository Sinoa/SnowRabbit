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

namespace SnowRabbit.Runtime
{
    /// <summary>
    /// CELFデータの入出力を行うIOクラスです
    /// </summary>
    public class CelfDataIO : IDisposable
    {
        // 定数定義
        private const int DefaultIOBufferSize = 4 << 10;
        private const int DefaultCharBufferSize = 2 << 10;
        private const bool DefaultLeaveOpen = false;

        // メンバ変数定義
        private bool disposed;
        private SrBinaryIO binaryIO;



        #region Constructor and Dispose
        /// <summary>
        /// CelfDataIO クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">ストリーム制御を行う対象のストリーム</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        public CelfDataIO(Stream stream) : this(stream, DefaultLeaveOpen, DefaultIOBufferSize, DefaultCharBufferSize)
        {
        }


        /// <summary>
        /// CelfDataIO クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">ストリーム制御を行う対象のストリーム</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時にストリームを開いたままにする場合は true を、閉じる場合は false</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        public CelfDataIO(Stream stream, bool leaveOpen) : this(stream, leaveOpen, DefaultIOBufferSize, DefaultCharBufferSize)
        {
        }


        /// <summary>
        /// CelfDataIO クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">ストリーム制御を行う対象のストリーム</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時にストリームを開いたままにする場合は true を、閉じる場合は false</param>
        /// <param name="ioBufferSize">ストリームコントロールに用いるIOバッファサイズ。必ず SrBinaryIO.MinIOBufferSize 定数以上のサイズを指定しなければいけません。</param>
        /// <param name="charBufferSize">文字データを読み込む際に用いる char バッファサイズ（2で割り切れない場合は切り上げされます）。必ず SrBinaryIO.MinCharBufferSize 定数以上のサイズを指定しなければいけません。</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">ioBufferSize に SrBinaryIO.MinIOBufferSize 未満の値は指定出来ません</exception>
        /// <exception cref="ArgumentOutOfRangeException">charBufferSize に SrBinaryIO.MinCharBufferSize 未満の値は指定出来ません</exception>
        public CelfDataIO(Stream stream, bool leaveOpen, int ioBufferSize, int charBufferSize)
        {
            // 初期化をする
            binaryIO = new SrBinaryIO(stream, leaveOpen, ioBufferSize, charBufferSize);
        }


        /// <summary>
        /// インスタンスのリソースを解放します
        /// </summary>
        //~CelfDataIO()
        //{
        //    // ファイナライザからのDispose呼び出し
        //    Dispose(false);
        //}


        /// <summary>
        /// インスタンスのリソースを解放します
        /// </summary>
        public void Dispose()
        {
            // DisposeからのDispose呼び出しをしてGCに自身のファイナライザを止めてもらう
            Dispose(true);
            //GC.SuppressFinalize(this);
        }


        /// <summary>
        /// インスタンスの実際のリソースを解放します
        /// </summary>
        /// <param name="disposing">マネージドを含む解放の場合は true を、アンマネージドのみの場合は false を指定</param>
        protected virtual void Dispose(bool disposing)
        {
            // 既に解放済みなら
            if (disposed)
            {
                // 何もしない
                return;
            }


            // マネージの解放なら
            if (disposing)
            {
                // バイナリIOを解放
                binaryIO.Dispose();
            }


            // 解放済みマーク
            disposed = true;
        }
        #endregion
    }
}