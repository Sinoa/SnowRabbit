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

# Document

Save the following text to "crc.txt".

"This crc test text for CRC 32 and 64.\nこれは、日本語です。UTF-8エンコードをした時の確認にもなります。\n使用するエンコードは「UTF-8 BOM無し LF改行コード」です。\n7zを使ってCRC32とCRC64(ECMA)のテストをします。\n\n[command]crc.txt = this text.\n7z h -scrccrc32 -scrccrc64 crc.txt"

Encode text as non-bom utf-8 lf line code.

Result:
CRC32 : 86BD72E6
CRC64 : E42D8FC1542F78D3
Size  : 336

*/

using System.Text;
using NUnit.Framework;
using SnowRabbit.Security;

namespace SnowRabbitTest
{
    /// <summary>
    /// CRCクラスのテストクラスです。テスト用のデータは上記にあります。
    /// </summary>
    [TestFixture]
    public class CrcTest
    {
        // メンバ変数定義
        private byte[] testData;



        /// <summary>
        /// テストに必要なデータのセットアップをします
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            // テスト用テキストデータをUTF-8エンコードする
            testData = new UTF8Encoding(false).GetBytes("This crc test text for CRC 32 and 64.\nこれは、日本語です。UTF-8エンコードをした時の確認にもなります。\n使用するエンコードは「UTF-8 BOM無し LF改行コード」です。\n7zを使ってCRC32とCRC64(ECMA)のテストをします。\n\n[command]crc.txt = this text.\n7z h -scrccrc32 -scrccrc64 crc.txt");
        }


        /// <summary>
        /// CRCのCaluculateをテストします
        /// </summary>
        [Test]
        public void CrcCalculateTest()
        {
            // CRC計算結果の予想値を変数で持っておく
            var crc32Expected = 0x86BD72E6U;
            var crc64Expected = 0xE42D8FC1542F78D3UL;


            // CRC32のテスト
            var crc32 = new Crc32Standard();
            var result32 = crc32.Calculate(testData);
            Assert.AreEqual(crc32Expected, result32);


            // CRC64-Ecmaのテスト
            var crc64Ecma = new Crc64Ecma();
            var result64 = crc64Ecma.Calculate(testData);
            Assert.AreEqual(crc64Expected, result64);
        }
    }
}