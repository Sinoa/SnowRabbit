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
using System.Collections.Generic;
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
        private static readonly ThreadLocal<SrProfilerContext> context = new ThreadLocal<SrProfilerContext>(() => new SrProfilerContext());
        private static readonly Dictionary<string, long> valueTable = new Dictionary<string, long>();



        /// <summary>
        /// 現在のスレッドにおけるパフォーマンスプロファイリングを開始します
        /// </summary>
        [Conditional(InternalString.Conditional.PROFILING)]
        public static void Begin()
        {
            // 自身のスレッドに紐付くプロファイラコンテキストを開始する
            context.Value.Begin();
        }


        /// <summary>
        /// 現在のスレッドにおけるパフォーマンスプロファイリングを終了します
        /// </summary>
        [Conditional(InternalString.Conditional.PROFILING)]
        public static void End()
        {
            // 自身のスレッドに紐付くプロファイラコンテキストを終了する
            context.Value.End();
        }


        /// <summary>
        /// 指定された名前のカウンター名に突入します。またカウンターはネストすることが出来ます。
        /// ネストから脱出するためには Exit() 関数を呼び出してください。
        /// </summary>
        /// <param name="counterName">突入するカウンター名</param>
        [Conditional(InternalString.Conditional.PROFILING)]
        public static void Enter(string counterName)
        {
            // 自身のスレッドに紐付くカウンターに突入する
            context.Value.Enter(counterName);
        }


        /// <summary>
        /// 現在のカウンターから脱出します。
        /// </summary>
        [Conditional(InternalString.Conditional.PROFILING)]
        public static void Exit()
        {
            // 自身のスレッドに紐付くカウンターから脱出する
            context.Value.Exit();
        }


        /// <summary>
        /// 現在突入しているカウンターのプロファイル値を操作します
        /// </summary>
        /// <param name="valueName">操作するプロファイル値の名前</param>
        /// <param name="handler">指定されたプロファイル値の操作関数</param>
        /// <exception cref="ArgumentException">プロファイル値名が null または 空文字列 です</exception>
        /// <exception cref="ArgumentNullException">handler が null です</exception>
        [Conditional(InternalString.Conditional.PROFILING)]
        public static void HandleCounterValue(string valueName, Func<long, long> handler)
        {
            // 自身のスレッドに紐付くカウンターのプロファイル値を操作する
            ThrowExceptionIfInvalidValueName(valueName);
            context.Value.HandleCounterValue(valueName, handler ?? throw new ArgumentNullException(nameof(handler)));
        }


        /// <summary>
        /// 現在のスレッドに対してのプロファイル値を操作します
        /// </summary>
        /// <param name="valueName">操作するプロファイル値の名前</param>
        /// <param name="handler">指定されたプロファイル値の操作関数</param>
        /// <exception cref="ArgumentException">プロファイル値名が null または 空文字列 です</exception>
        /// <exception cref="ArgumentNullException">handler が null です</exception>
        [Conditional(InternalString.Conditional.PROFILING)]
        public static void HandleThreadValue(string valueName, Func<long, long> handler)
        {
            // 自身のスレッドに紐付くプロファイラコンテキストのプロファイル値を操作する
            ThrowExceptionIfInvalidValueName(valueName);
            context.Value.HandleThreadValue(valueName, handler ?? throw new ArgumentNullException(nameof(handler)));
        }


        /// <summary>
        /// プロファイラー全体に対してのプロファイル値を操作します
        /// </summary>
        /// <param name="valueName">操作するプロファイル値の名前</param>
        /// <param name="handler">指定されたプロファイル値の操作関数</param>
        /// <exception cref="ArgumentException">プロファイル値名が null または 空文字列 です</exception>
        /// <exception cref="ArgumentNullException">handler が null です</exception>
        [Conditional(InternalString.Conditional.PROFILING)]
        public static void HandleGlobalValue(string valueName, Func<long, long> handler)
        {
            // プロファイル値テーブルをロックして値を操作する
            ThrowExceptionIfInvalidValueName(valueName);
            lock (valueTable) valueTable[valueName] = (handler ?? throw new ArgumentNullException(nameof(handler))).Invoke(valueTable[valueName]);
        }


        /// <summary>
        /// プロファイル値名に無効な値が渡された場合に例外をスローします
        /// </summary>
        /// <param name="valueName">確認するプロファイル値名</param>
        /// <exception cref="ArgumentException">プロファイル値名が null または 空文字列 です</exception>
        private static void ThrowExceptionIfInvalidValueName(string valueName)
        {
            // null または 空文字列 なら
            if (string.IsNullOrWhiteSpace(valueName))
            {
                // 例外を吐く
                throw new ArgumentException(InternalString.ExceptionMessage.Profiler.INVALID_VALUE_NAME);
            }
        }
    }
}