// zlib/libpng License
//
// Copyright(c) 2019 - 2026 Sinoa
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

using System.IO;
using NUnit.Framework;
using SnowRabbit.Compiler;
using SnowRabbit.Compiler.IO;
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.Compiler.Parser.SyntaxNodes;
using SnowRabbit.Compiler.Reporter;

namespace SnowRabbitTest
{
    /// <summary>
    /// 構文木の構造を検証するテストクラスです
    /// </summary>
    [TestFixture]
    public class SrSyntaxTreeStructureTest
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
        /// 関数宣言の構文木構造をテストします
        /// Children[0]: 戻り型 (TypeSyntaxNode)
        /// Children[1]: 関数名 (IdentifierSyntaxNode)
        /// Children[2]: パラメータリスト (ParameterListSyntaxNode または null)
        /// Children[3...]: 関数本体のステートメント
        /// </summary>
        [Test]
        public void FunctionDeclareStructureTest()
        {
            string script = @"
function int Add(int a, int b)
    return a + b;
end
";
            MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
            SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

            compiler.Parse("test.srs", out SyntaxNode node);

            // ルートはCompileUnitSyntaxNode
            Assert.IsInstanceOf<CompileUnitSyntaxNode>(node);
            CompileUnitSyntaxNode compileUnit = (CompileUnitSyntaxNode)node;
            Assert.AreEqual(1, compileUnit.Children.Count);

            // 最初の子は関数宣言
            SyntaxNode funcNode = compileUnit.Children[0];
            Assert.IsInstanceOf<FunctionDeclareSyntaxNode>(funcNode);

            // Children[0]: 戻り型 (int)
            Assert.IsInstanceOf<TypeSyntaxNode>(funcNode.Children[0]);
            Assert.AreEqual(SrTokenKind.TypeInt, funcNode.Children[0].Token.Kind);

            // Children[1]: 関数名 (Add)
            Assert.IsInstanceOf<IdentifierSyntaxNode>(funcNode.Children[1]);
            Assert.AreEqual("Add", funcNode.Children[1].Token.Text);

            // Children[2]: パラメータリスト
            Assert.IsInstanceOf<ParameterListSyntaxNode>(funcNode.Children[2]);
            ParameterListSyntaxNode paramList = (ParameterListSyntaxNode)funcNode.Children[2];
            Assert.AreEqual(2, paramList.Children.Count);

            // パラメータ1: int a
            ParameterSyntaxNode param1 = (ParameterSyntaxNode)paramList.Children[0];
            Assert.AreEqual(SrTokenKind.TypeInt, param1.Children[0].Token.Kind);
            Assert.AreEqual("a", param1.Children[1].Token.Text);

            // パラメータ2: int b
            ParameterSyntaxNode param2 = (ParameterSyntaxNode)paramList.Children[1];
            Assert.AreEqual(SrTokenKind.TypeInt, param2.Children[0].Token.Kind);
            Assert.AreEqual("b", param2.Children[1].Token.Text);

            // Children[3]: return文
            Assert.IsInstanceOf<ReturnStatementSyntaxNode>(funcNode.Children[3]);
        }

        /// <summary>
        /// グローバル変数宣言の構文木構造をテストします
        /// Children[0]: 型 (TypeSyntaxNode)
        /// Children[1]: 変数名 (IdentifierSyntaxNode)
        /// Children[2]: 初期値リテラル (LiteralSyntaxNode) - オプション
        /// </summary>
        [Test]
        public void GlobalVariableDeclareStructureTest()
        {
            string script = @"
global int counter = 100;
global string message = ""hello"";
global number value;
";
            MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
            SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

            compiler.Parse("test.srs", out SyntaxNode node);

            CompileUnitSyntaxNode compileUnit = (CompileUnitSyntaxNode)node;
            Assert.AreEqual(3, compileUnit.Children.Count);

            // 1つ目: global int counter = 100;
            GlobalVariableDeclareSyntaxNode var1 = (GlobalVariableDeclareSyntaxNode)compileUnit.Children[0];
            Assert.AreEqual(SrTokenKind.TypeInt, var1.Children[0].Token.Kind);
            Assert.AreEqual("counter", var1.Children[1].Token.Text);
            Assert.AreEqual(3, var1.Children.Count); // 型、名前、初期値
            Assert.IsInstanceOf<LiteralSyntaxNode>(var1.Children[2]);
            Assert.AreEqual(100, var1.Children[2].Token.Integer);

            // 2つ目: global string message = "hello";
            GlobalVariableDeclareSyntaxNode var2 = (GlobalVariableDeclareSyntaxNode)compileUnit.Children[1];
            Assert.AreEqual(SrTokenKind.TypeString, var2.Children[0].Token.Kind);
            Assert.AreEqual("message", var2.Children[1].Token.Text);
            Assert.AreEqual("hello", var2.Children[2].Token.Text);

            // 3つ目: global number value; (初期値なし)
            GlobalVariableDeclareSyntaxNode var3 = (GlobalVariableDeclareSyntaxNode)compileUnit.Children[2];
            Assert.AreEqual(SrTokenKind.TypeNumber, var3.Children[0].Token.Kind);
            Assert.AreEqual("value", var3.Children[1].Token.Text);
            Assert.AreEqual(2, var3.Children.Count); // 型と名前のみ
        }

        /// <summary>
        /// ローカル変数宣言の構文木構造をテストします
        /// Children[0]: 型 (TypeSyntaxNode)
        /// Children[1]: 変数名 (IdentifierSyntaxNode)
        /// Children[2]: 初期化式 (ExpressionSyntaxNode) - オプション
        /// </summary>
        [Test]
        public void LocalVariableDeclareStructureTest()
        {
            string script = @"
function void Main()
    local int x = 10;
    local string s;
end
";
            MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
            SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

            compiler.Parse("test.srs", out SyntaxNode node);

            CompileUnitSyntaxNode compileUnit = (CompileUnitSyntaxNode)node;
            FunctionDeclareSyntaxNode funcNode = (FunctionDeclareSyntaxNode)compileUnit.Children[0];

            // Children[3]: local int x = 10;
            LocalVariableDeclareSyntaxNode localVar1 = (LocalVariableDeclareSyntaxNode)funcNode.Children[3];
            Assert.AreEqual(SrTokenKind.TypeInt, localVar1.Children[0].Token.Kind);
            Assert.AreEqual("x", localVar1.Children[1].Token.Text);
            Assert.AreEqual(3, localVar1.Children.Count); // 型、名前、初期化式

            // Children[4]: local string s; (初期化なし)
            LocalVariableDeclareSyntaxNode localVar2 = (LocalVariableDeclareSyntaxNode)funcNode.Children[4];
            Assert.AreEqual(SrTokenKind.TypeString, localVar2.Children[0].Token.Kind);
            Assert.AreEqual("s", localVar2.Children[1].Token.Text);
            Assert.AreEqual(2, localVar2.Children.Count); // 型と名前のみ
        }

        /// <summary>
        /// if文の構文木構造をテストします
        /// Children[0]: 条件式 (ExpressionSyntaxNode)
        /// Children[1...]: if本体のステートメント
        /// </summary>
        [Test]
        public void IfStatementStructureTest()
        {
            string script = @"
function void Main()
    local int x = 5;
    if (x > 0)
        x = x - 1;
    end
end
";
            MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
            SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

            compiler.Parse("test.srs", out SyntaxNode node);

            CompileUnitSyntaxNode compileUnit = (CompileUnitSyntaxNode)node;
            FunctionDeclareSyntaxNode funcNode = (FunctionDeclareSyntaxNode)compileUnit.Children[0];

            // Children[4]: if文
            IfStatementSyntaxNode ifNode = (IfStatementSyntaxNode)funcNode.Children[4];

            // Children[0]: 条件式 (x > 0)
            Assert.IsInstanceOf<ExpressionSyntaxNode>(ifNode.Children[0]);

            // Children[1]: if本体 (x = x - 1;)
            Assert.IsInstanceOf<ExpressionSyntaxNode>(ifNode.Children[1]);
        }

        /// <summary>
        /// if-else文の構文木構造をテストします
        /// </summary>
        [Test]
        public void IfElseStatementStructureTest()
        {
            string script = @"
function void Main()
    local int x = 5;
    if (x > 0)
        x = 1;
    else
        x = 0;
    end
end
";
            MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
            SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

            compiler.Parse("test.srs", out SyntaxNode node);

            CompileUnitSyntaxNode compileUnit = (CompileUnitSyntaxNode)node;
            FunctionDeclareSyntaxNode funcNode = (FunctionDeclareSyntaxNode)compileUnit.Children[0];

            // Children[4]: if文
            IfStatementSyntaxNode ifNode = (IfStatementSyntaxNode)funcNode.Children[4];

            // Children[0]: 条件式
            Assert.IsInstanceOf<ExpressionSyntaxNode>(ifNode.Children[0]);

            // Children[1]: if本体
            Assert.IsInstanceOf<ExpressionSyntaxNode>(ifNode.Children[1]);

            // Children[2]: else文
            Assert.IsInstanceOf<ElseStatementSyntaxNode>(ifNode.Children[2]);
            ElseStatementSyntaxNode elseNode = (ElseStatementSyntaxNode)ifNode.Children[2];

            // else本体
            Assert.IsInstanceOf<ExpressionSyntaxNode>(elseNode.Children[0]);
        }

        /// <summary>
        /// while文の構文木構造をテストします
        /// Children[0]: 条件式 (ExpressionSyntaxNode)
        /// Children[1...]: ループ本体のステートメント
        /// </summary>
        [Test]
        public void WhileStatementStructureTest()
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

            CompileUnitSyntaxNode compileUnit = (CompileUnitSyntaxNode)node;
            FunctionDeclareSyntaxNode funcNode = (FunctionDeclareSyntaxNode)compileUnit.Children[0];

            // Children[4]: while文
            WhileStatementSyntaxNode whileNode = (WhileStatementSyntaxNode)funcNode.Children[4];

            // Children[0]: 条件式 (i < 10)
            Assert.IsInstanceOf<ExpressionSyntaxNode>(whileNode.Children[0]);

            // Children[1]: ループ本体 (i = i + 1;)
            Assert.IsInstanceOf<ExpressionSyntaxNode>(whileNode.Children[1]);
        }

        /// <summary>
        /// for文の構文木構造をテストします
        /// Children[0]: 初期化式 (ExpressionSyntaxNode または null)
        /// Children[1]: 条件式 (ExpressionSyntaxNode または null)
        /// Children[2]: 更新式 (ExpressionSyntaxNode または null)
        /// Children[3...]: ループ本体のステートメント
        /// </summary>
        [Test]
        public void ForStatementStructureTest()
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

            CompileUnitSyntaxNode compileUnit = (CompileUnitSyntaxNode)node;
            FunctionDeclareSyntaxNode funcNode = (FunctionDeclareSyntaxNode)compileUnit.Children[0];

            // Children[5]: for文
            ForStatementSyntaxNode forNode = (ForStatementSyntaxNode)funcNode.Children[5];

            // Children[0]: 初期化式 (i = 0)
            Assert.IsInstanceOf<ExpressionSyntaxNode>(forNode.Children[0]);

            // Children[1]: 条件式 (i < 10)
            Assert.IsInstanceOf<ExpressionSyntaxNode>(forNode.Children[1]);

            // Children[2]: 更新式 (i = i + 1)
            Assert.IsInstanceOf<ExpressionSyntaxNode>(forNode.Children[2]);

            // Children[3]: ループ本体 (sum = sum + i;)
            Assert.IsInstanceOf<ExpressionSyntaxNode>(forNode.Children[3]);
        }

        /// <summary>
        /// 関数呼び出しの構文木構造をテストします
        /// Children[0]: 関数名 (IdentifierSyntaxNode)
        /// Children[1]: 引数リスト (ArgumentListSyntaxNode)
        /// </summary>
        [Test]
        public void FunctionCallStructureTest()
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

            CompileUnitSyntaxNode compileUnit = (CompileUnitSyntaxNode)node;
            Assert.AreEqual(2, compileUnit.Children.Count); // Add関数とMain関数

            // Main関数
            FunctionDeclareSyntaxNode mainFunc = (FunctionDeclareSyntaxNode)compileUnit.Children[1];

            // local int result = Add(1, 2);
            LocalVariableDeclareSyntaxNode localVar = (LocalVariableDeclareSyntaxNode)mainFunc.Children[3];
            
            // 初期化式が関数呼び出し
            FunctionCallSyntaxNode funcCall = (FunctionCallSyntaxNode)localVar.Children[2];
            
            // Children[0]: 関数名
            Assert.AreEqual("Add", funcCall.Children[0].Token.Text);

            // Children[1]: 引数リスト
            Assert.IsInstanceOf<ArgumentListSyntaxNode>(funcCall.Children[1]);
            ArgumentListSyntaxNode argList = (ArgumentListSyntaxNode)funcCall.Children[1];
            Assert.AreEqual(2, argList.Children.Count); // 2つの引数
        }

        /// <summary>
        /// 定数定義の構文木構造をテストします
        /// </summary>
        [Test]
        public void ConstantDefineStructureTest()
        {
            string script = @"
#const MAX_VALUE 100
#const PI 3.14
#const NAME ""test""

function void Main()
end
";
            MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
            SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

            compiler.Parse("test.srs", out SyntaxNode node);

            CompileUnitSyntaxNode compileUnit = (CompileUnitSyntaxNode)node;
            Assert.AreEqual(4, compileUnit.Children.Count); // 3つの定数 + Main関数

            // 1つ目: #const MAX_VALUE 100
            ConstantDefineDirectiveSyntaxNode const1 = (ConstantDefineDirectiveSyntaxNode)compileUnit.Children[0];
            Assert.AreEqual("MAX_VALUE", const1.Children[0].Token.Text);
            Assert.AreEqual(100, const1.Children[1].Token.Integer);

            // 2つ目: #const PI 3.14
            ConstantDefineDirectiveSyntaxNode const2 = (ConstantDefineDirectiveSyntaxNode)compileUnit.Children[1];
            Assert.AreEqual("PI", const2.Children[0].Token.Text);
            Assert.AreEqual(3.14, const2.Children[1].Token.Number, 0.001);

            // 3つ目: #const NAME "test"
            ConstantDefineDirectiveSyntaxNode const3 = (ConstantDefineDirectiveSyntaxNode)compileUnit.Children[2];
            Assert.AreEqual("NAME", const3.Children[0].Token.Text);
            Assert.AreEqual("test", const3.Children[1].Token.Text);
        }

        /// <summary>
        /// return文の構文木構造をテストします
        /// Children[0]: 戻り値式 (ExpressionSyntaxNode) - void関数の場合は空
        /// </summary>
        [Test]
        public void ReturnStatementStructureTest()
        {
            string script = @"
function int GetValue()
    return 42;
end

function void DoNothing()
    return;
end
";
            MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
            SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

            compiler.Parse("test.srs", out SyntaxNode node);

            CompileUnitSyntaxNode compileUnit = (CompileUnitSyntaxNode)node;

            // GetValue関数のreturn
            FunctionDeclareSyntaxNode func1 = (FunctionDeclareSyntaxNode)compileUnit.Children[0];
            ReturnStatementSyntaxNode return1 = (ReturnStatementSyntaxNode)func1.Children[3];
            Assert.AreEqual(1, return1.Children.Count); // 戻り値あり

            // DoNothing関数のreturn
            FunctionDeclareSyntaxNode func2 = (FunctionDeclareSyntaxNode)compileUnit.Children[1];
            ReturnStatementSyntaxNode return2 = (ReturnStatementSyntaxNode)func2.Children[3];
            Assert.AreEqual(0, return2.Children.Count); // 戻り値なし (void)
        }

        /// <summary>
        /// break文の構文木構造をテストします
        /// </summary>
        [Test]
        public void BreakStatementStructureTest()
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

            CompileUnitSyntaxNode compileUnit = (CompileUnitSyntaxNode)node;
            FunctionDeclareSyntaxNode funcNode = (FunctionDeclareSyntaxNode)compileUnit.Children[0];

            // while文
            WhileStatementSyntaxNode whileNode = (WhileStatementSyntaxNode)funcNode.Children[4];

            // if文 (whileの2番目の子)
            IfStatementSyntaxNode ifNode = (IfStatementSyntaxNode)whileNode.Children[2];

            // break文 (ifの2番目の子)
            Assert.IsInstanceOf<BreakStatementSyntaxNode>(ifNode.Children[1]);
        }

        /// <summary>
        /// 二項演算式の構文木構造をテストします
        /// </summary>
        [Test]
        public void BinaryExpressionStructureTest()
        {
            string script = @"
function void Main()
    local int a = 1 + 2 * 3;
end
";
            MemoryScriptStorage storage = new MemoryScriptStorage("test.srs", script);
            SrCompiler compiler = new SrCompiler(storage, new NullReportPrinter());

            compiler.Parse("test.srs", out SyntaxNode node);

            CompileUnitSyntaxNode compileUnit = (CompileUnitSyntaxNode)node;
            FunctionDeclareSyntaxNode funcNode = (FunctionDeclareSyntaxNode)compileUnit.Children[0];

            // local int a = 1 + 2 * 3;
            LocalVariableDeclareSyntaxNode localVar = (LocalVariableDeclareSyntaxNode)funcNode.Children[3];
            
            // 初期化式が二項演算 (1 + (2 * 3))
            ExpressionSyntaxNode expr = (ExpressionSyntaxNode)localVar.Children[2];
            
            // 演算子優先度により、+ が最上位
            // ExpressionSyntaxNode構造を確認
            Assert.IsNotNull(expr);
            Assert.IsTrue(expr.Children.Count >= 2);
        }
    }
}
