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
using SnowRabbit.Diagnostics.Logging;

namespace SnowRabbit.RuntimeEngine.VirtualMachine
{
    /// <summary>
    /// SnowRabbit が実装する仮想マシンクラスです
    /// </summary>
    public class SrvmMachine : SrDisposable
    {
        // 以下メンバ変数定義
        private bool disposed;
        private readonly SrvmProcessor processor;
        private readonly SrvmMemory memory;
        private readonly SrvmFirmware firmware;
        private readonly SrvmStorage storage;



        /// <summary>
        /// SrvmMachine クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="factory">仮想マシンが使用する各種パーツクラスを生成する SrvmMachinePartsFactory のインスタンス</param>
        /// <exception cref="ArgumentNullException">factory が null です</exception>
        /// <exception cref="SrMachinePartsMissingException">Processor マシンパーツを見失いました</exception>
        /// <exception cref="SrMachinePartsMissingException">Memory マシンパーツを見失いました</exception>
        /// <exception cref="SrMachinePartsMissingException">Firmware マシンパーツを見失いました</exception>
        /// <exception cref="SrMachinePartsMissingException">Storage マシンパーツを見失いました</exception>
        public SrvmMachine(SrvmMachinePartsFactory factory)
        {
            // もし null を渡されたら
            if (factory == null)
            {
                // 何も作れない悲しみを背負った
                throw new ArgumentNullException(nameof(factory));
            }


            // プロセッサ、メモリ、ファームウェア、ストレージ を生成する
            SrLogger.Trace(SharedString.LogTag.VIRTUAL_MACHINE, "Create SrvmMachine.");
            processor = factory.CreateProcessor() ?? throw new SrMachinePartsMissingException("Processor マシンパーツを見失いました");
            memory = factory.CreateMemory() ?? throw new SrMachinePartsMissingException("Memory マシンパーツを見失いました");
            firmware = factory.CreateFirmware() ?? throw new SrMachinePartsMissingException("Firmware マシンパーツを見失いました");
            storage = factory.CreateStorage() ?? throw new SrMachinePartsMissingException("Storage マシンパーツを見失いました");
            SrLogger.Trace(SharedString.LogTag.VIRTUAL_MACHINE, "Created SrvmMachine.");
        }


        /// <summary>
        /// 確保したリソースを解放します。各種パーツのインスタンス解放も行います。
        /// </summary>
        /// <param name="disposing">マネージドを含む解放の場合は true を、アンマネージドのみ解放の場合は false を指定</param>
        protected override void Dispose(bool disposing)
        {
            // 既に解放済みなら何もせず終了
            if (disposed) return;
            SrLogger.Trace(SharedString.LogTag.VIRTUAL_MACHINE, "Dispose SrvmMachine.");


            // マネージドの解放なら
            if (disposing)
            {
                // ストレージ、ファームウェア、メモリ、プロセッサ の順で解放する
                storage.Dispose();
                firmware.Dispose();
                memory.Dispose();
                processor.Dispose();
            }


            // 解放済みをマークして基底のDisposeも呼ぶ
            disposed = true;
            base.Dispose(disposing);
        }


        /// <summary>
        /// この仮想マシンの状態を更新します
        /// </summary>
        public void Update()
        {
            ThrowExceptionIfObjectDisposed();
            throw new NotImplementedException();
        }


        /// <summary>
        /// 自身が既に解放済みの場合に例外をスローします
        /// </summary>
        /// <exception cref="ObjectDisposedException">既に解放済みです</exception>
        private void ThrowExceptionIfObjectDisposed()
        {
            // 解放済みのマークが付いているなら
            if (disposed)
            {
                // 解放済み例外を投げる
                throw new ObjectDisposedException(null);
            }
        }
    }
}