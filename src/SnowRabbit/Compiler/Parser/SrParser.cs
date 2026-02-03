// zlib License
// 
// Copyright (c) 2019 Sinoa
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

#nullable disable

/*

# 雪兎のスクリプト構文

## Simple syntax

### literal
    : <integer>
    | <number>
    | <string>
    | 'true'
    | 'false'
    | 'null'

### type
    : 'void'
    | 'int'
    | 'number'
    | 'string'
    | 'object'
    | 'bool'

### parameter
    : type <identifier>

### argument
    : expression

### type_list
    : type { ',' type }

### parameter_list
    : parameter { ',' parameter }

### argument_list
    : argument { ',' argument }

## Compile unit syntax

### compile_unit
    : { directives }
    | { peripheral_declare }
    | { global_variable_declare }
    | { function_declare }

## Pre-Processor directive syntax

### directives
    : '#' script_compile_directive
    | '#' link_object_directive
    | '#' constant_define_directive

### script_compile_directive
    : 'compile' <string>

### link_object_directive
    : 'link' <string>

### constant_define_directive
    : 'const' <identifier> literal

## Define and Declare syntax

### peripheral_declare
    : 'using' <identifier> '=' type <identifier> '.' <identifier> '(' [type_list] ')' ';' 

### global_variable_declare
    : 'global' type <identifier> [ '=' literal ] ';'

### local_variable_declare
    : 'local' type <identifier> [ '=' expression ] ';'

### function_declare
    : 'function' type <identifier> '(' [parameter_list] ')' { block } 'end'

## Block syntax

### block
    : statement

### statement
    : empty_statement
    | local_variable_declare
    | for_statement
    | while_statement
    | if_statement
    | break_statement
    | return_statement
    | expression ';'

## Statement syntax

### empty_statement
    : ';'

### for_statement
    : 'for' '(' [ expression ] ';' [ expression ] ';' [ expression ] ')' { block } 'end'

### while_statement
    : 'while' '(' expression ')' { block } 'end'

### if_statement
    : 'if' '(' expression ')' { block } 'end'
    | 'if' '(' expression ')' { block } else_statement

### else_statement
    : 'else' if_statement
    | 'else' { block } 'end'

### break_statement
    : 'break' ';'

### return_statement
    : 'return' [ expression ] ';'

## Expression syntax

### expression
    : assignment_expression

### assignment_expression
    : condition_or_expression
    | assignment_expression { '=' expression }
    | assignment_expression { '+=' expression }
    | assignment_expression { '-=' expression }
    | assignment_expression { '*=' expression }
    | assignment_expression { '/=' expression }
    | assignment_expression { '&=' expression }
    | assignment_expression { '|=' expression }
    | assignment_expression { '^=' expression }

### condition_or_expression
    : condition_and_expression
    | condition_or_expression { '||' condition_and_expression }

### condition_and_expression
    : logical_or_expression
    | condition_and_expression { '&&' logical_or_expression }

### logical_or_expression
    : logical_exclusive_or_expression
    | logical_or_expression { '|' logical_exclusive_or_expression }

### logical_exclusive_or_expression
    : logical_and_expression
    | logical_exclusive_or_expression { '^' logical_and_expression }

### logical_and_expression
    : equality_expression
    | logical_and_expression { '&' equality_expression }

### equality_expression
    : relational_expression
    | equality_expression { '==' relational_expression }
    | equality_expression { '!=' relational_expression }

### relational_expression
    : shift_expression
    | relational_expression { '<' shift_expression }
    | relational_expression { '>' shift_expression }
    | relational_expression { '<=' shift_expression }
    | relational_expression { '>=' shift_expression }

### shift_expression
    : addsub_expression
    | shift_expression { '<<' addsub_expression }
    | shift_expression { '>>' addsub_expression }

### addsub_expression
    : muldiv_expression
    | addsub_expression { '+' muldiv_expression }
    | addsub_expression { '-' muldiv_expression }

### muldiv_expression
    : unary_expression
    | muldiv_expression { '*' unary_expression }
    | muldiv_expression { '/' unary_expression }

### unary_expression
    : post_unary_expression
    | '+' unary_expression
    | '-' unary_expression
    | '!' unary_expression
    | '++' unary_expression
    | '--' unary_expression

### post_unary_expression
    : primary_expression
    | primary_expression '(' [ argument_list ] ')'

### primary_expression
    : literal
    | <identifier>
    | paren_expression

### paren_expression
    : '(' expression ')'

*/

using System;
using System.Collections.Generic;
using SnowRabbit.Compiler.IO;
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.Compiler.Parser.SyntaxErrors;
using SnowRabbit.Compiler.Parser.SyntaxNodes;
using SnowRabbit.Compiler.Reporter;

namespace SnowRabbit.Compiler.Parser
{
    /// <summary>
    /// SnowRabbit の構文解析を行う構文解析クラスです
    /// </summary>
    public class SrParser
    {
        // メンバ変数定義
        private readonly ISrScriptStorage scriptStorage;
        private readonly SyntaxErrorReporter errorReporter;
        private readonly Stack<SrLexer> lexerStack;
        private SrLexer currentLexer;

        #region コンストラクタとAPIインターフェイス
        /// <summary>
        /// SrParser クラスのインスタンスを初期化します
        /// </summary>
        public SrParser() : this(new SrFileSystemScriptStorage())
        {
        }

        /// <summary>
        /// SrParser クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="storage">構文解析するスクリプトを読み込むストレージ</param>
        /// <exception cref="ArgumentNullException">storage が null です</exception>
        public SrParser(ISrScriptStorage storage) : this(storage, new SrCompileReportConsolePrinter())
        {
        }

        public SrParser(ISrScriptStorage storage, ISrCompileReportPrinter printer)
        {
            // 諸々初期化する
            scriptStorage = storage ?? throw new ArgumentNullException(nameof(storage));
            errorReporter = new SyntaxErrorReporter(printer);
            lexerStack = new Stack<SrLexer>();
        }

        /// <summary>
        /// 指定されたパスにあるスクリプトの構文解析をします。
        /// </summary>
        /// <param name="path">構文解析する対象となるスクリプトのパス</param>
        /// <returns>構文解析された抽象構文木のルートノードを返します</returns>
        /// <exception cref="ScriptNotFoundException">スクリプト '{path}' が見つけられませんでした。</exception>
        public SyntaxNode Parse(string path)
        {
            // ストレージからスクリプトを開いてレキサを生成する
            var textReader = scriptStorage.OpenRead(path);
            using (var lexer = new SrLexer(path, textReader ?? throw new ScriptNotFoundException(path)))
            {
                // 現在処理中のレキサがいるのならスタックに積んでおいて、処理するべきレキサを切り替える
                if (currentLexer != null) lexerStack.Push(currentLexer);
                currentLexer = lexer;

                // レキサに一番最初のトークンを読み込ませて、コンパイル単位の翻訳を始める
                lexer.ReadNextToken();
                var rootNode = ParseCompileUnit();

                // スタックにレキサが積まれていればポップして処理するべきレキサを戻して、ルートノードを返す
                currentLexer = lexerStack.Count > 0 ? lexerStack.Pop() : null;
                return rootNode;
            }
        }
        #endregion

        #region Utilities
        /// <summary>
        /// トークンが最後まで読み込まれたかどうかを調べます
        /// </summary>
        /// <returns>最後までトークンが読み込まれた場合は true を、まだ読み込まれていない場合は false を返します</returns>
        private bool CheckEndOfToken()
        {
            // そのままのプロパティ値を返す
            return currentLexer.EndOfToken;
        }

        /// <summary>
        /// 次のトークンを読み込みます
        /// </summary>
        private void ReadNextToken()
        {
            // 次のトークンを読み込むだけ
            currentLexer.ReadNextToken();
        }

        /// <summary>
        /// 指定されたトークンかどうかを調べます
        /// </summary>
        /// <param name="tokenKind">調べるトークン</param>
        /// <returns>指定されたトークンの場合は true を、異なる場合は false を返します</returns>
        private bool CheckToken(int tokenKind)
        {
            // そのまま比較結果を返す
            return currentLexer.LastReadToken.Kind == tokenKind;
        }

        /// <summary>
        /// 指定されたトークンかどうかを調べて、指定されたトークンの場合は次のトークンを読み込みます
        /// </summary>
        /// <param name="tokenKind">調べるトークン</param>
        /// <returns>指定されたトークンの場合は true を、異なる場合は false を返します</returns>
        private bool CheckTokenAndReadNext(int tokenKind)
        {
            // もしトークン比較結果をもらって異なる場合はそのまま失敗を返す
            if (!CheckToken(tokenKind)) return false;
            currentLexer.ReadNextToken();
            return true;
        }

        /// <summary>
        /// 指定されたトークンのいずれかに一致するかどうかを調べます
        /// </summary>
        /// <param name="tokenKinds">調べるトークン種別の配列</param>
        /// <returns>いずれかのトークンに一致する場合は true を、異なる場合は false を返します</returns>
        private bool CheckAnyToken(params int[] tokenKinds)
        {
            foreach (int kind in tokenKinds)
            {
                if (CheckToken(kind)) return true;
            }
            return false;
        }

        /// <summary>
        /// 代入記号トークンかどうかを調べます
        /// </summary>
        /// <returns>代入記号トークンであれば true を、違う場合は false を返します</returns>
        private bool CheckAssignmentSimbolToken()
        {
            // 代入記号関連のトークンがいずれかに一致すれば true を返す
            return CheckAnyToken(
                TokenKind.Equal,
                TokenKind.PlusEqual,
                TokenKind.MinusEqual,
                TokenKind.AsteriskEqual,
                TokenKind.SlashEqual,
                TokenKind.AndEqual,
                TokenKind.VerticalbarEqual,
                TokenKind.CircumflexEqual);
        }

        /// <summary>
        /// 二項演算式をパースする共通メソッドです
        /// </summary>
        /// <param name="parseNext">次の優先度のパース関数</param>
        /// <param name="tokenKinds">この優先度で処理する演算子トークン</param>
        /// <returns>パースされた構文ノード</returns>
        private SyntaxNode ParseBinaryExpression(Func<SyntaxNode> parseNext, params int[] tokenKinds)
        {
            SyntaxNode expression = parseNext();
            while (CheckAnyToken(tokenKinds))
            {
                GetCurrentTokenAndReadNext(out Token operation);
                ExpressionSyntaxNode thisExpression = new ExpressionSyntaxNode(operation);
                SyntaxNode rightExpression = parseNext();
                thisExpression.Add(expression);
                thisExpression.Add(rightExpression);
                expression = thisExpression;
            }
            return expression;
        }

        /// <summary>
        /// 現在の位置にいるトークンを取り出して、次のトークンを読み込みます
        /// </summary>
        /// <param name="token">取り出したトークンを設定する参照パラメータ</param>
        private void GetCurrentTokenAndReadNext(out Token token)
        {
            // 最後に読み取ったトークンを設定して次のトークンを読み込む
            token = currentLexer.LastReadToken;
            currentLexer.ReadNextToken();
        }

        /// <summary>
        /// パース結果が null でないことを要求します。null の場合は例外をスローします。
        /// </summary>
        /// <param name="node">パース結果のノード</param>
        /// <returns>null でない場合はそのノードを返します</returns>
        private SyntaxNode Require(SyntaxNode node)
        {
            return node ?? throw errorReporter.UnknownToken(currentLexer.LastReadToken);
        }

        /// <summary>
        /// 指定されたトークンが存在することを要求し、読み進めます。存在しない場合は例外をスローします。
        /// </summary>
        /// <param name="tokenKind">要求するトークン種別</param>
        /// <param name="expectedSymbol">エラーメッセージに表示する期待される記号</param>
        private void RequireToken(int tokenKind, string expectedSymbol)
        {
            if (!CheckTokenAndReadNext(tokenKind))
                throw errorReporter.NotSymbolEnd(currentLexer.LastReadToken, expectedSymbol);
        }

        /// <summary>
        /// 対になるトークンが存在することを要求し、読み進めます。存在しない場合は例外をスローします。
        /// </summary>
        /// <param name="tokenKind">要求するトークン種別</param>
        /// <param name="openSymbol">開き記号</param>
        /// <param name="closeSymbol">閉じ記号</param>
        private void RequireTokenPair(int tokenKind, string openSymbol, string closeSymbol)
        {
            if (!CheckTokenAndReadNext(tokenKind))
                throw errorReporter.NotSymbolPair(currentLexer.LastReadToken, openSymbol, closeSymbol);
        }
        #endregion

        #region 全パース関数
        #region Simple syntax
        private SyntaxNode ParseIdentifier()
        {
            if (!CheckToken(TokenKind.Identifier)) return null;
            GetCurrentTokenAndReadNext(out var token);
            return new IdentifierSyntaxNode(token);
        }

        private SyntaxNode ParseLiteral()
        {
            if (CheckAnyToken(TokenKind.Integer, TokenKind.Number, TokenKind.String,
                              SrTokenKind.True, SrTokenKind.False, SrTokenKind.Null))
            {
                GetCurrentTokenAndReadNext(out var token);
                return new LiteralSyntaxNode(token);
            }

            return null;
        }

        private SyntaxNode ParseType()
        {
            if (CheckAnyToken(SrTokenKind.TypeVoid, SrTokenKind.TypeInt, SrTokenKind.TypeNumber,
                              SrTokenKind.TypeString, SrTokenKind.TypeObject, SrTokenKind.TypeBool))
            {
                GetCurrentTokenAndReadNext(out var token);
                return new TypeSyntaxNode(token);
            }

            return null;
        }

        private SyntaxNode ParseParameter()
        {
            var type = ParseType();
            if (type == null) return null;
            var name = ParseIdentifier();
            if (name == null) return null;

            var node = new ParameterSyntaxNode();
            node.Add(type);
            node.Add(name);
            return node;
        }

        private SyntaxNode ParseArgument()
        {
            var argument = new ArgumentSyntaxNode(currentLexer.LastReadToken);

            var expression = ParseExpression();
            if (expression == null) return null;

            argument.Add(expression);
            return argument;
        }

        private SyntaxNode ParseTypeList()
        {
            var type = ParseType();
            if (type == null) return null;

            var typeList = new TypeListSyntaxNode();
            typeList.Add(type);
            while (CheckTokenAndReadNext(TokenKind.Comma))
            {
                type = ParseType();
                if (type == null) return null;
                typeList.Add(type);
            }

            return typeList;
        }

        private SyntaxNode ParseParameterList()
        {
            var parameter = ParseParameter();
            if (parameter == null) return null;

            var parameterList = new ParameterListSyntaxNode();
            parameterList.Add(parameter);
            while (CheckTokenAndReadNext(TokenKind.Comma))
            {
                parameter = ParseParameter();
                if (parameter == null) return null;
                parameterList.Add(parameter);
            }

            return parameterList;
        }

        private SyntaxNode ParseArgumentList()
        {
            var argumentList = new ArgumentListSyntaxNode(currentLexer.LastReadToken);
            var argument = ParseArgument();
            if (argument == null) return argumentList;

            argumentList.Add(argument);
            while (CheckTokenAndReadNext(TokenKind.Comma))
            {
                argument = ParseArgument();
                if (argument == null) return null;
                argumentList.Add(argument);
            }

            return argumentList;
        }
        #endregion

        #region Compile unit syntax
        private SyntaxNode ParseCompileUnit()
        {
            var compileUnit = new CompileUnitSyntaxNode();
            while (!CheckEndOfToken())
            {
                var node =
                    ParseDirectives() ??
                    ParsePeripheralDeclare() ??
                    ParseGlobalVariableDeclare() ??
                    ParseFunctionDeclare() ??
                    null;

                if (node == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
                compileUnit.Add(node);
            }

            return compileUnit.Children.Count > 0 ? compileUnit : null;
        }
        #endregion

        #region Pre-Processor directive syntax
        private SyntaxNode ParseDirectives()
        {
            if (!CheckTokenAndReadNext(TokenKind.Sharp)) return null;

            return
                ParseScriptCompileDirective() ??
                ParseLinkObjectDirective() ??
                ParseConstantDefineDirective() ??
                null;
        }

        private SyntaxNode ParseScriptCompileDirective()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.Compile)) return null;
            if (!CheckToken(TokenKind.String)) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
            GetCurrentTokenAndReadNext(out var token);
            return Parse(token.Text);
        }

        private SyntaxNode ParseLinkObjectDirective()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.Link)) return null;
            if (!CheckToken(TokenKind.String)) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
            GetCurrentTokenAndReadNext(out var token);
            return new LinkObjectDirectiveSyntaxNode(token);
        }

        private SyntaxNode ParseConstantDefineDirective()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.Const)) return null;
            var name = ParseIdentifier();
            if (name == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
            var literal = ParseLiteral();
            if (literal == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);

            var constant = new ConstantDefineDirectiveSyntaxNode();
            constant.Add(name);
            constant.Add(literal);
            return constant;
        }
        #endregion

        #region Define and Declare syntax
        private SyntaxNode ParsePeripheralDeclare()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.Using)) return null;

            var name = ParseIdentifier();
            if (name == null) return null;
            if (!CheckTokenAndReadNext(TokenKind.Equal)) throw errorReporter.UnknownToken(currentLexer.LastReadToken);

            var type = ParseType();
            if (type == null) return null;

            var peripheralName = ParseIdentifier();
            if (peripheralName == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
            if (!CheckTokenAndReadNext(TokenKind.Period)) throw errorReporter.UnknownToken(currentLexer.LastReadToken);

            var functionName = ParseIdentifier();
            if (functionName == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
            if (!CheckTokenAndReadNext(TokenKind.OpenParen)) throw errorReporter.UnknownToken(currentLexer.LastReadToken);

            var typeList = ParseTypeList();
            if (!CheckTokenAndReadNext(TokenKind.CloseParen)) throw errorReporter.NotSymbolPair(currentLexer.LastReadToken, "(", ")");
            if (!CheckTokenAndReadNext(TokenKind.Semicolon)) throw errorReporter.NotSymbolEnd(currentLexer.LastReadToken, ";");

            var peripheralDeclare = new PeripheralDeclareSyntaxNode();
            peripheralDeclare.Add(name);
            peripheralDeclare.Add(type);
            peripheralDeclare.Add(peripheralName);
            peripheralDeclare.Add(functionName);
            peripheralDeclare.Add(typeList);
            return peripheralDeclare;
        }

        private SyntaxNode ParseGlobalVariableDeclare()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.Global)) return null;

            var type = ParseType();
            if (type == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);

            var name = ParseIdentifier();
            if (name == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);

            var globalVariableDeclare = new GlobalVariableDeclareSyntaxNode();
            globalVariableDeclare.Add(type);
            globalVariableDeclare.Add(name);

            if (CheckTokenAndReadNext(TokenKind.Equal))
            {
                var literal = ParseLiteral();
                if (literal == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
                globalVariableDeclare.Add(literal);
            }

            if (!CheckTokenAndReadNext(TokenKind.Semicolon)) throw errorReporter.NotSymbolEnd(currentLexer.LastReadToken, ";");
            return globalVariableDeclare;
        }

        private SyntaxNode ParseLocalVariableDeclare()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.Local)) return null;

            var type = ParseType();
            if (type == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);

            var name = ParseIdentifier();
            if (name == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);

            var localVariableDeclare = new LocalVariableDeclareSyntaxNode();
            localVariableDeclare.Add(type);
            localVariableDeclare.Add(name);

            if (CheckTokenAndReadNext(TokenKind.Equal))
            {
                var literal = ParseExpression();
                if (literal == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
                localVariableDeclare.Add(literal);
            }

            if (!CheckTokenAndReadNext(TokenKind.Semicolon)) throw errorReporter.NotSymbolEnd(currentLexer.LastReadToken, ";");
            return localVariableDeclare;
        }

        private SyntaxNode ParseFunctionDeclare()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.Function)) return null;
            var functionDeclare = new FunctionDeclareSyntaxNode();

            var type = ParseType();
            if (type == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
            functionDeclare.Add(type);

            var name = ParseIdentifier();
            if (name == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
            functionDeclare.Add(name);

            if (!CheckTokenAndReadNext(TokenKind.OpenParen)) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
            var parameterList = ParseParameterList();
            functionDeclare.Add(parameterList);
            if (!CheckTokenAndReadNext(TokenKind.CloseParen)) throw errorReporter.NotSymbolPair(currentLexer.LastReadToken, "(", ")");

            while (!CheckToken(SrTokenKind.End))
            {
                var block = ParseBlock();
                if (block == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
                functionDeclare.Add(block);
            }

            ReadNextToken();
            return functionDeclare;
        }
        #endregion

        #region Block syntax
        private SyntaxNode ParseBlock()
        {
            return ParseStatement();
        }

        private SyntaxNode ParseStatement()
        {
            var result =
                ParseEmptyStatement() ??
                ParseLocalVariableDeclare() ??
                ParseForStatement() ??
                ParseWhileStatement() ??
                ParseIfStatement() ??
                ParseBreakStatement() ??
                ParseReturnStatement() ??
                null;
            if (result != null) return result;

            result = ParseExpression();
            if (result == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
            if (!CheckTokenAndReadNext(TokenKind.Semicolon)) throw errorReporter.NotSymbolEnd(currentLexer.LastReadToken, ";");
            return result;
        }
        #endregion

        #region Statement syntax
        private SyntaxNode ParseEmptyStatement()
        {
            if (!CheckToken(TokenKind.Semicolon)) return null;
            GetCurrentTokenAndReadNext(out var token);
            return new EmptyStatementSyntaxNode(token);
        }

        private SyntaxNode ParseForStatement()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.For)) return null;
            if (!CheckTokenAndReadNext(TokenKind.OpenParen)) return null;
            var forStatement = new ForStatementSyntaxNode();

            // 初期化式（省略可能）
            forStatement.Add(ParseOptionalForClause(TokenKind.Semicolon));

            // 条件式（省略可能）
            forStatement.Add(ParseOptionalForClause(TokenKind.Semicolon));

            // ループ式（省略可能、終端は閉じ括弧）
            forStatement.Add(ParseOptionalForClause(TokenKind.CloseParen));

            // ブロック本体
            while (!CheckToken(SrTokenKind.End))
            {
                forStatement.Add(Require(ParseBlock()));
            }

            ReadNextToken();
            return forStatement;
        }

        /// <summary>
        /// for文の各節（初期化、条件、ループ）をパースします。省略可能です。
        /// </summary>
        /// <param name="terminatorToken">この節を終了するトークン</param>
        /// <returns>パースされた式、または省略時は null</returns>
        private SyntaxNode ParseOptionalForClause(int terminatorToken)
        {
            if (CheckTokenAndReadNext(terminatorToken))
            {
                return null;
            }

            SyntaxNode expression = Require(ParseExpression());
            if (terminatorToken == TokenKind.CloseParen)
            {
                RequireTokenPair(terminatorToken, "(", ")");
            }
            else
            {
                RequireToken(terminatorToken, ";");
            }
            return expression;
        }

        private SyntaxNode ParseWhileStatement()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.While)) return null;
            if (!CheckTokenAndReadNext(TokenKind.OpenParen)) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
            var whileStatement = new WhileStatementSyntaxNode();

            whileStatement.Add(Require(ParseExpression()));
            RequireTokenPair(TokenKind.CloseParen, "(", ")");

            while (!CheckToken(SrTokenKind.End))
            {
                whileStatement.Add(Require(ParseBlock()));
            }

            ReadNextToken();
            return whileStatement;
        }

        private SyntaxNode ParseIfStatement()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.If)) return null;
            if (!CheckTokenAndReadNext(TokenKind.OpenParen)) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
            var ifStatement = new IfStatementSyntaxNode();

            ifStatement.Add(Require(ParseExpression()));
            RequireTokenPair(TokenKind.CloseParen, "(", ")");

            SyntaxNode elseStatement = null;
            while (!CheckToken(SrTokenKind.End) && (elseStatement = ParseElseStatement()) == null)
            {
                ifStatement.Add(Require(ParseBlock()));
            }

            if (elseStatement != null)
            {
                ifStatement.Add(elseStatement);
                return ifStatement;
            }
            else if (CheckTokenAndReadNext(SrTokenKind.End))
            {
                return ifStatement;
            }

            throw errorReporter.NotSymbolEnd(currentLexer.LastReadToken, "end");
        }

        private SyntaxNode ParseElseStatement()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.Else)) return null;
            var elseStatement = new ElseStatementSyntaxNode();

            var ifStatement = ParseIfStatement();
            if (ifStatement != null)
            {
                elseStatement.Add(ifStatement);
                return elseStatement;
            }

            while (!CheckToken(SrTokenKind.End))
            {
                elseStatement.Add(Require(ParseBlock()));
            }

            ReadNextToken();
            return elseStatement;
        }

        private SyntaxNode ParseBreakStatement()
        {
            if (CheckTokenAndReadNext(SrTokenKind.Break) &&
                CheckTokenAndReadNext(TokenKind.Semicolon))
            {
                return new BreakStatementSyntaxNode();
            }

            return null;
        }

        private SyntaxNode ParseReturnStatement()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.Return)) return null;
            if (CheckTokenAndReadNext(TokenKind.Semicolon))
            {
                return new ReturnStatementSyntaxNode();
            }

            var expression = ParseExpression();
            if (expression == null) throw errorReporter.UnknownToken(currentLexer.LastReadToken);
            if (!CheckTokenAndReadNext(TokenKind.Semicolon)) throw errorReporter.NotSymbolEnd(currentLexer.LastReadToken, ";");
            var returnStatement = new ReturnStatementSyntaxNode();
            returnStatement.Add(expression);
            return returnStatement;
        }
        #endregion

        #region Expression syntax
        private SyntaxNode ParseExpression()
        {
            return ParseAssignmentExpression();
        }

        private SyntaxNode ParseAssignmentExpression()
        {
            var expression = ParseConditionOrExpression();
            while (CheckAssignmentSimbolToken())
            {
                GetCurrentTokenAndReadNext(out var operation);
                var thisExpression = new ExpressionSyntaxNode(operation);
                var rightExpression = ParseExpression();
                thisExpression.Add(expression);
                thisExpression.Add(rightExpression);
                expression = thisExpression;
            }

            return expression;
        }

        private SyntaxNode ParseConditionOrExpression()
            => ParseBinaryExpression(ParseConditionAndExpression, TokenKind.DoubleVerticalbar);

        private SyntaxNode ParseConditionAndExpression()
            => ParseBinaryExpression(ParseLogicalOrExpression, TokenKind.DoubleAnd);

        private SyntaxNode ParseLogicalOrExpression()
            => ParseBinaryExpression(ParseLogicalExclusiveOrExpression, TokenKind.Verticalbar);

        private SyntaxNode ParseLogicalExclusiveOrExpression()
            => ParseBinaryExpression(ParseLogicalAndExpression, TokenKind.Circumflex);

        private SyntaxNode ParseLogicalAndExpression()
            => ParseBinaryExpression(ParseEqualityExpression, TokenKind.And);

        private SyntaxNode ParseEqualityExpression()
            => ParseBinaryExpression(ParseRelationalExpression, TokenKind.DoubleEqual, TokenKind.NotEqual);

        private SyntaxNode ParseRelationalExpression()
            => ParseBinaryExpression(ParseShiftExpression, TokenKind.OpenAngle, TokenKind.CloseAngle, TokenKind.LesserEqual, TokenKind.GreaterEqual);

        private SyntaxNode ParseShiftExpression()
            => ParseBinaryExpression(ParseAddSubExpression, TokenKind.DoubleOpenAngle, TokenKind.DoubleCloseAngle);

        private SyntaxNode ParseAddSubExpression()
            => ParseBinaryExpression(ParseMulDivExpression, TokenKind.Plus, TokenKind.Minus);

        private SyntaxNode ParseMulDivExpression()
            => ParseBinaryExpression(ParseUnaryExpression, TokenKind.Asterisk, TokenKind.Slash);

        private SyntaxNode ParseUnaryExpression()
        {
            if (CheckAnyToken(TokenKind.Plus, TokenKind.Minus, TokenKind.Exclamation,
                              TokenKind.DoublePlus, TokenKind.DoubleMinus))
            {
                GetCurrentTokenAndReadNext(out var operation);
                var thisExpression = new ExpressionSyntaxNode(operation);
                var unaryExpression = ParseUnaryExpression();
                thisExpression.Add(unaryExpression);
                return thisExpression;
            }

            return ParsePostUnaryExpression();
        }

        private SyntaxNode ParsePostUnaryExpression()
        {
            var expression = ParsePrimaryExpression();
            if (!CheckTokenAndReadNext(TokenKind.OpenParen)) return expression;
            var functionCall = new FunctionCallSyntaxNode();
            functionCall.Add(expression);
            functionCall.Add(ParseArgumentList());
            if (!CheckTokenAndReadNext(TokenKind.CloseParen)) throw errorReporter.NotSymbolPair(currentLexer.LastReadToken, "(", ")");
            return functionCall;
        }

        private SyntaxNode ParsePrimaryExpression()
        {
            return
                ParseLiteral() ??
                ParseIdentifier() ??
                ParseParenExpression() ??
                null;
        }

        private SyntaxNode ParseParenExpression()
        {
            if (!CheckTokenAndReadNext(TokenKind.OpenParen)) return null;
            var expression = ParseExpression();
            if (!CheckTokenAndReadNext(TokenKind.CloseParen)) return null;
            return expression;
        }
        #endregion
        #endregion
    }
}