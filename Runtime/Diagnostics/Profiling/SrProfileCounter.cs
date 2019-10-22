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
    internal class SrProfileCounter
    {
        // メンバ変数定義
        private Dictionary<string, SrProfileCounter> childTable = new Dictionary<string, SrProfileCounter>();
        private long startTick = 0;



        /// <summary>
        /// カウンター名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// このカウンターの親。null の場合はルートのカウンターであることを意味します。
        /// </summary>
        public SrProfileCounter Parent { get; private set; }


        /// <summary>
        /// このカウンターに突入した回数
        /// </summary>
        public int EnterCount { get; private set; }


        /// <summary>
        /// このカウンターに滞在したタイマー刻みカウント
        /// </summary>
        public long ElapsedTick { get; private set; }



        /// <summary>
        /// SrProfileCounter クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="name">カウンター名</param>
        /// <exception cref="ArgumentNullException">name が null です</exception>
        public SrProfileCounter(string name)
        {
            // 名前を設定して状態リセットをする
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parent = null;
            Reset();
        }


        /// <summary>
        /// カウンタの内部情報をリセットします
        /// </summary>
        public void Reset()
        {
            // テーブルのクリアとカウント値のクリア
            childTable.Clear();
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
            // 開始タイマー刻みを覚えて突入カウントをインクリメント
            this.startTick = startTick;
            ++EnterCount;
        }


        /// <summary>
        /// カウンターの停止して処理時間を計算します
        /// </summary>
        /// <param name="endTick">停止したタイマーのチックカウント値</param>
        public void End(long endTick)
        {
            // 自身の処理時間を設定する
            ElapsedTick += endTick - startTick;
        }


        /// <summary>
        /// カウンターの子を取得または生成します
        /// </summary>
        /// <param name="name">取得または生成する名前</param>
        /// <returns>取得または生成したカウンターの子を返します</returns>
        public SrProfileCounter GetOrCreateChild(string name)
        {
            // 名前からカウンターの取得ができるかを試みる
            if (childTable.TryGetValue(name, out var counter))
            {
                // 取得できたカウンターを返す
                return counter;
            }


            // 取得できなかったら新しく生成して返す
            counter = new SrProfileCounter(name);
            counter.Parent = this;
            childTable[name] = counter;
            return counter;
        }
    }
}