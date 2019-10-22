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
using System.Diagnostics;
using SnowRabbit.Diagnostics.Logging;

namespace SnowRabbit.Diagnostics.Profiling
{
    /// <summary>
    /// 単一スレッドのプロファイリング状態を保持するプロファイルコンテキストクラスです
    /// </summary>
    internal class SrProfilerContext
    {
        // メンバ変数定義
        private bool contextReady = false;
        private Stopwatch stopwatch = new Stopwatch();
        private SrProfileCounter rootCounter = new SrProfileCounter(SrProfileCounter.RootCounterName);
        private SrProfileCounter previousCounter = null;
        private SrProfileCounter currentCounter = null;



        /// <summary>
        /// コンテキストの状態を準備します
        /// </summary>
        public void Begin()
        {
            // 既に状態の準備が出来ているなら
            if (contextReady)
            {
                // 終了する
                SrLogger.Warning(InternalString.LogTag.PROFILER, InternalString.LogMessage.Profiler.ALREADY_READY);
                return;
            }


            // ルートカウンタのリセット、カウンタ参照のクリアをしてストップウォッチを再起動する
            rootCounter.Reset();
            previousCounter = null;
            currentCounter = rootCounter;
            stopwatch.Restart();
        }


        /// <summary>
        /// コンテキストの状態を完了します
        /// </summary>
        public void End()
        {
            // 状態の準備が出来ていないなら
            if (!contextReady)
            {
                // 終了する
                SrLogger.Warning(InternalString.LogTag.PROFILER, InternalString.LogMessage.Profiler.NOT_READY);
                return;
            }


            // ストップウォッチを停止
            stopwatch.Stop();
        }


        /// <summary>
        /// 指定された名前のカウンターに突入します
        /// </summary>
        /// <param name="counterName">突入するカウンター名</param>
        public void Enter(string counterName)
        {
            // 状態の準備が出来ていないなら
            if (!contextReady)
            {
                // 終了する
                SrLogger.Warning(InternalString.LogTag.PROFILER, InternalString.LogMessage.Profiler.NOT_READY);
                return;
            }
        }


        public void Exit()
        {
            // 状態の準備が出来ていないなら
            if (!contextReady)
            {
                // 終了する
                SrLogger.Warning(InternalString.LogTag.PROFILER, InternalString.LogMessage.Profiler.NOT_READY);
                return;
            }
        }


        public void HandleCounterValue(string valueName, Func<long, long> handler)
        {
            // 状態の準備が出来ていないなら
            if (!contextReady)
            {
                // 終了する
                SrLogger.Warning(InternalString.LogTag.PROFILER, InternalString.LogMessage.Profiler.NOT_READY);
                return;
            }
        }


        public void HandleThreadValue(string valueName, Func<long, long> handler)
        {
            // 状態の準備が出来ていないなら
            if (!contextReady)
            {
                // 終了する
                SrLogger.Warning(InternalString.LogTag.PROFILER, InternalString.LogMessage.Profiler.NOT_READY);
                return;
            }
        }
    }
}