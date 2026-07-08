// zlib License
//
// Copyright (c) 2026 Sinoa
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
using SnowRabbit.Compiler.Assembler;
using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.Compiler.IO;
using SnowRabbit.Compiler.Parser.SyntaxErrors;
using SnowRabbit.Compiler.Reporter;
using SnowRabbit.IO;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;
using SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral;

namespace SnowRabbit.Tests;

/// <summary>
/// オブジェクトファイル (SROB) のコンパイルと #link によるリンクのテストクラスです
/// </summary>
[TestFixture]
public class SrLinkerTest
{
    /// <summary>
    /// コンパイルレポートを無視するプリンタです
    /// </summary>
    private class NullReportPrinter : ISrCompileReportPrinter
    {
        public void PrintReport(CompileReport report) { }
    }

    /// <summary>
    /// インメモリのオブジェクトファイルストレージです
    /// </summary>
    private class MemoryObjectStorage : ISrObjectStorage
    {
        private readonly Dictionary<string, byte[]> objects = new Dictionary<string, byte[]>();

        public void SetObject(string path, byte[] data)
        {
            objects[path] = data;
        }

        public Stream OpenRead(string path)
        {
            return objects.TryGetValue(path, out var data) ? new MemoryStream(data, false) : null!;
        }
    }

    /// <summary>
    /// リンクテスト用の出力記録ペリフェラルです
    /// </summary>
    [SrPeripheral("Test")]
    private class LinkTestPeripheral
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
    /// リンクテスト用のマシンパーツファクトリです
    /// </summary>
    private class LinkTestMachinePartsFactory : SrvmDefaultMachinePartsFactory
    {
        private readonly byte[] binaryData;
        private readonly LinkTestPeripheral peripheral;

        public LinkTestMachinePartsFactory(byte[] binaryData, LinkTestPeripheral peripheral)
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
    /// 指定されたスクリプトをオブジェクトファイル (SROB) としてコンパイルします
    /// </summary>
    private static byte[] CompileObjectBinary(string script, params (string Path, byte[] Data)[] linkObjects)
    {
        SrStringScriptStorage storage = new SrStringScriptStorage();
        storage.SetScript("library.srs", script);
        MemoryObjectStorage objectStorage = new MemoryObjectStorage();
        foreach (var (path, data) in linkObjects)
        {
            objectStorage.SetObject(path, data);
        }
        using SrCompiler compiler = new SrCompiler(storage, objectStorage, new NullReportPrinter());

        using MemoryStream stream = new MemoryStream();
        compiler.CompileObject("library.srs", stream);
        return stream.ToArray();
    }

    /// <summary>
    /// メインスクリプトをオブジェクトとリンクしてコンパイルし、VMで停止まで実行します
    /// </summary>
    private static LinkTestPeripheral CompileAndRunWithLink(IDictionary<string, string> scripts, params (string Path, byte[] Data)[] linkObjects)
    {
        SrStringScriptStorage scriptStorage = new SrStringScriptStorage();
        foreach (var pair in scripts)
        {
            scriptStorage.SetScript(pair.Key, pair.Value);
        }
        MemoryObjectStorage objectStorage = new MemoryObjectStorage();
        foreach (var (path, data) in linkObjects)
        {
            objectStorage.SetObject(path, data);
        }

        byte[] binaryData;
        using (SrCompiler compiler = new SrCompiler(scriptStorage, objectStorage, new NullReportPrinter()))
        {
            using MemoryStream stream = new MemoryStream();
            compiler.Compile("main.srs", stream);
            binaryData = stream.ToArray();
        }

        LinkTestPeripheral peripheral = new LinkTestPeripheral();
        using (SrvmMachine vm = new SrvmMachine(new LinkTestMachinePartsFactory(binaryData, peripheral)))
        {
            SrProcess process = vm.CreateProcess("main.bin");
            int runCount = 0;
            while (process.ProcessState != SrProcessStatus.Stopped && process.ProcessState != SrProcessStatus.Panic)
            {
                process.Run();
                if (++runCount > 100000)
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
    /// メインスクリプト1つとオブジェクトをリンクしてコンパイル・実行します
    /// </summary>
    private static LinkTestPeripheral CompileAndRunWithLink(string mainScript, params (string Path, byte[] Data)[] linkObjects)
    {
        return CompileAndRunWithLink(new Dictionary<string, string> { { "main.srs", mainScript } }, linkObjects);
    }

    /// <summary>
    /// main の無いスクリプトがオブジェクトとしてコンパイルでき、
    /// 通常コンパイルでは main 未定義エラーになることをテストします
    /// </summary>
    [Test]
    public void CompileObjectWithoutMainTest()
    {
        string script = @"
function int Add(int a, int b)
    return a + b;
end
";
        // オブジェクトモードでは成功する
        byte[] objectBinary = CompileObjectBinary(script);
        Assert.That(objectBinary, Is.Not.Empty);

        // 通常コンパイルでは main が無いためエラーになる
        SrStringScriptStorage storage = new SrStringScriptStorage();
        storage.SetScript("library.srs", script);
        using SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());
        Assert.Throws<SrSyntaxErrorException>(() =>
        {
            using MemoryStream stream = new MemoryStream();
            compiler.Compile("library.srs", stream);
        });
    }

    /// <summary>
    /// オブジェクトファイルの内容が書き込み時の情報を保持していることをテストします（ラウンドトリップ）
    /// </summary>
    [Test]
    public void ObjectDataRoundTripTest()
    {
        string script = @"
using Write = void Test.Write(string);

#const LIB_VERSION 2

global int g_LibCounter = 42;
global string g_LibMessage = ""from library"";

function int Twice(int value)
    local int result = value * 2;
    return result;
end

function void Announce()
    local int i = 0;
    while (i < 1)
        i += 1;
    end
    Write(""announce"");
end
";
        byte[] objectBinary = CompileObjectBinary(script);

        SrObjectData objectData;
        using (SrObjectDataReader reader = new SrObjectDataReader(new MemoryStream(objectBinary, false)))
        {
            objectData = reader.Read();
        }

        // 関数コード: Twice / Announce の2つ（___init は含まれない）
        Assert.That(objectData.FunctionCodes.Select(x => x.FunctionName),
            Is.EquivalentTo(new[] { "Twice", "Announce" }));
        Assert.That(objectData.FunctionCodes.All(x => x.Codes.Count > 0), Is.True);
        Assert.That(objectData.FunctionCodes.SelectMany(x => x.Codes).Any(x => x.UnresolvedAddress), Is.True);

        // シンボル: 関数・ペリフェラル関数・グローバル変数・定数・ラベルが含まれる
        Assert.That(objectData.Symbols.Any(x => x.Kind == SrSymbolKind.ScriptFunction && x.Name == "Twice" && x.Type == SrRuntimeType.Integer), Is.True);
        Assert.That(objectData.Symbols.Any(x => x.Kind == SrSymbolKind.PeripheralFunction && x.Name == "Write" && x.PeripheralName == "Test"), Is.True);
        Assert.That(objectData.Symbols.Any(x => x.Kind == SrSymbolKind.Constant && x.Name == "LIB_VERSION" && x.LiteralInteger == 2), Is.True);
        Assert.That(objectData.Symbols.Any(x => x.Kind == SrSymbolKind.Label), Is.True);

        // グローバル変数は初期化リテラルを保持している
        SrObjectSymbolData counter = objectData.Symbols.Single(x => x.Kind == SrSymbolKind.GlobalVariable && x.Name == "g_LibCounter");
        Assert.That(counter.HasLiteral, Is.True);
        Assert.That(counter.LiteralInteger, Is.EqualTo(42));

        // Twice のパラメータとローカル変数が保持されている
        SrObjectSymbolData twice = objectData.Symbols.Single(x => x.Kind == SrSymbolKind.ScriptFunction && x.Name == "Twice");
        Assert.That(twice.Parameters.Select(x => x.Name), Is.EqualTo(new[] { "value" }));
        Assert.That(twice.LocalVariables.Select(x => x.Name), Is.EqualTo(new[] { "result" }));
        Assert.That(twice.UsedRegisters, Is.Not.Empty);

        // 文字列表にはペリフェラル名とコード中の文字列リテラルが含まれる
        // （グローバル初期化子の文字列はリテラルとしてシンボル側に保持され、リンク時の ___init 生成で解決される）
        Assert.That(objectData.Strings.Select(x => x.Text), Does.Contain("Test"));
        Assert.That(objectData.Strings.Select(x => x.Text), Does.Contain("announce"));
        SrObjectSymbolData message = objectData.Symbols.Single(x => x.Kind == SrSymbolKind.GlobalVariable && x.Name == "g_LibMessage");
        Assert.That(message.HasLiteral, Is.True);
        Assert.That(message.LiteralText, Is.EqualTo("from library"));
    }

    /// <summary>
    /// 実行形式 (SROF) をオブジェクトとして読み込もうとすると専用メッセージの例外になることをテストします
    /// </summary>
    [Test]
    public void ObjectReaderRejectsExecutableTest()
    {
        string script = @"
function void main()
end
";
        SrStringScriptStorage storage = new SrStringScriptStorage();
        storage.SetScript("main.srs", script);
        using SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());
        using MemoryStream executableStream = new MemoryStream();
        compiler.Compile("main.srs", executableStream);

        using SrObjectDataReader reader = new SrObjectDataReader(new MemoryStream(executableStream.ToArray(), false));
        var exception = Assert.Throws<SrMalformedObjectDataException>(() => reader.Read());
        Assert.That(exception!.Message, Does.Contain("SROF"));
    }

    /// <summary>
    /// サポート外バージョンのオブジェクトファイルが例外になることをテストします
    /// </summary>
    [Test]
    public void ObjectVersionMismatchTest()
    {
        using MemoryStream crafted = new MemoryStream();
        using (SrBinaryIO io = new SrBinaryIO(crafted, true))
        {
            io.Write(SrObjectData.MagicNumber);
            io.Write(999); // 未来のバージョン
        }
        crafted.Position = 0;

        using SrObjectDataReader reader = new SrObjectDataReader(crafted);
        Assert.Throws<SrMalformedObjectDataException>(() => reader.Read());
    }

    /// <summary>
    /// 途中で切り詰められたオブジェクトファイルが例外になることをテストします
    /// </summary>
    [Test]
    public void TruncatedObjectTest()
    {
        byte[] objectBinary = CompileObjectBinary(@"
function int One()
    return 1;
end
");
        byte[] truncated = new byte[objectBinary.Length / 2];
        Array.Copy(objectBinary, truncated, truncated.Length);

        using SrObjectDataReader reader = new SrObjectDataReader(new MemoryStream(truncated, false));
        Assert.Throws<SrMalformedObjectDataException>(() => reader.Read());
    }

    /// <summary>
    /// リンクしたオブジェクトの関数と定数をメインスクリプトから使用できることをテストします
    /// </summary>
    [Test]
    public void LinkedFunctionAndConstantExecutionTest()
    {
        byte[] library = CompileObjectBinary(@"
#const LIB_BASE 100

function int Add(int a, int b)
    return a + b;
end
");
        LinkTestPeripheral peripheral = CompileAndRunWithLink(@"
#link ""lib.sro""

using WriteInt = void Test.WriteInt(int);

function void main()
    WriteInt(Add(1, 2));
    WriteInt(LIB_BASE);
    WriteInt(Add(LIB_BASE, 23));
end
", ("lib.sro", library));

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "3", "100", "123" }));
    }

    /// <summary>
    /// リンクしたオブジェクトのグローバル変数が初期化子込みで動作することをテストします
    /// （リンク側のグローバル初期化がメインの ___init に取り込まれることの検証）
    /// </summary>
    [Test]
    public void LinkedGlobalInitializationExecutionTest()
    {
        byte[] library = CompileObjectBinary(@"
global int g_LibCounter = 42;
global string g_LibMessage = ""from library"";

function int NextCount()
    g_LibCounter += 1;
    return g_LibCounter;
end
");
        LinkTestPeripheral peripheral = CompileAndRunWithLink(@"
#link ""lib.sro""

using Write = void Test.Write(string);
using WriteInt = void Test.WriteInt(int);

function void main()
    WriteInt(g_LibCounter);
    Write(g_LibMessage);
    WriteInt(NextCount());
    WriteInt(g_LibCounter);
end
", ("lib.sro", library));

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "42", "from library", "43", "43" }));
    }

    /// <summary>
    /// リンクしたオブジェクトのペリフェラル using が動作し、
    /// メイン側の同一シグネチャの再宣言も許容されることをテストします
    /// </summary>
    [Test]
    public void LinkedPeripheralUsingExecutionTest()
    {
        byte[] library = CompileObjectBinary(@"
using Write = void Test.Write(string);

function void Announce(string message)
    Write(""[lib] "" + message);
end
");
        LinkTestPeripheral peripheral = CompileAndRunWithLink(@"
#link ""lib.sro""

using Write = void Test.Write(string);

function void main()
    Announce(""hello"");
    Write(""from main"");
end
", ("lib.sro", library));

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "[lib] hello", "from main" }));
    }

    /// <summary>
    /// 複数オブジェクトのリンクと、オブジェクト作成時の #link（静的取り込み）が動作することをテストします
    /// </summary>
    [Test]
    public void LinkedMultipleAndTransitiveObjectsExecutionTest()
    {
        byte[] libraryA = CompileObjectBinary(@"
function int BaseValue()
    return 10;
end
");
        // libraryB は libraryA を静的に取り込む（transitive link）
        byte[] libraryB = CompileObjectBinary(@"
#link ""libA.sro""

function int DoubledBase()
    return BaseValue() * 2;
end
", ("libA.sro", libraryA));

        byte[] libraryC = CompileObjectBinary(@"
using WriteInt = void Test.WriteInt(int);

function void Report(int value)
    WriteInt(value + 1000);
end
");
        LinkTestPeripheral peripheral = CompileAndRunWithLink(@"
#link ""libB.sro""
#link ""libC.sro""

using WriteInt = void Test.WriteInt(int);

function void main()
    WriteInt(BaseValue());
    WriteInt(DoubledBase());
    Report(DoubledBase());
end
", ("libB.sro", libraryB), ("libC.sro", libraryC));

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "10", "20", "1020" }));
    }

    /// <summary>
    /// #compile したスクリプト内の #link が処理され、同一パスの重複リンクがスキップされることをテストします
    /// </summary>
    [Test]
    public void LinkInsideCompiledScriptExecutionTest()
    {
        byte[] library = CompileObjectBinary(@"
function int Triple(int value)
    return value * 3;
end
");
        var scripts = new Dictionary<string, string>
        {
            ["main.srs"] = @"
#link ""lib.sro""
#compile ""sub.srs""

using WriteInt = void Test.WriteInt(int);

function void main()
    WriteInt(Triple(3));
    WriteInt(FromSub());
end
",
            ["sub.srs"] = @"
#link ""lib.sro""

function int FromSub()
    return Triple(10);
end
",
        };
        LinkTestPeripheral peripheral = CompileAndRunWithLink(scripts, ("lib.sro", library));

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "9", "30" }));
    }

    /// <summary>
    /// 制御構文（ループ・break）を含むリンク関数のラベル解決が正しいことをテストします
    /// </summary>
    [Test]
    public void LinkedControlFlowExecutionTest()
    {
        byte[] library = CompileObjectBinary(@"
function int SumUpTo(int limit)
    local int total = 0;
    local int i = 0;
    for (i = 1; ; i++)
        if (i > limit)
            break;
        end
        total += i;
    end
    while (total % 10 != 0)
        total += 1;
    end
    return total;
end
");
        LinkTestPeripheral peripheral = CompileAndRunWithLink(@"
#link ""lib.sro""

using WriteInt = void Test.WriteInt(int);

function void main()
    WriteInt(SumUpTo(4));
end
", ("lib.sro", library));

        // 1+2+3+4 = 10 (すでに10の倍数)
        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "10" }));
    }

    /// <summary>
    /// main 関数をリンクされたオブジェクト側が提供できることをテストします
    /// </summary>
    [Test]
    public void MainProvidedByLibraryExecutionTest()
    {
        byte[] library = CompileObjectBinary(@"
using WriteInt = void Test.WriteInt(int);

function void main()
    WriteInt(777);
end
");
        LinkTestPeripheral peripheral = CompileAndRunWithLink(@"
#link ""lib.sro""
", ("lib.sro", library));

        Assert.That(peripheral.Outputs, Is.EqualTo(new[] { "777" }));
    }

    /// <summary>
    /// リンクの各エラーケースがコンパイルエラーとして報告されることをテストします
    /// </summary>
    [Test]
    public void LinkErrorTest()
    {
        byte[] library = CompileObjectBinary(@"
using Write = void Test.Write(string);

function int Twice(int value)
    return value * 2;
end
");

        // 存在しないオブジェクトパス
        Assert.Throws<SrSyntaxErrorException>(() => CompileAndRunWithLink(@"
#link ""missing.sro""

function void main()
end
", ("lib.sro", library)));

        // 関数名の衝突 (メイン側が同名関数を定義)
        Assert.Throws<SrSyntaxErrorException>(() => CompileAndRunWithLink(@"
#link ""lib.sro""

function int Twice(int value)
    return value;
end

function void main()
end
", ("lib.sro", library)));

        // 同名オブジェクトの実体二重リンク (別パスで同じ内容 → シンボル衝突)
        Assert.Throws<SrSyntaxErrorException>(() => CompileAndRunWithLink(@"
#link ""lib.sro""
#link ""copy.sro""

function void main()
end
", ("lib.sro", library), ("copy.sro", library)));

        // 実行形式 (SROF) の誤指定
        SrStringScriptStorage executableStorage = new SrStringScriptStorage();
        executableStorage.SetScript("exe.srs", @"
function void main()
end
");
        byte[] executable;
        using (SrCompiler executableCompiler = new SrCompiler(executableStorage, new NullReportPrinter()))
        {
            using MemoryStream stream = new MemoryStream();
            executableCompiler.Compile("exe.srs", stream);
            executable = stream.ToArray();
        }
        Assert.Throws<SrSyntaxErrorException>(() => CompileAndRunWithLink(@"
#link ""exe.sro""

function void main()
end
", ("exe.sro", executable)));

        // 同名 using のシグネチャ不一致 (ライブラリは Write(string)、メインは Write(int) を宣言)
        Assert.Throws<SrSyntaxErrorException>(() => CompileAndRunWithLink(@"
#link ""lib.sro""

using Write = void Test.Write(int);

function void main()
end
", ("lib.sro", library)));
    }
}
