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
using SnowRabbit.Runtime;

namespace SnowRabbit.Machine
{
    /// <summary>
    /// 仮想マシン本体を表す仮想マシン抽象クラスです
    /// </summary>
    public abstract class SrvmMachine : IDisposable
    {
        // メンバ変数定義
        private bool disposed;



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



        #region Constructor and dispose
        /// <summary>
        /// SrvmMachine のインスタンスを初期化します。
        /// 引数として渡された各インスタンスは、このインスタンスが破棄される時に同時にDisposeを呼び出します。
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
        /// インスタンスのリソースを解放します
        /// </summary>
        ~SrvmMachine()
        {
            // ファイナライザからのDispose呼び出し
            Dispose(false);
        }


        /// <summary>
        /// インスタンスのリソースを解放します
        /// </summary>
        public void Dispose()
        {
            // DisposeからのDispose呼び出しをしてGCに自身のファイナライザを止めてもらう
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// インスタンスの実際のリソースを解放します
        /// </summary>
        /// <param name="disposing">マネージドを含む解放の場合は true を、アンマネージドのみの場合は false を指定</param>
        protected virtual void Dispose(bool disposing)
        {
            // 既に解放済みなら
            if (disposed)
            {
                // 何もしない
                return;
            }


            // マネージの解放なら
            if (disposing)
            {
                // 各種パーツの解放を行う
                Firmware.Dispose();
                Memory.Dispose();
                Storage.Dispose();
                Processor.Dispose();
            }


            // 解放済みマーク
            disposed = true;
        }
        #endregion


        #region Peripheral functions
        /// <summary>
        /// 仮想マシンに周辺機器装置を接続します。
        /// 周辺機器装置を接続したままこのインスタンスが破棄された時に、渡されたインスタンスもDisposeを呼び出します。
        /// </summary>
        /// <param name="peripheral">接続する周辺機器装置</param>
        /// <exception cref="ArgumentException">指定された周辺機器は同名の周辺機器が接続済みです</exception>
        public void AttachPeripheral(SrvmPeripheral peripheral)
        {
            // ファームウェアに周辺機器を追加する
            Firmware.AddPeripheral(peripheral ?? throw new ArgumentNullException(nameof(peripheral)));
        }


        /// <summary>
        /// 仮想マシンから周辺機器装置を外します。
        /// 外した場合は渡されたインスタンスのDisposeは呼び出しません。
        /// </summary>
        /// <param name="peripheralName">外す周辺機器装置名</param>
        public void DetachPeripheral(string peripheralName)
        {
            // ファームウェアに周辺機器を外してもらう
            Firmware.RemovePeripheral(peripheralName);
        }


        /// <summary>
        /// 指定された周辺機器名のインスタンスを取得します。
        /// </summary>
        /// <param name="peripheralName">取得する周辺機器名</param>
        /// <returns>指定された周辺機器のインスタンスが取得された場合は参照を返しますが、見つからない場合は null を返します</returns>
        /// <exception cref="ArgumentException">peripheralName が null または 空文字列 です</exception>
        public SrvmPeripheral GetPeripheral(string peripheralName)
        {
            // 周辺機器名が不正なら
            if (string.IsNullOrWhiteSpace(peripheralName))
            {
                // 無効な引数であることを例外で吐く
                throw new ArgumentException($"{nameof(peripheralName)} が null または 空文字列 です");
            }


            // 周辺機器IDを取得してから実体を取得する
            var peripheralID = Firmware.GetPeripheralID(peripheralName);
            return peripheralID != -1 ? Firmware.GetPeripheral(peripheralID) : null;
        }
        #endregion


        #region Process functions
        /// <summary>
        /// 仮想マシンに実行してもらうプログラムを指定してプロセスを生成します
        /// </summary>
        /// <param name="programPath">実行するプログラムパス</param>
        /// <param name="process">プログラムを実行するプロセスを出力します</param>
        /// <exception cref="ArgumentNullException">programPath が null です</exception>
        /// <exception cref="SrProgramNotFoundException">指定されたパス '{path}' のプログラムを見つけられませんでした</exception>
        public void CreateProcess(string programPath, out SrProcess process)
        {
            // ファームウェアでプロセスを生成する
            Firmware.CreateProcess(programPath, out process);
        }


        /// <summary>
        /// 仮想マシンに実行してもらうプログラムデータを指定してプロセスを生成します
        /// </summary>
        /// <param name="programData">実行するプログラムデータ</param>
        /// <param name="process">プログラムを実行するプロセスを出力します</param>
        /// <exception cref="ArgumentNullException">programData が null です</exception>
        public void CreateProcess(byte[] programData, out SrProcess process)
        {
            // ファームウェアでプロセスを生成する
            Firmware.CreateProcess(programData ?? throw new ArgumentNullException(nameof(programData)), out process);
        }


        /// <summary>
        /// 仮想マシンに実行してもらうプログラムストリームを指定してプロセスを生成します
        /// </summary>
        /// <param name="programStream">実行するプログラムを読み取れるストリーム</param>
        /// <param name="process">プログラムを実行するプロセスを出力します</param>
        /// <exception cref="ArgumentNullException">programStream が null です</exception>
        public void CreateProcess(Stream programStream, out SrProcess process)
        {
            // ファームウェアでプロセスを生成する
            Firmware.CreateProcess(programStream ?? throw new ArgumentNullException(nameof(programStream)), out process);
        }


        /// <summary>
        /// 仮想マシンに指定されたプロセスを終了させます
        /// </summary>
        /// <param name="process">終了させるプロセス</param>
        public void TerminateProcess(ref SrProcess process)
        {
            // ファームウェアでプロセスを終了させる
            Firmware.TerminateProcess(ref process);
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
        #endregion
    }
}