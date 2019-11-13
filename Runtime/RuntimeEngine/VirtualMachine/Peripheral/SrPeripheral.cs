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
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SnowRabbit.Diagnostics.Logging;

namespace SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral
{
    /// <summary>
    /// SnowRabbit の周辺機器を表すクラスです
    /// </summary>
    internal class SrPeripheral
    {
        // メンバ変数定義
        private object instance = null;
        private Dictionary<string, int> functionTable = new Dictionary<string, int>();


        /// <summary>
        /// 周辺機器名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// SrPeripheral クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="targetInstance">SrPeripheralAttribute 属性がついたインスタンス</param>
        /// <exception cref="ArgumentNullException">targetInstance が null です</exception>
        /// <exception cref="SrPeripheralAttributeNotFoundException">targetInstance に SrPeripheralAttribute 属性が見つかりませんでした</exception>
        public SrPeripheral(object targetInstance)
        {
            // まずは型情報を取得する
            instance = targetInstance ?? throw new ArgumentNullException(nameof(targetInstance));
            var targetType = targetInstance.GetType();
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, $"SrPeripheral Initialize of '{targetType.FullName}'.");


            // 周辺機器属性を取得するが見つからなかった場合は、例外を投げる
            var attribute = targetType.GetCustomAttribute<SrPeripheralAttribute>() ?? throw new SrPeripheralAttributeNotFoundException($"'{targetType.FullName}' に SrPeripheralAttribute 属性が見つかりませんでした");
            var peripheralName = attribute.Name;
        }
    }
}