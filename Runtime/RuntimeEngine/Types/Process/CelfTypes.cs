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

namespace SnowRabbit.Runtime
{
    /// <summary>
    /// CELFヘッダを表す構造体です
    /// </summary>
    public struct CelfHeader
    {
        /// <summary>
        /// CELFデータである証明となる、シグネチャ '0xCC','0xEE','0x11','0xFF'
        /// </summary>
        public byte[] MagicSignature;

        /// <summary>
        /// オブジェクトファイルのバージョン上位8bitをメジャーバージョン下位8bitをマイナーバージョン 例1.0'0x0100' 1.3'0x0103'
        /// </summary>
        public ushort ObjectVersion;

        /// <summary>
        /// 実行されるランタイムの想定バージョン（フォーマットは ObjectVersion と同じ）
        /// </summary>
        public ushort RuntimeVersion;

        /// <summary>
        /// CELFの格納タイプ'未定義ファイル=0x0000',実行可能ファイル=0x0001','共有ライブラリファイル=0x0002'
        /// </summary>
        public ushort CelfType;

        /// <summary>
        /// CELFヘッダ自体のサイズ
        /// </summary>
        public ushort CelfHeaderSize;

        /// <summary>
        /// セクションヘッダテーブルが存在するCELFデータの先頭からのオフセット
        /// </summary>
        public uint SectionHeaderTableOffset;
    }



    /// <summary>
    /// セクションヘッダテーブルを表す構造体です
    /// </summary>
    public struct SectionHeaderTable
    {
        /// <summary>
        /// テーブルに書き込まれているセクションヘッダの数
        /// </summary>
        public ushort SectionHeaderCount;

        /// <summary>
        /// 実行可能なプログラムコードを持つセクションがあるインデックス
        /// </summary>
        public ushort ProgramCodeSectionIndex;

        /// <summary>
        /// 文字列テーブルを持つセクションがあるインデックス
        /// </summary>
        public ushort StringTableScetionIndex;

        /// <summary>
        /// 予約領域（0で埋めなければならない）
        /// </summary>
        public ushort Reserved1;

        /// <summary>
        /// 予約領域（0で埋めなければならない）
        /// </summary>
        public ulong Reserved2;

        /// <summary>
        /// 複数セクションヘッダ
        /// </summary>
        public SectionHeader[] SectionHeaders;
    }



    /// <summary>
    /// セクションヘッダを表す構造体です
    /// </summary>
    public struct SectionHeader
    {
        /// <summary>
        /// NULLで終わらない16文字のASCIIエンコードデータのセクション名
        /// </summary>
        public byte[] SectionName;

        /// <summary>
        /// セクションの実体が格納されているCELFデータの先頭からのオフセット
        /// </summary>
        public uint SectionOffset;

        /// <summary>
        /// セクションの実体サイズ
        /// </summary>
        public uint SectionSize;

        /// <summary>
        /// 予約領域（0で埋めなければならない）
        /// </summary>
        public ulong Reserved;
    }



    /// <summary>
    /// プログラムコードセクションを表す構造体です
    /// </summary>
    public struct ProgramCodeSection
    {
        /// <summary>
        /// 格納されている命令の数（サイズではなく数）
        /// </summary>
        public uint CodeCount;

        /// <summary>
        /// 実際の命令配列
        /// </summary>
        public InstructionCode[] Codes;
    }



    /// <summary>
    /// 文字列テーブルセクションを表す構造体です
    /// </summary>
    public struct StringTableSection
    {
        /// <summary>
        /// 格納されている文字列の数
        /// </summary>
        public uint StringCount;

        /// <summary>
        /// 文字列データの配列
        /// </summary>
        public StringData[] Strings;
    }



    /// <summary>
    /// 文字列データそのものを表す構造体です
    /// </summary>
    public struct StringData
    {
        /// <summary>
        /// ロード先インデックス
        /// </summary>
        public uint LoadDestinationIndex;

        /// <summary>
        /// UTF-8エンコードされたデータのサイズ
        /// </summary>
        public uint StringDataSize;

        /// <summary>
        /// UTF-8エンコードされた文字列データ
        /// </summary>
        public byte[] EncodedStringData;
    }
}