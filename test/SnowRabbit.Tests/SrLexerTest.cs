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

using SnowRabbit.Compiler.Lexer;

namespace SnowRabbit.Tests;

/// <summary>
/// SrLexer クラスのテストクラスです
/// </summary>
[TestFixture]
public class SrLexerTest
{
    /// <summary>
    /// キーワードトークンが正しく認識されることをテストします
    /// </summary>
    [Test]
    public void KeywordTokenTest()
    {
        // テスト用のスクリプト
        string script = "function void int number string object bool end global local if else for while return break true false null";

        using (StringReader reader = new StringReader(script))
        {
            SrLexer lexer = new SrLexer("test", reader);

            // 各キーワードを順番に確認
            lexer.ReadNextToken(out Token token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.Function));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.TypeVoid));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.TypeInt));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.TypeNumber));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.TypeString));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.TypeObject));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.TypeBool));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.End));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.Global));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.Local));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.If));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.Else));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.For));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.While));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.Return));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.Break));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.True));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.False));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(SrTokenKind.Null));
        }
    }

    /// <summary>
    /// 識別子トークンが正しく認識されることをテストします
    /// </summary>
    [Test]
    public void IdentifierTokenTest()
    {
        string script = "myVariable _underscore camelCase PascalCase var123";

        using (StringReader reader = new StringReader(script))
        {
            SrLexer lexer = new SrLexer("test", reader);

            lexer.ReadNextToken(out Token token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Identifier));
            Assert.That(token.Text, Is.EqualTo("myVariable"));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Identifier));
            Assert.That(token.Text, Is.EqualTo("_underscore"));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Identifier));
            Assert.That(token.Text, Is.EqualTo("camelCase"));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Identifier));
            Assert.That(token.Text, Is.EqualTo("PascalCase"));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Identifier));
            Assert.That(token.Text, Is.EqualTo("var123"));
        }
    }

    /// <summary>
    /// 整数リテラルが正しく認識されることをテストします
    /// </summary>
    [Test]
    public void IntegerLiteralTest()
    {
        string script = "0 123 456789";

        using (StringReader reader = new StringReader(script))
        {
            SrLexer lexer = new SrLexer("test", reader);

            lexer.ReadNextToken(out Token token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Integer));
            Assert.That(token.Integer, Is.EqualTo(0));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Integer));
            Assert.That(token.Integer, Is.EqualTo(123));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Integer));
            Assert.That(token.Integer, Is.EqualTo(456789));
        }
    }

    /// <summary>
    /// 実数リテラルが正しく認識されることをテストします
    /// </summary>
    [Test]
    public void NumberLiteralTest()
    {
        string script = "0.0 3.14 123.456";

        using (StringReader reader = new StringReader(script))
        {
            SrLexer lexer = new SrLexer("test", reader);

            lexer.ReadNextToken(out Token token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Number));
            Assert.That(token.Number, Is.EqualTo(0.0).Within(0.001));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Number));
            Assert.That(token.Number, Is.EqualTo(3.14).Within(0.001));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Number));
            Assert.That(token.Number, Is.EqualTo(123.456).Within(0.001));
        }
    }

    /// <summary>
    /// 文字列リテラルが正しく認識されることをテストします
    /// </summary>
    [Test]
    public void StringLiteralTest()
    {
        string script = "\"hello\" \"world\" \"日本語テスト\"";

        using (StringReader reader = new StringReader(script))
        {
            SrLexer lexer = new SrLexer("test", reader);

            lexer.ReadNextToken(out Token token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.String));
            Assert.That(token.Text, Is.EqualTo("hello"));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.String));
            Assert.That(token.Text, Is.EqualTo("world"));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.String));
            Assert.That(token.Text, Is.EqualTo("日本語テスト"));
        }
    }

    /// <summary>
    /// 演算子トークンが正しく認識されることをテストします
    /// </summary>
    [Test]
    public void OperatorTokenTest()
    {
        string script = "+ - * / = == != < > <= >=";

        using (StringReader reader = new StringReader(script))
        {
            SrLexer lexer = new SrLexer("test", reader);

            lexer.ReadNextToken(out Token token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Plus));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Minus));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Asterisk));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Slash));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.Equal));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.DoubleEqual));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.NotEqual));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.OpenAngle));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.CloseAngle));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.LesserEqual));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.GreaterEqual));
        }
    }

    /// <summary>
    /// 括弧トークンが正しく認識されることをテストします
    /// </summary>
    [Test]
    public void BracketTokenTest()
    {
        string script = "( ) { } [ ]";

        using (StringReader reader = new StringReader(script))
        {
            SrLexer lexer = new SrLexer("test", reader);

            lexer.ReadNextToken(out Token token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.OpenParen));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.CloseParen));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.OpenBrace));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.CloseBrace));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.OpenBracket));

            lexer.ReadNextToken(out token);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.CloseBracket));
        }
    }

    /// <summary>
    /// 行番号が正しく記録されることをテストします
    /// </summary>
    [Test]
    public void LineNumberTest()
    {
        string script = "first\nsecond\nthird";

        using (StringReader reader = new StringReader(script))
        {
            SrLexer lexer = new SrLexer("test", reader);

            lexer.ReadNextToken(out Token token);
            Assert.That(token.Text, Is.EqualTo("first"));
            Assert.That(token.LineNumber, Is.EqualTo(1));

            lexer.ReadNextToken(out token);
            Assert.That(token.Text, Is.EqualTo("second"));
            Assert.That(token.LineNumber, Is.EqualTo(2));

            lexer.ReadNextToken(out token);
            Assert.That(token.Text, Is.EqualTo("third"));
            Assert.That(token.LineNumber, Is.EqualTo(3));
        }
    }

    /// <summary>
    /// コメントが正しくスキップされることをテストします
    /// </summary>
    [Test]
    public void CommentSkipTest()
    {
        string script = "before // this is comment\nafter";

        using (StringReader reader = new StringReader(script))
        {
            SrLexer lexer = new SrLexer("test", reader);

            lexer.ReadNextToken(out Token token);
            Assert.That(token.Text, Is.EqualTo("before"));

            lexer.ReadNextToken(out token);
            Assert.That(token.Text, Is.EqualTo("after"));
        }
    }

    /// <summary>
    /// EndOfTokenが正しく返されることをテストします
    /// </summary>
    [Test]
    public void EndOfTokenTest()
    {
        string script = "only";

        using (StringReader reader = new StringReader(script))
        {
            SrLexer lexer = new SrLexer("test", reader);

            bool hasMore = lexer.ReadNextToken(out Token token);
            Assert.That(hasMore, Is.True);
            Assert.That(token.Text, Is.EqualTo("only"));

            hasMore = lexer.ReadNextToken(out token);
            Assert.That(hasMore, Is.False);
            Assert.That(token.Kind, Is.EqualTo(TokenKind.EndOfToken));
        }
    }
}
