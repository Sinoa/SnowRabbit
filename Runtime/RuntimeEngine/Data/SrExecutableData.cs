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

/*

SnowRabbit Object Format

MagicNumber         [4] : "SROF"
CodeCount           [4] : int
StringRecordCount   [4] : int
Code                [x] : Hint CodeCount * sizeof(ulong)
StringRecords       [x] : Hint StringRecordCount * StringRecord
StringPool          [x] : Hint StringRecord content info

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SnowRabbit.RuntimeEngine.Data
{
    /// <summary>
    /// SnowRabbit ランタイムが実行可能とするデータを持つクラスです
    /// </summary>
    public class SrExecutableData
    {
        // 定数定義
        public const uint MagicNumber = (byte)'F' << 24 | (byte)'O' << 16 | (byte)'R' << 8 | (byte)'S'; // for LittleEndian

        // クラス変数宣言
        private static readonly Encoding encoding = new UTF8Encoding(false);

        // メンバ変数定義
        private readonly SrValue[] codes;
        private StringRecord[] records;
        private byte[] stringPool;



        public int CodeCount => codes.Length;


        public int CodeSize => codes.Length * sizeof(ulong);


        public int StringRecordCount => records.Length;



        public SrExecutableData(SrValue[] codes, StringRecord[] records, byte[] stringPool)
        {
            this.codes = codes ?? throw new ArgumentNullException(nameof(codes));
            this.records = records ?? throw new ArgumentNullException(nameof(records));
            this.stringPool = stringPool ?? throw new ArgumentNullException(nameof(stringPool));
        }


        public SrValue[] GetInstructionCodes()
        {
            return codes;
        }


        public StringRecord[] GetRawRecords()
        {
            return records;
        }


        public byte[] GetRawStringPool()
        {
            return stringPool;
        }


        public (int Address, string String) GetString(int index)
        {
            var record = records[index];
            var text = encoding.GetString(stringPool, record.Offset, record.Length);
            return (record.Address, text);
        }


        public void SetStrings(IEnumerable<(int Address, string String)> strings)
        {
            List<StringRecord> recordList = new List<StringRecord>();
            var buffer = new MemoryStream(1 << 10);
            var offset = 0;
            foreach (var info in strings)
            {
                var data = encoding.GetBytes(info.String);
                var record = new StringRecord();
                record.Address = info.Address;
                record.Offset = offset;
                record.Length = data.Length;


                buffer.Write(data, 0, data.Length);
                offset += data.Length;
            }


            records = recordList.ToArray();
            stringPool = buffer.ToArray();
        }
    }
}
