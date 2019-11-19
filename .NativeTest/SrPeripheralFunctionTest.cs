﻿// zlib/libpng License
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
using System.Threading.Tasks;
using NUnit.Framework;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral;

namespace SnowRabbitTest
{
    /// <summary>
    /// SrPeripheralFunction クラスのテストクラスです
    /// </summary>
    [TestFixture]
    public class SrPeripheralFunctionTest
    {
        // メンバ変数定義
        private SrPeripheral peripheral = null;



        /// <summary>
        /// インスタンスの生成をテストします
        /// </summary>
        [Test, Order(0)]
        public void CreateInstanceTest()
        {
            // 各種例外発生パターン分（null, 非周辺機器クラス）の確認をして、正しく生成されれば問題なし
            Assert.Throws<ArgumentNullException>(() => new SrPeripheral(null));
            Assert.Throws<SrPeripheralAttributeNotFoundException>(() => new SrPeripheral(new MyNonePeripheralTestClass()));
            Assert.DoesNotThrow(() => peripheral = new SrPeripheral(new MyPeripheralTestClass()));
            Assert.AreEqual("MyPeripheral", peripheral.Name);
        }


        /// <summary>
        /// 単純な周辺機器関数の呼び出しをテストします
        /// </summary>
        [Test, Order(1)]
        public void CallSimplePeripheralFunction()
        {
            // そもそも関数が見つからない例外を確認をする
            Assert.Throws<ArgumentException>(() => peripheral.GetPeripheralFunction(null));
            Assert.Throws<ArgumentException>(() => peripheral.GetPeripheralFunction(""));
            Assert.Throws<ArgumentException>(() => peripheral.GetPeripheralFunction("  "));
            Assert.Throws<SrPeripheralFunctionNotFoundException>(() => peripheral.GetPeripheralFunction("HogeMoge"));


            // 周辺機器から関数を取り出して実行する
            var function = peripheral.GetPeripheralFunction("Simple");
            Assert.IsNotNull(function);
            var task = function.Call(Array.Empty<SrValue>(), 0, 0, 0);
            Assert.True(task.IsCompleted);


            // 周辺機器から引数付き関数を取り出して実行する（引数は第一引数からプッシュする仮想マシンになるので、配列に収める順番は逆順になる）
            function = peripheral.GetPeripheralFunction("SimpleEx");
            var arg = new SrValue[6];
            arg[3].Object = "足し算をするよ";
            arg[4].Primitive.Int = 456;
            arg[5].Primitive.Int = 123;
            Assert.IsNotNull(function);
            task = function.Call(arg, 3, 3, 0);
            Assert.True(task.IsCompleted);


            // 単純な戻り地を受け取る関数を取り出して実行する
            function = peripheral.GetPeripheralFunction("RetSimple");
            Assert.IsNotNull(function);
            function.Call(Array.Empty<SrValue>(), 0, 0, 0);
            Assert.AreEqual("Simple Return Function", function.GetResult().Object);


            // 単純な引数の受け取りと結果を返す関数を取り出して実行する
            function = peripheral.GetPeripheralFunction("RetSimpleEx");
            Assert.IsNotNull(function);
            arg[0].Primitive.Int = 456;
            arg[1].Primitive.Int = 123;
            function.Call(arg, 0, 2, 0);
            Assert.AreEqual(579, function.GetResult().Primitive.Int);
        }


        /// <summary>
        /// 非同期周辺機器関数の呼び出しをテストします
        /// </summary>
        [Test, Order(2)]
        public void CallTaskPeripheralFunction()
        {
            // 直ちに完了するはずの関数を取り出して完了済みであることを確認する
            var taskFunc = peripheral.GetPeripheralFunction("CompTaskFunc");
            Assert.IsNotNull(taskFunc);
            Assert.True(taskFunc.Call(Array.Empty<SrValue>(), 0, 0, 0).IsCompleted);


            // 少しだけ待つタスクを取得して待機しているかを確認する
            taskFunc = peripheral.GetPeripheralFunction("WaitTaskFunc");
            Assert.IsNotNull(taskFunc);
            var task = taskFunc.Call(Array.Empty<SrValue>(), 0, 0, 0);
            Assert.False(task.IsCompleted);
            task.Wait();


            // 結果を返してくれるタスクを実行して結果が想定通りか確認する
            taskFunc = peripheral.GetPeripheralFunction("AddTaskFunc");
            Assert.IsNotNull(taskFunc);
            var arg = new SrValue[4];
            arg[2].Primitive.Int = 456;
            arg[3].Primitive.Int = 123;
            task = taskFunc.Call(arg, 2, 2, 0);
            Assert.True(task.IsCompleted);
            task.Wait();
            var result = taskFunc.GetResult();
            Assert.AreEqual(579, result.Primitive.Int);


            // 少しだけ待つ時の結果を返すタスクを実行して結果が想定通りか確認する
            taskFunc = peripheral.GetPeripheralFunction("CombWaitTaskFunc");
            Assert.IsNotNull(taskFunc);
            arg[0] = "結合されるはずです。";
            arg[1] = "このメッセージは、";
            task = taskFunc.Call(arg, 0, 2, 0);
            Assert.False(task.IsCompleted);
            task.Wait();
            result = taskFunc.GetResult();
            Assert.AreEqual("このメッセージは、結合されるはずです。", (string)result.Object);
        }


        /// <summary>
        /// 少々特別な周辺機器関数の呼び出しテストをします
        /// </summary>
        [Test, Order(3)]
        public void CallSpecialPeripheralFunction()
        {
            // プロセスIDが渡ってくする関数の呼び出しをするが、プロセスIDは引数リストからは渡すことはない（ここからは見えない引数として扱う）
            var function = peripheral.GetPeripheralFunction("ProcFunc");
            Assert.IsNotNull(function);
            var arg = new SrValue[4];
            arg[0].Primitive.Int = 456;
            arg[1].Primitive.Int = 123;
            function.Call(arg, 0, 2, 579);


            // プライベートな関数でも属性がついている場合はアクセスが可能であることを確認する
            function = peripheral.GetPeripheralFunction("PrivateFunc");
            Assert.IsNotNull(function);
            function.Call(Array.Empty<SrValue>(), 0, 0, 0);


            // 静的な関数でも呼び出せることを確認
            function = peripheral.GetPeripheralFunction("StaticFunc");
            Assert.IsNotNull(function);
            function.Call(Array.Empty<SrValue>(), 0, 0, 0);
        }
    }



    /// <summary>
    /// SrPeripheralAttribute 属性がついていない場合のクラスを用意した場合のテスト用クラスです
    /// </summary>
    public class MyNonePeripheralTestClass
    {
        /// <summary>
        /// 非常に単純な周辺機器関数の定義です
        /// </summary>
        [SrHostFunction("Function")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<保留中>")]
        public void TestFunction()
        {
            // この関数は呼べてしまっているのかどうか、呼べてしまったら駄目
            Assert.Fail("この関数は呼び出せてはいけません");
        }
    }



    /// <summary>
    /// 周辺機器クラスとして実装した時のテスト用クラスです
    /// </summary>
    [SrPeripheral("MyPeripheral")]
    public class MyPeripheralTestClass
    {
        /// <summary>
        /// 非常に単純な周辺機器関数の定義です
        /// </summary>
        [SrHostFunction("Simple")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<保留中>")]
        public void SimpleFunction()
        {
            // 関数は呼び出せています
            Console.WriteLine("Called SimpleFunction");
        }


        /// <summary>
        /// 引数付きの単純な周辺機器関数の定義です
        /// </summary>
        /// <param name="a">引数１</param>
        /// <param name="b">引数２</param>
        /// <param name="message">引数３</param>
        [SrHostFunction("SimpleEx")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<保留中>")]
        public void SimpleFunctionEx(int a, int b, string message)
        {
            // 関数は読み出せています
            Console.WriteLine($"[{message}] {a} + {b} = {a + b}");
        }


        /// <summary>
        /// 値を返す単純な周辺機器関数の定義です
        /// </summary>
        /// <returns>単純な文字列を返します</returns>
        [SrHostFunction("RetSimple")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<保留中>")]
        public string SimpleReturnableFunction()
        {
            // 単純に結果を返します
            return "Simple Return Function";
        }


        /// <summary>
        /// 値を返す単純な周辺機器関数の定義です
        /// </summary>
        /// <param name="a">引数１</param>
        /// <param name="b">引数２</param>
        /// <returns>引数１と２の加算結果を返します</returns>
        [SrHostFunction("RetSimpleEx")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<保留中>")]
        public int SimpleReturnableFunction(int a, int b)
        {
            // 単純に加算結果を返す
            return a + b;
        }


        /// <summary>
        /// すぐに完了を返す非同期関数の定義です
        /// </summary>
        /// <returns>完了済みタスクを返します</returns>
        [SrHostFunction("CompTaskFunc")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<保留中>")]
        public Task CompletedTaskFunction()
        {
            // 完了済みタスクを返す
            Console.WriteLine("Called CompletedTaskFunction");
            return Task.CompletedTask;
        }


        /// <summary>
        /// 少しだけ待機する非同期関数の定義です
        /// </summary>
        /// <returns>少しだけ待機するタスクを返します</returns>
        [SrHostFunction("WaitTaskFunc")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<保留中>")]
        public async Task WaitTaskFunction()
        {
            // 数秒待つ
            Console.WriteLine("Called WaitTaskFunction");
            await Task.Delay(1000);
            Console.WriteLine("Complete WaitTaskFunction");
        }


        /// <summary>
        /// 結果を返せる非同期関数の定義です
        /// </summary>
        /// <param name="a">引数１</param>
        /// <param name="b">引数２</param>
        /// <returns>結果を返すタスクを返します</returns>
        [SrHostFunction("AddTaskFunc")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<保留中>")]
        public Task<int> ReturnableTaskFunction(int a, int b)
        {
            // 足して返す
            Console.WriteLine($"Called ReturnableTaskFunction({a}, {b})");
            return Task.FromResult(a + b);
        }


        /// <summary>
        /// 結果を返す少しだけ待つ非同期関数の定義です
        /// </summary>
        /// <param name="messageA">メッセージA</param>
        /// <param name="messageB">メッセージB</param>
        /// <returns>少しだけ待機するタスクを返します</returns>
        [SrHostFunction("CombWaitTaskFunc")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<保留中>")]
        public async Task<string> WaitReturnableTaskFunction(string messageA, string messageB)
        {
            // 少しだけ待機して結果を返す
            Console.WriteLine($"Called WaitReturnableTaskFunction({messageA}, {messageB})");
            await Task.Delay(1000);
            Console.WriteLine($"Complete WaitReturnableTaskFunction({messageA}, {messageB})");
            return messageA + messageB;
        }


        /// <summary>
        /// プロセスIDを受け取る関数の定義です
        /// </summary>
        /// <param name="a">単純な引数１</param>
        /// <param name="processID">プロセスID</param>
        /// <param name="b">単純な引数２</param>
        /// <returns>引数１と２の合計を返します</returns>
        [SrHostFunction("ProcFunc")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<保留中>")]
        public int ProcessIDFunction(int a, [SrProcessID]int processID, int b)
        {
            // プロセスIDを出力しつつ a + b の結果が processID になると予想する
            Console.WriteLine($"ProcessID : {processID}");
            Assert.True(processID == a + b);
            return a + b;
        }


        /// <summary>
        /// プライベートな関数の定義です
        /// </summary>
        [SrHostFunction("PrivateFunc")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("コードの品質", "IDE0051:使用されていないプライベート メンバーを削除する", Justification = "<保留中>")]
        private void PrivateFunction()
        {
            // 呼び出されたことを出力
            Console.WriteLine("Called PrivateFunc");
        }


        /// <summary>
        /// 静的関数の定義です
        /// </summary>
        [SrHostFunction("StaticFunc")]
        public static void StaticFunction()
        {
            // 呼び出されたことを出力
            Console.WriteLine("Called StaticFunc");
        }
    }
}