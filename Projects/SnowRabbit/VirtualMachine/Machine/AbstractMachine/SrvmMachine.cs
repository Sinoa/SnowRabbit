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
using System.IO;
using SnowRabbit.VirtualMachine.Runtime;

namespace SnowRabbit.VirtualMachine.Machine
{
    /// <summary>
    /// 仮想マシン本体を表す仮想マシン抽象クラスです
    /// </summary>
    public abstract class SrvmMachine
    {
        // メンバ変数定義
        private int nextProcessID;



        /// <summary>
        /// 仮想マシンに搭載されているファームウェアモジュール
        /// </summary>
        public SrvmFirmware Firmware { get; private set; }


        /// <summary>
        /// 仮想マシンに搭載されているプロセッサモジュール
        /// </summary>
        public SrvmProcessor Processor { get; private set; }


        /// <summary>
        /// 仮想マシンに搭載されているメモリモジュール
        /// </summary>
        public SrvmMemory Memory { get; private set; }


        /// <summary>
        /// 仮想マシンに搭載されているストレージモジュール
        /// </summary>
        public SrvmStorage Storage { get; private set; }



        /// <summary>
        /// SrvmMachine のインスタンスを初期化します。
        /// </summary>
        /// <param name="processor">仮想マシンが使用するプロセッサ</param>
        /// <param name="firmware">仮想マシンが使用するファームウェア</param>
        /// <param name="memory">仮想マシンが使用するメモリ</param>
        /// <param name="storage">仮想マシンが使用するストレージ</param>
        /// <exception cref="ArgumentNullException">processor が null です</exception>
        /// <exception cref="ArgumentNullException">firmware が null です</exception>
        /// <exception cref="ArgumentNullException">memory が null です</exception>
        /// <exception cref="ArgumentNullException">storage が null です</exception>
        public SrvmMachine(SrvmProcessor processor, SrvmFirmware firmware, SrvmMemory memory, SrvmStorage storage)
        {
            // 仮想マシンパーツを初期化する
            (Processor = processor ?? throw new ArgumentNullException(nameof(processor))).Machine = this;
            (Firmware = firmware ?? throw new ArgumentNullException(nameof(firmware))).Machine = this;
            (Memory = memory ?? throw new ArgumentNullException(nameof(memory))).Machine = this;
            (Storage = storage ?? throw new ArgumentNullException(nameof(storage))).Machine = this;
        }


        /// <summary>
        /// 仮想マシンに実行してもらうプログラムを指定してプロセスを生成します。
        /// </summary>
        /// <param name="programPath">実行するプログラムパス</param>
        /// <param name="process">プログラムを実行するプロセスを出力します</param>
        /// <exception cref="ArgumentNullException">programPath が null です</exception>
        /// <exception cref="SrProgramNotFoundException">指定されたパス '{path}' のプログラムを見つけられませんでした</exception>
        public void CreateProcess(string programPath, out SrProcess process)
        {
            // プロセスの既定初期化をしてからファームウェアにプログラムのロードをしてもらう
            process = default;
            Firmware.LoadProgram(programPath ?? throw new ArgumentNullException(nameof(programPath)), ref process);
            Processor.InitializeContext(ref process);


            // プロセスIDをインクリメントして振る
            process.ProcessID = ++nextProcessID;
        }


        /// <summary>
        /// 仮想マシンに実行してもらうプログラムを指定してプロセスを生成します。
        /// </summary>
        /// <param name="programData">実行するプログラムバイト配列</param>
        /// <param name="process">プログラムを実行するプロセスを出力します</param>
        /// <exception cref="ArgumentNullException">programData が null です</exception>
        public void CreateProcess(byte[] programData, out SrProcess process)
        {
            // プロセスの既定初期化をしてからファームウェアにプログラムのロードをしてもらう
            process = default;
            Firmware.LoadProgram(programData ?? throw new ArgumentNullException(nameof(programData)), ref process);
            Processor.InitializeContext(ref process);


            // プロセスIDをインクリメントして振る
            process.ProcessID = ++nextProcessID;
        }


        /// <summary>
        /// 仮想マシンに実行してもらうプログラムを指定してプロセスを生成します。
        /// </summary>
        /// <param name="programStream">実行するプログラムストリーム</param>
        /// <param name="process">プログラムを実行するプロセスを出力します</param>
        /// <exception cref="ArgumentNullException">programStream が null です</exception>
        public void CreateProcess(Stream programStream, out SrProcess process)
        {
            // プロセスの既定初期化をしてからファームウェアにプログラムのロードをしてもらう
            process = default;
            Firmware.LoadProgram(programStream ?? throw new ArgumentNullException(nameof(programStream)), ref process);
            Processor.InitializeContext(ref process);


            // プロセスIDをインクリメントして振る
            process.ProcessID = ++nextProcessID;
        }


        /// <summary>
        /// 仮想マシンに指定されたプロセスを実行させます
        /// </summary>
        /// <param name="process">実行するプロセス</param>
        public void ExecuteProcess(ref SrProcess process)
        {
            // プロセッサにそのまま実行してもらう
            Processor.Execute(ref process);
        }
    }
}