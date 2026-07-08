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
    /// 指定されたスクリプトをオブジェクトファイル (SROB) としてコンパイルします
    /// </summary>
    private static byte[] CompileObjectBinary(string script)
    {
        SrStringScriptStorage storage = new SrStringScriptStorage();
        storage.SetScript("library.srs", script);
        using SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        using MemoryStream stream = new MemoryStream();
        compiler.CompileObject("library.srs", stream);
        return stream.ToArray();
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
}
