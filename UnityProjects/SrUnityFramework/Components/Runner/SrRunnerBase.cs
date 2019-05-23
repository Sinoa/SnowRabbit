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

using System.IO;
using SnowRabbit.VirtualMachine.Machine;
using SnowRabbit.VirtualMachine.Runtime;
using UnityEngine;

namespace SrUnityFramework.Components
{
    /// <summary>
    /// 雪兎仮想マシンに実行コードを投入してUnityのインスペクタから容易に実行するランナー基本コンポーネントクラスです
    /// </summary>
    public abstract class SrRunnerBase : MonoBehaviour
    {
        // インスペクタ公開用メンバ変数定義
        [SerializeField]
        protected SrVirtualMachineInstanceComponentBase instance = null;

        // メンバ変数定義
        private SrvmMachine machine;
        protected SrProcess process;



        /// <summary>
        /// ランナーが実行を開始出来る準備ができているかどうか
        /// </summary>
        public bool IsReady { get; private set; }



        /// <summary>
        /// コンポーネントの動作を開始します
        /// </summary>
        private void Start()
        {
            // 仮想マシンの参照を取得してプロセスの生成をする
            machine = GetVirtualMachine();
            CreateNewProcess();
        }


        /// <summary>
        /// ランナーが使用する仮想マシンを設定します
        /// </summary>
        /// <param name="machine">設定する仮想マシン</param>
        public void SetVirtualMachine(SrvmMachine machine)
        {
            // 受け取った値をそのまま受け入れる
            this.machine = machine;
        }


        /// <summary>
        /// ランナーが使用するプロセスを新しく生成します
        /// </summary>
        /// <returns>プロセスが正しく生成された場合は true を、失敗した場合は false を返します</returns>
        public bool CreateNewProcess()
        {
            // 仮想マシンがnullなら
            if (machine == null)
            {
                // 失敗を返す
                IsReady = false;
                return IsReady;
            }


            // まずはプログラムパスの取得を試みる
            if (GetProgramPath(out var path))
            {
                // プログラムパスからプロセスのインスタンスを生成して成功を返す
                machine.CreateProcess(path, out process);
                IsReady = true;
                return IsReady;
            }


            // プログラムコードの取得を試みる
            if (GetProgramCode(out var code))
            {
                // プログラムコードからプロセスのインスタンスを生成して成功を返す
                machine.CreateProcess(code, out process);
                IsReady = true;
                return IsReady;
            }


            // ストリームの取得を試みる
            if (GetProgramStream(out var stream))
            {
                // プログラムストリームからインスタンスを生成して成功を返す
                machine.CreateProcess(stream, out process);
                IsReady = true;
                return IsReady;
            }


            // どの方法でも生成出来なかった場合はプロセスの生成に失敗したということを返す
            IsReady = false;
            return IsReady;
        }


        /// <summary>
        /// 現在のランナーが持っているプロセスを実行します
        /// </summary>
        public void Run()
        {
            // 準備が出来ていないなら
            if (!IsReady)
            {
                // 何もせず終了
                return;
            }


            // プロセスを実行する
            machine.ExecuteProcess(ref process);
        }


        /// <summary>
        /// 仮想マシンの取得をします
        /// </summary>
        /// <returns>取得した仮想マシンがあればその参照を返しますが、取得出来なかった場合は null を返します</returns>
        protected virtual SrvmMachine GetVirtualMachine()
        {
            // 既定動作はインスペクタから取得する
            return instance?.Instance;
        }


        /// <summary>
        /// プロセスを生成するプログラムが存在するパスを取得します
        /// </summary>
        /// <param name="programPath">取得したパスを設定する参照</param>
        /// <returns>正しくパスの取得ができた場合は true を、取得出来なかった場合は false を返します</returns>
        protected virtual bool GetProgramPath(out string programPath)
        {
            // 既定動作は空文字列を設定して失敗を返す
            programPath = string.Empty;
            return false;
        }


        /// <summary>
        /// プロセスを生成するバイト配列状態になっている実行コードを取得します
        /// </summary>
        /// <param name="programCode">バイト配列上の実行コードを設定する参照</param>
        /// <returns>正しくコードの取得ができた場合は true を、取得出来なかった場合は false を返します</returns>
        protected virtual bool GetProgramCode(out byte[] programCode)
        {
            // 既定動作はnullを設定して失敗を返す
            programCode = null;
            return false;
        }


        /// <summary>
        /// プロセスを生成する為の実行コードを読み取れるストリームを取得します
        /// </summary>
        /// <param name="programStream">実行コードを読み取れるストリームを設定する参照</param>
        /// <returns>正しくストリームの取得ができた場合は true を、取得出来なかった場合は false を返します</returns>
        protected virtual bool GetProgramStream(out Stream programStream)
        {
            // 既定動作はnullを設定して失敗を返す
            programStream = null;
            return false;
        }
    }
}