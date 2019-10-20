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
using System.Threading;

namespace SnowRabbit.Diagnostics.Profiling
{
    /// <summary>
    /// SnowRabbit 全体の性能評価を行うためのプロファイラクラスです。
    /// プロファイラを使用するためには "SR_PROFILING" コンパイル定数を定義する必要があります。
    /// </summary>
    public static class SrProfiler
    {
        // クラス変数宣言
        private static readonly ThreadLocal<SrProfileContext> context = new ThreadLocal<SrProfileContext>(() => new SrProfileContext());



        [Conditional("SR_PROFILING")]
        public static void Begin()
        {
            context.Value.Begin();
        }


        [Conditional("SR_PROFILING")]
        public static void End()
        {
            context.Value.End();
        }


        [Conditional("SR_PROFILING")]
        public static void Enter(string counterName)
        {
            context.Value.Enter(counterName);
        }


        [Conditional("SR_PROFILING")]
        public static void Exit()
        {
            context.Value.Exit();
        }


        [Conditional("SR_PROFILING")]
        public static void HandleValue(string valueName, Func<long, long> handler)
        {
            context.Value.HandleValue(valueName, handler);
        }


        [Conditional("SR_PROFILING")]
        public static void HandleGlobalValue(string valueName, Func<long, long> handler)
        {
            context.Value.HandleGlobalValue(valueName, handler);
        }
    }
}