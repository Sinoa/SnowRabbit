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

/*

# ホスト関数のシグネチャ

サポートするホスト関数のシグネチャは、Task型またはTask<TResult>またはvoidを返却し、引数はプリミティブ型及びstring型、object型のいずれかを受け取る自由な引数配置です。

次の例が許された関数シグネチャです。

```
void MySimpleFunction();
Task MySampleFunction(int arg1);
Task<int> MySampleFunction2(string arg1, int arg2, Transform arg3);
```

ただし、C#のプリミティブ型及びstring型などの基本的な型以外は原則的に、object型として処理されます。

*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SnowRabbit.Diagnostics.Logging;

namespace SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral
{
    /// <summary>
    /// 仮想マシンとホスト間の関数を取り扱う周辺機器関数クラスです
    /// </summary>
    internal class SrPeripheralFunction
    {
        // クラス変数宣言
        private static readonly Dictionary<Type, Func<SrValue, object>> fromValueConvertTable;
        private static readonly Dictionary<Type, Func<object, SrValue>> toValueConvertTable;

        // メンバ変数定義
        private readonly object targetInstance;
        private readonly MethodInfo methodInfo;
        private object[] arguments;
        private Func<SrValue, object>[] argumentSetters;
        private bool isVoidReturn;
        private Type taskResultType;
        private PropertyInfo taskResultProperty;
        private Func<object, SrValue> resultSetter;



        /// <summary>
        /// PeripheralFunction クラスの初期化をします
        /// </summary>
#pragma warning disable CA1810
        static SrPeripheralFunction()
#pragma warning restore CA1810
        {
            // SrValue から特定の型へ正しくキャストする関数テーブルを初期化
            fromValueConvertTable = new Dictionary<Type, Func<SrValue, object>>()
            {
                // 各型に合わせた返却関数を用意
                { typeof(sbyte), x => x.Primitive.Sbyte },
                { typeof(byte), x => x.Primitive.Byte },
                { typeof(char), x => x.Primitive.Char },
                { typeof(short), x => x.Primitive.Short },
                { typeof(ushort), x => x.Primitive.Ushort },
                { typeof(int), x => x.Primitive.Int },
                { typeof(uint), x => x.Primitive.Uint },
                { typeof(long), x => x.Primitive.Long },
                { typeof(ulong), x => x.Primitive.Ulong },
                { typeof(float), x => x.Primitive.Float },
                { typeof(double), x => x.Primitive.Double },
                { typeof(bool), x => x.Primitive.Int != 0 },
                { typeof(IntPtr), x => x.Primitive.IntPtr },
                { typeof(UIntPtr), x => x.Primitive.UIntPtr },
                { typeof(string), x => x.Object },
                { typeof(object), x => x.Object },
            };


            // object から SrValue へ正しくキャストする関数テーブルを初期化
            toValueConvertTable = new Dictionary<Type, Func<object, SrValue>>()
            {
                // 各型に合わせた返却関数を用意
                { typeof(sbyte), x => (sbyte)x },
                { typeof(byte), x => (byte)x },
                { typeof(char), x => (char)x },
                { typeof(short), x => (short)x },
                { typeof(ushort), x => (ushort)x },
                { typeof(int), x => (int)x },
                { typeof(uint), x => (uint)x },
                { typeof(long), x => (long)x },
                { typeof(ulong), x => (ulong)x },
                { typeof(float), x => (float)x },
                { typeof(double), x => (double)x },
                { typeof(bool), x => (bool)x },
                { typeof(IntPtr), x => (IntPtr)x },
                { typeof(UIntPtr), x => (UIntPtr)x },
                { typeof(string), x => (string)x },
                { typeof(object), x => { var res = new SrValue { Object = x }; return res; } },
            };
        }


        /// <summary>
        /// PeripheralFunction クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="targetInstance">インスタンス関数呼び出しの際に使用する呼び出す対象のインスタンス、静的関数の場合は null を指定</param>
        /// <param name="info">周辺機器関数として使用する関数の情報</param>
        /// <exception cref="ArgumentNullException">info が null です</exception>
        public SrPeripheralFunction(object targetInstance, MethodInfo info)
        {
            // ひとまず参照を受け取る
            methodInfo = info ?? throw new ArgumentNullException(nameof(info));
            this.targetInstance = targetInstance;
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, $"Begin create peripheral function for '{info.DeclaringType}.{info.Name}'.");


            // 引数と戻り値のセットアップをする
            SetupArgumentInfo(info);
            SetupReturnInfo(info);
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, $"End create peripheral function for '{info.DeclaringType}.{info.Name}'.");
        }


        /// <summary>
        /// 引数情報をセットアップします
        /// </summary>
        /// <param name="info">周辺機器関数として使用する関数情報</param>
        private void SetupArgumentInfo(MethodInfo info)
        {
            // 引数の数を知る
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, "Setup argument info.");
            var parameters = info.GetParameters();


            // 引数が0件なら
            if (parameters.Length == 0)
            {
                // 引数関連は空で初期化
                SrLogger.Trace(SharedString.LogTag.PERIPHERAL, "Argument empty.");
                arguments = Array.Empty<object>();
                argumentSetters = Array.Empty<Func<SrValue, object>>();
                return;
            }


            // 引数と引数設定関数を初期化子て引数の数分ループ
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, $"Argument count = {parameters.Length}.");
            arguments = new object[parameters.Length];
            argumentSetters = new Func<SrValue, object>[parameters.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                // 引数情報を取得して変換対象の型から変換関数を取り出して次へ
                var parameter = parameters[i];
                if (fromValueConvertTable.TryGetValue(parameter.ParameterType, out argumentSetters[i]))
                {
                    // そのまま次の引数へ
                    continue;
                }


                // 対応関数が無いなら警告を出してobject型そのまま出力する変換関数を利用する
                SrLogger.Warning(SharedString.LogTag.PERIPHERAL, $"'{parameter.ParameterType.FullName}' convert function not found.");
                argumentSetters[i] = fromValueConvertTable[typeof(object)];
            }
        }


        /// <summary>
        /// 返却情報をセットアップします
        /// </summary>
        /// <param name="info">周辺機器関数として使用する関数情報</param>
        /// <exception cref="ArgumentException">関数 '{info.DeclaringType.FullName}.{info.Name}' は Task または Task\<TResult\> または void を返しません</exception>
        private void SetupReturnInfo(MethodInfo info)
        {
            // 関数の戻り値型を取得
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, $"Setup return info. from '{info.DeclaringType.FullName}.{info.Name}'");
            var returnType = info.ReturnType;


            // ただの void 返却なら
            if (returnType == typeof(void))
            {
                // void 戻り値であることを覚える
                SrLogger.Trace(SharedString.LogTag.PERIPHERAL, "Simple void return.");
                taskResultType = typeof(void);
                isVoidReturn = true;
                taskResultProperty = null;
                resultSetter = null;
                return;
            }


            // もしTask型を返さない関数なら
            if (!typeof(Task).IsAssignableFrom(returnType))
            {
                // 周辺機器関数は必ずTask型を返すようなシグネチャでなければならない
                throw new ArgumentException($"関数 '{info.DeclaringType.FullName}.{info.Name}' は Task または Task<TResult> または void を返しません");
            }


            // Task返却であることを覚える
            taskResultType = typeof(void);
            taskResultProperty = null;
            isVoidReturn = false;
            resultSetter = null;


            // もし単純なTask型なら
            if (!returnType.IsGenericType)
            {
                // 非ジェネリックであることを伝えて終了
                SrLogger.Trace(SharedString.LogTag.PERIPHERAL, "Task is not generic.");
                return;
            }


            // Task<TResult>.Result プロパティの取得と型の取得
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, "Return type is task.");
            taskResultProperty = returnType.GetProperty("Result");
            taskResultType = returnType.GenericTypeArguments[0];


            // 返却する型によって設定関数を選択するが、選択出来なかった場合は
            if (!toValueConvertTable.TryGetValue(taskResultType, out resultSetter))
            {
                // 通常の型では解決しないときはobject型として処理をする
                SrLogger.Warning(SharedString.LogTag.PERIPHERAL, $"'{taskResultType}' convert function not found.");
                resultSetter = toValueConvertTable[typeof(object)];
            }
        }


        /// <summary>
        /// 指定された値配列の位置と長さから引数を取り出し、関数を呼び出します。
        /// 引数は記述とは逆に設定されている前提となっています。
        /// Hint : [Code] arg1, arg2, arg3 [Data] arg3, arg2, arg1
        /// </summary>
        /// <param name="args">引数として使用される配列の参照</param>
        /// <param name="index">引数として使用する開始インデックス</param>
        /// <param name="count">引数として使用する長さ</param>
        /// <returns>呼び出した関数を待機するタスクを返します</returns>
        public Task Call(SrValue[] args, int index, int count)
        {
            // 配列外参照例外を承知でいきなりループでアクセス（呼び出しコードは極力実行速度優先で実装）
            for (int i = 0; i < count; ++i)
            {
                // 引数設定関数を用いて値配列から引数配列へ参照コピー（ボクシングは現状やむなし、改善方法を検討）
                arguments[i] = argumentSetters[i](args[index + count - 1 - i]);
            }


            // 関数を呼び出して引数をクリア後、もしvoid戻り値の関数なのであれば直ちに完了タスクを返す
            var result = methodInfo.Invoke(targetInstance, arguments);
            Array.Clear(arguments, 0, arguments.Length);
            if (isVoidReturn) return Task.CompletedTask;


            // void戻り値ではないのならTaskとして返す
            return (Task)result;
        }


        /// <summary>
        /// 非同期タスクの時の場合、結果をタスクから受け取ります
        /// </summary>
        /// <returns>タスクが終了している場合の結果を受け取ります</returns>
        public SrValue GetResult()
        {
            // void 返却または 結果設定関数がない（単純なTask）の場合は
            if (isVoidReturn || resultSetter == null)
            {
                // GetResultは操作できませんよ
                throw new InvalidOperationException("void または Task 戻り値に対して結果を受け取れません");
            }


            // プロパティのGet関数を使って結果を拾い上げて結果を設定関数を経由して返す
            return resultSetter(taskResultProperty.GetValue(targetInstance));
        }
    }
}