﻿// zlib/libpng License
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

namespace SnowRabbit.VirtualMachine.Machine
{
    /// <summary>
    /// 仮想マシン本体を表す仮想マシン抽象クラスです
    /// </summary>
    public abstract class SrvmMachine
    {
        // メンバ変数定義
        private MachinePartsInfo machineParts;



        /// <summary>
        /// 仮想マシンに搭載されているファームウェアモジュール
        /// </summary>
        public SrvmFirmware Firmware => machineParts.Firmware;


        /// <summary>
        /// 仮想マシンに搭載されているプロセッサモジュール
        /// </summary>
        public SrvmProcessor Processor => machineParts.Processor;


        /// <summary>
        /// 仮想マシンに搭載されているメモリモジュール
        /// </summary>
        public SrvmMemory Memory => machineParts.Memory;


        /// <summary>
        /// 仮想マシンに搭載されているストレージモジュール
        /// </summary>
        public SrvmStorage Storage => machineParts.Storage;



        /// <summary>
        /// SrvmMachine のインスタンスを初期化します。
        /// また、仮想マシンが必要なマシンパーツを生成するために BuildMachine を実行します。
        /// </summary>
        /// <exception cref="InvalidOperationException">仮想マシンのファームウェアが正しく実装されていません</exception>
        /// <exception cref="InvalidOperationException">仮想マシンのプロセッサが正しく実装されていません</exception>
        /// <exception cref="InvalidOperationException">仮想マシンのメモリが正しく実装されていません</exception>
        /// <exception cref="InvalidOperationException">仮想マシンのストレージが正しく実装されていません</exception>
        /// <exception cref="InvalidOperationException">仮想マシンの周辺装置が正しく実装されていません</exception>
        public SrvmMachine()
        {
            // 仮想マシンのパーツを組み立てる
            BuildMachine(out machineParts);


            // 1つでもマシンパーツがnullになっていたら駄目なのでチェックしつつ仮想マシンの参照を設定する
            (machineParts.Firmware ?? throw new InvalidOperationException("仮想マシンのファームウェアが正しく実装されていません")).Machine = this;
            (machineParts.Processor ?? throw new InvalidOperationException("仮想マシンのプロセッサが正しく実装されていません")).Machine = this;
            (machineParts.Memory ?? throw new InvalidOperationException("仮想マシンのメモリが正しく実装されていません")).Machine = this;
            (machineParts.Storage ?? throw new InvalidOperationException("仮想マシンのストレージが正しく実装されていません")).Machine = this;


            // 周辺装置の数分回って仮想マシンの参照を設定するが、配列がnullなら長さ0で初期化する
            machineParts.Peripherals = machineParts.Peripherals ?? Array.Empty<SrvmPeripheral>();
            for (int i = 0; i < machineParts.Peripherals.Length; ++i)
            {
                // もし周辺装置の一つでもnullになっていたら死亡するようにする
                (machineParts.Peripherals[i] ?? throw new InvalidOperationException("仮想マシンの周辺装置が正しく実装されていません")).Machine = this;
            }
        }


        /// <summary>
        /// 仮想マシンに必要な仮想マシンパーツインスタンスを初期化して組み立てます
        /// </summary>
        /// <param name="machinePartsInfo">組み立てる仮想マシンのパーツ情報</param>
        protected abstract void BuildMachine(out MachinePartsInfo machinePartsInfo);


        /// <summary>
        /// 仮想マシンの電源を投入をします。
        /// 仮想マシンパーツのすべてを初期化してから、仮想マシンが動作出来るようにします。
        /// </summary>
        public void PowerOn()
        {
            // 仮想マシンパーツの初期化をしていく
            machineParts.Firmware.Startup();
            machineParts.Processor.Startup();
            machineParts.Memory.Startup();
            machineParts.Storage.Startup();


            // 周辺装置の初期化もしていく
            for (int i = 0; i < machineParts.Peripherals.Length; ++i)
            {
                // 初期化を呼ぶ
                machineParts.Peripherals[i].Startup();
            }
        }


        /// <summary>
        /// 仮想マシンの電源を切ります。
        /// 仮想マシンパーツのすべてを停止してから、想マシンが動作停止出来るようにします
        /// </summary>
        public void PowerOff()
        {
            // 周辺装置の停止をしていく
            for (int i = machineParts.Peripherals.Length - 1; i >= 0; --i)
            {
                // 停止を呼ぶ
                machineParts.Peripherals[i].Shutdown();
            }


            // 仮想マシンパーツの停止をしていく
            machineParts.Storage.Shutdown();
            machineParts.Memory.Shutdown();
            machineParts.Processor.Shutdown();
            machineParts.Firmware.Shutdown();
        }



        /// <summary>
        /// 仮想マシンが使用する各種マシンパーツの参照を保持した構造体です
        /// </summary>
        protected struct MachinePartsInfo
        {
            /// <summary>
            /// 仮想マシンが使用するファームウェアモジュール
            /// </summary>
            public SrvmFirmware Firmware;

            /// <summary>
            /// 仮想マシンが使用するプロセッサモジュール
            /// </summary>
            public SrvmProcessor Processor;

            /// <summary>
            /// 仮想マシンが使用するメモリモジュール
            /// </summary>
            public SrvmMemory Memory;

            /// <summary>
            /// 仮想マシンが使用するストレージモジュール
            /// </summary>
            public SrvmStorage Storage;

            /// <summary>
            /// 仮想マシンが使用する複数の周辺機器モジュール
            /// </summary>
            /// <remarks>
            /// 仮想マシン構築時はnullを設定していても問題ありませんが、要素内がnullの設定されてはいけません。
            /// また、構築された時は長さ0の配列として初期化されます。
            /// </remarks>
            public SrvmPeripheral[] Peripherals;
        }
    }
}