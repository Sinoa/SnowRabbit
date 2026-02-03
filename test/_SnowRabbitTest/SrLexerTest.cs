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
using SnowRabbit.Compiler.Lexer;

namespace SnowRabbitTest
{
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
                Assert.AreEqual(SrTokenKind.Function, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.TypeVoid, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.TypeInt, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.TypeNumber, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.TypeString, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.TypeObject, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.TypeBool, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.End, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.Global, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.Local, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.If, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.Else, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.For, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.While, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.Return, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.Break, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.True, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.False, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(SrTokenKind.Null, token.Kind);
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
                Assert.AreEqual(TokenKind.Identifier, token.Kind);
                Assert.AreEqual("myVariable", token.Text);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.Identifier, token.Kind);
                Assert.AreEqual("_underscore", token.Text);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.Identifier, token.Kind);
                Assert.AreEqual("camelCase", token.Text);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.Identifier, token.Kind);
                Assert.AreEqual("PascalCase", token.Text);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.Identifier, token.Kind);
                Assert.AreEqual("var123", token.Text);
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
                Assert.AreEqual(TokenKind.Integer, token.Kind);
                Assert.AreEqual(0, token.Integer);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.Integer, token.Kind);
                Assert.AreEqual(123, token.Integer);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.Integer, token.Kind);
                Assert.AreEqual(456789, token.Integer);
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
                Assert.AreEqual(TokenKind.Number, token.Kind);
                Assert.AreEqual(0.0, token.Number, 0.001);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.Number, token.Kind);
                Assert.AreEqual(3.14, token.Number, 0.001);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.Number, token.Kind);
                Assert.AreEqual(123.456, token.Number, 0.001);
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
                Assert.AreEqual(TokenKind.String, token.Kind);
                Assert.AreEqual("hello", token.Text);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.String, token.Kind);
                Assert.AreEqual("world", token.Text);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.String, token.Kind);
                Assert.AreEqual("日本語テスト", token.Text);
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
                Assert.AreEqual(TokenKind.Plus, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.Minus, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.Asterisk, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.Slash, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.Equal, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.DoubleEqual, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.NotEqual, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.OpenAngle, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.CloseAngle, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.LesserEqual, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.GreaterEqual, token.Kind);
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
                Assert.AreEqual(TokenKind.OpenParen, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.CloseParen, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.OpenBrace, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.CloseBrace, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.OpenBracket, token.Kind);

                lexer.ReadNextToken(out token);
                Assert.AreEqual(TokenKind.CloseBracket, token.Kind);
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
                Assert.AreEqual("first", token.Text);
                Assert.AreEqual(1, token.LineNumber);

                lexer.ReadNextToken(out token);
                Assert.AreEqual("second", token.Text);
                Assert.AreEqual(2, token.LineNumber);

                lexer.ReadNextToken(out token);
                Assert.AreEqual("third", token.Text);
                Assert.AreEqual(3, token.LineNumber);
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
                Assert.AreEqual("before", token.Text);

                lexer.ReadNextToken(out token);
                Assert.AreEqual("after", token.Text);
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
                Assert.IsTrue(hasMore);
                Assert.AreEqual("only", token.Text);

                hasMore = lexer.ReadNextToken(out token);
                Assert.IsFalse(hasMore);
                Assert.AreEqual(TokenKind.EndOfToken, token.Kind);
            }
        }
    }
}
