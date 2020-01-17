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
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.Data;

namespace SnowRabbit.IO
{
    public class SrExecutableDataReader : SrDisposable
    {
        // メンバ変数定義
        private bool disposed;
        private readonly SrBinaryIO binaryIO;



        public SrExecutableDataReader(Stream stream)
        {
            if (!(stream ?? throw new ArgumentNullException(nameof(stream))).CanRead)
            {
                throw new ArgumentException("stream に読み込みが許可されていません");
            }


            if (!stream.CanSeek)
            {
                throw new ArgumentException("stream にシークが許可されていません");
            }


            binaryIO = new SrBinaryIO(stream ?? throw new ArgumentNullException(nameof(stream)));
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


        public SrExecutableData Read()
        {
            var magicNumber = binaryIO.ReadUInt();
            if (magicNumber != SrExecutableData.MagicNumber)
            {
                throw new Exception();
            }


            var codeCount = binaryIO.ReadInt();
            var recordCount = binaryIO.ReadInt();


            var codes = new SrValue[codeCount];
            for (int i = 0; i < codeCount; ++i)
            {
                var code = new SrValue();
                code.Primitive.Ulong = binaryIO.ReadULong();
                codes[i] = code;
            }


            var recodes = new StringRecord[recordCount];
            var dataSize = 0;
            for (int i = 0; i < recordCount; ++i)
            {
                var record = new StringRecord();
                record.Address = binaryIO.ReadInt();
                record.Offset = binaryIO.ReadInt();
                record.Length = binaryIO.ReadInt();
                dataSize += record.Length;
            }


            var stringPool = new byte[dataSize];
            binaryIO.Read(stringPool);


            return new SrExecutableData(codes, recodes, stringPool);
        }
    }
}
