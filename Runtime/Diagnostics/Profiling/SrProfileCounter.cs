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

namespace SnowRabbit.Diagnostics.Profiling
{
    /// <summary>
    /// プロファイリングする最小単位として特定区間のパフォーマンスを保持するクラスです
    /// </summary>
    public class SrProfileCounter
    {
        // 定数定義
        public const string RootCounterName = "ROOT_COUNTER";

        // メンバ変数定義
        private Dictionary<string, SrProfileCounter> childTable = new Dictionary<string, SrProfileCounter>();
        private Dictionary<string, long> countTable = new Dictionary<string, long>();
        private long startTick = 0;



        /// <summary>
        /// カウンター名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// このカウンターが生成された順番
        /// </summary>
        public int Order { get; private set; }


        /// <summary>
        /// このカウンターに突入した回数
        /// </summary>
        public int EnterCount { get; private set; }


        /// <summary>
        /// このカウンターに滞在したタイマー刻みカウント
        /// </summary>
        public ulong ElapsedTick { get; private set; }



        /// <summary>
        /// SrProfileCounter クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="name">カウンター名</param>
        /// <exception cref="ArgumentNullException">name が null です</exception>
        public SrProfileCounter(string name)
        {
            // 名前を設定して状態リセットをする
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Reset();
        }


        /// <summary>
        /// カウンタの内部情報をリセットします
        /// </summary>
        public void Reset()
        {
            // テーブルのクリアとカウント値のクリア
            childTable.Clear();
            countTable.Clear();
            startTick = 0;
            EnterCount = 0;
            ElapsedTick = 0;
        }


        /// <summary>
        /// カウンターの起動を開始します
        /// </summary>
        /// <param name="startTick">開始したタイマーのチックカウント値</param>
        public void Start(long startTick)
        {
            // 開始タイマー刻みを覚える
            this.startTick = startTick;
        }


        public void End()
        {
        }
    }
}