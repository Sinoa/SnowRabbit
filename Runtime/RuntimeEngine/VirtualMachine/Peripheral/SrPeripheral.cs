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
using System.Linq;
using System.Reflection;
using SnowRabbit.Diagnostics.Logging;

namespace SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral
{
    /// <summary>
    /// SnowRabbit の周辺機器を表すクラスです
    /// </summary>
    internal class SrPeripheral
    {
        // メンバ変数定義
        private readonly Dictionary<string, SrPeripheralFunction> functionTable = new Dictionary<string, SrPeripheralFunction>();



        /// <summary>
        /// このインスタンスに設定された周辺機器名
        /// </summary>
        public string Name { get; }



        /// <summary>
        /// SrPeripheral クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="targetInstance">SrPeripheralAttribute 属性がついたインスタンス</param>
        /// <exception cref="ArgumentNullException">targetInstance が null です</exception>
        /// <exception cref="SrPeripheralAttributeNotFoundException">targetInstance に SrPeripheralAttribute 属性が見つかりませんでした</exception>
        public SrPeripheral(object targetInstance)
        {
            // まずは型情報を取得する
            var targetType = (targetInstance ?? throw new ArgumentNullException(nameof(targetInstance))).GetType();
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, $"SrPeripheral Initialize of '{targetType.FullName}'.");


            // 名前の取得と関数の登録をする
            Name = GetPeripheralAttribute(targetType).Name;
            RegisterHostFunctions(targetInstance, GetHostFunctions(targetType));
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, $"SrPeripheral Initialize Success '{targetType.FullName}'.");
        }


        /// <summary>
        /// 指定された関数列挙オブジェクトから実際の周辺機器関数として登録をします
        /// </summary>
        /// <param name="targetInstance">周辺機器関数を保持しているインスタンスへの参照</param>
        /// <param name="functions">取得された周辺機器関数列挙オブジェクト</param>
        private void RegisterHostFunctions(object targetInstance, IEnumerable<(MethodInfo info, SrHostFunctionAttribute attribute)> functions)
        {
            // 取得された関数をすべて回る
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, $"RegisterHostFunctions({targetInstance.GetType().FullName}, functions);");
            foreach (var (info, attribute) in functions)
            {
                // 既に同じ名前の関数がテーブルに存在するなら
                var functionName = attribute.Name;
                if (functionTable.ContainsKey(functionName))
                {
                    // 警告を出して次へ
                    SrLogger.Warning(SharedString.LogTag.PERIPHERAL, $"Type '{targetInstance.GetType().FullName}' is function '{functionName}' already exists.");
                    continue;
                }


                // 周辺機器関数オブジェクトを生成してテーブルに設定
                SrLogger.Trace(SharedString.LogTag.PERIPHERAL, $"Create and Register '{functionName}' peripheral function.");
                functionTable[functionName] = new SrPeripheralFunction(info.IsStatic ? null : targetInstance, info);
            }
        }


        /// <summary>
        /// 指定された型から SrPeripheralAttribute 属性を取得します
        /// </summary>
        /// <param name="targetType">周辺機器として扱う型</param>
        /// <returns>targetType に SrPeripheralAttribute 属性がついている場合は属性のインスタンスを返します</returns>
        /// <exception cref="SrPeripheralAttributeNotFoundException">'Type' に SrPeripheralAttribute 属性が見つかりませんでした</exception>
        private static SrPeripheralAttribute GetPeripheralAttribute(Type targetType)
        {
            // SrPeripheralAttribute の取得をして成功したのならインスタンスを返す
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, $"GetPeripheralAttribute({targetType.FullName});");
            var attribute = targetType.GetCustomAttribute<SrPeripheralAttribute>();
            if (attribute != null) return attribute;


            // 属性の取得に失敗したのなら例外を吐く
            var message = $"'{targetType.FullName}' に SrPeripheralAttribute 属性が見つかりませんでした";
            throw new SrPeripheralAttributeNotFoundException(message);
        }


        /// <summary>
        /// 指定された型から SrHostFunctionAttribute 属性を設定された関数を列挙します
        /// </summary>
        /// <param name="targetType">周辺機器関数を取り出す元になる型</param>
        /// <returns>指定された型から取り出せる周辺機器関数の列挙可能オブジェクトを返します</returns>
        private static IEnumerable<(MethodInfo, SrHostFunctionAttribute)> GetHostFunctions(Type targetType)
        {
            // まずはすべての関数を取り出して、SrHostFunctionAttribute 属性を取得後に属性が null でない関数のみを列挙する
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, $"GetHostFunctions({targetType.FullName});");
            return targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Select(info => (info, attribute: info.GetCustomAttribute<SrHostFunctionAttribute>()))
                .Where(x => x.attribute != null);
        }


        /// <summary>
        /// 指定された名前の周辺機器関数を取得します
        /// </summary>
        /// <param name="name">取得する周辺機器関数の名前</param>
        /// <returns>指定された名前の周辺機器関数を返します</returns>
        /// <exception cref="ArgumentException">name に null または 空文字列 または 空白文字列 を指定されました</exception>
        /// <exception cref="SrPeripheralFunctionNotFoundException">指定された名前 'name' の周辺機器関数が見つかりませんでした</exception>
        internal SrPeripheralFunction GetPeripheralFunction(string name)
        {
            // 指定された名前が無効な名前なら
            if (string.IsNullOrWhiteSpace(name))
            {
                // 何をしでかすつもりじゃ
                throw new ArgumentException($"'{nameof(name)}' に null または 空文字列 または 空白文字列 を指定されました");
            }


            // 関数の取得を試みて成功したら返す
            SrLogger.Trace(SharedString.LogTag.PERIPHERAL, $"SrPeripheralFunction({name});");
            if (functionTable.TryGetValue(name, out var function)) return function;


            // 見つけられなかった例外を吐く
            throw new SrPeripheralFunctionNotFoundException($"指定された名前 '{name}' の周辺機器関数が見つかりませんでした");
        }
    }
}