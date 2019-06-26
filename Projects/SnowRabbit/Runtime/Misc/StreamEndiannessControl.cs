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

namespace SnowRabbit.Runtime
{
    #region StreamEndianness interface and abstract class
    /// <summary>
    /// 特定エンディアンの読み書きをするコントロールインターフェイスです
    /// </summary>
    public interface IStreamEndiannessControl
    {
        #region Read function
        /// <summary>
        /// 符号付き8bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        sbyte ReadSbyte();


        /// <summary>
        /// 符号付き16bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        short ReadShort();


        /// <summary>
        /// 符号付き32bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        int ReadInt();


        /// <summary>
        /// 符号付き64bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        long ReadLong();


        /// <summary>
        /// 符号なし8bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        byte ReadByte();


        /// <summary>
        /// 符号なし16bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        ushort ReadUshort();


        /// <summary>
        /// 符号なし32bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        uint ReadUint();


        /// <summary>
        /// 符号なし64bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        ulong ReadUlong();


        /// <summary>
        /// 32bit浮動小数点を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        float ReadFloat();


        /// <summary>
        /// 64bit浮動小数点を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        double ReadDouble();


        /// <summary>
        /// 文字列を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        string ReadString();
        #endregion


        #region Write function
        /// <summary>
        /// 符号付き8bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        void Write(sbyte value);


        /// <summary>
        /// 符号付き16bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        void Write(short value);


        /// <summary>
        /// 符号付き32bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        void Write(int value);


        /// <summary>
        /// 符号付き64bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        void Write(long value);


        /// <summary>
        /// 符号なし8bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        void Write(byte value);


        /// <summary>
        /// 符号なし16bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        void Write(ushort value);


        /// <summary>
        /// 符号なし32bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        void Write(uint value);


        /// <summary>
        /// 符号なし64bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        void Write(ulong value);


        /// <summary>
        /// 32bit浮動小数点を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        void Write(float value);


        /// <summary>
        /// 64bit浮動小数点を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        void Write(double value);


        /// <summary>
        /// 文字列を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        void Write(string value);
        #endregion
    }



    /// <summary>
    /// 特定エンディアンの読み書きをするコントロール抽象クラスです
    /// </summary>
    public abstract class StreamEndiannessControl : IStreamEndiannessControl, IDisposable
    {
        // 定数定義
        private const int IOBufferSize = 4 << 10;
        private const int CharBufferSize = 2 << 10;
        public const int MinIOBufferSize = 8;
        public const int MinCharBufferSize = 16;

        // クラス変数宣言
        private static readonly Encoding encoding = new UTF8Encoding(false);

        // メンバ変数定義
        private bool disposed;
        private bool leaveOpen;
        protected byte[] streamBuffer;
        protected char[] charBuffer;



        /// <summary>
        /// このコントロールクラスが取り扱っているストリーム
        /// </summary>
        public Stream BaseStream { get; private set; }



        #region Constructor and Dispose
        /// <summary>
        /// StreamEndiannessControl クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">ストリーム制御を行う対象のストリーム</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        public StreamEndiannessControl(Stream stream) : this(stream, false)
        {
        }


        /// <summary>
        /// StreamEndiannessControl クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">ストリーム制御を行う対象のストリーム</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時にストリームを開いたままにする場合は true を、閉じる場合は false</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        public StreamEndiannessControl(Stream stream, bool leaveOpen) : this(stream, leaveOpen, IOBufferSize, CharBufferSize)
        {
        }


        /// <summary>
        /// StreamEndiannessControl クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">ストリーム制御を行う対象のストリーム</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時にストリームを開いたままにする場合は true を、閉じる場合は false</param>
        /// <param name="ioBufferSize">ストリームコントロールに用いるIOバッファサイズ。必ず MinIOBufferSize 定数以上のサイズを指定しなければいけません。</param>
        /// <param name="charBufferSize">文字データを読み込む際に用いる char バッファサイズ（2で割り切れない場合は切り上げされます）。必ず MinCharBufferSize 定数以上のサイズを指定しなければいけません。</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">ioBufferSize に MinIOBufferSize 未満の値は指定出来ません</exception>
        /// <exception cref="ArgumentOutOfRangeException">charBufferSize に MinCharBufferSize 未満の値は指定出来ません</exception>
        public StreamEndiannessControl(Stream stream, bool leaveOpen, int ioBufferSize, int charBufferSize)
        {
            // 初期化をする
            BaseStream = stream ?? throw new ArgumentNullException(nameof(stream));
            streamBuffer = new byte[ioBufferSize >= MinIOBufferSize ? ioBufferSize : throw new ArgumentOutOfRangeException(nameof(ioBufferSize), $"{nameof(ioBufferSize)} に {nameof(MinIOBufferSize)} 未満の値は指定出来ません")];
            charBuffer = new char[charBufferSize >= MinCharBufferSize ? charBufferSize + (charBufferSize & 1) : throw new ArgumentOutOfRangeException(nameof(charBufferSize), $"{nameof(charBufferSize)} に {nameof(MinCharBufferSize)} 未満の値は指定出来ません")];
            this.leaveOpen = leaveOpen;
        }


        /// <summary>
        /// インスタンスのリソースを解放します
        /// </summary>
        ~StreamEndiannessControl()
        {
            // ファイナライザからのDispose呼び出し
            Dispose(false);
        }


        /// <summary>
        /// インスタンスのリソースを解放します
        /// </summary>
        public void Dispose()
        {
            // DisposeからのDispose呼び出しをしてGCに自身のファイナライザを止めてもらう
            Dispose(true);
            GC.SuppressFinalize(this);
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
                // もし解放時にストリームを閉じる場合は
                if (!leaveOpen)
                {
                    // ストリームを閉じる
                    BaseStream.Dispose();
                }
            }


            // 解放済みマーク
            disposed = true;
        }
        #endregion


        #region Utility function
        /// <summary>
        /// ストリームから文字列として読み込むためのUTF-8データを読み込んで、文字バッファ 'charBuffer' にデコードした文字を満たします
        /// </summary>
        /// <remarks>
        /// この関数は現在のストリーム位置から読み取るべきUTF-8データを読み込みますが
        /// 同時に 'streamBuffer' が読み取るべきバッファサイズが不足する場合は、バッファサイズの拡張を試みます
        /// </remarks>
        /// <returns>読み込んだ文字バッファのサイズを返します</returns>
        /// <exception cref="ObjectDisposedException">オブジェクトリソースが解放済みです</exception>
        protected int ReadChars()
        {
            // 事前例外処理を行っておく
            ThrowExceptionIfObjectDisposed();


            // 符号付き32bit整数を読み込んで読み取るべきデータサイズを知って、バッファに収まらない場合は
            var dataSize = ReadInt();
            if (streamBuffer.Length < dataSize)
            {
                // 読み込むべきデータサイズを満たすバッファを新しく確保する
                // （結局は全部読み込まれて以前のバッファ内容が消えるので、素直なnewで対応）
                streamBuffer = new byte[dataSize];
            }


            // データを読み取る
            BaseStream.Read(streamBuffer, 0, dataSize);


            // 読み取られたデータからデコードされる文字数を知って、文字バッファに収まらない場合は
            var charCount = encoding.GetCharCount(streamBuffer, 0, dataSize);
            if (charBuffer.Length < charCount)
            {
                // 必要なバッファサイズを確保する
                // （結局は全部読み込まれて以前のバッファ内容が消えるので、素直なnewで対応）
                charBuffer = new char[charCount];
            }


            // UTF-8配列をデコードしてデコードした文字の数を返す
            encoding.GetChars(streamBuffer, 0, dataSize, charBuffer, 0);
            return charCount;
        }


        /// <summary>
        /// ストリームから指定された長さを 'streamBuffer' の先頭に読み込みます。また読み込んだデータを反転するかどうかの指示が出来ます。
        /// </summary>
        /// <param name="size">読み取る長さ</param>
        /// <param name="reverse">読み取ったデータを反転するかどうか</param>
        /// <exception cref="ObjectDisposedException">オブジェクトリソースが解放済みです</exception>
        protected void Read(int size, bool reverse)
        {
            // 事前例外処理を行っておく
            ThrowExceptionIfObjectDisposed();


            // 読み込もうとしているサイズがバッファサイズを超えるのであれば
            if (streamBuffer.Length < size)
            {
                // 新しくバッファを確保する
                streamBuffer = new byte[size];
            }


            // 指定された長さをストリームから読み込んで反転指示が出ているなら
            BaseStream.Read(streamBuffer, 0, size);
            if (reverse)
            {
                // 読み取ったデータを反転する
                Array.Reverse(streamBuffer, 0, size);
            }
        }


        /// <summary>
        /// ストリームに 'streamBuffer' から指定されたサイズを書き込みます。また書き込む前にデータを反転するかどうかの指示が出来ます。
        /// </summary>
        /// <param name="size">書き込むサイズ</param>
        /// <param name="reverse">バッファの内容を反転するかどうか</param>
        /// <exception cref="ObjectDisposedException">オブジェクトリソースが解放済みです</exception>
        protected void Write(int size, bool reverse)
        {
            // 事前例外処理を行っておく
            ThrowExceptionIfObjectDisposed();


            // もし反転指示が出ているのなら
            if (reverse)
            {
                // 指定されたサイズの反転をする
                Array.Reverse(streamBuffer, 0, size);
            }


            // 指定されたサイズのバッファを書き込む
            BaseStream.Write(streamBuffer, 0, size);
        }


#if UNITY_2018_3_OR_NEWER
        /// <summary>
        /// 32bit浮動小数点を32bit整数にビット変換をします
        /// </summary>
        /// <param name="value">変換する32bit浮動小数点</param>
        /// <returns>変換された32bit整数を返します</returns>
        unsafe protected int FloatToInt32(float value)
        {
            return *(int*)&value;
        }
#endif
        #endregion


        #region Read function
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
            return (short)ReadUshort();
        }


        /// <summary>
        /// 符号付き32bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public int ReadInt()
        {
            // 符号なし32bit整数を符号付きとして返す
            return (int)ReadUint();
        }


        /// <summary>
        /// 符号付き64bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public long ReadLong()
        {
            // 符号なし64bit整数を符号付きとして返す
            return (long)ReadUlong();
        }


        /// <summary>
        /// 符号なし8bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public abstract byte ReadByte();


        /// <summary>
        /// 符号なし16bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public abstract ushort ReadUshort();


        /// <summary>
        /// 符号なし32bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public abstract uint ReadUint();


        /// <summary>
        /// 符号なし64bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public abstract ulong ReadUlong();


        /// <summary>
        /// 32bit浮動小数点を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public abstract float ReadFloat();


        /// <summary>
        /// 64bit浮動小数点を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public abstract double ReadDouble();


        /// <summary>
        /// 文字列を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public string ReadString()
        {
            // 文字バッファに文字を詰めてから文字列として返す
            var charCount = ReadChars();
            return new string(charBuffer, 0, charCount);
        }
        #endregion


        #region Write function
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
        public abstract void Write(byte value);


        /// <summary>
        /// 符号なし16bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public abstract void Write(ushort value);


        /// <summary>
        /// 符号なし32bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public abstract void Write(uint value);


        /// <summary>
        /// 符号なし64bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public abstract void Write(ulong value);


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
            var encodeSize = encoding.GetByteCount(value);
            if (streamBuffer.Length < encodeSize)
            {
                // バッファを広げる
                streamBuffer = new byte[encodeSize];
            }


            // エンコード後、データサイズを書き込んでエンコードデータを書き込む
            encoding.GetBytes(value, 0, value.Length, streamBuffer, 0);
            Write(encodeSize);
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
    #endregion



    #region LittleEndian ver
    /// <summary>
    /// ストリームの対象がリトルエンディアン向けコントロールクラスです
    /// </summary>
    public class LittleEndianControl : StreamEndiannessControl
    {
        #region Constructor
        /// <summary>
        /// LittleEndianControl クラスのインスタンスを初期化します
        /// </summary>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        public LittleEndianControl(Stream stream) : base(stream)
        {
        }


        /// <summary>
        /// LittleEndianControl クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">ストリーム制御を行う対象のストリーム</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時にストリームを開いたままにする場合は true を、閉じる場合は false</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        public LittleEndianControl(Stream stream, bool leaveOpen) : base(stream, leaveOpen)
        {
        }


        /// <summary>
        /// LittleEndianControl クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">ストリーム制御を行う対象のストリーム</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時にストリームを開いたままにする場合は true を、閉じる場合は false</param>
        /// <param name="ioBufferSize">ストリームコントロールに用いるIOバッファサイズ。必ず MinIOBufferSize 定数以上のサイズを指定しなければいけません。</param>
        /// <param name="charBufferSize">文字データを読み込む際に用いる char バッファサイズ（2で割り切れない場合は切り上げされます）。必ず MinCharBufferSize 定数以上のサイズを指定しなければいけません。</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">ioBufferSize に MinIOBufferSize 未満の値は指定出来ません</exception>
        /// <exception cref="ArgumentOutOfRangeException">charBufferSize に MinCharBufferSize 未満の値は指定出来ません</exception>
        public LittleEndianControl(Stream stream, bool leaveOpen, int ioBufferSize, int charBufferSize) : base(stream, leaveOpen, ioBufferSize, charBufferSize)
        {
        }
        #endregion


        #region Read function
        /// <summary>
        /// 符号なし8bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public override byte ReadByte()
        {
            // 型サイズ分読み取ってそのまま返す
            Read(sizeof(byte), false);
            return streamBuffer[0];
        }


        /// <summary>
        /// 符号なし16bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public override ushort ReadUshort()
        {
            // 型サイズ分読み取って変換して返す
            Read(sizeof(ushort), !BitConverter.IsLittleEndian);
            return BitConverter.ToUInt16(streamBuffer, 0);
        }


        /// <summary>
        /// 符号なし32bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public override uint ReadUint()
        {
            // 型サイズ分読み取って変換して返す
            Read(sizeof(uint), !BitConverter.IsLittleEndian);
            return BitConverter.ToUInt32(streamBuffer, 0);
        }


        /// <summary>
        /// 符号なし64bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public override ulong ReadUlong()
        {
            // 型サイズ分読み取って変換して返す
            Read(sizeof(ulong), !BitConverter.IsLittleEndian);
            return BitConverter.ToUInt64(streamBuffer, 0);
        }


        /// <summary>
        /// 32bit浮動小数点を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public override float ReadFloat()
        {
            // 型サイズ分読み取って変換して返す
            Read(sizeof(float), !BitConverter.IsLittleEndian);
            return BitConverter.ToSingle(streamBuffer, 0);
        }


        /// <summary>
        /// 64bit浮動小数点を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public override double ReadDouble()
        {
            // 型サイズ分読み取って変換して返す
            Read(sizeof(double), !BitConverter.IsLittleEndian);
            return BitConverter.ToDouble(streamBuffer, 0);
        }
        #endregion


        #region Write function
        /// <summary>
        /// 符号なし8bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public override void Write(byte value)
        {
            // バッファに入れてそのまま書き込む
            streamBuffer[0] = value;
            Write(1, false);
        }


        /// <summary>
        /// 符号なし16bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public override void Write(ushort value)
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


            // 書き込む
            Write(2, false);
        }


        /// <summary>
        /// 符号なし32bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public override void Write(uint value)
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


            // 書き込む
            Write(4, false);
        }


        /// <summary>
        /// 符号なし64bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public override void Write(ulong value)
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


            // 書き込む
            Write(8, false);
        }
        #endregion
    }
    #endregion



    #region BigEndian ver
    /// <summary>
    /// ストリームの対象がビッグエンディアン向けコントロールクラスです
    /// </summary>
    public class BigEndianControl : StreamEndiannessControl
    {
        #region Constructor
        /// <summary>
        /// BigEndianControl クラスのインスタンスを初期化します
        /// </summary>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        public BigEndianControl(Stream stream) : base(stream, false)
        {
        }


        /// <summary>
        /// BigEndianControl クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">ストリーム制御を行う対象のストリーム</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時にストリームを開いたままにする場合は true を、閉じる場合は false</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        public BigEndianControl(Stream stream, bool leaveOpen) : base(stream, leaveOpen)
        {
        }


        /// <summary>
        /// BigEndianControl クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">ストリーム制御を行う対象のストリーム</param>
        /// <param name="leaveOpen">このインスタンスが破棄される時にストリームを開いたままにする場合は true を、閉じる場合は false</param>
        /// <param name="ioBufferSize">ストリームコントロールに用いるIOバッファサイズ。必ず MinIOBufferSize 定数以上のサイズを指定しなければいけません。</param>
        /// <param name="charBufferSize">文字データを読み込む際に用いる char バッファサイズ（2で割り切れない場合は切り上げされます）。必ず MinCharBufferSize 定数以上のサイズを指定しなければいけません。</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        /// <exception cref="ArgumentOutOfRangeException">ioBufferSize に MinIOBufferSize 未満の値は指定出来ません</exception>
        /// <exception cref="ArgumentOutOfRangeException">charBufferSize に MinCharBufferSize 未満の値は指定出来ません</exception>
        public BigEndianControl(Stream stream, bool leaveOpen, int ioBufferSize, int charBufferSize) : base(stream, leaveOpen, ioBufferSize, charBufferSize)
        {
        }
        #endregion


        #region Read function
        /// <summary>
        /// 符号なし8bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public override byte ReadByte()
        {
            // 型サイズ分読み取ってそのまま返す
            Read(sizeof(byte), false);
            return streamBuffer[0];
        }


        /// <summary>
        /// 符号なし16bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public override ushort ReadUshort()
        {
            // 型サイズ分読み取って変換して返す
            Read(sizeof(ushort), BitConverter.IsLittleEndian);
            return BitConverter.ToUInt16(streamBuffer, 0);
        }


        /// <summary>
        /// 符号なし32bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public override uint ReadUint()
        {
            // 型サイズ分読み取って変換して返す
            Read(sizeof(uint), BitConverter.IsLittleEndian);
            return BitConverter.ToUInt32(streamBuffer, 0);
        }


        /// <summary>
        /// 符号なし64bit整数を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public override ulong ReadUlong()
        {
            // 型サイズ分読み取って変換して返す
            Read(sizeof(ulong), BitConverter.IsLittleEndian);
            return BitConverter.ToUInt64(streamBuffer, 0);
        }


        /// <summary>
        /// 32bit浮動小数点を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public override float ReadFloat()
        {
            // 型サイズ分読み取って変換して返す
            Read(sizeof(float), BitConverter.IsLittleEndian);
            return BitConverter.ToSingle(streamBuffer, 0);
        }


        /// <summary>
        /// 64bit浮動小数点を読み込みます
        /// </summary>
        /// <returns>読み込まれた値を返します</returns>
        public override double ReadDouble()
        {
            // 型サイズ分読み取って変換して返す
            Read(sizeof(double), BitConverter.IsLittleEndian);
            return BitConverter.ToDouble(streamBuffer, 0);
        }
        #endregion


        #region Write function
        /// <summary>
        /// 符号なし8bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public override void Write(byte value)
        {
            // バッファに入れてそのまま書き込む
            streamBuffer[0] = value;
            Write(1, false);
        }


        /// <summary>
        /// 符号なし16bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public override void Write(ushort value)
        {
            // もしリトルエンディアン動作なら
            if (BitConverter.IsLittleEndian)
            {
                // ビッグエンディアンの場合のバイト配列の詰め込みをする
                streamBuffer[0] = (byte)((value >> 8) & 0xFF);
                streamBuffer[1] = (byte)((value >> 0) & 0xFF);
            }
            else
            {
                // リトルエンディアンの場合のバイト配列の詰め込みをする
                streamBuffer[0] = (byte)((value >> 0) & 0xFF);
                streamBuffer[1] = (byte)((value >> 8) & 0xFF);
            }


            // 書き込む
            Write(2, false);
        }


        /// <summary>
        /// 符号なし32bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public override void Write(uint value)
        {
            // もしリトルエンディアン動作なら
            if (BitConverter.IsLittleEndian)
            {
                // ビッグエンディアンの場合のバイト配列の詰め込みをする
                streamBuffer[0] = (byte)((value >> 24) & 0xFF);
                streamBuffer[1] = (byte)((value >> 16) & 0xFF);
                streamBuffer[2] = (byte)((value >> 8) & 0xFF);
                streamBuffer[3] = (byte)((value >> 0) & 0xFF);
            }
            else
            {
                // リトルエンディアンの場合のバイト配列の詰め込みをする
                streamBuffer[0] = (byte)((value >> 0) & 0xFF);
                streamBuffer[1] = (byte)((value >> 8) & 0xFF);
                streamBuffer[2] = (byte)((value >> 16) & 0xFF);
                streamBuffer[3] = (byte)((value >> 24) & 0xFF);
            }


            // 書き込む
            Write(4, false);
        }


        /// <summary>
        /// 符号なし64bit整数を書き込みます
        /// </summary>
        /// <param name="value">書き込む値</param>
        public override void Write(ulong value)
        {
            // もしリトルエンディアン動作なら
            if (BitConverter.IsLittleEndian)
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
            else
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


            // 書き込む
            Write(8, false);
        }
        #endregion
    }
    #endregion
}