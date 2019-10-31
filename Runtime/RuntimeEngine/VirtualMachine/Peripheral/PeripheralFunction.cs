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
using System.Reflection;
using SnowRabbit.Diagnostics.Logging;

namespace SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral
{
    /// <summary>
    /// 仮想マシンとホスト間の関数を取り扱う周辺機器関数クラスです
    /// </summary>
    internal class PeripheralFunction
    {
        // メンバ変数定義
        private MethodInfo method;
        private bool isTask;
        private Type returnType;
        private Type taskResultType;
        private PropertyInfo taskResultProperty;



        /// <summary>
        /// PeripheralFunction クラスのインスタンスを初期化子ます
        /// </summary>
        /// <param name="info">周辺機器関数として使用する関数情報</param>
        /// <exception cref="ArgumentNullException">info が null です</exception>
        public PeripheralFunction(MethodInfo info)
        {
            // 関数情報を覚える
            SrLogger.Trace(InternalString.LogTag.PERIPHERAL, InternalString.LogMessage.Peripheral.BEGIN_CREATE_PERIPHERAL_FUNCTION);
            method = info ?? throw new ArgumentNullException(nameof(info));


            // 各種情報のセットアップをする
            SetupReturnInfo(info);
            SetupArgumentInfo(info);
        }


        private void SetupReturnInfo(MethodInfo info)
        {
            returnType = info.ReturnType;
            if ((returnType == typeof(void) || returnType.IsPrimitive) && returnType != typeof(IntPtr) && returnType != typeof(UIntPtr))
            {
                isTask = false;
                taskResultType = null;
                taskResultProperty = null;
                return;
            }
        }


        private void SetupArgumentInfo(MethodInfo info)
        {
        }
    }
}