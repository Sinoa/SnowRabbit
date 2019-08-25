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
    /// 仮想マシンがホストマシンの関数を呼び出す時に、スタック情報を提供するデータ保持した構造体です
    /// </summary>
    public struct SrStackFrame
    {
        /// <summary>
        /// プロセスのコンテキスト
        /// </summary>
        private MemoryBlock<SrValue> context;

        /// <summary>
        /// ホストマシンへ提供するスタックフレーム。リターンアドレスは含まれません。
        /// </summary>
        private MemoryBlock<SrValue> stack;

        /// <summary>
        /// 仮想マシンのプロセスが持っているオブジェクトメモリ
        /// </summary>
        private MemoryBlock<SrObject> objectMemory;

        /// <summary>
        /// このスタックフレームを持つプロセスのID
        /// </summary>
        private readonly int processID;



        /// <summary>
        /// このスタックフレームの提供するプロセスID
        /// </summary>
        public int ProcessID => processID;


        /// <summary>
        /// リターンアドレスを含まないスタックフレームのスタックの数
        /// </summary>
        public int StackCount => stack.Length;



        /// <summary>
        /// SrStackFrame 構造体のインスタンスを初期化します
        /// </summary>
        /// <param name="processID">このスタックフレームを提供するプロセスのID</param>
        /// <param name="context">このスタックフレームを提供するプロセスコンテキスト</param>
        /// <param name="stack">スタックフレームとして切り出されたメモリブロック</param>
        /// <param name="objectMemory">仮想マシンのオブジェクトメモリ本体</param>
        public SrStackFrame(int processID, MemoryBlock<SrValue> context, MemoryBlock<SrValue> stack, MemoryBlock<SrObject> objectMemory)
        {
            // メンバ変数の初期化をする
            this.processID = processID;
            this.context = context;
            this.stack = stack;
            this.objectMemory = objectMemory;
        }


        #region スタックフレームのデータ取得関数群
        /// <summary>
        /// スタックフレームから整数型として取得します
        /// </summary>
        /// <param name="index">スタックフレームから取得するスタックのインデックス。インデックス 0 がスタックフレームトップを指します。</param>
        /// <returns>指定されたインデックスの整数データを返します</returns>
        public unsafe long GetInteger(int index)
        {
            // 指定されたインデックスのデータを返す（実際のインデックス境界チェックはデバッグビルド時のみ保証しています）
            return stack[index].Value.Long[0];
        }


        /// <summary>
        /// スタックフレームから浮動小数点型として取得します
        /// </summary>
        /// <param name="index">スタックフレームから取得するスタックのインデックス。インデックス 0 がスタックフレームトップを指します。</param>
        /// <returns>指定されたインデックスの浮動小数点データを返します</returns>
        public unsafe float GetNumber(int index)
        {
            // 指定されたインデックスのデータを返す（実際のインデックス境界チェックはデバッグビルド時のみ保証しています）
            return stack[index].Value.Float[0];
        }


        /// <summary>
        /// スタックフレームからオブジェクト型として取得します
        /// </summary>
        /// <param name="index">スタックフレームから取得するスタックのインデックス。インデックス 0 がスタックフレームトップを指します。</param>
        /// <returns>指定されたインデックスのオブジェクトデータを返します</returns>
        public unsafe object GetObject(int index)
        {
            // 指定されたインデックスのデータを返す（実際のインデックス境界チェックはデバッグビルド時のみ保証しています）
            return objectMemory[(int)stack[index].Value.Long[0]].Value;
        }
        #endregion


        #region スタックフレームに関数の戻り値を設定する関数群
        /// <summary>
        /// 仮想マシンのAレジスタに値を設定します
        /// </summary>
        /// <param name="value">スタックフレームに設定する値</param>
        public unsafe void SetResult(long value)
        {
            // Aレジスタに値を設定する
            SetResult(value, SrvmProcessor.RegisterAIndex);
        }


        /// <summary>
        /// 仮想マシンの指定レジスタに値を設定します
        /// </summary>
        /// <param name="value">設定する値</param>
        /// <param name="registerIndex">設定する対象のレジスタインデックス</param>
        public unsafe void SetResult(long value, int registerIndex)
        {
            // 指定インデックスに値を設定する
            context[registerIndex].Value.Long[0] = value;
        }


        /// <summary>
        /// 仮想マシンのAレジスタに値を設定します
        /// </summary>
        /// <param name="value">スタックフレームに設定する値</param>
        public unsafe void SetResult(float value)
        {
            // Aレジスタに値を設定する
            SetResult(value, SrvmProcessor.RegisterAIndex);
        }


        /// <summary>
        /// 仮想マシンの指定レジスタに値を設定します
        /// </summary>
        /// <param name="value">設定する値</param>
        /// <param name="registerIndex">設定する対象のレジスタインデックス</param>
        public unsafe void SetResult(float value, int registerIndex)
        {
            // 指定インデックスに値を設定する
            context[registerIndex].Value.Float[0] = value;
        }


        /// <summary>
        /// 仮想マシンのオブジェクトメモリに値を設定し、そのアドレスをAレジスタに設定します
        /// </summary>
        /// <param name="value">オブジェクトメモリに設定する値</param>
        /// <param name="objectMemoryAddress">設定する値のオブジェクトメモリのアドレス</param>
        public unsafe void SetResult(object value, int objectMemoryAddress)
        {
            // Aレジスタに値を設定する
            SetResult(value, objectMemoryAddress, SrvmProcessor.RegisterAIndex);
        }


        /// <summary>
        /// 仮想マシンのオブジェクトメモリに値を設定し、そのアドレスを指定レジスタに設定します
        /// </summary>
        /// <param name="value">オブジェクトメモリに設定する値</param>
        /// <param name="objectMemoryAddress">設定する値のオブジェクトメモリのアドレス</param>
        /// <param name="registerIndex">設定する対象のレジスタインデックス</param>
        public unsafe void SetResult(object value, int objectMemoryAddress, int registerIndex)
        {
            // オブジェクトメモリの指定場所に値を設定して、レジスタにはその場所を設定する
            objectMemory[objectMemoryAddress].Value = value;
            context[registerIndex].Value.Long[0] = objectMemoryAddress;
        }
        #endregion


        #region スタックフレームから関数の戻り値を取得する関数群
        /// <summary>
        /// 仮想マシンのAレジスタに設定されている値を取得します
        /// </summary>
        /// <returns>Aレジスタの値を返します</returns>
        public unsafe long GetResultInteger()
        {
            // どんな値が入っていようとそのまま返す
            return GetResultInteger(SrvmProcessor.RegisterAIndex);
        }


        /// <summary>
        /// 仮想マシンの指定されたレジスタから値を取得します
        /// </summary>
        /// <param name="registerIndex">値を取得するレジスタインデックス</param>
        /// <returns>指定されたレジスタの値を返します</returns>
        public unsafe long GetResultInteger(int registerIndex)
        {
            // どんな値が入っていようとそのまま返す
            return context[registerIndex].Value.Long[0];
        }


        /// <summary>
        /// 仮想マシンのAレジスタに設定されている値を取得します
        /// </summary>
        /// <returns>Aレジスタの値を返します</returns>
        public unsafe float GetResultNumber()
        {
            // どんな値が入っていようとそのまま返す
            return GetResultNumber(SrvmProcessor.RegisterAIndex);
        }


        /// <summary>
        /// 仮想マシンの指定されたレジスタから値を取得します
        /// </summary>
        /// <param name="registerIndex">値を取得するレジスタインデックス</param>
        /// <returns>指定されたレジスタの値を返します</returns>
        public unsafe float GetResultNumber(int registerIndex)
        {
            // どんな値が入っていようとそのまま返す
            return context[registerIndex].Value.Float[0];
        }


        /// <summary>
        /// 仮想マシンのAレジスタに設定されいるオブジェクトメモリアドレスからオブジェクトメモリの内容を取得します
        /// </summary>
        /// <returns>Aレジスタが指しているオブジェクトメモリアドレスの位置のオブジェクトを返します</returns>
        public unsafe object GetResultObject()
        {
            // どんな値が入っていようとそのまま返す
            return GetResultObject(SrvmProcessor.RegisterAIndex);
        }


        /// <summary>
        /// 仮想マシンの指定されたレジスタに設定されいるオブジェクトメモリアドレスからオブジェクトメモリの内容を取得します
        /// </summary>
        /// <returns>指定されたレジスタが指しているオブジェクトメモリアドレスの位置のオブジェクトを返します</returns>
        public unsafe object GetResultObject(int registerIndex)
        {
            // どんな値が入っていようとそのまま返す
            return objectMemory[(int)context[registerIndex].Value.Long[0]].Value;
        }
        #endregion
    }
}