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
        private static readonly Func<object, SrValue> taskToValueConvert;

        // メンバ変数定義
        private readonly object targetInstance;
        private readonly MethodInfo methodInfo;
        private object[] arguments;
        private Func<SrValue, object>[] argumentSetters;
        private Func<object, SrValue> resultSetter;
        private object result;
        private bool isTask;
        private int processIDArgumentIndex = -1;



        /// <summary>
        /// PeripheralFunction クラスの初期化をします
        /// </summary>
#pragma warning disable CA1810
        static SrPeripheralFunction()
#pragma warning restore CA1810
        {
            // SrValue から特定の型へ正しくキャストする関数テーブルを初期化
            // TODO 素直にリゾルバ化したほうが良いかもしれない
            fromValueConvertTable = new Dictionary<Type, Func<SrValue, object>>()
            {
                // 各型に合わせた返却関数を用意
                { typeof(sbyte), x => (sbyte)x.Primitive.Long },
                { typeof(byte), x => (byte)x.Primitive.Long },
                { typeof(char), x => (char)x.Primitive.Long },
                { typeof(short), x => (short)x.Primitive.Long },
                { typeof(ushort), x => (ushort)x.Primitive.Long },
                { typeof(int), x => (int)x.Primitive.Long },
                { typeof(uint), x => (uint)x.Primitive.Long },
                { typeof(long), x => x.Primitive.Long },
                { typeof(ulong), x => (ulong)x.Primitive.Long },
                { typeof(float), x => x.Primitive.Float },
                { typeof(double), x => x.Primitive.Double },
                { typeof(bool), x => x.Primitive.Long != 0 },
                { typeof(IntPtr), x => (IntPtr)x.Primitive.Long },
                { typeof(UIntPtr), x => (UIntPtr)x.Primitive.Long },
                { typeof(string), x => x.Object },
                { typeof(object), x => x.Object },
            };


            // object から SrValue へ正しくキャストする関数テーブルを初期化
            // TODO 素直にリゾルバ化したほうが良いかもしれない
            toValueConvertTable = new Dictionary<Type, Func<object, SrValue>>()
            {
                // 各型に合わせた返却関数を用意
                { typeof(void), x => default },
                { typeof(sbyte), x => (long)(sbyte)x },
                { typeof(byte), x => (long)(byte)x },
                { typeof(char), x => (long)(char)x },
                { typeof(short), x => (long)(short)x },
                { typeof(ushort), x => (long)(ushort)x },
                { typeof(int), x => (long)(int)x },
                { typeof(uint), x => (long)(uint)x },
                { typeof(long), x => (long)x },
                { typeof(ulong), x => (ulong)(long)x },
                { typeof(float), x => (float)x },
                { typeof(double), x => (double)x },
                { typeof(bool), x => ((bool)x) ? 1L : 0L },
                { typeof(IntPtr), x => (long)x },
                { typeof(UIntPtr), x => (long)x },
                { typeof(string), x => (string)x },
                { typeof(object), x => new SrValue { Object = x } },
                { typeof(Task), x => default },
                { typeof(Task<sbyte>), x => (long)((Task<sbyte>)x).Result },
                { typeof(Task<byte>), x => (long)((Task<byte>)x).Result },
                { typeof(Task<char>), x => (long)((Task<char>)x).Result },
                { typeof(Task<short>), x => (long)((Task<short>)x).Result },
                { typeof(Task<ushort>), x => (long)((Task<ushort>)x).Result },
                { typeof(Task<int>), x => (long)((Task<int>)x).Result },
                { typeof(Task<uint>), x => (long)((Task<uint>)x).Result },
                { typeof(Task<long>), x => ((Task<long>)x).Result },
                { typeof(Task<ulong>), x => (long)((Task<ulong>)x).Result },
                { typeof(Task<float>), x => ((Task<float>)x).Result },
                { typeof(Task<double>), x => ((Task<double>)x).Result },
                { typeof(Task<bool>), x => ((Task<bool>)x).Result ? 1L : 0L },
                { typeof(Task<IntPtr>), x => (long)((Task<IntPtr>)x).Result },
                { typeof(Task<UIntPtr>), x => (long)((Task<UIntPtr>)x).Result },
                { typeof(Task<string>), x => ((Task<string>)x).Result },
                { typeof(Task<object>), x => new SrValue { Object = ((Task<object>)x).Result } },
            };


            // Task<TResult>にて定義済み以外からの解決ができない場合の汎用解決関数
            // TODO 素直にリゾルバ化したほうが良いかもしれない
            taskToValueConvert = x => new SrValue { Object = x.GetType().GetProperty("Result").GetValue(x) };
        }


        /// <summary>
        /// PeripheralFunction クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="target">インスタンス関数呼び出しの際に使用する呼び出す対象のインスタンス、静的関数の場合は null を指定</param>
        /// <param name="info">周辺機器関数として使用する関数の情報</param>
        /// <exception cref="ArgumentNullException">info が null です</exception>
        public SrPeripheralFunction(object target, MethodInfo info)
        {
            // ひとまず参照を受け取る
            targetInstance = target;
            methodInfo = info ?? throw new ArgumentNullException(nameof(info));


            // 引数と戻り値のセットアップをする
            SetupArgumentInfo(info);
            SetupReturnInfo(info);
        }


        /// <summary>
        /// 引数情報をセットアップします
        /// </summary>
        /// <param name="info">周辺機器関数として使用する関数情報</param>
        /// <exception cref="NotSupportedException">SrProcessIDAttribute 属性の引数は int でなければなりません</exception>
        private void SetupArgumentInfo(MethodInfo info)
        {
            // 引数の数を知る
            var parameters = info.GetParameters();


            // 引数が0件なら
            if (parameters.Length == 0)
            {
                // 引数関連は空で初期化
                arguments = Array.Empty<object>();
                argumentSetters = Array.Empty<Func<SrValue, object>>();
                return;
            }


            // 引数と引数設定関数を初期化子て引数の数分ループ
            arguments = new object[parameters.Length];
            argumentSetters = new Func<SrValue, object>[parameters.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                // 引数情報の取得
                var parameter = parameters[i];


                // もし SrProcessIDAttribute 属性がついていたら
                if (parameter.GetCustomAttribute<SrProcessIDAttribute>() != null)
                {
                    // パラメータがintでないなら
                    if (parameter.ParameterType != typeof(int))
                    {
                        // int以外は非サポートである例外を吐く
                        throw new NotSupportedException("SrProcessIDAttribute 属性の引数は int でなければなりません");
                    }


                    // この引数インデックスをプロセスID用引数であることを覚える
                    processIDArgumentIndex = i;
                }


                // 変換対象の型から変換関数を取り出す
                if (!fromValueConvertTable.TryGetValue(parameter.ParameterType, out argumentSetters[i]))
                {
                    // 対応関数が無いなら警告を出してobject型そのまま出力する変換関数を利用する
                    argumentSetters[i] = fromValueConvertTable[typeof(object)];
                }
            }
        }


        /// <summary>
        /// 返却情報をセットアップします
        /// </summary>
        /// <param name="info">周辺機器関数として使用する関数情報</param>
        /// <exception cref="ArgumentException">関数 '{info.DeclaringType.FullName}.{info.Name}' は Task または Task\<TResult\> または void を返しません</exception>
        private void SetupReturnInfo(MethodInfo info)
        {
            // 関数の戻り値型を取得してタスクかどうかを知る
            var returnType = info.ReturnType;
            isTask = typeof(Task).IsAssignableFrom(returnType);


            // 変換テーブルから変換関数を取り出せるのなら
            if (toValueConvertTable.TryGetValue(returnType, out resultSetter))
            {
                // これ以上解析はしない
                return;
            }


            // Task<TResult> かどうかによってデフォルトの解決関数を選択する
            resultSetter = isTask ? taskToValueConvert : toValueConvertTable[typeof(object)];
        }


        /// <summary>
        /// 指定された値配列の位置と長さから引数を取り出し、関数を呼び出します。
        /// </summary>
        /// <param name="memory">関数呼び出しに使用する仮想メモリ</param>
        /// <param name="address">引数として使用する開始アドレス</param>
        /// <param name="processID">プロセスIDとして渡す値</param>
        /// <returns>呼び出した関数を待機するタスクを返します</returns>
        public Task Call(SrVirtualMemory memory, int address, int processID)
        {
            // 配列外参照例外を承知でいきなりループでアクセス（呼び出しコードは極力実行速度優先で実装）
            int indexGap = 0;
            for (int i = 0; i < arguments.Length; ++i)
            {
                // もしプロセスIDを渡すインデックスなら
                if (processIDArgumentIndex == i)
                {
                    // 引数にプロセスIDを入れてインデックスギャップを調整後次へ
                    arguments[i] = processID;
                    indexGap += 1;
                    continue;
                }


                // 引数設定関数を用いて値配列から引数配列へ参照コピー（ボクシングは現状やむなし、改善方法を検討）
                arguments[i] = argumentSetters[i](memory[address + i - indexGap]);
            }


            // 関数を呼び出して引数をクリア
            result = methodInfo.Invoke(targetInstance, arguments);
            Array.Clear(arguments, 0, arguments.Length);


            // タスクの場合は単純なTaskへキャストして返して、タスクでないなら直ちに完了を返す
            return isTask ? (Task)result : Task.CompletedTask;
        }


        /// <summary>
        /// 非同期タスクの時の場合、結果をタスクから受け取ります
        /// </summary>
        /// <returns>タスクが終了している場合の結果を受け取ります</returns>
        public SrValue GetResult()
        {
            // 変換関数を通して返す
            return resultSetter(result);
        }
    }
}