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
using System.Runtime.Serialization;

namespace SnowRabbit.VirtualMachine.Runtime
{
    /// <summary>
    /// SnowRabbit仮想マシンがプログラムを見つけられなかった時に送出する例外です
    /// </summary>
    [Serializable]
    public class SrProgramNotFoundException : SrException
    {
        /// <summary>
        /// 見つけられなかったプログラムのパス
        /// </summary>
        public string Path { get; private set; }



        /// <summary>
        /// SrProgramNotFoundException のインスタンスを初期化します
        /// </summary>
        public SrProgramNotFoundException() : base()
        {
        }


        /// <summary>
        /// SrProgramNotFoundException のインスタンスを初期化します
        /// </summary>
        /// <param name="path">見つけられなかったプログラムのパス</param>
        public SrProgramNotFoundException(string path) : base($"指定されたパス '{path}' のプログラムを見つけられませんでした")
        {
            // パスを覚える
            Path = path;
        }


        /// <summary>
        /// SrProgramNotFoundException のインスタンスを初期化します
        /// </summary>
        /// <param name="path">見つけられなかったプログラムのパス</param>
        /// <param name="innerException">この例外を発生させた原因の例外</param>
        public SrProgramNotFoundException(string path, Exception innerException) : base($"指定されたパス '{path}' のプログラムを見つけられませんでした", innerException)
        {
            // パスを覚える
            Path = path;
        }


        /// <summary>
        /// シリアル化したデータから SrProgramNotFoundException のインスタンスを初期化します
        /// </summary>
        /// <param name="info">シリアル化されたオブジェクト情報</param>
        /// <param name="context">シリアルデータの転送コンテキスト</param>
        protected SrProgramNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}