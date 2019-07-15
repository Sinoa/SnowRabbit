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

namespace CarrotCompilerCollection.Compiler
{
    /// <summary>
    /// Ccc コンパイラのコンパイルエラーが発生した場合の例外クラスです
    /// </summary>
    public class CccCompileErrorException : Exception
    {
        /// <summary>
        /// CccCompileErrorException のインスタンスを初期化します
        /// </summary>
        public CccCompileErrorException() : base()
        {
        }


        /// <summary>
        /// CccCompileErrorException のインスタンスを初期化します
        /// </summary>
        /// <param name="message">発生した例外のメッセージ</param>
        public CccCompileErrorException(string message) : base(message)
        {
        }


        /// <summary>
        /// CccCompileErrorException のインスタンスを初期化します
        /// </summary>
        /// <param name="message">発生した例外のメッセージ</param>
        /// <param name="innerException">この例外を発生させた原因の例外</param>
        public CccCompileErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }


        /// <summary>
        /// シリアル化したデータから CccCompileErrorException のインスタンスを初期化します
        /// </summary>
        /// <param name="info">シリアル化されたオブジェクト情報</param>
        /// <param name="context">シリアルデータの転送コンテキスト</param>
        protected CccCompileErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}