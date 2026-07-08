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
using SnowRabbit.Compiler.Parser.SyntaxErrors;
using SnowRabbit.Compiler.Reporter;

namespace SnowRabbit.Tests;

/// <summary>
/// コンパイラエラーケースのテストクラスです
/// </summary>
[TestFixture]
public class SrCompilerErrorTest
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
    /// 閉じられていない文字列リテラルでエラーが発生することをテストします
    /// </summary>
    [Test]
    public void UnclosedStringLiteralTest()
    {
        string script = @"
function void Main()
    local string s = ""unclosed string;
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        Assert.Throws<SrSyntaxErrorException>(() =>
        {
            compiler.Parse("test.srs", out _);
        });
    }

    /// <summary>
    /// 不正なトークンでエラーが発生することをテストします
    /// </summary>
    [Test]
    public void InvalidTokenTest()
    {
        string script = @"
function void Main()
    local int x = @invalid;
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        Assert.Throws<SrSyntaxErrorException>(() =>
        {
            compiler.Parse("test.srs", out _);
        });
    }

    /// <summary>
    /// 閉じられていない関数でエラーが発生することをテストします
    /// </summary>
    [Test]
    public void UnclosedFunctionTest()
    {
        string script = @"
function void Main()
    local int x = 10;
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        Assert.Throws<SrSyntaxErrorException>(() =>
        {
            compiler.Parse("test.srs", out _);
        });
    }

    /// <summary>
    /// 閉じられていないif文でエラーが発生することをテストします
    /// </summary>
    [Test]
    public void UnclosedIfStatementTest()
    {
        string script = @"
function void Main()
    if (true)
        local int x = 10;
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        Assert.Throws<SrSyntaxErrorException>(() =>
        {
            compiler.Parse("test.srs", out _);
        });
    }

    /// <summary>
    /// 閉じられていない括弧でエラーが発生することをテストします
    /// </summary>
    [Test]
    public void UnclosedParenthesisTest()
    {
        string script = @"
function void Main()
    local int x = (1 + 2;
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        Assert.Throws<SrSyntaxErrorException>(() =>
        {
            compiler.Parse("test.srs", out _);
        });
    }

    /// <summary>
    /// 不正な型名でエラーが発生することをテストします
    /// </summary>
    [Test]
    public void InvalidTypeTest()
    {
        string script = @"
function invalidtype Main()
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        Assert.Throws<SrSyntaxErrorException>(() =>
        {
            compiler.Parse("test.srs", out _);
        });
    }

    /// <summary>
    /// セミコロン欠落でエラーが発生することをテストします
    /// </summary>
    [Test]
    public void MissingSemicolonTest()
    {
        string script = @"
global int x = 10
global int y = 20;
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        Assert.Throws<SrSyntaxErrorException>(() =>
        {
            compiler.Parse("test.srs", out _);
        });
    }

    /// <summary>
    /// 不正な演算子でエラーが発生することをテストします
    /// </summary>
    [Test]
    public void InvalidOperatorTest()
    {
        string script = @"
function void Main()
    local int x = 1 ++ 2;
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        Assert.Throws<SrSyntaxErrorException>(() =>
        {
            compiler.Parse("test.srs", out _);
        });
    }

    /// <summary>
    /// 空のfor文条件でもパースできることをテストします
    /// </summary>
    [Test]
    public void EmptyForConditionTest()
    {
        string script = @"
function void Main()
    for (;;)
        break;
    end
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out var node);
        Assert.That(node, Is.Not.Null);
    }

    /// <summary>
    /// 関数名が欠落でエラーが発生することをテストします
    /// </summary>
    [Test]
    public void MissingFunctionNameTest()
    {
        string script = @"
function void ()
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        Assert.Throws<SrSyntaxErrorException>(() =>
        {
            compiler.Parse("test.srs", out _);
        });
    }

    /// <summary>
    /// 指定されたスクリプトのコンパイル（コード生成まで）が型エラーになることを検証します
    /// </summary>
    private static void AssertCompileError(string script)
    {
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        Assert.Throws<SrSyntaxErrorException>(() =>
        {
            using MemoryStream outputStream = new MemoryStream();
            compiler.Compile("test.srs", outputStream);
        });
    }

    /// <summary>
    /// ローカル変数の初期化子の型不一致がコンパイルエラーになることをテストします
    /// </summary>
    [Test]
    public void LocalVariableInitializerTypeMismatchTest()
    {
        AssertCompileError(@"
function void main()
    local int x = 1.5;
end
");
        AssertCompileError(@"
function void main()
    local int x = ""hello"";
end
");
    }

    /// <summary>
    /// 代入の型不一致がコンパイルエラーになることをテストします
    /// </summary>
    [Test]
    public void AssignmentTypeMismatchTest()
    {
        AssertCompileError(@"
function void main()
    local int x = 0;
    x = ""hello"";
end
");
        AssertCompileError(@"
function void main()
    local int x = 0;
    x = 1.5;
end
");
    }

    /// <summary>
    /// 複合代入の縮小変換（int変数へのnumber）がコンパイルエラーになることをテストします
    /// </summary>
    [Test]
    public void CompoundAssignmentNarrowingTest()
    {
        AssertCompileError(@"
function void main()
    local int i = 0;
    i += 1.5;
end
");
    }

    /// <summary>
    /// return の型不一致がコンパイルエラーになることをテストします
    /// </summary>
    [Test]
    public void ReturnTypeMismatchTest()
    {
        AssertCompileError(@"
function int F()
    return ""text"";
end

function void main()
    local int x = F();
end
");
    }

    /// <summary>
    /// 実数から整数パラメータへの縮小渡しがコンパイルエラーになることをテストします
    /// </summary>
    [Test]
    public void ArgumentNarrowingTypeMismatchTest()
    {
        AssertCompileError(@"
function void Take(int value)
end

function void main()
    Take(1.5);
end
");
    }

    /// <summary>
    /// 型の混在する比較・条件がコンパイルエラーになることをテストします
    /// </summary>
    [Test]
    public void MixedTypeOperationTest()
    {
        AssertCompileError(@"
function void main()
    local bool b = true;
    local int x = 0;
    if (b == x)
        x = 1;
    end
end
");
        AssertCompileError(@"
function void main()
    local string s = ""text"";
    while (s)
        break;
    end
end
");
    }
}
