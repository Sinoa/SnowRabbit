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

using SnowRabbit.Machine;

namespace SnowRabbit.Runtime
{
    /// <summary>
    /// 仮想マシンが実際に実行するプロセスを表す構造体です
    /// </summary>
    public struct SrProcess
    {
        /// <summary>
        /// このプロセスを持つマシンへの参照
        /// </summary>
        internal SrvmMachine Machine;

        /// <summary>
        /// このプロセスのID。負の値の場合は無効なプロセスIDとなる。
        /// </summary>
        internal int ProcessID;

        /// <summary>
        /// このプロセスを実行するプロセッサコンテキストのメモリブロック
        /// </summary>
        internal MemoryBlock<SrValue> ProcessorContext;

        /// <summary>
        /// このプロセスが持つプロセス空間のメモリブロック
        /// </summary>
        internal MemoryBlock<SrValue> ProcessMemory;

        /// <summary>
        /// このプロセスが持つプロセス空間のオブジェクトメモリブロック
        /// </summary>
        internal MemoryBlock<SrObject> ObjectMemory;



        /// <summary>
        /// このプロセスを実行します
        /// </summary>
        public void Run()
        {
            // 仮想マシンに実行を要求する
            Machine.ExecuteProcess(ref this);
        }


        /// <summary>
        /// このプロセスを終了します
        /// </summary>
        public void Terminate()
        {
            // 仮想マシンに終了を要求する
            Machine.TerminateProcess(ref this);
        }
    }
}