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

namespace SnowRabbit.VirtualMachine.Runtime
{
    /// <summary>
    /// 仮想マシンがホストマシンの関数を呼び出す時に、スタック情報を提供するデータ保持した構造体です
    /// </summary>
    public struct SrStackFrame
    {
        /// <summary>
        /// ホストマシンへ提供するスタックフレーム。リターンアドレスは含まれません。
        /// </summary>
        private MemoryBlock<SrValue> stack;

        /// <summary>
        /// 仮想マシンのプロセスが持っているオブジェクトメモリ
        /// </summary>
        private MemoryBlock<SrObject> objectMemory;

        /// <summary>
        /// ホストマシンからの値型としての戻り値を保持します。
        /// </summary>
        private SrValue resultValue;

        /// <summary>
        /// ホストマシンからの参照型としての戻り値を保持します。
        /// </summary>
        private SrObject resultObject;



        /// <summary>
        /// リターンアドレスを含まないスタックフレームのスタックの数
        /// </summary>
        public int StackCount => stack.Length;



        /// <summary>
        /// SrStackFrame 構造体のインスタンスを初期化します
        /// </summary>
        /// <param name="stack">スタックフレームとして切り出されたメモリブロック</param>
        /// <param name="objectMemory">仮想マシンのオブジェクトメモリ本体</param>
        public SrStackFrame(MemoryBlock<SrValue> stack, MemoryBlock<SrObject> objectMemory)
        {
            // メンバ変数の初期化をする
            this.stack = stack;
            this.objectMemory = objectMemory;
            resultValue = default;
            resultObject = default;
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
        /// 仮想マシンに対して返すための関数の戻り値を、スタックフレームに設定します
        /// </summary>
        /// <param name="value">スタックフレームに設定する値</param>
        public unsafe void SetResultInteger(long value)
        {
            // 受け取った値をそのまま設定
            resultValue.Value.Long[0] = value;
        }


        /// <summary>
        /// 仮想マシンに対して返すための関数の戻り値を、スタックフレームに設定します
        /// </summary>
        /// <param name="value">スタックフレームに設定する値</param>
        public unsafe void SetResultNumber(float value)
        {
            // 受け取った値をそのまま設定
            resultValue.Value.Float[0] = value;
        }


        /// <summary>
        /// 仮想マシンに対して返すための関数の戻り値を、スタックフレームに設定します
        /// </summary>
        /// <param name="value">スタックフレームに設定する値</param>
        public unsafe void SetResultObject(object value)
        {
            // 受け取った値をそのまま設定
            resultObject.Value = value;
        }
        #endregion


        #region スタックフレームから関数の戻り値を取得する関数群
        /// <summary>
        /// スタックフレームに設定した関数の戻り値を取得します
        /// </summary>
        /// <returns>設定された関数の戻り値を返します</returns>
        public unsafe long GetResultInteger()
        {
            // どんな値が入っていようとそのまま返す
            return resultValue.Value.Long[0];
        }


        /// <summary>
        /// スタックフレームに設定した関数の戻り値を取得します
        /// </summary>
        /// <returns>設定された関数の戻り値を返します</returns>
        public unsafe float GetResultNumber()
        {
            // どんな値が入っていようとそのまま返す
            return resultValue.Value.Float[0];
        }


        /// <summary>
        /// スタックフレームに設定した関数の戻り値を取得します
        /// </summary>
        /// <returns>設定された関数の戻り値を返します</returns>
        public unsafe object GetResultObject()
        {
            // どんな値が入っていようとそのまま返す
            return resultObject.Value;
        }
        #endregion
    }
}