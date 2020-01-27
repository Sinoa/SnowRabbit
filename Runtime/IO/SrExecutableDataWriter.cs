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
using System.IO;
using SnowRabbit.RuntimeEngine.Data;
using SnowRabbit.Compiler.Assembler.Symbols;

namespace SnowRabbit.IO
{
    /// <summary>
    /// SnowRabbit の実行可能データの書き込みクラスです
    /// </summary>
    public class SrExecutableDataWriter : SrDisposable
    {
        // メンバ変数定義
        private bool disposed;
        private readonly SrBinaryIO binaryIO;



        /// <summary>
        /// SrExecutableDataWriter クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">書き込みを行うストリーム</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        /// <exception cref="ArgumentException">stream に書き込みが許可されていません</exception>
        /// <exception cref="ArgumentException">stream にシークが許可されていません</exception>
        public SrExecutableDataWriter(Stream stream) : this(stream, false)
        {
        }


        public SrExecutableDataWriter(Stream stream, bool leaveOpen)
        {
            // 書き込み許可をされていないなら
            if (!(stream ?? throw new ArgumentNullException(nameof(stream))).CanWrite)
            {
                // 書き込み許可が無いので駄目
                throw new ArgumentException("stream に書き込みが許可されていません");
            }


            // シーク許可をされていないなら
            if (!stream.CanSeek)
            {
                // シーク許可が無いので駄目
                throw new ArgumentException("stream にシークが許可されていません");
            }


            // バイナリIOを生成する
            binaryIO = new SrBinaryIO(stream ?? throw new ArgumentNullException(nameof(stream)), leaveOpen);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposed) return;


            if (disposing)
            {
                binaryIO.Dispose();
            }


            disposed = true;
            base.Dispose(disposing);
        }


        public void Write(SrExecutableData data)
        {
            var symbols = data.GetSymbols();


            binaryIO.Write(SrExecutableData.MagicNumber);
            binaryIO.Write(data.CodeCount);
            binaryIO.Write(data.StringRecordCount);
            binaryIO.Write(symbols.Length > 0 ? symbols.Length : -1);


            var codes = data.GetInstructionCodes();
            for (int i = 0; i < data.CodeCount; ++i)
            {
                binaryIO.Write(codes[i].Primitive.Ulong);
            }


            var records = data.GetRawRecords();
            var dataSize = 0;
            for (int i = 0; i < data.StringRecordCount; ++i)
            {
                binaryIO.Write(records[i].Address);
                binaryIO.Write(records[i].Offset);
                binaryIO.Write(records[i].Length);
                dataSize += records[i].Length;
            }


            binaryIO.Write(data.GetRawStringPool());


            for (int i = 0; i < symbols.Length; ++i)
            {
                binaryIO.Write(symbols[i].Name);
                binaryIO.Write(symbols[i].Address);
                binaryIO.Write((int)symbols[i].Scope);
                binaryIO.Write((int)symbols[i].Kind);
            }
        }
    }
}
