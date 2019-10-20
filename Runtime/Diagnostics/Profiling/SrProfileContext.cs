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

namespace SnowRabbit.Diagnostics.Profiling
{
    /// <summary>
    /// 単一スレッドのプロファイリング状態を保持するプロファイルコンテキストクラスです
    /// </summary>
    internal class SrProfileContext
    {
        // メンバ変数定義
        private Stopwatch stopwatch = new Stopwatch();
        private SrProfileCounter rootCounter = new SrProfileCounter();
        private SrProfileCounter previousCounter;
        private SrProfileCounter currentCounter;



        public void Begin()
        {
        }


        public void End()
        {
        }


        public void Enter(string counterName)
        {
        }


        public void Exit()
        {
        }


        public void HandleValue(string valueName, Func<long, long> handler)
        {
        }


        public void HandleGlobalValue(string valueName, Func<long, long> handler)
        {
        }
    }
}