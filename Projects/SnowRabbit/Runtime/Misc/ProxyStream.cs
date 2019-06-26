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

using System.IO;

namespace SnowRabbit.VirtualMachine.Runtime
{
    /// <summary>
    /// ストリームの参照を変わりに持つストリームクラスです
    /// </summary>
    public class ProxyStream : Stream
    {
        /// <summary>
        /// 制御対象となるストリーム
        /// </summary>
        public Stream TargetStream { get; set; }


        /// <summary>
        /// 対象ストリームの CanRead プロパティにアクセスします
        /// </summary>
        public override bool CanRead => TargetStream.CanRead;


        /// <summary>
        /// 対象ストリームの CanSeek プロパティにアクセスします
        /// </summary>
        public override bool CanSeek => TargetStream.CanSeek;


        /// <summary>
        /// 対象ストリームの CanWrite プロパティにアクセスします
        /// </summary>
        public override bool CanWrite => TargetStream.CanWrite;


        /// <summary>
        /// 対象ストリームの Length プロパティにアクセスします
        /// </summary>
        public override long Length => TargetStream.Length;


        /// <summary>
        /// 対象ストリームの Position プロパティにアクセスします
        /// </summary>
        public override long Position { get => TargetStream.Position; set => TargetStream.Position = value; }



        /// <summary>
        /// 対象ストリームの Flush 関数にアクセスします
        /// </summary>
        public override void Flush()
        {
            // Flush をそのまま呼び出す
            TargetStream.Flush();
        }


        /// <summary>
        /// 対象ストリームの Read 関数にアクセスします
        /// </summary>
        /// <param name="buffer">Read関数の buffer 引数</param>
        /// <param name="offset">Read関数の offset 引数</param>
        /// <param name="count">Read関数の count 引数</param>
        /// <returns>Read関数の戻り値を返します</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // Read をそのまま呼び出す
            return TargetStream.Read(buffer, offset, count);
        }


        /// <summary>
        /// 対象ストリームの Seek 関数にアクセスします
        /// </summary>
        /// <param name="offset">Seek関数の offset 引数</param>
        /// <param name="origin">Seek関数の origin 引数</param>
        /// <returns>Seek関数の戻り値を返します</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            // Seek をそのまま呼び出す
            return TargetStream.Seek(offset, origin);
        }


        /// <summary>
        /// 対象ストリームの SetLength 関数にアクセスします
        /// </summary>
        /// <param name="value">SetLength関数の value 引数</param>
        public override void SetLength(long value)
        {
            // SetLength をそのまま呼び出す
            TargetStream.SetLength(value);
        }


        /// <summary>
        /// 対象ストリームの Write 関数にアクセスします
        /// </summary>
        /// <param name="buffer">Wriet関数の buffer 引数</param>
        /// <param name="offset">Write関数の offset 引数</param>
        /// <param name="count">Write関数の count 引数</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            // Write をそのまま呼び出す
            TargetStream.Write(buffer, offset, count);
        }
    }
}