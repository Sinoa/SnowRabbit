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
using SnowRabbit.Diagnostics.Logging;
using SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral;

namespace SnowRabbit.RuntimeEngine.VirtualMachine
{
    /// <summary>
    /// SnowRabbit が実装する仮想マシンファームウェアクラスです
    /// </summary>
    public class SrvmFirmware : SrvmMachineParts
    {
        // 以下メンバ変数定義
        private bool disposed;
        private readonly Dictionary<string, SrPeripheral> peripheralTable = new Dictionary<string, SrPeripheral>();
        private readonly List<IDisposable> disposableObjectList = new List<IDisposable>();



        /// <summary>
        /// 指定された周辺機器クラスのインスタンスをアタッチします。
        /// もし、アタッチするインスタンスが IDisposable インターフェイスを、実装している場合に SrvmFirmware インスタンスが破棄される時に Dispose を呼び出します。
        /// </summary>
        /// <param name="targetInstance">アタッチする周辺機器クラスのインスタンス</param>
        /// <exception cref="ArgumentNullException">targetInstance が null です</exception>
        public void AttachPeripheral(object targetInstance)
        {
            // ペリフェラルを生成して存在を確認する
            var peripheral = new SrPeripheral(targetInstance ?? throw new ArgumentNullException(nameof(targetInstance)));
            var name = peripheral.Name;
            if (peripheralTable.ContainsKey(name))
            {
                // 既にあるなら警告を出して何もせず終了
                SrLogger.Warning(SharedString.LogTag.SR_VM_FIRMWARE, $"Already exsits peripheral '{peripheral.Name}'.");
                return;
            }


            // 存在していなければ追加する
            SrLogger.Trace(SharedString.LogTag.SR_VM_FIRMWARE, $"Attach peripheral '{peripheral.Name}'.");
            peripheralTable[name] = peripheral;


            // もし IDisposable を実装しているのなら
            if (targetInstance is IDisposable)
            {
                // 解放リストに突っ込む
                SrLogger.Trace(SharedString.LogTag.SR_VM_FIRMWARE, $"{peripheral.Name} is disposable, Adding to disposableObjectList.");
                disposableObjectList.Add((IDisposable)targetInstance);
            }
        }


        /// <summary>
        /// 確保したリソースを解放します
        /// </summary>
        /// <param name="disposing">マネージドを含む解放の場合は true を、アンマネージドのみ解放の場合は false を指定</param>
        protected override void Dispose(bool disposing)
        {
            // 既に解放済みなら即終了
            if (disposed) return;


            // マネージド解放なら
            if (disposing)
            {
                // 解放リストの中にあるオブジェクト分回る
                SrLogger.Trace(SharedString.LogTag.SR_VM_FIRMWARE, $"Dispose target count '{disposableObjectList.Count}'.");
                foreach (var disposable in disposableObjectList)
                {
                    // 解放する
                    disposable.Dispose();
                }


                // リストを空にする
                disposableObjectList.Clear();
            }


            // 解放済みをマークして基底のDisposeも呼ぶ
            disposed = true;
            base.Dispose(disposing);
        }
    }
}