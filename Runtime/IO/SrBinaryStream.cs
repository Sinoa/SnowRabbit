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

namespace SnowRabbit.IO
{
    /// <summary>
    /// SnowRabbit が扱うリトルエンディアンなストリームを扱うクラスです。
    /// リトルエンディアンCPUであればパススルーな動作をしますが、ビッグエンディアンCPUの場合はエンディアン変換が行われます。
    /// </summary>
    public class SrBinaryStream : SrDisposable
    {
        // 定数定義
        private const int DefaultIOBufferSize = 16 << 10;
        private const int DefaultCharBufferSize = 4 << 10;
        private const bool DefaultLeaveOpen = false;
        public const int MinIOBufferSize = 8;
        public const int MinCharBufferSize = 16;

        // クラス変数宣言
        private static readonly Encoding encoding = new UTF8Encoding(false);

        // メンバ変数定義
        private bool disposed;
        private readonly bool leaveOpen;
        private byte[] streamBuffer;
        private char[] charBuffer;



        /// <summary>
        /// このコントロールクラスが取り扱っているストリーム
        /// </summary>
        public Stream BaseStream { get; private set; }



        #region Constructor and Dispose
        /// <summary>
        /// SrBinaryStream クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">ストリーム制御を行う対象のストリーム</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        public SrBinaryStream(Stream stream) : this(stream, DefaultLeaveOpen, DefaultIOBufferSize, DefaultCharBufferSize)
        {
        }


        /// <summary>
        /// SrBinaryStream クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">ストリーム制御を行う対象のストリーム</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時にストリームを開いたままにする場合は true を、閉じる場合は false</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        public SrBinaryStream(Stream stream, bool leaveOpen) : this(stream, leaveOpen, DefaultIOBufferSize, DefaultCharBufferSize)
        {
        }


        /// <summary>
        /// SrBinaryStream クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">ストリーム制御を行う対象のストリーム</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時にストリームを開いたままにする場合は true を、閉じる場合は false</param>
        /// <param name="ioBufferSize">ストリームコントロールに用いるIOバッファサイズ。必ず MinIOBufferSize 定数以上のサイズを指定しなければいけません。</param>
        /// <param name="charBufferSize">文字データを読み込む際に用いる char バッファサイズ（2で割り切れない場合は切り上げされます）。必ず MinCharBufferSize 定数以上のサイズを指定しなければいけません。</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">ioBufferSize に MinIOBufferSize 未満の値は指定出来ません</exception>
        /// <exception cref="ArgumentOutOfRangeException">charBufferSize に MinCharBufferSize 未満の値は指定出来ません</exception>
        public SrBinaryStream(Stream stream, bool leaveOpen, int ioBufferSize, int charBufferSize)
        {
            // 初期化をする
            BaseStream = stream ?? throw new ArgumentNullException(nameof(stream));
            streamBuffer = new byte[ioBufferSize >= MinIOBufferSize ? ioBufferSize : throw new ArgumentOutOfRangeException(nameof(ioBufferSize), $"{nameof(ioBufferSize)} に {nameof(MinIOBufferSize)} 未満の値は指定出来ません")];
            charBuffer = new char[charBufferSize >= MinCharBufferSize ? charBufferSize + (charBufferSize & 1) : throw new ArgumentOutOfRangeException(nameof(charBufferSize), $"{nameof(charBufferSize)} に {nameof(MinCharBufferSize)} 未満の値は指定出来ません")];
            this.leaveOpen = leaveOpen;
        }


        /// <summary>
        /// インスタンスの実際のリソースを解放します
        /// </summary>
        /// <param name="disposing">マネージドを含む解放の場合は true を、アンマネージドのみの場合は false を指定</param>
        protected override void Dispose(bool disposing)
        {
            // 既に解放済みなら何もしない
            if (disposed) return;


            // マネージの解放かつ解放時にストリームを閉じるなら
            if (disposing && !leaveOpen)
            {
                // ストリームを閉じる
                BaseStream.Dispose();
            }


            // 解放済みマーク
            disposed = true;
            base.Dispose(disposing);
        }
        #endregion


        #region Utility function
        /// <summary>
        /// ストリームから指定された長さを、バッファの指定位置に読み込みます。また読み込んだデータを反転するかどうかの指示が出来ます。
        /// </summary>
        /// <param name="index">読み取ったデータを格納する開始位置</param>
        /// <param name="size">読み取る長さ</param>
        /// <param name="reverse">読み取ったデータを反転するかどうか</param>
        /// <exception cref="ObjectDisposedException">オブジェクトリソースが解放済みです</exception>
        private void Read(int index, int size, bool reverse)
        {
            // 事前例外処理を行っておく
            ThrowExceptionIfObjectDisposed();


            // 読み込もうとしている位置とサイズがバッファサイズを超えるのであれば
            if (streamBuffer.Length < index + size)
            {
                // 新しいバッファを生成してコピー
                Array.Resize(ref streamBuffer, index + size);
            }


            // 指定された長さをストリームから読み込む
            BaseStream.Read(streamBuffer, index, size);


            // もしデータ反転指示が出ていたら
            if (reverse)
            {
                // 読み取ったデータを反転する
                Array.Reverse(streamBuffer, index, size);
            }
        }


        /// <summary>
        /// バッファの指定した位置から、指定された長さのデータをストリームに書き込みます。
        /// </summary>
        /// <param name="index">書き込むデータを持っているバッファのインデックス位置</param>
        /// <param name="size">書き込む長さ</param>
        private void Write(int index, int size)
        {
            // 事前例外処理をしてから、ストリームにデータを書き込む
            ThrowExceptionIfObjectDisposed();
            BaseStream.Write(streamBuffer, index, size);
        }


#if UNITY_2018_3_OR_NEWER
        /// <summary>
        /// 32bit浮動小数点を32bit整数にビット変換をします
        /// </summary>
        /// <param name="value">変換する32bit浮動小数点</param>
        /// <returns>変換された32bit整数を返します</returns>
        unsafe private int FloatToInt32(float value)
        {
            return *(int*)&value;
        }
#endif
        #endregion


        #region Read functions
        /// <summary>
        /// 符号付き8bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public sbyte ReadSbyte()
        {
            // 符号なし8bit整数を符号付きとして返す
            return (sbyte)ReadByte();
        }


        /// <summary>
        /// 符号付き16bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public short ReadShort()
        {
            // 符号なし16bit整数を符号付きとして返す
            return (short)ReadUShort();
        }


        /// <summary>
        /// 符号付き32bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public int ReadInt()
        {
            // 符号なし32bit整数を符号付きとして返す
            return (int)ReadUInt();
        }


        /// <summary>
        /// 符号付き64bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public long ReadLong()
        {
            // 符号なし64bit整数を符号付きとして返す
            return (long)ReadULong();
        }


        /// <summary>
        /// 符号なし8bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public byte ReadByte()
        {
            // 1バイト読み込んでそのまま返す
            Read(0, 1, false);
            return streamBuffer[0];
        }


        /// <summary>
        /// 符号なし16bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public ushort ReadUShort()
        {
            // 2バイト読み込んでから変換して返す
            Read(0, 2, !BitConverter.IsLittleEndian);
            return BitConverter.ToUInt16(streamBuffer, 0);
        }


        /// <summary>
        /// 符号なし32bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public uint ReadUInt()
        {
            // 4バイト読み込んでから変換して返す
            Read(0, 4, !BitConverter.IsLittleEndian);
            return BitConverter.ToUInt32(streamBuffer, 0);
        }


        /// <summary>
        /// 符号なし64bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public ulong ReadULong()
        {
            // 8バイト読み込んでから変換して返す
            Read(0, 8, !BitConverter.IsLittleEndian);
            return BitConverter.ToUInt64(streamBuffer, 0);
        }


        /// <summary>
        /// 32bit浮動小数点を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public float ReadFloat()
        {
            // 4バイト読み込んでから変換して返す
            Read(0, 4, !BitConverter.IsLittleEndian);
            return BitConverter.ToSingle(streamBuffer, 0);
        }


        /// <summary>
        /// 64bit浮動小数点を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public double ReadDouble()
        {
            // 8バイト読み込んでから変換して返す
            Read(0, 8, !BitConverter.IsLittleEndian);
            return BitConverter.ToDouble(streamBuffer, 0);
        }


        /// <summary>
        /// 文字列を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public string ReadString()
        {
            // 符号付き32bit整数を読み込んで、読み取るべきデータサイズ分もう一度読み込む
            var dataSize = ReadInt();
            Read(0, dataSize, false);


            // 読み取られたデータからデコードされる文字数を知って、文字バッファに収まらない場合は
            var charCount = encoding.GetCharCount(streamBuffer, 0, dataSize);
            if (charBuffer.Length < charCount)
            {
                // 必要なバッファサイズを確保する
                // （結局は全部読み込まれて以前のバッファ内容が消えるので、素直なnewで対応）
                charBuffer = new char[charCount];
            }


            // UTF-8配列をデコードしてデコードした文字を文字列として返す
            encoding.GetChars(streamBuffer, 0, dataSize, charBuffer, 0);
            return new string(charBuffer, 0, charCount);
        }
        #endregion


        #region Write functions
        /// <summary>
        /// 符号付き8bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public void Write(sbyte value)
        {
            // 符号なし8bitの書き込みをする
            Write((byte)value);
        }


        /// <summary>
        /// 符号付き16bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public void Write(short value)
        {
            // 符号なし16bitの書き込みをする
            Write((ushort)value);
        }


        /// <summary>
        /// 符号付き32bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public void Write(int value)
        {
            // 符号なし32bitの書き込みをする
            Write((uint)value);
        }


        /// <summary>
        /// 符号付き64bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public void Write(long value)
        {
            // 符号なし64bitの書き込みをする
            Write((ulong)value);
        }


        /// <summary>
        /// 符号なし8bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public void Write(byte value)
        {
            // バッファに入れてそのまま書き込む
            streamBuffer[0] = value;
            Write(0, 1);
        }


        /// <summary>
        /// 符号なし16bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public void Write(ushort value)
        {
            // もしリトルエンディアン動作なら
            if (BitConverter.IsLittleEndian)
            {
                // リトルエンディアンの場合のバイト配列の詰め込みをする
                streamBuffer[0] = (byte)((value >> 0) & 0xFF);
                streamBuffer[1] = (byte)((value >> 8) & 0xFF);
            }
            else
            {
                // ビッグエンディアンの場合のバイト配列の詰め込みをする
                streamBuffer[0] = (byte)((value >> 8) & 0xFF);
                streamBuffer[1] = (byte)((value >> 0) & 0xFF);
            }


            // 2バイト書き込む
            Write(0, 2);
        }


        /// <summary>
        /// 符号なし32bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public void Write(uint value)
        {
            // もしリトルエンディアン動作なら
            if (BitConverter.IsLittleEndian)
            {
                // リトルエンディアンの場合のバイト配列の詰め込みをする
                streamBuffer[0] = (byte)((value >> 0) & 0xFF);
                streamBuffer[1] = (byte)((value >> 8) & 0xFF);
                streamBuffer[2] = (byte)((value >> 16) & 0xFF);
                streamBuffer[3] = (byte)((value >> 24) & 0xFF);
            }
            else
            {
                // ビッグエンディアンの場合のバイト配列の詰め込みをする
                streamBuffer[0] = (byte)((value >> 24) & 0xFF);
                streamBuffer[1] = (byte)((value >> 16) & 0xFF);
                streamBuffer[2] = (byte)((value >> 8) & 0xFF);
                streamBuffer[3] = (byte)((value >> 0) & 0xFF);
            }


            // 4バイト書き込む
            Write(0, 4);
        }


        /// <summary>
        /// 符号なし64bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public void Write(ulong value)
        {
            // もしリトルエンディアン動作なら
            if (BitConverter.IsLittleEndian)
            {
                // リトルエンディアンの場合のバイト配列の詰め込みをする
                streamBuffer[0] = (byte)((value >> 0) & 0xFF);
                streamBuffer[1] = (byte)((value >> 8) & 0xFF);
                streamBuffer[2] = (byte)((value >> 16) & 0xFF);
                streamBuffer[3] = (byte)((value >> 24) & 0xFF);
                streamBuffer[4] = (byte)((value >> 32) & 0xFF);
                streamBuffer[5] = (byte)((value >> 40) & 0xFF);
                streamBuffer[6] = (byte)((value >> 48) & 0xFF);
                streamBuffer[7] = (byte)((value >> 56) & 0xFF);
            }
            else
            {
                // ビッグエンディアンの場合のバイト配列の詰め込みをする
                streamBuffer[0] = (byte)((value >> 56) & 0xFF);
                streamBuffer[1] = (byte)((value >> 48) & 0xFF);
                streamBuffer[2] = (byte)((value >> 40) & 0xFF);
                streamBuffer[3] = (byte)((value >> 32) & 0xFF);
                streamBuffer[4] = (byte)((value >> 24) & 0xFF);
                streamBuffer[5] = (byte)((value >> 16) & 0xFF);
                streamBuffer[6] = (byte)((value >> 8) & 0xFF);
                streamBuffer[7] = (byte)((value >> 0) & 0xFF);
            }


            // 8バイト書き込む
            Write(0, 8);
        }


        /// <summary>
        /// 32bit浮動小数点を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public void Write(float value)
        {
#if UNITY_2018_3_OR_NEWER
            // ビット値はそのままの状態で32bit化して符号なし32bit書き込みをする
            Write((uint)FloatToInt32(value));
#else
            // ビット値はそのままの状態で32bit化して符号なし32bit書き込みをする
            Write((uint)BitConverter.SingleToInt32Bits(value));
#endif
        }


        /// <summary>
        /// 64bit浮動小数点を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public void Write(double value)
        {
            // ビット値はそのままの状態で64bit化して符号なし64bit書き込みをする
            Write((ulong)BitConverter.DoubleToInt64Bits(value));
        }


        /// <summary>
        /// 文字列を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public void Write(string value)
        {
            // 必要になるバッファサイズを調べて、もしバッファサイズが足りないなら
            int encodeSize = encoding.GetByteCount(value);
            if (streamBuffer.Length < encodeSize)
            {
                // バッファを広げる
                streamBuffer = new byte[encodeSize];
            }


            // これから書き込むデータサイズを書き込む
            Write(encodeSize);


            // エンコードをしてデータを書き込む
            encoding.GetBytes(value, 0, value.Length, streamBuffer, 0);
            BaseStream.Write(streamBuffer, 0, encodeSize);
        }
        #endregion


        #region Exception thrower
        /// <summary>
        /// オブジェクトが解放済みの場合に例外を送出します
        /// </summary>
        /// <exception cref="ObjectDisposedException">オブジェクトリソースが解放済みです</exception>
        protected void ThrowExceptionIfObjectDisposed()
        {
            // 解放済みなら
            if (disposed)
            {
                // 例外を投げる
                throw new ObjectDisposedException(null);
            }
        }
        #endregion
    }
}
