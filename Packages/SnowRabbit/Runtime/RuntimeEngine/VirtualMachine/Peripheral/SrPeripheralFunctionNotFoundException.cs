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

namespace SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral
{
    /// <summary>
    /// 周辺機器関数を見つけられなかった場合の例外クラスです
    /// </summary>
    [Serializable]
    public class SrPeripheralFunctionNotFoundException : SnowRabbitException
    {
        /// <summary>
        /// SrPeripheralFunctionNotFoundException クラスのインスタンスを初期化します
        /// </summary>
        public SrPeripheralFunctionNotFoundException()
        {
        }


        /// <summary>
        /// SrPeripheralFunctionNotFoundException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="message">例外に設定するメッセージ</param>
        public SrPeripheralFunctionNotFoundException(string message) : base(message)
        {
        }


        /// <summary>
        /// SrPeripheralFunctionNotFoundException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="message">例外に設定するメッセージ</param>
        /// <param name="inner">この例外を発生させる原因となった例外</param>
        public SrPeripheralFunctionNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }


        /// <summary>
        /// シリアル化したデータを使用して SrPeripheralFunctionNotFoundException クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">スローされている例外に関するシリアル化済みオブジェクトデータを保持している SerializationInfo</param>
        /// <param name="context">転送元または転送先についてのコンテキスト情報を含む StreamingContext</param>
        protected SrPeripheralFunctionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}