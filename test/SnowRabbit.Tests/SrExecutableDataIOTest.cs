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
using SnowRabbit.Compiler.IO;
using SnowRabbit.Compiler.Reporter;
using SnowRabbit.IO;
using SnowRabbit.RuntimeEngine.Data;

namespace SnowRabbit.Tests;

/// <summary>
/// 実行データ (SROF) の書き込みと読み込みのテストクラスです
/// </summary>
[TestFixture]
public class SrExecutableDataIOTest
{
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
            return null!;
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
    /// 文字列とシンボル情報を含む実行データのバイト列を生成します
    /// </summary>
    private static byte[] CompileSampleBinary()
    {
        string script = @"
using Print = void Console.WriteLine(string);

global string g_Message = ""hello"";

function void main()
    Print(g_Message);
    Print(""direct"");
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        using SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());
        compiler.IsContainSymbolInfo = true;

        using MemoryStream stream = new MemoryStream();
        compiler.Compile("test.srs", stream);
        return stream.ToArray();
    }

    /// <summary>
    /// 実行データが writer → reader → writer でバイト列として完全に往復することをテストします
    /// </summary>
    [Test]
    public void RoundTripTest()
    {
        byte[] original = CompileSampleBinary();

        SrExecutableData data;
        using (SrExecutableDataReader reader = new SrExecutableDataReader(new MemoryStream(original, false)))
        {
            data = reader.Read();
        }

        Assert.That(data.CodeCount, Is.GreaterThan(0));
        Assert.That(data.StringRecordCount, Is.GreaterThan(0));
        Assert.That(data.GetSymbols(), Is.Not.Empty);

        using MemoryStream rewritten = new MemoryStream();
        using (SrExecutableDataWriter writer = new SrExecutableDataWriter(rewritten, true))
        {
            writer.Write(data);
        }

        Assert.That(rewritten.ToArray(), Is.EqualTo(original));
    }

    /// <summary>
    /// マジックナンバー不一致がメッセージ付きの専用例外になることをテストします
    /// </summary>
    [Test]
    public void BadMagicNumberTest()
    {
        byte[] corrupted = CompileSampleBinary();
        corrupted[0] ^= 0xFF;

        using SrExecutableDataReader reader = new SrExecutableDataReader(new MemoryStream(corrupted, false));
        Assert.Throws<SrMalformedExecutableDataException>(() => reader.Read());
    }

    /// <summary>
    /// 途中で切り詰められた実行データが専用例外になることをテストします
    /// </summary>
    [Test]
    public void TruncatedDataTest()
    {
        byte[] original = CompileSampleBinary();
        byte[] truncated = new byte[original.Length / 2];
        Array.Copy(original, truncated, truncated.Length);

        using SrExecutableDataReader reader = new SrExecutableDataReader(new MemoryStream(truncated, false));
        Assert.Throws<SrMalformedExecutableDataException>(() => reader.Read());
    }

    /// <summary>
    /// 負の要素数を持つ実行データが専用例外になることをテストします
    /// </summary>
    [Test]
    public void NegativeCodeCountTest()
    {
        using MemoryStream crafted = new MemoryStream();
        using (SrBinaryIO io = new SrBinaryIO(crafted, true))
        {
            io.Write(SrExecutableData.MagicNumber);
            io.Write(-1); // codeCount
            io.Write(0);  // recordCount
            io.Write(-1); // symbolCount
        }
        crafted.Position = 0;

        using SrExecutableDataReader reader = new SrExecutableDataReader(crafted);
        Assert.Throws<SrMalformedExecutableDataException>(() => reader.Read());
    }

    /// <summary>
    /// 負の長さの文字列レコードを持つ実行データが専用例外になることをテストします
    /// </summary>
    [Test]
    public void NegativeRecordLengthTest()
    {
        using MemoryStream crafted = new MemoryStream();
        using (SrBinaryIO io = new SrBinaryIO(crafted, true))
        {
            io.Write(SrExecutableData.MagicNumber);
            io.Write(0);  // codeCount
            io.Write(1);  // recordCount
            io.Write(-1); // symbolCount
            io.Write(0);  // record.Address
            io.Write(0);  // record.Offset
            io.Write(-5); // record.Length
        }
        crafted.Position = 0;

        using SrExecutableDataReader reader = new SrExecutableDataReader(crafted);
        Assert.Throws<SrMalformedExecutableDataException>(() => reader.Read());
    }
}
