// zlib/libpng License
//
// Copyright(c) 2019 Sinoa
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
    | 'if' '(' expression ')' { block } 'else' { block } 'end'

### break_statement
    : 'break' ';'

### return_statement
    : 'return' [ expression ] ';'



## Expression syntax

### expression
    : assignment_expression

### assignment_expression
    : condition_or_expression
    | assignment_expression { '=' condition_or_expression }
    | assignment_expression { '+=' condition_or_expression }
    | assignment_expression { '-=' condition_or_expression }
    | assignment_expression { '*=' condition_or_expression }
    | assignment_expression { '/=' condition_or_expression }
    | assignment_expression { '&=' condition_or_expression }
    | assignment_expression { '|=' condition_or_expression }
    | assignment_expression { '^=' condition_or_expression }

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
    : function_call
    | '+' unary_expression
    | '-' unary_expression
    | '!' unary_expression

### function_call
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
using SnowRabbit.Compiler.Parser.SyntaxNodes;

namespace SnowRabbit.Compiler.Parser
{
    /// <summary>
    /// SnowRabbit の構文解析を行う構文解析クラスです
    /// </summary>
    public class SrParser
    {
        // メンバ変数定義
        private readonly ISrScriptStorage scriptStorage;
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
        public SrParser(ISrScriptStorage storage)
        {
            // 諸々初期化する
            scriptStorage = storage ?? throw new ArgumentNullException(nameof(storage));
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
        /// 代入記号トークンかどうかを調べます
        /// </summary>
        /// <returns>代入記号トークンであれば true を、違う場合は false を返します</returns>
        private bool CheckAssignmentSimbolToken()
        {
            // 代入記号関連のトークンがいずれかに一致すれば true を返す
            return
                CheckToken(TokenKind.Equal) ||
                CheckToken(TokenKind.PlusEqual) ||
                CheckToken(TokenKind.MinusEqual) ||
                CheckToken(TokenKind.AsteriskEqual) ||
                CheckToken(TokenKind.SlashEqual) ||
                CheckToken(TokenKind.AndEqual) ||
                CheckToken(TokenKind.VerticalbarEqual) ||
                CheckToken(TokenKind.CircumflexEqual);
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
            if (CheckToken(TokenKind.Integer) ||
                CheckToken(TokenKind.Number) ||
                CheckToken(TokenKind.String) ||
                CheckToken(SrTokenKind.True) ||
                CheckToken(SrTokenKind.False) ||
                CheckToken(SrTokenKind.Null))
            {
                GetCurrentTokenAndReadNext(out var token);
                return new LiteralSyntaxNode(token);
            }


            return null;
        }


        private SyntaxNode ParseType()
        {
            if (CheckToken(SrTokenKind.TypeVoid) ||
                CheckToken(SrTokenKind.TypeInt) ||
                CheckToken(SrTokenKind.TypeNumber) ||
                CheckToken(SrTokenKind.TypeString) ||
                CheckToken(SrTokenKind.TypeObject) ||
                CheckToken(SrTokenKind.TypeBool))
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
            var expression = ParseExpression();
            if (expression == null) return null;


            var argument = new ArgumentSyntaxNode();
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
            var argument = ParseArgument();
            if (argument == null) return null;


            var argumentList = new ArgumentListSyntaxNode();
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


                if (node == null) return null;
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
            if (!CheckToken(TokenKind.String)) return null;
            GetCurrentTokenAndReadNext(out var token);
            return Parse(token.Text);
        }


        private SyntaxNode ParseLinkObjectDirective()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.Link)) return null;
            if (!CheckToken(TokenKind.String)) return null;
            GetCurrentTokenAndReadNext(out var token);
            return new LinkObjectDirectiveSyntaxNode(token);
        }


        private SyntaxNode ParseConstantDefineDirective()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.Const)) return null;
            var name = ParseIdentifier();
            if (name == null) return null;
            var literal = ParseLiteral();
            if (literal == null) return null;


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
            if (!CheckTokenAndReadNext(TokenKind.Equal)) return null;


            var type = ParseType();
            if (type == null) return null;


            var peripheralName = ParseIdentifier();
            if (peripheralName == null && !CheckTokenAndReadNext(TokenKind.Period)) return null;


            var functionName = ParseIdentifier();
            if (functionName == null && !CheckTokenAndReadNext(TokenKind.OpenParen)) return null;


            var typeList = ParseTypeList();
            if (!CheckTokenAndReadNext(TokenKind.CloseParen)) return null;
            if (!CheckTokenAndReadNext(TokenKind.Semicolon)) return null;


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
            if (type == null) return null;


            var name = ParseIdentifier();
            if (name == null) return null;


            var globalVariableDeclare = new GlobalVariableDeclareSyntaxNode();
            globalVariableDeclare.Add(type);
            globalVariableDeclare.Add(name);


            if (CheckTokenAndReadNext(TokenKind.Equal))
            {
                var literal = ParseLiteral();
                if (literal == null) return null;
                globalVariableDeclare.Add(literal);
            }


            if (!CheckTokenAndReadNext(TokenKind.Semicolon)) return null;
            return globalVariableDeclare;
        }


        private SyntaxNode ParseLocalVariableDeclare()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.Local)) return null;


            var type = ParseType();
            if (type == null) return null;


            var name = ParseIdentifier();
            if (name == null) return null;


            var localVariableDeclare = new LocalVariableDeclareSyntaxNode();
            localVariableDeclare.Add(type);
            localVariableDeclare.Add(name);


            if (CheckTokenAndReadNext(TokenKind.Equal))
            {
                var literal = ParseExpression();
                if (literal == null) return null;
                localVariableDeclare.Add(literal);
            }


            if (!CheckTokenAndReadNext(TokenKind.Semicolon)) return null;
            return localVariableDeclare;
        }


        private SyntaxNode ParseFunctionDeclare()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.Function)) return null;
            var functionDeclare = new FunctionDeclareSyntaxNode();


            var type = ParseType();
            if (type == null) return null;
            functionDeclare.Add(type);


            var name = ParseIdentifier();
            if (name == null) return null;
            functionDeclare.Add(name);


            if (!CheckTokenAndReadNext(TokenKind.OpenParen)) return null;
            var parameterList = ParseParameterList();
            functionDeclare.Add(parameterList);
            if (!CheckTokenAndReadNext(TokenKind.CloseParen)) return null;


            while (!CheckToken(SrTokenKind.End))
            {
                var block = ParseBlock();
                if (block == null) return null;
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
            if (result == null || !CheckTokenAndReadNext(TokenKind.Semicolon)) return null;
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


            if (CheckTokenAndReadNext(TokenKind.Semicolon))
            {
                forStatement.Add(null);
            }
            else
            {
                var initializeExpression = ParseExpression();
                if (initializeExpression == null) return null;
                forStatement.Add(initializeExpression);
            }


            if (CheckTokenAndReadNext(TokenKind.Semicolon))
            {
                forStatement.Add(null);
            }
            else
            {
                var conditionExpression = ParseExpression();
                if (conditionExpression == null) return null;
                forStatement.Add(conditionExpression);
            }


            if (CheckTokenAndReadNext(TokenKind.CloseParen))
            {
                forStatement.Add(null);
            }
            else
            {
                var loopExpression = ParseExpression();
                if (loopExpression == null) return null;
                forStatement.Add(loopExpression);
            }


            while (!CheckToken(SrTokenKind.End))
            {
                var block = ParseBlock();
                if (block == null) return null;
                forStatement.Add(block);
            }


            ReadNextToken();
            return forStatement;
        }


        private SyntaxNode ParseWhileStatement()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.While)) return null;
            if (!CheckTokenAndReadNext(TokenKind.OpenParen)) return null;
            var whileStatement = new WhileStatementSyntaxNode();


            var conditionExpression = ParseExpression();
            if (conditionExpression == null) return null;
            whileStatement.Add(conditionExpression);


            if (!CheckTokenAndReadNext(TokenKind.CloseParen)) return null;


            while (!CheckToken(SrTokenKind.End))
            {
                var block = ParseBlock();
                if (block == null) return null;
                whileStatement.Add(block);
            }


            ReadNextToken();
            return whileStatement;
        }


        private SyntaxNode ParseIfStatement()
        {
            if (!CheckTokenAndReadNext(SrTokenKind.If)) return null;
            if (!CheckTokenAndReadNext(TokenKind.OpenParen)) return null;
            var ifStatement = new IfStatementSyntaxNode();


            var expression = ParseExpression();
            if (expression == null) return null;
            ifStatement.Add(expression);


            if (!CheckTokenAndReadNext(TokenKind.CloseParen)) return null;


            while (!CheckToken(SrTokenKind.End) && !CheckToken(SrTokenKind.Else))
            {
                ReadNextToken();
                var block = ParseBlock();
                if (block == null) return null;
                ifStatement.Add(block);
            }

            if (CheckTokenAndReadNext(SrTokenKind.End))
            {
                return ifStatement;
            }


            if (!CheckTokenAndReadNext(SrTokenKind.Else)) return null;


            while (!CheckTokenAndReadNext(SrTokenKind.End))
            {
                var block = ParseBlock();
                if (block == null) return null;
                ifStatement.Add(block);
            }


            return ifStatement;
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
            if (expression == null) return null;
            if (!CheckTokenAndReadNext(TokenKind.Semicolon)) return null;
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
                var rightExpression = ParseConditionOrExpression();
                thisExpression.Add(expression);
                thisExpression.Add(rightExpression);
                expression = thisExpression;
            }


            return expression;
        }


        private SyntaxNode ParseConditionOrExpression()
        {
            var expression = ParseConditionAndExpression();
            while (CheckToken(TokenKind.DoubleVerticalbar))
            {
                GetCurrentTokenAndReadNext(out var operation);
                var thisExpression = new ExpressionSyntaxNode(operation);
                var rightExpression = ParseConditionAndExpression();
                thisExpression.Add(expression);
                thisExpression.Add(rightExpression);
                expression = thisExpression;
            }


            return expression;
        }


        private SyntaxNode ParseConditionAndExpression()
        {
            var expression = ParseLogicalOrExpression();
            while (CheckToken(TokenKind.DoubleAnd))
            {
                GetCurrentTokenAndReadNext(out var operation);
                var thisExpression = new ExpressionSyntaxNode(operation);
                var rightExpression = ParseLogicalOrExpression();
                thisExpression.Add(expression);
                thisExpression.Add(rightExpression);
                expression = thisExpression;
            }


            return expression;
        }


        private SyntaxNode ParseLogicalOrExpression()
        {
            var expression = ParseLogicalExclusiveOrExpression();
            while (CheckToken(TokenKind.Verticalbar))
            {
                GetCurrentTokenAndReadNext(out var operation);
                var thisExpression = new ExpressionSyntaxNode(operation);
                var rightExpression = ParseLogicalExclusiveOrExpression();
                thisExpression.Add(expression);
                thisExpression.Add(rightExpression);
                expression = thisExpression;
            }


            return expression;
        }


        private SyntaxNode ParseLogicalExclusiveOrExpression()
        {
            var expression = ParseLogicalAndExpression();
            while (CheckToken(TokenKind.Circumflex))
            {
                GetCurrentTokenAndReadNext(out var operation);
                var thisExpression = new ExpressionSyntaxNode(operation);
                var rightExpression = ParseLogicalAndExpression();
                thisExpression.Add(expression);
                thisExpression.Add(rightExpression);
                expression = thisExpression;
            }


            return expression;
        }


        private SyntaxNode ParseLogicalAndExpression()
        {
            var expression = ParseEqualityExpression();
            while (CheckToken(TokenKind.And))
            {
                GetCurrentTokenAndReadNext(out var operation);
                var thisExpression = new ExpressionSyntaxNode(operation);
                var rightExpression = ParseEqualityExpression();
                thisExpression.Add(expression);
                thisExpression.Add(rightExpression);
                expression = thisExpression;
            }


            return expression;
        }


        private SyntaxNode ParseEqualityExpression()
        {
            var expression = ParseRelationalExpression();
            while (CheckToken(TokenKind.DoubleEqual) || CheckToken(TokenKind.NotEqual))
            {
                GetCurrentTokenAndReadNext(out var operation);
                var thisExpression = new ExpressionSyntaxNode(operation);
                var rightExpression = ParseRelationalExpression();
                thisExpression.Add(expression);
                thisExpression.Add(rightExpression);
                expression = thisExpression;
            }


            return expression;
        }


        private SyntaxNode ParseRelationalExpression()
        {
            var expression = ParseShiftExpression();
            while (CheckToken(TokenKind.OpenAngle) || CheckToken(TokenKind.CloseAngle) || CheckToken(TokenKind.LesserEqual) || CheckToken(TokenKind.GreaterEqual))
            {
                GetCurrentTokenAndReadNext(out var operation);
                var thisExpression = new ExpressionSyntaxNode(operation);
                var rightExpression = ParseShiftExpression();
                thisExpression.Add(expression);
                thisExpression.Add(rightExpression);
                expression = thisExpression;
            }


            return expression;
        }


        private SyntaxNode ParseShiftExpression()
        {
            var expression = ParseAddSubExpression();
            while (CheckToken(TokenKind.DoubleOpenAngle) || CheckToken(TokenKind.DoubleCloseAngle))
            {
                GetCurrentTokenAndReadNext(out var operation);
                var thisExpression = new ExpressionSyntaxNode(operation);
                var rightExpression = ParseAddSubExpression();
                thisExpression.Add(expression);
                thisExpression.Add(rightExpression);
                expression = thisExpression;
            }


            return expression;
        }


        private SyntaxNode ParseAddSubExpression()
        {
            var expression = ParseMulDivExpression();
            while (CheckToken(TokenKind.Plus) || CheckToken(TokenKind.Minus))
            {
                GetCurrentTokenAndReadNext(out var operation);
                var thisExpression = new ExpressionSyntaxNode(operation);
                var rightExpression = ParseMulDivExpression();
                thisExpression.Add(expression);
                thisExpression.Add(rightExpression);
                expression = thisExpression;
            }


            return expression;
        }


        private SyntaxNode ParseMulDivExpression()
        {
            var expression = ParseUnaryExpression();
            while (CheckToken(TokenKind.Asterisk) || CheckToken(TokenKind.Slash))
            {
                GetCurrentTokenAndReadNext(out var operation);
                var thisExpression = new ExpressionSyntaxNode(operation);
                var rightExpression = ParseUnaryExpression();
                thisExpression.Add(expression);
                thisExpression.Add(rightExpression);
                expression = thisExpression;
            }


            return expression;
        }


        private SyntaxNode ParseUnaryExpression()
        {
            if (CheckToken(TokenKind.Plus) || CheckToken(TokenKind.Minus) || CheckToken(TokenKind.Exclamation))
            {
                GetCurrentTokenAndReadNext(out var operation);
                var thisExpression = new ExpressionSyntaxNode(operation);
                var unaryExpression = ParseUnaryExpression();
                thisExpression.Add(unaryExpression);
                return thisExpression;
            }


            return ParseFunctionCall();
        }


        private SyntaxNode ParseFunctionCall()
        {
            var expression = ParsePrimaryExpression();
            if (!CheckTokenAndReadNext(TokenKind.OpenParen)) return expression;
            var functionCall = new FunctionCallSyntaxNode();
            functionCall.Add(expression);
            functionCall.Add(ParseArgumentList());
            if (!CheckTokenAndReadNext(TokenKind.CloseParen)) return null;
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