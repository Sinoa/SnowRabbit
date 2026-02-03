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
using SnowRabbit.Compiler.Parser.SyntaxNodes;
using SnowRabbit.Compiler.Reporter;

namespace SnowRabbit.Tests;

/// <summary>
/// コンパイラ統合テストクラスです
/// </summary>
[TestFixture]
public class SrCompilerIntegrationTest
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
    /// 空の関数がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseEmptyFunctionTest()
    {
        string script = @"
function void Main()
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
        Assert.That(node, Is.InstanceOf<CompileUnitSyntaxNode>());
    }

    /// <summary>
    /// グローバル変数宣言がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseGlobalVariableTest()
    {
        string script = @"
global int counter = 0;
global number pi = 3.14;
global string message = ""hello"";
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
        CompileUnitSyntaxNode compileUnit = (CompileUnitSyntaxNode)node;
        // CompileUnitに子ノードが含まれていることを確認
        Assert.That(compileUnit.Children.Count, Is.GreaterThan(0));
    }

    /// <summary>
    /// ローカル変数宣言がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseLocalVariableTest()
    {
        string script = @"
function void Main()
    local int x = 10;
    local number y = 2.5;
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
    }

    /// <summary>
    /// if文がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseIfStatementTest()
    {
        string script = @"
function void Main()
    local int x = 10;
    if (x > 5)
        x = 0;
    end
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
    }

    /// <summary>
    /// if-else文がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseIfElseStatementTest()
    {
        string script = @"
function void Main()
    local int x = 10;
    if (x > 5)
        x = 1;
    else
        x = 0;
    end
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
    }

    /// <summary>
    /// while文がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseWhileStatementTest()
    {
        string script = @"
function void Main()
    local int i = 0;
    while (i < 10)
        i = i + 1;
    end
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
    }

    /// <summary>
    /// for文がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseForStatementTest()
    {
        string script = @"
function void Main()
    local int sum = 0;
    local int i = 0;
    for (i = 0; i < 10; i = i + 1)
        sum = sum + i;
    end
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
    }

    /// <summary>
    /// return文がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseReturnStatementTest()
    {
        string script = @"
function int Add(int a, int b)
    return a + b;
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
    }

    /// <summary>
    /// 関数呼び出しがパースできることをテストします
    /// </summary>
    [Test]
    public void ParseFunctionCallTest()
    {
        string script = @"
function int Add(int a, int b)
    return a + b;
end

function void Main()
    local int result = Add(1, 2);
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
    }

    /// <summary>
    /// 算術演算式がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseArithmeticExpressionTest()
    {
        string script = @"
function int Calc()
    local int a = 1 + 2 * 3;
    local int b = (1 + 2) * 3;
    local int c = 10 / 2 - 1;
    return a + b + c;
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
    }

    /// <summary>
    /// 比較演算式がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseComparisonExpressionTest()
    {
        string script = @"
function void Main()
    local bool a = 1 < 2;
    local bool b = 1 > 2;
    local bool c = 1 <= 2;
    local bool d = 1 >= 2;
    local bool e = 1 == 2;
    local bool f = 1 != 2;
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
    }

    /// <summary>
    /// 論理演算式がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseLogicalExpressionTest()
    {
        string script = @"
function void Main()
    local bool a = true && false;
    local bool b = true || false;
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
    }

    /// <summary>
    /// 定数定義がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseConstantDefineTest()
    {
        string script = @"
#const MAX_VALUE 100
#const PI 3.14159

function void Main()
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
    }

    /// <summary>
    /// 完全なスクリプトがコンパイルできることをテストします
    /// </summary>
    [Test]
    public void CompileCompleteScriptTest()
    {
        string script = @"
#const MAX_COUNT 10

global int totalSum = 0;

function int Add(int a, int b)
    return a + b;
end

function void main()
    local int sum = 0;
    local int i = 0;
    for (i = 0; i < MAX_COUNT; i = i + 1)
        sum = Add(sum, i);
    end
    totalSum = sum;
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        using (MemoryStream outputStream = new MemoryStream())
        {
            compiler.Compile("test.srs", outputStream);

            // バイナリが生成されていることを確認
            Assert.That(outputStream.Length, Is.GreaterThan(0));
        }
    }

    /// <summary>
    /// break文がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseBreakStatementTest()
    {
        string script = @"
function void Main()
    local int i = 0;
    while (true)
        i = i + 1;
        if (i > 5)
            break;
        end
    end
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
    }

    /// <summary>
    /// 複合代入演算子がパースできることをテストします
    /// </summary>
    [Test]
    public void ParseCompoundAssignmentTest()
    {
        string script = @"
function void Main()
    local int x = 10;
    x += 5;
    x -= 3;
    x *= 2;
    x /= 4;
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        compiler.Parse("test.srs", out SyntaxNode node);

        Assert.That(node, Is.Not.Null);
    }
}
