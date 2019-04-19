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
        /// SrvmMachine のインスタンスを初期化します
        /// </summary>
        /// <exception cref="InvalidOperationException">仮想マシンのファームウェアが正しく実装されていません</exception>
        /// <exception cref="InvalidOperationException">仮想マシンのプロセッサが正しく実装されていません</exception>
        /// <exception cref="InvalidOperationException">仮想マシンのメモリが正しく実装されていません</exception>
        /// <exception cref="InvalidOperationException">仮想マシンのストレージが正しく実装されていません</exception>
        public SrvmMachine()
        {
            // 仮想マシンのパーツを組み立てる
            BuildMachine(out machineParts);


            // 1つでもマシンパーツがnullになっていたら駄目なのでチェックしつつ仮想マシンの参照を設定する
            (machineParts.Firmware ?? throw new InvalidOperationException("仮想マシンのファームウェアが正しく実装されていません")).Machine = this;
            (machineParts.Processor ?? throw new InvalidOperationException("仮想マシンのプロセッサが正しく実装されていません")).Machine = this;
            (machineParts.Memory ?? throw new InvalidOperationException("仮想マシンのメモリが正しく実装されていません")).Machine = this;
            (machineParts.Storage ?? throw new InvalidOperationException("仮想マシンのストレージが正しく実装されていません")).Machine = this;
        }


        /// <summary>
        /// 仮想マシンに必要な仮想マシンパーツを初期化して組み立てます
        /// </summary>
        /// <param name="machinePartsInfo">組み立てる仮想マシンのパーツ情報</param>
        protected abstract void BuildMachine(out MachinePartsInfo machinePartsInfo);


        /// <summary>
        /// 仮想マシンの電源を投入し、仮想マシンが動作出来るようにします
        /// </summary>
        protected abstract void PowerOn();


        /// <summary>
        /// 仮想マシンの電源を切り、仮想マシンが動作停止出来るようにします
        /// </summary>
        protected abstract void PowerOff();



        /// <summary>
        /// 仮想マシン
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
        }
    }
}