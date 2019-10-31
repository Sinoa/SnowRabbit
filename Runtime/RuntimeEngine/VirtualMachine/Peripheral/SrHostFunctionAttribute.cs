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

namespace SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral
{
    /// <summary>
    /// SnowRabbit の周辺機器関数名クラスであることを示す属性クラスです
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class SrHostFunctionAttribute : Attribute
    {
        /// <summary>
        /// 登録される関数名
        /// </summary>
        public string Name { get; }



        /// <summary>
        /// SrHostFunctionAttribute クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="name">登録する周辺機器名</param>
        /// <exception cref="ArgumentException">関数名に null または 空文字列 または 空白 を設定することは出来ません。</exception>
        public SrHostFunctionAttribute(string name)
        {
            // 関数名を覚える
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(InternalString.ExceptionMessage.SrHostFunctionAttribute.INVALID_FUNCTION_NAME, nameof(name)) : name;
        }
    }
}