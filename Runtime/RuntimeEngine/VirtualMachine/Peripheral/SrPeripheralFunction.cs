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

        // メンバ変数定義
        private object targetInstance;
        private MethodInfo methodInfo;

        private object[] arguments;
        private Func<SrValue, object>[] argumentSetters;
        private Func<object, SrValue> setResult;
        private bool isTask;
        private PropertyInfo taskResultProperty;



        /// <summary>
        /// PeripheralFunction クラスの初期化をします
        /// </summary>
        static SrPeripheralFunction()
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
                { typeof(IntPtr), x => new IntPtr(x.Primitive.Long) },
                { typeof(UIntPtr), x => new UIntPtr(x.Primitive.Ulong) },
                { typeof(string), x => x.Object },
                { typeof(object), x => x.Object },
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
        }


        /// <summary>
        /// PeripheralFunction クラスのインスタンスを初期化をします
        /// </summary>
        /// <param name="info">周辺機器関数として使用する関数情報</param>
        /// <exception cref="ArgumentNullException">info が null です</exception>
        public SrPeripheralFunction(MethodInfo info)
        {
            // 関数情報を覚える
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, "Begin create peripheral function.");
            methodInfo = info ?? throw new ArgumentNullException(nameof(info));


            // 各種情報のセットアップをする
            SetupArgumentInfo(info);
            SetupReturnInfo(info);
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
                // 引数情報を取得して変換関数があるなら
                var parameter = parameters[i];
                if (fromValueConvertTable.TryGetValue(parameter.ParameterType, out argumentSetters[i]))
                {
                    // そのまま次の引数へ
                    continue;
                }


                // 対応関数が無いならobject型そのまま出力する変換関数を利用する
                SrLogger.Debug(SharedString.LogTag.PERIPHERAL, $"'{parameter.ParameterType.FullName}' convert function not found.");
                argumentSetters[i] = fromValueConvertTable[typeof(object)];
            }
        }


        private void SetupReturnInfo(MethodInfo info)
        {
            // 関数の戻り値型を取得
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, "Setup return info.");
            var returnType = info.ReturnType;


            // もしTask型の戻り値なら
            if (typeof(Task).IsAssignableFrom(returnType))
            {
                // Task型の戻り値であることを覚える
                SrLogger.Trace(SharedString.LogTag.PERIPHERAL, "Return type is task.");
                isTask = true;


                // もし単純なTask型なら
                if (!returnType.IsGenericType)
                {
                    // 非ジェネリックのTaskであることを覚えて終了
                    SrLogger.Trace(SharedString.LogTag.PERIPHERAL, "Task is not generic.");
                    taskResultProperty = null;
                    setResult = null;
                    return;
                }


                // ジェネリックなTaskならどんな値を返すかを知るのとResultプロパティを取得
                var taskResultType = returnType.GenericTypeArguments[0];
                taskResultProperty = returnType.GetProperty("Result");
                SrLogger.Trace(SharedString.LogTag.PERIPHERAL, $"Task generic type is '{taskResultType.FullName}'.");
            }


            // タスクではないことを覚える
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, "Return type is not task.");
            isTask = false;
            taskResultProperty = null;


            if ((returnType == typeof(void) || returnType.IsPrimitive) && returnType != typeof(IntPtr) && returnType != typeof(UIntPtr))
            {
                isTask = false;
                taskResultProperty = null;
                return;
            }
        }


        public void Call(SrValue[] args, int index, int count)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                arguments[i] = argumentSetters[i](args[i]);
            }


            methodInfo.Invoke(null, arguments);
            Array.Clear(arguments, 0, arguments.Length);
        }
    }
}