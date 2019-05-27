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



        public SrStackFrame(MemoryBlock<SrValue> stack, MemoryBlock<SrObject> objectMemory)
        {
            this.stack = stack;
            this.objectMemory = objectMemory;
            resultValue = default;
            resultObject = default;
        }


        public unsafe long GetInteger(int index)
        {
            return stack[index].Value.Long[0];
        }


        public unsafe float GetNumber(int index)
        {
            return stack[index].Value.Float[0];
        }


        public unsafe object GetObject(int index)
        {
            return objectMemory[(int)stack[index].Value.Long[0]].Value;
        }


        public unsafe long GetResultInteger()
        {
            return resultValue.Value.Long[0];
        }


        public unsafe float GetResultNumber()
        {
            return resultValue.Value.Float[0];
        }


        public unsafe object GetResultObject()
        {
            return resultObject.Value;
        }


        public unsafe void SetResultInteger(long value)
        {
            resultValue.Value.Long[0] = value;
        }


        public unsafe void SetResultNumber(float value)
        {
            resultValue.Value.Float[0] = value;
        }


        public unsafe void SetResultObject(object value)
        {
            resultObject.Value = value;
        }
    }
}