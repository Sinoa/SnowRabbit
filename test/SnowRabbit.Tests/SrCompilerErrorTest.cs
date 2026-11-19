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
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.Compiler.Parser.SyntaxErrors;
using SnowRabbit.Compiler.Parser.SyntaxNodes;
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
    /// コンパイルレポートのメッセージを記録するプリンタです
    /// </summary>
    private class RecordingReportPrinter : ISrCompileReportPrinter
    {
        public List<string> Messages { get; } = new List<string>();

        public void PrintReport(CompileReport report)
        {
            Messages.Add(report.Message);
        }
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
    /// 指定されたスクリプトのコンパイルが、指定されたメッセージのエラーになることを検証します
    /// </summary>
    private static void AssertCompileErrorMessage(string script, string expectedMessage)
    {
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        RecordingReportPrinter printer = new RecordingReportPrinter();
        SrCompiler compiler = new SrCompiler(storage, printer);

        Assert.Throws<SrSyntaxErrorException>(() =>
        {
            using MemoryStream outputStream = new MemoryStream();
            compiler.Compile("test.srs", outputStream);
        });
        Assert.That(printer.Messages, Is.EqualTo(new[] { expectedMessage }));
    }

    /// <summary>
    /// 指定されたスクリプトのコンパイル（コード生成まで）が成功することを検証します
    /// </summary>
    private static void AssertCompileSuccess(string script)
    {
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

        Assert.DoesNotThrow(() =>
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
    /// number を返すペリフェラル関数の戻り値を int で受けるとコンパイルエラーになることをテストします
    /// </summary>
    [Test]
    public void PeripheralFunctionReturnNarrowingTest()
    {
        AssertCompileError(@"
using GetValue = number Test.GetValue();

function void main()
    local int x = GetValue();
end
");
        AssertCompileError(@"
using GetValue = number Test.GetValue();

function void main()
    local int x = 0;
    x = GetValue();
end
");
    }

    /// <summary>
    /// 文字列と数値の混合連結・文字列への非対応演算がコンパイルエラーになることをテストします
    /// </summary>
    [Test]
    public void InvalidStringOperationTest()
    {
        AssertCompileError(@"
function void main()
    local string s = ""a"" + 1;
end
");
        AssertCompileError(@"
function void main()
    local string s = 1 + ""a"";
end
");
        AssertCompileError(@"
function void main()
    local string s = ""a"" - ""b"";
end
");
    }

    /// <summary>
    /// 変数以外への後置インクリメントがコンパイルエラーになることをテストします
    /// </summary>
    [Test]
    public void PostfixIncrementOnNonVariableTest()
    {
        AssertCompileError(@"
function void main()
    local int x = 5++;
end
");
        AssertCompileError(@"
function int GetValue()
    return 1;
end

function void main()
    local int x = GetValue()++;
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

    /// <summary>
    /// 非void関数に return 文が無いとコンパイルエラーになることをテストします
    /// </summary>
    [Test]
    public void NonVoidFunctionWithoutReturnTest()
    {
        AssertCompileError(@"
function int GetValue()
    local int x = 10;
end

function void main()
    local int x = GetValue();
end
");
        AssertCompileError(@"
function int GetValue()
end

function void main()
    local int x = GetValue();
end
");
    }

    /// <summary>
    /// if分岐の一部の経路でしか return しない非void関数がコンパイルエラーになることをテストします
    /// </summary>
    [Test]
    public void NonVoidFunctionIfNotAllPathsReturnTest()
    {
        // else節が無いifは条件が偽の経路で return しない
        AssertCompileError(@"
function int GetValue(int a)
    if (a > 0)
        return 1;
    end
end

function void main()
    local int x = GetValue(1);
end
");
        // else側の経路が return しない
        AssertCompileError(@"
function int GetValue(int a)
    if (a > 0)
        return 1;
    else
        a = 0;
    end
end

function void main()
    local int x = GetValue(1);
end
");
        // else if チェーンに最終elseが無い
        AssertCompileError(@"
function int GetValue(int a)
    if (a > 0)
        return 1;
    else if (a < 0)
        return -1;
    end
end

function void main()
    local int x = GetValue(1);
end
");
    }

    /// <summary>
    /// ループ内の return は経路の保証と見なされずコンパイルエラーになることをテストします
    /// </summary>
    [Test]
    public void NonVoidFunctionLoopOnlyReturnTest()
    {
        AssertCompileError(@"
function int GetValue(int a)
    while (a > 0)
        return 1;
    end
end

function void main()
    local int x = GetValue(1);
end
");
        // 条件が定数 true のループも保証なしと判断する（保守的解析の仕様固定）
        AssertCompileError(@"
function int GetValue()
    while (true)
        return 1;
    end
end

function void main()
    local int x = GetValue();
end
");
        AssertCompileError(@"
function int GetValue()
    local int i = 0;
    for (i = 0; i < 10; ++i)
        return 1;
    end
end

function void main()
    local int x = GetValue();
end
");
    }

    /// <summary>
    /// 全ての経路で return する関数がコンパイルに成功することをテストします
    /// </summary>
    [Test]
    public void AllPathsReturnCompileSuccessTest()
    {
        // if-else 両分岐 return（末尾 return 無し）
        AssertCompileSuccess(@"
function int Sign(int a)
    if (a >= 0)
        return 1;
    else
        return -1;
    end
end

function void main()
    local int x = Sign(1);
end
");
        // else if チェーン全分岐 return
        AssertCompileSuccess(@"
function int Compare(int a)
    if (a > 0)
        return 1;
    else if (a < 0)
        return -1;
    else
        return 0;
    end
end

function void main()
    local int x = Compare(1);
end
");
        // 早期 return + 末尾 return
        AssertCompileSuccess(@"
function int Abs(int a)
    if (a < 0)
        return -a;
    end
    return a;
end

function void main()
    local int x = Abs(-1);
end
");
        // ループ後の末尾 return
        AssertCompileSuccess(@"
function int Sum(int n)
    local int total = 0;
    local int i = 0;
    for (i = 0; i < n; ++i)
        total += i;
    end
    return total;
end

function void main()
    local int x = Sum(10);
end
");
        // return 後にデッドコードが続く（文リストのいずれかが return すれば良い）
        AssertCompileSuccess(@"
function int GetValue()
    return 1;
    local int unused = 0;
end

function void main()
    local int x = GetValue();
end
");
        // ネストした if-else の全経路 return
        AssertCompileSuccess(@"
function int Classify(int a, int b)
    if (a > 0)
        if (b > 0)
            return 1;
        else
            return 2;
        end
    else
        return 3;
    end
end

function void main()
    local int x = Classify(1, 2);
end
");
        // void 関数は return 無しで良い（回帰確認）
        AssertCompileSuccess(@"
function void DoNothing()
end

function void main()
    DoNothing();
end
");
    }

    /// <summary>
    /// 関数呼び出しの引数リストで ',' の後に引数が無い場合に、専用メッセージのエラーになることをテストします
    /// （末尾カンマが黙って通り、引数を1つも積まない呼び出しが生成されていた問題の回帰テスト）
    /// </summary>
    [Test]
    public void ArgumentListMissingArgumentAfterCommaTest()
    {
        const string expectedMessage = "',' の後に引数がありません。";

        // ホスト関数の呼び出しの末尾カンマ
        AssertCompileErrorMessage(@"
using Fade = void Sample.Fade(number, number, int, int);

function void main()
    Fade(1.0, 1.0, 0, 2,);
end
", expectedMessage);

        // スクリプト関数の呼び出しの末尾カンマ
        AssertCompileErrorMessage(@"
function void Take(int a)
end

function void main()
    Take(1,);
end
", expectedMessage);

        // 式の中の呼び出しの末尾カンマ
        AssertCompileErrorMessage(@"
function int Twice(int a)
    return a * 2;
end

function void main()
    local int x = 1 + Twice(2,);
end
", expectedMessage);

        // 引数の間でカンマが連続している
        AssertCompileErrorMessage(@"
using Fade = void Sample.Fade(number, number, int, int);

function void main()
    Fade(1.0,,1.0);
end
", expectedMessage);
    }

    /// <summary>
    /// 引数リストがカンマから始まる場合に、従来通り括弧の対応エラーになることをテストします
    /// </summary>
    [Test]
    public void ArgumentListLeadingCommaTest()
    {
        AssertCompileErrorMessage(@"
using Fade = void Sample.Fade(number, number, int, int);

function void main()
    Fade(,1.0);
end
", "'(' に対応する ')' がありません。");
    }

    /// <summary>
    /// 引数の個数の不一致と、引数なしの呼び出しの扱いが変わっていないことをテストします
    /// </summary>
    [Test]
    public void ArgumentCountCheckTest()
    {
        AssertCompileErrorMessage(@"
using Fade = void Sample.Fade(number, number, int, int);

function void main()
    Fade(1.0, 1.0, 0);
end
", "関数 'Fade' のパラメータは '4' 個ですが、引数の数が '3' 個です。");

        AssertCompileErrorMessage(@"
using Fade = void Sample.Fade(number, number, int, int);

function void main()
    Fade();
end
", "関数 'Fade' のパラメータは '4' 個ですが、引数の数が '0' 個です。");

        AssertCompileSuccess(@"
using Fade = void Sample.Fade(number, number, int, int);
using Reset = void Sample.Reset();

function void DoNothing()
end

function void main()
    Fade(1.0, 1.0, 0, 2);
    Reset();
    DoNothing();
end
");
    }

    /// <summary>
    /// using 宣言の型リストで ',' の後に型が無い場合に、エラーになることをテストします
    /// （型リストごと「型無し」として黙って通り、引数なしのホスト関数として宣言されていた問題の回帰テスト）
    /// </summary>
    [Test]
    public void TypeListMissingTypeAfterCommaTest()
    {
        const string expectedMessage = "',' の後に型がありません。";

        AssertCompileErrorMessage(@"
using Show = void Sample.Show(int, );

function void main()
end
", expectedMessage);

        AssertCompileErrorMessage(@"
using Show = void Sample.Show(int, number, );

function void main()
    Show();
end
", expectedMessage);

        AssertCompileErrorMessage(@"
using Show = void Sample.Show(int,, int);

function void main()
end
", expectedMessage);

        // 型が1つも無い宣言と、正しい型リストは従来通りコンパイルできる
        AssertCompileSuccess(@"
using Reset = void Sample.Reset();
using Show = void Sample.Show(int, number);

function void main()
    Reset();
    Show(1, 2.0);
end
");
    }

    /// <summary>
    /// 関数宣言のパラメータリストで ',' の後にパラメータが無い場合に、エラーになることをテストします
    /// （パラメータリストごと「パラメータ無し」として黙って通っていた問題の回帰テスト）
    /// </summary>
    [Test]
    public void ParameterListMissingParameterAfterCommaTest()
    {
        const string expectedMessage = "',' の後にパラメータがありません。";

        AssertCompileErrorMessage(@"
function void Take(int a, )
end

function void main()
    Take();
end
", expectedMessage);

        AssertCompileErrorMessage(@"
function void Take(int a, int b, )
end

function void main()
end
", expectedMessage);

        AssertCompileErrorMessage(@"
function void Take(int a,, int b)
end

function void main()
end
", expectedMessage);

        // パラメータが1つも無い宣言と、正しいパラメータリストは従来通りコンパイルできる
        AssertCompileSuccess(@"
function void DoNothing()
end

function int Add(int a, int b)
    return a + b;
end

function void main()
    DoNothing();
    local int x = Add(1, 2);
end
");
    }

    /// <summary>
    /// 関数宣言のパラメータに名前が無い場合に、エラーになることをテストします
    /// （型だけ読んでパラメータリストごと「パラメータ無し」として黙って通っていた問題の回帰テスト）
    /// </summary>
    [Test]
    public void ParameterWithoutNameTest()
    {
        const string expectedMessage = "パラメータの名前がありません。";

        AssertCompileErrorMessage(@"
function void Take(int)
end

function void main()
    Take();
end
", expectedMessage);

        AssertCompileErrorMessage(@"
function void Take(int a, int)
end

function void main()
    Take();
end
", expectedMessage);
    }

    /// <summary>
    /// 引数リストを持たない関数呼び出しノードが、黙ってコンパイルされずエラーになることをテストします
    /// （引数の個数と型の検査は引数リストのコンパイルが担うため、引数リストが無いと検査ごと飛ばされる）
    /// </summary>
    [Test]
    public void FunctionCallNodeWithoutArgumentListTest()
    {
        const string script = @"
function void Take(int a)
end

function void main()
end
";
        MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
        RecordingReportPrinter printer = new RecordingReportPrinter();
        SrCompiler compiler = new SrCompiler(storage, printer);
        compiler.Parse("test.srs", out SyntaxNode root);

        // パーサは必ず引数リストを生成するため、引数リストが null の呼び出しを main の末尾へ手で組み立てる
        SyntaxNode mainFunction = root.Children.Last(x => x is FunctionDeclareSyntaxNode);
        Assert.That(mainFunction.Children[1].Token.Text, Is.EqualTo("main"));
        FunctionCallSyntaxNode functionCall = new FunctionCallSyntaxNode();
        functionCall.Add(new IdentifierSyntaxNode(new Token(TokenKind.Identifier, "Take", 0, 0.0, "test.srs", 6, 5)));
        functionCall.Add(null!);
        mainFunction.Add(functionCall);

        Assert.Throws<SrSyntaxErrorException>(() => compiler.Compile(root, out _));
        Assert.That(printer.Messages, Is.EqualTo(new[] { "関数 'Take' の呼び出しに引数リストがありません。" }));
    }
}
