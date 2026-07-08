// zlib License
// 
// Copyright (c) 2019 Sinoa
// 
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
// 
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software
// in a product, an acknowledgment in the product documentation would be
// appreciated but is not required.
// 
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
// 
// 3. This notice may not be removed or altered from any source
// distribution.

using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral;

namespace SnowRabbit.Tests;

/// <summary>
/// SrPeripheralFunction クラスのテストクラスです
/// </summary>
[TestFixture]
public class SrPeripheralFunctionTest
{
    // メンバ変数定義
    private SrPeripheral peripheral = null!;
    private MyPeripheralTestClass peripheralInstance = null!;
    private SrVirtualMemory memory = default;



    /// <summary>
    /// テストのセットアップをします
    /// </summary>
    [OneTimeSetUp]
    public void Setup()
    {
        // 仮想メモリを用意する
        SrValue[] memoryPool = new SrValue[100];
        MemoryBlock<SrValue> programMemory = new MemoryBlock<SrValue>(memoryPool, 0, 10);
        MemoryBlock<SrValue> globalMemory = new MemoryBlock<SrValue>(memoryPool, 10, 10);
        MemoryBlock<SrValue> heapMemory = new MemoryBlock<SrValue>(memoryPool, 20, 10);
        MemoryBlock<SrValue> stackMemory = new MemoryBlock<SrValue>(memoryPool, 30, 10);
        memory = new SrVirtualMemory(programMemory, globalMemory, heapMemory, stackMemory);
    }


    /// <summary>
    /// インスタンスの生成をテストします
    /// </summary>
    [Test, Order(0)]
    public void CreateInstanceTest()
    {
        // 各種例外発生パターン分（null, 非周辺機器クラス）の確認をして、正しく生成されれば問題なし
        Assert.Throws<ArgumentNullException>(() => new SrPeripheral(null!));
        Assert.Throws<SrPeripheralAttributeNotFoundException>(() => new SrPeripheral(new MyNonePeripheralTestClass()));
        peripheralInstance = new MyPeripheralTestClass();
        Assert.DoesNotThrow(() => peripheral = new SrPeripheral(peripheralInstance));
        Assert.That(peripheral.Name, Is.EqualTo("MyPeripheral"));
    }


    /// <summary>
    /// 単純な周辺機器関数の呼び出しをテストします
    /// </summary>
    [Test, Order(1)]
    public void CallSimplePeripheralFunctionTest()
    {
        // そもそも関数が見つからない例外を確認をする
        Assert.Throws<ArgumentException>(() => peripheral.GetPeripheralFunction(null!));
        Assert.Throws<ArgumentException>(() => peripheral.GetPeripheralFunction(""));
        Assert.Throws<ArgumentException>(() => peripheral.GetPeripheralFunction("  "));
        Assert.Throws<SrPeripheralFunctionNotFoundException>(() => peripheral.GetPeripheralFunction("HogeMoge"));


        // 周辺機器から関数を取り出して実行する
        SrPeripheralFunction function = peripheral.GetPeripheralFunction("Simple");
        Assert.That(function, Is.Not.Null);
        Task task = function.Call(memory, 0, 0, out _);
        Assert.That(task.IsCompleted, Is.True);


        // 周辺機器から引数付き関数を取り出して実行する
        function = peripheral.GetPeripheralFunction("SimpleEx");
        memory[3] = 123;
        memory[4] = 456;
        memory[5] = "足し算をするよ";
        Assert.That(function, Is.Not.Null);
        task = function.Call(memory, 3, 0, out _);
        Assert.That(task.IsCompleted, Is.True);


        // 単純な戻り地を受け取る関数を取り出して実行する
        function = peripheral.GetPeripheralFunction("RetSimple");
        Assert.That(function, Is.Not.Null);
        function.Call(memory, 0, 0, out SrValue retSimpleResult);
        Assert.That(retSimpleResult.Object, Is.EqualTo("Simple Return Function"));


        // 単純な引数の受け取りと結果を返す関数を取り出して実行する
        function = peripheral.GetPeripheralFunction("RetSimpleEx");
        Assert.That(function, Is.Not.Null);
        memory[0] = 123;
        memory[1] = 456;
        function.Call(memory, 0, 0, out SrValue retSimpleExResult);
        Assert.That(retSimpleExResult.Primitive.Int, Is.EqualTo(579));
    }


    /// <summary>
    /// 非同期周辺機器関数の呼び出しをテストします
    /// </summary>
    [Test, Order(2)]
    public void CallTaskPeripheralFunctionTest()
    {
        // 直ちに完了するはずの関数を取り出して完了済みであることを確認する
        SrPeripheralFunction taskFunc = peripheral.GetPeripheralFunction("CompTaskFunc");
        Assert.That(taskFunc, Is.Not.Null);
        Assert.That(taskFunc.Call(memory, 0, 0, out _).IsCompleted, Is.True);


        // 少しだけ待つタスクを取得して待機しているかを確認する
        taskFunc = peripheral.GetPeripheralFunction("WaitTaskFunc");
        Assert.That(taskFunc, Is.Not.Null);
        Task task = taskFunc.Call(memory, 0, 0, out _);
        Assert.That(task.IsCompleted, Is.False);
        task.Wait();


        // 結果を返してくれるタスクを実行して結果が想定通りか確認する（同期完了時は out 引数から受け取れる）
        taskFunc = peripheral.GetPeripheralFunction("AddTaskFunc");
        Assert.That(taskFunc, Is.Not.Null);
        memory[2] = 123;
        memory[3] = 456;
        task = taskFunc.Call(memory, 2, 0, out SrValue addResult);
        Assert.That(task.IsCompleted, Is.True);
        task.Wait();
        Assert.That(addResult.Primitive.Int, Is.EqualTo(579));
        Assert.That(taskFunc.ConvertResult(task).Primitive.Int, Is.EqualTo(579));


        // 少しだけ待つ時の結果を返すタスクを実行して結果が想定通りか確認する（完了後にタスクから受け取る）
        taskFunc = peripheral.GetPeripheralFunction("CombWaitTaskFunc");
        Assert.That(taskFunc, Is.Not.Null);
        memory[0] = "このメッセージは、";
        memory[1] = "結合されるはずです。";
        task = taskFunc.Call(memory, 0, 0, out _);
        Assert.That(task.IsCompleted, Is.False);
        task.Wait();
        SrValue result = taskFunc.ConvertResult(task);
        Assert.That((string)result.Object!, Is.EqualTo("このメッセージは、結合されるはずです。"));
    }


    /// <summary>
    /// 少々特別な周辺機器関数の呼び出しテストをします
    /// </summary>
    [Test, Order(3)]
    public void CallSpecialPeripheralFunctionTest()
    {
        // プロセスIDが渡ってくする関数の呼び出しをするが、プロセスIDは引数リストからは渡すことはない（ここからは見えない引数として扱う）
        SrPeripheralFunction function = peripheral.GetPeripheralFunction("ProcFunc");
        Assert.That(function, Is.Not.Null);
        memory[0] = 123;
        memory[1] = 456;
        memory[2] = 0;
        memory[3] = 0;
        function.Call(memory, 0, 579, out _);


        // プライベートな関数でも属性がついている場合はアクセスが可能であることを確認する
        function = peripheral.GetPeripheralFunction("PrivateFunc");
        Assert.That(function, Is.Not.Null);
        function.Call(memory, 0, 0, out _);


        // 静的な関数でも呼び出せることを確認
        function = peripheral.GetPeripheralFunction("StaticFunc");
        Assert.That(function, Is.Not.Null);
        function.Call(memory, 0, 0, out _);
    }


    /// <summary>
    /// ユーザー定義クラスの扱う周辺機器関数の呼び出しテストをします
    /// </summary>
    [Test, Order(4)]
    public void CallMyDataClassParamFuctionTest()
    {
        // 自前の定義クラスのインスタンスを作って呼び出して更に確認もする
        SrPeripheralFunction function = peripheral.GetPeripheralFunction("MyDataFunc");
        Assert.That(function, Is.Not.Null);
        MyDataClass obj = new MyDataClass();
        obj.ParameterA = 123;
        obj.ParameterB = "Message";
        memory[0] = new SrValue() { Object = obj };
        function.Call(memory, 0, 123, out SrValue myDataResult);
        MyDataClass result = (MyDataClass)myDataResult.Object!;
        Assert.That(result.ParameterA, Is.EqualTo(12300));
        Assert.That(result.ParameterB, Is.EqualTo("ParameterB : Message"));


        // 非同期で同じ確認をする
        function = peripheral.GetPeripheralFunction("MyDataFuncAsync");
        Assert.That(function, Is.Not.Null);
        obj.ParameterA = 456;
        Task task = function.Call(memory, 0, 456, out _);
        Assert.That(task.IsCompleted, Is.False);
        task.Wait();
        result = (MyDataClass)function.ConvertResult(task).Object!;
        Assert.That(result.ParameterA, Is.EqualTo(456000));
        Assert.That(result.ParameterB, Is.EqualTo("ParameterB : Message Async"));
    }


    /// <summary>
    /// 複数プロセス相当の非同期呼び出しが混線しないことをテストします
    /// （共有フィールド経由の結果受け渡しの回帰テスト）
    /// </summary>
    [Test, Order(5)]
    public void ConcurrentAsyncCallResultSeparationTest()
    {
        SrPeripheralFunction function = peripheral.GetPeripheralFunction("PendingValueFunc");
        Assert.That(function, Is.Not.Null);


        // 2つのプロセスに相当する2回の呼び出しを、どちらも未完了のまま保持する
        Task taskA = function.Call(memory, 0, 0, out _);
        Task taskB = function.Call(memory, 0, 0, out _);
        Assert.That(taskA.IsCompleted, Is.False);
        Assert.That(taskB.IsCompleted, Is.False);


        // B → A の順で完了させても、それぞれのタスクから自分の結果を受け取れる
        peripheralInstance.CompletePendingValue(1, 222);
        peripheralInstance.CompletePendingValue(0, 111);
        Assert.That(function.ConvertResult(taskB).Primitive.Int, Is.EqualTo(222));
        Assert.That(function.ConvertResult(taskA).Primitive.Int, Is.EqualTo(111));
    }


    /// <summary>
    /// キャンセルされたタスクの結果変換が例外になることをテストします
    /// </summary>
    [Test, Order(6)]
    public void CanceledTaskResultTest()
    {
        SrPeripheralFunction function = peripheral.GetPeripheralFunction("PendingValueFunc");
        Assert.That(function, Is.Not.Null);


        Task task = function.Call(memory, 0, 0, out _);
        peripheralInstance.CancelPendingValue();
        Assert.That(task.IsCanceled, Is.True);
        Assert.Throws<AggregateException>(() => function.ConvertResult(task));
    }
}



/// <summary>
/// ユーザー定義クラスを拡張関数経由からでも渡せたり返せたり出来るかを確認するためのクラスです
/// </summary>
public class MyDataClass
{
    /// <summary>
    /// パラメータAプロパティ
    /// </summary>
    public int ParameterA { get; set; }


    /// <summary>
    /// パラメータBプロパティ
    /// </summary>
    public string ParameterB { get; set; } = "";
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
    // 外部から完了・キャンセルを制御できる保留タスクのリスト
    private readonly List<TaskCompletionSource<int>> pendingValueSources = new List<TaskCompletionSource<int>>();


    /// <summary>
    /// 外部から完了させるまで保留し続けるタスクを返す周辺機器関数の定義です
    /// </summary>
    /// <returns>外部から完了させるまで待機するタスクを返します</returns>
    [SrHostFunction("PendingValueFunc")]
    public Task<int> PendingValueFunction()
    {
        var source = new TaskCompletionSource<int>();
        pendingValueSources.Add(source);
        return source.Task;
    }


    /// <summary>
    /// 指定されたインデックスの保留タスクを結果付きで完了させます
    /// </summary>
    /// <param name="index">完了させる保留タスクのインデックス</param>
    /// <param name="value">タスクの結果として設定する値</param>
    public void CompletePendingValue(int index, int value)
    {
        pendingValueSources[index].SetResult(value);
    }


    /// <summary>
    /// 最後に生成された保留タスクをキャンセルします
    /// </summary>
    public void CancelPendingValue()
    {
        pendingValueSources[pendingValueSources.Count - 1].SetCanceled();
    }


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
    public int ProcessIDFunction(int a, [SrProcessID] int processID, int b)
    {
        // プロセスIDを出力しつつ a + b の結果が processID になると予想する
        Console.WriteLine($"ProcessID : {processID}");
        Assert.That(processID, Is.EqualTo(a + b));
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


    /// <summary>
    /// ユーザー定義クラスを扱えるかの確認関数の定義です
    /// </summary>
    /// <param name="inArg">引数１</param>
    /// <returns>ユーザー定義クラスのインスタンスを返します</returns>
    [SrHostFunction("MyDataFunc")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("コードの品質", "IDE0051:使用されていないプライベート メンバーを削除する", Justification = "<保留中>")]
    public MyDataClass MyDataFunc(MyDataClass inArg)
    {
        // 新しいインスタンスを作って返す
        MyDataClass newData = new MyDataClass();
        newData.ParameterA = inArg.ParameterA * 100;
        newData.ParameterB = $"ParameterB : {inArg.ParameterB}";
        return newData;
    }


    /// <summary>
    /// タスクでもユーザー定義クラスを扱えるかの確認関数の定義です
    /// </summary>
    /// <param name="inArg">引数１</param>
    /// <returns>ユーザー定義クラスのインスタンスを返します</returns>
    [SrHostFunction("MyDataFuncAsync")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("コードの品質", "IDE0051:使用されていないプライベート メンバーを削除する", Justification = "<保留中>")]
    public async Task<MyDataClass> MyDataFuncAsync(MyDataClass inArg)
    {
        // 意図的に長く待つ
        await Task.Delay(1000);


        // 新しいインスタンスを作って返す
        MyDataClass newData = new MyDataClass();
        newData.ParameterA = inArg.ParameterA * 1000;
        newData.ParameterB = $"ParameterB : {inArg.ParameterB} Async";
        return newData;
    }
}
