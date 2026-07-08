// zlib License
//
// Copyright (c) 2019 - 2026 Sinoa
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

using SnowRabbit.Compiler;
using SnowRabbit.Compiler.IO;
using SnowRabbit.Compiler.Reporter;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;
using SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral;

namespace SnowRabbit.Tests;

/// <summary>
/// スクリプトのコンパイルから仮想マシンでの実行までを通しで検証するテストクラスです
/// </summary>
[TestFixture]
public class SrEndToEndExecutionTest
{
    /// <summary>
    /// 実行が停止するまでに許容する Run() 呼び出し回数の上限
    /// </summary>
    private const int MaxRunCount = 100000;

    /// <summary>
    /// インメモリスクリプトストレージの実装です
    /// </summary>
    private class MemoryScriptStorage : ISrScriptStorage
    {
        private readonly string scriptContent;
        private readonly string scriptPath;

        public MemoryScriptStorage(string path, string content)
        {
            scriptPath = path;
            scriptContent = content;
        }

        public TextReader OpenRead(string path)
        {
            if (path == scriptPath)
            {
                return new StringReader(scriptContent);
            }
            throw new FileNotFoundException($"Script not found: {path}");
        }
    }

    /// <summary>
    /// コンパイルレポートを無視するプリンタです
    /// </summary>
    private class NullReportPrinter : ISrCompileReportPrinter
    {
        public void PrintReport(CompileReport report) { }
    }

    /// <summary>
    /// コンパイル済みバイナリをメモリから提供するストレージです
    /// </summary>
    private class MemoryBinaryStorage : SrvmStorage
    {
        private readonly byte[] binaryData;

        public MemoryBinaryStorage(byte[] binaryData)
        {
            this.binaryData = binaryData;
        }

        public override Stream Open(string path)
        {
            return new MemoryStream(binaryData, false);
        }
    }

    /// <summary>
    /// テスト用のストレージとペリフェラルを組み込むマシンパーツファクトリです
    /// </summary>
    private class TestMachinePartsFactory : SrvmDefaultMachinePartsFactory
    {
        private readonly byte[] binaryData;
        private readonly TestPeripheral peripheral;

        public TestMachinePartsFactory(byte[] binaryData, TestPeripheral peripheral)
        {
            this.binaryData = binaryData;
            this.peripheral = peripheral;
        }

        public override SrvmStorage CreateStorage()
        {
            return new MemoryBinaryStorage(binaryData);
        }

        public override SrvmFirmware CreateFirmware()
        {
            SrvmFirmware firmware = base.CreateFirmware();
            firmware.AttachPeripheral(peripheral);
            return firmware;
        }
    }

    /// <summary>
    /// スクリプトからの出力を記録するテスト用ペリフェラルです
    /// </summary>
    [SrPeripheral("Test")]
    private class TestPeripheral
    {
        public List<string> Outputs { get; } = new List<string>();

        [SrHostFunction("Write")]
        public void Write(string text)
        {
            Outputs.Add(text);
        }

        [SrHostFunction("WriteInt")]
        public void WriteInt(int value)
        {
            Outputs.Add(value.ToString());
        }

        [SrHostFunction("WriteBool")]
        public void WriteBool(bool value)
        {
            Outputs.Add(value.ToString());
        }

        public List<TaskCompletionSource<string>> ValueWaiters { get; } = new List<TaskCompletionSource<string>>();

        [SrHostFunction("WaitValue")]
        public Task<string> WaitValue()
        {
            var source = new TaskCompletionSource<string>();
            ValueWaiters.Add(source);
            return source.Task;
        }
    }

    /// <summary>
    /// スクリプトをコンパイルしてバイナリを取得します
    /// </summary>
    /// <param name="script">コンパイルするスクリプト</param>
    /// <returns>コンパイルされた実行バイナリ</returns>
    private static byte[] CompileScript(string script)
    {
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        using (MemoryStream outputStream = new MemoryStream())
        {
            compiler.Compile("test.srs", outputStream);
            return outputStream.ToArray();
        }
    }

    /// <summary>
    /// スクリプトをコンパイルして仮想マシンで停止まで実行し、ペリフェラルへの出力を返します
    /// </summary>
    /// <param name="script">実行するスクリプト</param>
    /// <returns>スクリプトの出力を記録したテストペリフェラル</returns>
    private static TestPeripheral CompileAndRun(string script)
    {
        byte[] binaryData = CompileScript(script);
        TestPeripheral peripheral = new TestPeripheral();

        using (SrvmMachine vm = new SrvmMachine(new TestMachinePartsFactory(binaryData, peripheral)))
        {
            SrProcess process = vm.CreateProcess("test.bin");
            int runCount = 0;
            while (process.ProcessState != SrProcessStatus.Stopped && process.ProcessState != SrProcessStatus.Panic)
            {
                process.Run();
                if (++runCount > MaxRunCount)
                {
                    Assert.Fail("スクリプトが規定回数以内に停止しませんでした");
                }
            }

            Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Stopped));
            process.Dispose();
        }

        return peripheral;
    }

    /// <summary>
    /// 文字列リテラルがグローバル初期化・ローカル初期化・直接引数のいずれでも
    /// 実行時に正しく渡されることをテストします（文字列シンボルのアドレス解決の回帰テスト）
    /// </summary>
    [Test]
    public void StringLiteralExecutionTest()
    {
        string script = @"
using Write = void Test.Write(string);

global string g_Message = ""global string"";

function void main()
    local string message = ""local string"";
    Write(g_Message);
    Write(message);
    Write(""direct literal"");
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "global string", "local string", "direct literal" }));
    }

    /// <summary>
    /// 剰余演算子を使用した FizzBuzz が正しく実行されることをテストします
    /// （言語リファレンスのサンプルコードと同じ構造の実行検証）
    /// </summary>
    [Test]
    public void ModuloFizzBuzzExecutionTest()
    {
        string script = @"
using Write = void Test.Write(string);
using WriteInt = void Test.WriteInt(int);

function void main()
    local int i = 0;
    for (i = 1; i <= 15; i = i + 1)
        if (i % 15 == 0)
            Write(""FizzBuzz"");
        else if (i % 3 == 0)
            Write(""Fizz"");
        else if (i % 5 == 0)
            Write(""Buzz"");
        else
            WriteInt(i);
        end
    end
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        string[] expected =
        {
            "1", "2", "Fizz", "4", "Buzz", "Fizz", "7", "8", "Fizz", "Buzz", "11", "Fizz", "13", "14", "FizzBuzz",
        };
        Assert.That(peripheral.Outputs, Is.EqualTo(expected));
    }

    /// <summary>
    /// 定数・グローバル変数・関数呼び出しを含む整数演算が正しく実行されることをテストします
    /// </summary>
    [Test]
    public void IntegerArithmeticExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);

#const BASE_VALUE 100

global int g_Total = 0;

function int Add(int a, int b)
    return a + b;
end

function void main()
    g_Total = Add(BASE_VALUE, 23);
    WriteInt(g_Total);
    WriteInt(g_Total % 10);
    WriteInt(g_Total / 10 * 10);
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "123", "3", "120" }));
    }

    /// <summary>
    /// グローバル変数の初期化リテラルが実行時に反映されることをテストします（初期値破棄の回帰テスト）
    /// </summary>
    [Test]
    public void GlobalVariableInitializerExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);

global int g_Value = 42;
global bool g_Flag = true;
global int g_NoInit;

function void main()
    WriteInt(g_Value);
    if (g_Flag)
        WriteInt(1);
    else
        WriteInt(0);
    end
    WriteInt(g_NoInit);
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "42", "1", "0" }));
    }

    /// <summary>
    /// 最終 else の無い else if チェインが正しく分岐することをテストします（未パッチ分岐の回帰テスト）
    /// </summary>
    [Test]
    public void ElseIfWithoutFinalElseExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);

function void Check(int x)
    if (x == 1)
        WriteInt(100);
    else if (x == 2)
        WriteInt(200);
    end
    WriteInt(999);
end

function void main()
    Check(1);
    Check(2);
    Check(3);
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "100", "999", "200", "999", "999" }));
    }

    /// <summary>
    /// 論理否定演算子が真偽値を正しく反転することをテストします（算術否定実装の回帰テスト）
    /// </summary>
    [Test]
    public void LogicalNotExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);

global bool g_True = true;
global bool g_False = false;

function void main()
    if (!g_True)
        WriteInt(1);
    else
        WriteInt(2);
    end
    if (!g_False)
        WriteInt(3);
    else
        WriteInt(4);
    end
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "2", "3" }));
    }

    /// <summary>
    /// 条件式を省略した for 文が無限ループとして動作し break で脱出できることをテストします
    /// </summary>
    [Test]
    public void ForWithoutConditionExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);

function void main()
    local int count = 0;
    for (;;)
        count += 1;
        WriteInt(count);
        if (count >= 3)
            break;
        end
    end
    WriteInt(100);
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "1", "2", "3", "100" }));
    }

    /// <summary>
    /// 式の中に関数呼び出しが含まれても評価途中の値が破壊されないことをテストします
    /// （レジスタプールリセットによる値破壊の回帰テスト）
    /// </summary>
    [Test]
    public void FunctionCallInExpressionExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);

function int Identity(int value)
    return value;
end

function int Times10(int value)
    return value * 10;
end

function void main()
    local int a = 100;
    local int b = 1;
    WriteInt(a + Identity(b));
    WriteInt(Identity(b) + a);
    WriteInt(Times10(1) + Times10(2));
    WriteInt(Times10(1) + Times10(2) * Times10(3));
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "101", "101", "30", "610" }));
    }

    /// <summary>
    /// ネストした引数の関数呼び出しと複数引数の関数呼び出しが正しく評価されることをテストします
    /// </summary>
    [Test]
    public void NestedFunctionCallArgumentExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);

function int Times10(int value)
    return value * 10;
end

function int AddBoth(int a, int b)
    return a + b;
end

function void main()
    WriteInt(Times10(Times10(1) + 2) * 3);
    WriteInt(AddBoth(Times10(1), Times10(2)));
    WriteInt(AddBoth(AddBoth(1, 2), AddBoth(3, 4)));
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "360", "30", "10" }));
    }

    /// <summary>
    /// 呼び出し先が多数のレジスタを使用しても、呼び出し元の評価途中の値が保存されることをテストします
    /// （UsedRegisterSet 過少申告によるレジスタ退避漏れの回帰テスト）
    /// </summary>
    [Test]
    public void RegisterPreservationAcrossCallExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);

function int Sum(int a, int b)
    local int t1 = a * 2;
    local int t2 = b * 3;
    local int t3 = t1 + t2;
    local int t4 = t3 - a;
    local int t5 = t4 + b;
    return t1 + t2 + t3 + t4 + t5 - (t1 + t2 + t3 + t4 + t5) + a + b;
end

function void main()
    local int a = 5;
    local int b = 7;
    local int c = 11;
    WriteInt((a * 2 + b * 3) + Sum(a + b, a * b) + c * 5);
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        // (10 + 21) + (12 + 35) + 55 = 133
        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "133" }));
    }

    /// <summary>
    /// 条件式・更新式・return・代入の中の関数呼び出しが正しく動作することをテストします
    /// </summary>
    [Test]
    public void FunctionCallInControlPositionsExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);

global int g_Calls = 0;

function int Zero()
    return 0;
end

function int One()
    g_Calls += 1;
    return 1;
end

function int Wrap(int value)
    return value + 1;
end

function void main()
    if (Zero() + 1 == 1)
        WriteInt(1);
    end

    local int x = 0;
    while (x + One() < 5)
        x = x + 1;
    end
    WriteInt(x);

    local int i = 0;
    local int total = 0;
    for (i = Zero(); i < Wrap(2); i = i + One())
        total += i;
    end
    WriteInt(total);

    WriteInt(Wrap(Zero()) + 1);
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "1", "4", "3", "2" }));
    }

    /// <summary>
    /// 整数から実数への暗黙昇格が代入・初期化・引数・戻り値で機能することをテストします
    /// </summary>
    [Test]
    public void ImplicitIntToNumberPromotionExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);

function number Half(number value)
    return value / 2;
end

function number GetOne()
    return 1;
end

function void main()
    local number n = 1;
    n += 1;
    if (n == 2.0)
        WriteInt(1);
    end
    if (Half(5) == 2.5)
        WriteInt(2);
    end
    if (GetOne() == 1.0)
        WriteInt(3);
    end
    local string s = null;
    if (s == null)
        WriteInt(4);
    end
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "1", "2", "3", "4" }));
    }

    /// <summary>
    /// 比較・条件演算の結果が真偽値として bool 引数のホスト関数へ渡せることをテストします
    /// （比較演算の結果型の回帰テスト）
    /// </summary>
    [Test]
    public void ComparisonResultAsBooleanExecutionTest()
    {
        string script = @"
using WriteBool = void Test.WriteBool(bool);

function void main()
    WriteBool(1 == 1);
    WriteBool(1 != 1);
    WriteBool(2 > 1 && 1 < 2);
    WriteBool(1 > 2 || 2 < 1);
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "True", "False", "True", "False" }));
    }

    /// <summary>
    /// 条件論理演算子（&amp;&amp; / ||）が短絡評価されることをテストします
    /// （左辺で結果が確定した場合に右辺の副作用が発生しないことの検証）
    /// </summary>
    [Test]
    public void ShortCircuitEvaluationExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);
using WriteBool = void Test.WriteBool(bool);

global int g_Calls = 0;

function bool TrueWithCount()
    g_Calls += 1;
    return true;
end

function bool FalseWithCount()
    g_Calls += 1;
    return false;
end

function void main()
    // && の左辺が偽なら右辺は評価されない
    g_Calls = 0;
    WriteBool(FalseWithCount() && TrueWithCount());
    WriteInt(g_Calls);

    // || の左辺が真なら右辺は評価されない
    g_Calls = 0;
    WriteBool(TrueWithCount() || TrueWithCount());
    WriteInt(g_Calls);

    // 左辺で確定しない場合は右辺も評価される
    g_Calls = 0;
    WriteBool(TrueWithCount() && FalseWithCount());
    WriteInt(g_Calls);

    // 真偽値の全組み合わせ
    WriteBool(true && true);
    WriteBool(true && false);
    WriteBool(false || true);
    WriteBool(false || false);

    // 比較式との混在
    local int count = 3;
    local bool flag = true;
    if (flag && count > 0)
        WriteInt(100);
    end
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[]
        {
            "False", "1",
            "True", "1",
            "False", "2",
            "True", "False", "True", "False",
            "100",
        }));
    }

    /// <summary>
    /// 後置インクリメント/デクリメントが変更前の値を返し、変数を正しく更新することをテストします
    /// </summary>
    [Test]
    public void PostfixIncrementDecrementExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);

global int g_Value = 10;

function void main()
    local int a = 1;
    local int b = a++;
    WriteInt(a);
    WriteInt(b);

    local int c = a--;
    WriteInt(a);
    WriteInt(c);

    local int d = ++a;
    WriteInt(a);
    WriteInt(d);

    // for 文の更新式として使用
    local int i = 0;
    local int total = 0;
    for (i = 0; i < 3; i++)
        total += i;
    end
    WriteInt(total);

    // 引数の中で使用（変更前の値が渡される）
    WriteInt(a++);
    WriteInt(a);

    // グローバル変数への後置演算
    WriteInt(g_Value--);
    WriteInt(g_Value);
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[]
        {
            "2", "1",
            "1", "2",
            "2", "2",
            "3",
            "2", "3",
            "10", "9",
        }));
    }

    /// <summary>
    /// 同一マシン上の2つのプロセスが同じ非同期ホスト関数で中断しても、
    /// それぞれが自分の呼び出しの結果を受け取ることをテストします（非同期結果混線の回帰テスト）
    /// </summary>
    [Test]
    public void TwoProcessAsyncResultSeparationTest()
    {
        string script = @"
using Write = void Test.Write(string);
using WaitValue = string Test.WaitValue();

function void main()
    Write(WaitValue());
end
";
        byte[] binaryData = CompileScript(script);
        TestPeripheral peripheral = new TestPeripheral();

        using (SrvmMachine vm = new SrvmMachine(new TestMachinePartsFactory(binaryData, peripheral)))
        {
            SrProcess process1 = vm.CreateProcess("test.bin");
            SrProcess process2 = vm.CreateProcess("test.bin");

            // 両プロセスとも非同期ホスト関数で中断する
            process1.Run();
            process2.Run();
            Assert.That(process1.ProcessState, Is.EqualTo(SrProcessStatus.Suspended));
            Assert.That(process2.ProcessState, Is.EqualTo(SrProcessStatus.Suspended));
            Assert.That(peripheral.ValueWaiters, Has.Count.EqualTo(2));

            // 後に中断した process2 側を先に完了・再開させる
            peripheral.ValueWaiters[1].SetResult("second");
            process2.Run();
            Assert.That(process2.ProcessState, Is.EqualTo(SrProcessStatus.Stopped));

            // process1 は自分の呼び出しの結果を受け取る（process2 の結果に上書きされない）
            peripheral.ValueWaiters[0].SetResult("first");
            process1.Run();
            Assert.That(process1.ProcessState, Is.EqualTo(SrProcessStatus.Stopped));

            process1.Dispose();
            process2.Dispose();
        }

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "second", "first" }));
    }

    /// <summary>
    /// キャンセルされたタスクを待つプロセスがパニック状態へ遷移することをテストします
    /// （従来は Suspended のまま固まっていた問題の回帰テスト）
    /// </summary>
    [Test]
    public void CanceledTaskPanicsProcessTest()
    {
        string script = @"
using Write = void Test.Write(string);
using WaitValue = string Test.WaitValue();

function void main()
    Write(WaitValue());
end
";
        byte[] binaryData = CompileScript(script);
        TestPeripheral peripheral = new TestPeripheral();

        using (SrvmMachine vm = new SrvmMachine(new TestMachinePartsFactory(binaryData, peripheral)))
        {
            SrProcess process = vm.CreateProcess("test.bin");
            process.Run();
            Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Suspended));

            // タスクをキャンセルすると、次の Run でパニックへ遷移して例外が通知される
            peripheral.ValueWaiters[0].SetCanceled();
            Assert.Throws<TaskCanceledException>(() => process.Run());
            Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));

            process.Dispose();
        }
    }

    /// <summary>
    /// 文字列連結が連鎖・変数・複合代入・引数・戻り値・グローバル代入で正しく動作することをテストします
    /// </summary>
    [Test]
    public void StringConcatenationExecutionTest()
    {
        string script = @"
using Write = void Test.Write(string);

global string g_Message = """";

function string Decorate(string text)
    return ""["" + text + ""]"";
end

function void main()
    local string name = ""World"";
    local string message = ""Hello, "" + name + ""!"";
    Write(message);

    message += ""!!"";
    Write(message);

    Write(""a"" + ""b"" + ""c"");
    Write(Decorate(""deco""));

    g_Message = ""global"" + "" "" + ""concat"";
    Write(g_Message);

    local string empty = """" + """";
    Write(""x"" + empty + ""y"");
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[]
        {
            "Hello, World!",
            "Hello, World!!!",
            "abc",
            "[deco]",
            "global concat",
            "xy",
        }));
    }

    /// <summary>
    /// 言語リファレンスのフィボナッチ数列サンプルが正しく実行されることをテストします
    /// （再帰呼び出しとローカル変数への関数戻り値格納の検証）
    /// </summary>
    [Test]
    public void FibonacciExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);

function int Fibonacci(int n)
    if (n <= 1)
        return n;
    end
    local int previous1 = Fibonacci(n - 1);
    local int previous2 = Fibonacci(n - 2);
    return previous1 + previous2;
end

function void main()
    local int i = 0;
    for (i = 0; i < 10; i = i + 1)
        local int fib = Fibonacci(i);
        WriteInt(fib);
    end
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "0", "1", "1", "2", "3", "5", "8", "13", "21", "34" }));
    }

    /// <summary>
    /// 関数呼び出しの戻り値を return する場合と while の条件にする場合が
    /// 正しく動作することをテストします（rax 受け渡し規約の回帰テスト）
    /// </summary>
    [Test]
    public void FunctionCallResultForwardingExecutionTest()
    {
        string script = @"
using WriteInt = void Test.WriteInt(int);

global int g_Countdown = 3;

function int GetAnswer()
    return 42;
end

function int Forward()
    return GetAnswer();
end

function int Countdown()
    g_Countdown -= 1;
    if (g_Countdown < 0)
        return 0;
    end
    return 1;
end

function void main()
    WriteInt(Forward());
    while (Countdown())
        WriteInt(g_Countdown);
    end
end
";
        TestPeripheral peripheral = CompileAndRun(script);

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "42", "2", "1", "0" }));
    }
}
