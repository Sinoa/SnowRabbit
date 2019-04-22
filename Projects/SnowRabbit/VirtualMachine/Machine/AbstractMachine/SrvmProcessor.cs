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

using SnowRabbit.VirtualMachine.Runtime;

namespace SnowRabbit.VirtualMachine.Machine
{
    /// <summary>
    /// 仮想マシンが実装する仮想マシンプロセッサの抽象クラスです
    /// </summary>
    public abstract class SrvmProcessor : SrvmMachineParts
    {
        // 定数定義
        // Register Index
        public const int RegisterAIndex = 0;
        public const int RegisterBIndex = 1;
        public const int RegisterCIndex = 2;
        public const int RegisterDIndex = 3;
        public const int RegisterSIIndex = 4;
        public const int RegisterDIIndex = 5;
        public const int RegisterBPIndex = 6;
        public const int RegisterSPIndex = 7;
        public const int RegisterRnBaseIndex = 8;
        public const int RegisterIPIndex = 16;
        public const int RegisterFlagIndex = 17;
        // Register Information
        public const int RegisterTotalCount = RegisterFlagIndex + 1;
        public const int ProcessorContextSize = RegisterTotalCount * 8;



        /// <summary>
        /// 指定されたプロセスのプロセッサコンテキストを初期化します
        /// </summary>
        /// <param name="process">初期化するプロセス</param>
        internal unsafe void InitializeContext(ref SrProcess process)
        {
            // メモリモジュールからVMコンテキストとしてメモリを貰って全てゼロクリアする
            process.ProcessorContext = Machine.Memory.AllocateValue(ProcessorContextSize, AllocationType.VMContext);
            for (int i = 0; i < RegisterTotalCount; ++i)
            {
                // 値を0クリア
                process.ProcessorContext[i].Value.Ulong[0] = 0UL;
            }
        }


        /// <summary>
        /// 指定されたプロセスを実行します
        /// </summary>
        /// <param name="process">実行するプロセスへの参照</param>
        internal void Execute(ref SrProcess process)
        {
        }
    }
}