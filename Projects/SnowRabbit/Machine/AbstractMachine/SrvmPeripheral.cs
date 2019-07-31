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
using SnowRabbit.Runtime;

namespace SnowRabbit.Machine
{
    /// <summary>
    /// 仮想マシンが実装する仮想マシン周辺装置の抽象クラスです
    /// </summary>
    public abstract class SrvmPeripheral : SrvmMachineParts
    {
        // メンバ変数定義
        private bool disposed;
        private int nextFunctionID;
        private Dictionary<string, int> functionIDTable;
        private Dictionary<int, Func<SrStackFrame, HostFunctionResult>> functionTable;



        /// <summary>
        /// 周辺機器名を取得します
        /// </summary>
        public abstract string Name { get; }



        /// <summary>
        /// SrvmPeripheral クラスのインスタンスを初期化します
        /// </summary>
        public SrvmPeripheral()
        {
            // 必要なフィールドの初期化をする
            nextFunctionID = 0;
            functionIDTable = new Dictionary<string, int>();
            functionTable = new Dictionary<int, Func<SrStackFrame, HostFunctionResult>>();
        }


        /// <summary>
        /// リソースの解放を行います
        /// </summary>
        /// <param name="disposing">マネージ解放の時はtrueを、アンマネージのみの解放の時はfalse</param>
        protected override void Dispose(bool disposing)
        {
            // 既に解放済みなら
            if (disposed)
            {
                // 何もせず終了
                return;
            }


            // マネージ解放なら
            if (disposing)
            {
                // 関数IDテーブルと関数テーブルのクリア
                functionTable.Clear();
                functionIDTable.Clear();
            }


            // 解放済みをマーク
            disposed = true;


            // ベースクラスのDisposeも呼ぶ
            base.Dispose(disposing);
        }


        /// <summary>
        /// 周辺機器装置が提供する関数をセットアップします
        /// </summary>
        /// <param name="registryHandler">関数を登録する関数が渡されます</param>
        protected abstract void SetupFunction(Action<string, Func<SrStackFrame, HostFunctionResult>> registryHandler);


        /// <summary>
        /// 関数テーブルを初期化します
        /// </summary>
        internal void InitializeFunctionTable()
        {
            // 関数テーブルに登録するための関数を関数セットアップ関数に渡す
            SetupFunction(AddFunction);
        }


        /// <summary>
        /// 指定された名前の関数IDを取得します
        /// </summary>
        /// <param name="name">取得するIDの関数名</param>
        /// <returns>関数IDが正しく取得された場合はIDを返しますが、取得できなかった場合は -1 を返します</returns>
        internal int GetFunctionID(string name)
        {
            // 指定された名前でID取得を試みて、成功したら返して、駄目だったら-1を返す
            return functionIDTable.TryGetValue(name, out var id) ? id : -1;
        }


        /// <summary>
        /// 指定されたIDの関数を取得します
        /// </summary>
        /// <param name="id">取得したい関数のID</param>
        /// <returns>指定されたIDに該当する関数が取得された場合その関数を返しますが、取得できなかった場合は null を返します</returns>
        internal Func<SrStackFrame, HostFunctionResult> GetFunction(int id)
        {
            // 指定されたIDで関数取得を試みて、成功したら返して、駄目だったら null を返す
            return functionTable.TryGetValue(id, out var function) ? function : null;
        }


        /// <summary>
        /// 関数テーブルに関数を登録します。また、この関数は同じ名前が指定された場合は上書きすることに注意して下さい。
        /// </summary>
        /// <param name="name">登録する関数の名前</param>
        /// <param name="hostFunction">登録する関数の実体</param>
        /// <exception cref="ArgumentException">name が null か 空白のみ です。必ず有効な名前である必要があります</exception>
        private void AddFunction(string name, Func<SrStackFrame, HostFunctionResult> hostFunction)
        {
            // もし取り扱えない名前なら
            if (string.IsNullOrWhiteSpace(name))
            {
                // 取り扱えない名前は拒否
                throw new ArgumentException("name が null か 空白のみ です。必ず有効な名前である必要があります", nameof(name));
            }


            // 関数IDを用意して関数テーブルに関数を登録する
            var functionID = nextFunctionID++;
            functionIDTable[name] = functionID;
            functionTable[functionID] = hostFunction;
        }
    }
}