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

## Compile unit syntax

### compile_unit
    : directive
    | peripheral_declare
    | global_variable_declare
    | function_declare


## Common syntax

### non_void_types
    : 'int'
    | 'number'
    | 'string'
    | 'object'
    | 'bool'

### types
    : 'void'
    | non_void_types

### return_type
    : types


## Pre-Processor directive syntax

### directive
    : '#' directives

### directives
    : 'link' link_object_name
    | 'compile' script_name
    | 'const' constant_var_name constant_value

### link_object_name
    : '<string>'

### script_name
    : '<string>'

### constant_var_name
    : '<identifier>'

### constant_value
    : '<integer>'
    | '<number>'
    | '<string>'


## Peripheral syntax

### peripheral_declare
    : 'using' peripheral_function_name '=' return_type import_peripheral_name '.' import_peripheral_function_name '(' [type_list] ')' ';' 

### peripheral_function_name
    : '<identifier>'

### import_peripheral_name
    : '<identifier>'

### import_peripheral_function_name
    : '<identifier>'

### type_list
    : non_void_types { ',' non_void_types }


## GlobalVariable syntax

### global_variable_declare
    : 'global' non_void_types global_var_name ';'

### global_var_name
    : '<identifier>'


## Function declare syntax

### function_declare
    : 'function' return_type function_name '(' [argument_list] ')' { block } 'end'

### function_name
    : '<identifier>'

### argument_list
    : argument { ',' argument }

### argument
    : non_void_types argument_name

### argument_name
    : '<identifier>'


## Block syntax

### block
    : statement_list

### statement_list
    : ';'
    | local_var_declare
    | for_statement
    | while_statement
    | if_statement
    | break_statement
    | return_statement
    | expression


## LocalVariable declare syntax

### local_var_declare
    : 'local' non_void_types local_var_name ';'

### local_var_name
    : '<identifier>'


## For statement syntax

### for_statement
    : 'for' '(' [for_initializer] ';' [for_condition] ';' [for_iterator] ')' { block } 'end'

### for_initializer
    : expression

### for_condition
    : expression

### for_iterator
    : expression


## While statement syntax

### while_statement
    : 'while' '(' while_condition ')' { block } 'end'

### while_condition
    : expression


## If statement syntax

### if_statement
    : 'if' '(' if_condition ')' { block } { 'else' 'if' '(' if_condition ')' { block } } 'end'

### if_condition
    : expression


## Break statement syntax

### break_statement
    : 'break' ';'


## Return statement syntax

### return_statement
    : 'return' [ expression ] ';'


## Expression syntax

### expression
    : '(' expression ')'
    | simple_expression

### simple_expression
    : assignment_expression

### assignment_expression
    : condition_or_expression
    | unary_expression '=' expression
    | unary_expression '+=' expression
    | unary_expression '-=' expression
    | unary_expression '*=' expression
    | unary_expression '/=' expression
    | unary_expression '&=' expression
    | unary_expression '|=' expression
    | unary_expression '^=' expression

### condition_or_expression
    : condition_and_expression
    | condition_or_expression { '||' condition_and_expression }

### condition_and_expression
    : or_expression
    | condition_and_expression { '&&' or_expression }

### or_expression
    : exclusive_or_expression
    | or_expression { '|' exclusive_or_expression }

### exclusive_or_expression
    : and_expression
    | exclusive_or_expression { '^' and_expression }

### and_expression
    : equality_expression
    | and_expression { '&' equality_expression }

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
    : primary_expression
    | '+' primary_expression
    | '-' primary_expression
    | '!' primary_expression

### primary_expression
    : global_var_name
    | local_var_name
    | argument_name
    | literal
    | function_call

### literal
    : '<integer>'
    | '<number>'
    | '<string>'
    | 'true'
    | 'false'

### function_call
    : function_name '(' { expression } ')'
    | import_peripheral_function_name '(' { expression } ')'

*/

/*
using System;
using System.Collections.Generic;
using System.IO;
using SnowRabbit.Compiler.Lexer;

namespace SnowRabbit.Compiler.Parser
{
    /// <summary>
    /// Carrot スクリプトコードの構文解析及び実行コードを生成するクラスです
    /// </summary>
    public class SrParser
    {
        // クラス変数定義
        private static readonly Dictionary<int, (int left, int right)> OperatorPriorityTable;
        private static readonly int UnaryOperatorPriority = 20;

        // メンバ変数定義
        private CccBinaryCoder coder;
        private IScriptStorage storage;
        private ICccParserLogger logger;
        private Stack<ParserContext> contextStack;
        private ParserContext currentContext;
        private string currentParseFunctionName;



        #region Constructor and initialize
        /// <summary>
        /// CccParser クラスの初期化をします
        /// </summary>
        static SrParser()
        {
            // 演算子の優先順位テーブルを初期化する
            OperatorPriorityTable = new Dictionary<int, (int left, int right)>()
            {
                // 優先順位は値が大きいほど高い、左右の優先順位値は等価なら左結合あるいは非結合、他は低い値から高い値への結合規則になります（殆どの場合は右結合）
                { SrTokenKind.Asterisk, (15, 15) }, { SrTokenKind.Slash, (15, 15) }, { SrTokenKind.Percent, (15, 15) },      // '*', '/', '%'
                { SrTokenKind.Plus, (12, 12) }, { SrTokenKind.Minus, (12, 12) },                                              // '+', '-'
                { SrTokenKind.DoubleAnd, (10, 10) }, { SrTokenKind.DoubleVerticalbar, (10, 10) },                             // '&&', '||'
                { SrTokenKind.DoubleCloseAngle, (7, 6) }, { SrTokenKind.DoubleOpenAngle, (7, 6) },                            // '>>', '<<'
                { SrTokenKind.DoubleEqual, (5, 5) }, { SrTokenKind.CloseAngle, (5, 5) }, { SrTokenKind.OpenAngle, (5, 5) },  // '==', '>', '<'
                { SrTokenKind.GreaterEqual, (5, 5) }, { SrTokenKind.LesserEqual, (5, 5) }, { SrTokenKind.NotEqual, (5, 5) }, // '>=', '<=', '!='
                { SrTokenKind.PlusEqual, (2, 1) }, { SrTokenKind.MinusEqual, (2, 1) },                                        // '+=', '-='
                { SrTokenKind.Equal, (2, 1) } ,                                                                                // '='
            };
        }


        /// <summary>
        /// CccParser クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="logger">構文解析ログを出力するロガー</param>
        /// <exception cref="ArgumentNullException">storage が null です</exception>
        /// <exception cref="ArgumentNullException">logger が null です</exception>
        public SrParser(IScriptStorage storage, ICccParserLogger logger)
        {
            // 初期化をする
            contextStack = new Stack<ParserContext>();
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        #endregion


        #region Compile function
        /// <summary>
        /// コンパイルを行い実行コードを出力します
        /// </summary>
        /// <param name="scriptName">コンパイルするスクリプト名</param>
        /// <param name="outputStream">コンパイルした結果を出力する出力ストリーム</param>
        public void Compile(string scriptName, Stream outputStream)
        {
            // レキサのリセットとバイナリコーダーの初期化をする
            currentContext = new ParserContext(scriptName, this);
            contextStack.Push(currentContext);
            coder = new CccBinaryCoder(outputStream ?? throw new ArgumentNullException(nameof(outputStream)));


            Compile();
        }


        /// <summary>
        /// 新しいコンテキストを用意してネストされたスクリプトのコンパイルを行います
        /// </summary>
        /// <param name="scriptName">コンパイルするべきスクリプト名</param>
        private void Compile(string scriptName)
        {
            // コンテキストスタックをなめる
            foreach (var context in contextStack)
            {
                // もし同じスクリプト名が登場したら
                if (context.ScriptName == scriptName)
                {
                    // 再帰コンパイルしている恐れがあるため例外を吐く
                    ThrowExceptionRecursiveScriptCompile(scriptName);
                    return;
                }
            }


            currentContext = new ParserContext(scriptName, this);
            contextStack.Push(currentContext);


            Compile();
        }


        private void Compile()
        {
            ParseCompileUnit();
            var context = contextStack.Pop();
            context.Lexer.Dispose();


            if (contextStack.Count > 0)
            {
                currentContext = contextStack.Peek();
                return;
            }


            coder.OutputExecuteCode();
        }
        #endregion


        #region Disassemble function
        public static string Disassemble(Stream programStream)
        {
            return CccBinaryCoder.DisassembleExecuteCode(programStream);
        }
        #endregion


        #region Parse
        /// <summary>
        /// コンパイル単位のルートになる構文解析関数です
        /// </summary>
        private void ParseCompileUnit()
        {
            currentContext.Lexer.ReadNextToken();
            while (!currentContext.Lexer.EndOfToken)
            {
                ParseDirective();
                ParsePeripheralDeclare();
                ParseGlobalVariableDeclare();
                ParseFunctionDeclare();
            }
        }


        #region Parse directive
        private void ParseDirective()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            while (token.Kind == SrTokenKind.Sharp)
            {
                currentContext.Lexer.ReadNextToken();
                ParseDirectives();
            }
        }


        private void ParseDirectives()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            switch (token.Kind)
            {
                case SrTokenKind.Link:
                    currentContext.Lexer.ReadNextToken();
                    ParseLinkDirective();
                    break;


                case SrTokenKind.Compile:
                    currentContext.Lexer.ReadNextToken();
                    ParseCompileDirective();
                    break;


                case SrTokenKind.Const:
                    currentContext.Lexer.ReadNextToken();
                    ParseConstantDirective();
                    break;


                default:
                    ThrowExceptionUnknownDirective(token.Text);
                    break;
            }
        }


        private void ParseLinkDirective()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            ThrowExceptionIfUnknownLinkObjectName(ref token);


            // DoLink -> CallLinkFunction


            currentContext.Lexer.ReadNextToken();
        }


        private void ParseCompileDirective()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            ThrowExceptionUnknownIfCompileScriptName(ref token);
            Compile(token.Text);
            currentContext.Lexer.ReadNextToken();
        }


        private void ParseConstantDirective()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            ThrowExceptionIfInvalidConstantName(ref token);
            var name = token.Text;


            currentContext.Lexer.ReadNextToken();
            if (token.Kind == SrTokenKind.Minus)
            {
                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfInvalidConstantNumberValue(ref token);
                switch (token.Kind)
                {
                    case SrTokenKind.Integer:
                        coder.RegisterConstantValue(name, CccBinaryCoder.CccType.Int, -token.Integer, 0.0f, null);
                        break;


                    case SrTokenKind.Number:
                        coder.RegisterConstantValue(name, CccBinaryCoder.CccType.Number, 0, -(float)token.Number, null);
                        break;
                }
            }
            else
            {
                ThrowExceptionIfInvalidConstantValue(ref token);
                switch (token.Kind)
                {
                    case SrTokenKind.Integer:
                        coder.RegisterConstantValue(name, CccBinaryCoder.CccType.Int, token.Integer, 0.0f, null);
                        break;


                    case SrTokenKind.Number:
                        coder.RegisterConstantValue(name, CccBinaryCoder.CccType.Number, 0, (float)token.Number, null);
                        break;


                    case SrTokenKind.String:
                        coder.RegisterConstantValue(name, CccBinaryCoder.CccType.String, 0, 0.0f, token.Text);
                        break;
                }
            }


            currentContext.Lexer.ReadNextToken();
        }
        #endregion


        #region Parse peripheral
        private void ParsePeripheralDeclare()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            while (token.Kind == SrTokenKind.Using)
            {
                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfInvalidPeripheralFunctionName(ref token);
                var functionName = token.Text;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfUnknownToken(ref token, SrTokenKind.Equal);


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfNotReturnType(ref token);
                var returnTypeKind = token.Kind;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfInvalidPeripheralName(ref token);
                var peripheralName = token.Text;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfUnknownToken(ref token, SrTokenKind.Period);


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfInvalidImportPeripheralFunctionName(ref token);
                var importFunctionName = token.Text;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionNotStartOpenSymbol(ref token, SrTokenKind.OpenParen, "(");
                var argumentList = new List<int>();


                currentContext.Lexer.ReadNextToken();
                ParsePeripheralDeclareTypeList(argumentList);
                ThrowExceptionNotEndCloseSymbol(ref token, SrTokenKind.CloseParen, ")");


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfUnknownToken(ref token, SrTokenKind.Semicolon);


                coder.RegisterPeripheralFunction(functionName, returnTypeKind, argumentList, peripheralName, importFunctionName);
                currentContext.Lexer.ReadNextToken();
            }
        }


        private void ParsePeripheralDeclareTypeList(IList<int> typeList)
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            if (token.Kind == SrTokenKind.CloseParen)
            {
                return;
            }


            while (true)
            {
                ThrowExceptionIfNotArgumentType(ref token);
                typeList.Add(token.Kind);


                currentContext.Lexer.ReadNextToken();
                if (token.Kind != SrTokenKind.Comma)
                {
                    break;
                }


                currentContext.Lexer.ReadNextToken();
            }
        }
        #endregion


        #region Parse global variable
        private void ParseGlobalVariableDeclare()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            while (token.Kind == SrTokenKind.Global)
            {
                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfNotVariableType(ref token);
                var variableType = token.Kind;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfInvalidGlobalVariableName(ref token);
                var varName = token.Text;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfUnknownToken(ref token, SrTokenKind.Semicolon);


                coder.RegisterVariable(varName, variableType);


                currentContext.Lexer.ReadNextToken();
            }
        }
        #endregion


        #region Parse function
        private void ParseFunctionDeclare()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            while (token.Kind == SrTokenKind.Function)
            {
                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfNotReturnType(ref token);
                var returnType = token.Kind;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfInvalidFunctionName(ref token);
                var functionName = token.Text;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionNotStartOpenSymbol(ref token, SrTokenKind.OpenParen, "(");


                currentContext.Lexer.ReadNextToken();
                var argumentList = new List<CccBinaryCoder.CccArgumentInfo>();
                ParseFunctionArgumentList(argumentList);
                ThrowExceptionNotEndCloseSymbol(ref token, SrTokenKind.CloseParen, ")");


                // Register Function
                coder.RegisterFunction(functionName, returnType, argumentList);
                currentParseFunctionName = functionName;


                // Generate function enter
                var function = coder.GetFunction(functionName);
                var patchIndex = coder.GenerateFunctionEnter(function);


                currentContext.Lexer.ReadNextToken();
                while (token.Kind != SrTokenKind.End)
                {
                    ParseBlock();
                    ThrowExceptionNotEndFinish(ref token);
                }


                coder.UpdateFunctionLocalVariableCount(function, patchIndex, function.LocalVariableTable.Count);


                // Generate function leave
                function.FixReturnAddress();
                coder.GenerateFunctionLeave(function, function.LocalVariableTable.Count);
                currentContext.Lexer.ReadNextToken();
            }
        }


        private void ParseFunctionArgumentList(IList<CccBinaryCoder.CccArgumentInfo> argumentList)
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            if (token.Kind == SrTokenKind.CloseParen)
            {
                return;
            }


            while (true)
            {
                ThrowExceptionIfNotArgumentType(ref token);
                var type =
                    token.Kind == SrTokenKind.TypeInt ? CccBinaryCoder.CccType.Int :
                    token.Kind == SrTokenKind.TypeNumber ? CccBinaryCoder.CccType.Number :
                    CccBinaryCoder.CccType.String;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfInvalidArgumentName(ref token);
                var name = token.Text;


                var argument = new CccBinaryCoder.CccArgumentInfo();
                argument.Name = name;
                argument.Type = type;
                argumentList.Add(argument);


                currentContext.Lexer.ReadNextToken();
                if (token.Kind != SrTokenKind.Comma)
                {
                    break;
                }


                currentContext.Lexer.ReadNextToken();
            }
        }


        private void ParsePeripheralFunctionCall()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            var function = coder.GetFunction(currentParseFunctionName);
            var peripheralFunction = coder.GetFunction(token.Text);
            var argumentCount = peripheralFunction.ArgumentTable.Count;


            currentContext.Lexer.ReadNextToken();
            ThrowExceptionNotStartOpenSymbol(ref token, SrTokenKind.OpenParen, "(");


            currentContext.Lexer.ReadNextToken();
            if (token.Kind != SrTokenKind.CloseParen)
            {
                while (true)
                {
                    ParseExpression();
                    coder.GeneratePushR15(function);
                    argumentCount--;


                    if (token.Kind != SrTokenKind.Comma)
                    {
                        break;
                    }


                    currentContext.Lexer.ReadNextToken();
                }
            }


            if (argumentCount != 0)
            {
                ThrowExceptionDifferArgumentCount(peripheralFunction.Name);
            }


            ThrowExceptionNotEndCloseSymbol(ref token, SrTokenKind.CloseParen, ")");


            // generate peripheral function call
            coder.GeneratePeripheralFunctionCall(function, peripheralFunction.Name);
            coder.GenerateStackPointerAdd(function, peripheralFunction.ArgumentTable.Count);


            currentContext.Lexer.ReadNextToken();
        }


        private void ParseFunctionCall()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            var function = coder.GetFunction(currentParseFunctionName);
            var targetFunction = coder.GetFunction(token.Text);
            var argumentCount = targetFunction.ArgumentTable.Count;


            currentContext.Lexer.ReadNextToken();
            ThrowExceptionNotStartOpenSymbol(ref token, SrTokenKind.OpenParen, "(");


            currentContext.Lexer.ReadNextToken();
            if (token.Kind != SrTokenKind.CloseParen)
            {
                while (true)
                {
                    ParseExpression();
                    coder.GeneratePushR15(function);
                    argumentCount--;


                    if (token.Kind != SrTokenKind.Comma)
                    {
                        break;
                    }


                    currentContext.Lexer.ReadNextToken();
                }
            }


            if (argumentCount != 0)
            {
                ThrowExceptionDifferArgumentCount(targetFunction.Name);
            }


            ThrowExceptionNotEndCloseSymbol(ref token, SrTokenKind.CloseParen, ")");


            // generate peripheral function call
            coder.GenerateFunctionCall(function, targetFunction.Name);
            coder.GenerateStackPointerAdd(function, targetFunction.ArgumentTable.Count);


            currentContext.Lexer.ReadNextToken();
        }
        #endregion


        #region Parse statements
        private void ParseBlock()
        {
            ParseStatementList();
        }


        private void ParseStatementList()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            switch (token.Kind)
            {
                case SrTokenKind.Semicolon:
                    currentContext.Lexer.ReadNextToken();
                    return;


                case SrTokenKind.Local:
                    currentContext.Lexer.ReadNextToken();
                    ParseLocalVariableDeclare();
                    break;


                case SrTokenKind.For:
                    currentContext.Lexer.ReadNextToken();
                    ParseForStatement();
                    break;


                case SrTokenKind.While:
                    currentContext.Lexer.ReadNextToken();
                    ParseWhileStatement();
                    break;


                case SrTokenKind.If:
                    currentContext.Lexer.ReadNextToken();
                    ParseIfStatement();
                    break;


                case SrTokenKind.Break:
                    currentContext.Lexer.ReadNextToken();
                    ParseBreakStatement();
                    break;


                case SrTokenKind.Return:
                    currentContext.Lexer.ReadNextToken();
                    ParseReturnStatement();
                    break;


                default:
                    ParseExpression();
                    break;
            }
        }


        private void ParseBreakStatement()
        {
            var function = coder.GetFunction(currentParseFunctionName);
            ref var token = ref currentContext.Lexer.LastReadToken;
            ThrowExceptionIfUnknownToken(ref token, SrTokenKind.Semicolon);
            var patchAddress = coder.GenerateOffsetJump(function, 0);
            function.RegisterBreakInstruction(patchAddress);
            currentContext.Lexer.ReadNextToken();
        }


        private void ParseReturnStatement()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            var function = coder.GetFunction(currentParseFunctionName);


            if (token.Kind == SrTokenKind.Semicolon)
            {
                if (function.ReturnType != CccBinaryCoder.CccType.Void)
                {
                    ThrowExceptionFunctionNeedReturnExpression(function.Name);
                    return;
                }
            }
            else
            {
                if (function.ReturnType == CccBinaryCoder.CccType.Void)
                {
                    ThrowExceptionFunctionNotReturnExpression(function.Name);
                    return;
                }


                ParseExpression();
                coder.GenerateFunctionReturnCode(function);
            }


            ThrowExceptionIfUnknownToken(ref token, SrTokenKind.Semicolon);
            currentContext.Lexer.ReadNextToken();
        }


        private void ParseLocalVariableDeclare()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            ThrowExceptionIfNotVariableType(ref token);
            var type = token.Kind;


            currentContext.Lexer.ReadNextToken();
            ThrowExceptionIfInvalidLocalVariableName(ref token);
            var name = token.Text;


            currentContext.Lexer.ReadNextToken();
            ThrowExceptionIfUnknownToken(ref token, SrTokenKind.Semicolon);


            coder.GetFunction(currentParseFunctionName).RegisterVariable(name, type);
            currentContext.Lexer.ReadNextToken();
        }


        private void ParseForStatement()
        {
            var function = coder.GetFunction(currentParseFunctionName);


            ref var token = ref currentContext.Lexer.LastReadToken;
            ThrowExceptionNotStartOpenSymbol(ref token, SrTokenKind.OpenParen, "(");
            currentContext.Lexer.ReadNextToken();
            if (token.Kind != SrTokenKind.Semicolon)
            {
                // initial expression
                ParseExpression();
            }
            ThrowExceptionIfUnknownToken(ref token, SrTokenKind.Semicolon);
            currentContext.Lexer.ReadNextToken();
            var forConditionHead = function.CurrentInstructionCount;
            function.StatementHeadAddressStack.Push(forConditionHead);
            if (token.Kind != SrTokenKind.Semicolon)
            {
                // condition expression
                ParseExpression();
            }
            var forEndPatchIndex = coder.GenerateJumpTest(function, 0);
            var iterateSkipPatchIndex = coder.GenerateOffsetJump(function, 0);
            ThrowExceptionIfUnknownToken(ref token, SrTokenKind.Semicolon);
            currentContext.Lexer.ReadNextToken();
            var forIterateHead = function.CurrentInstructionCount;
            if (token.Kind != SrTokenKind.CloseParen)
            {
                // iterate expression
                ParseExpression();
            }
            ThrowExceptionNotEndCloseSymbol(ref token, SrTokenKind.CloseParen, ")");
            coder.GenerateOffsetJump(function, forConditionHead - function.CurrentInstructionCount);
            coder.UpdateJumpAddress(function, iterateSkipPatchIndex, function.CurrentInstructionCount - iterateSkipPatchIndex);


            currentContext.Lexer.ReadNextToken();
            while (token.Kind != SrTokenKind.End)
            {
                ParseBlock();
            }
            ThrowExceptionNotEndCloseSymbol(ref token, SrTokenKind.End, "end");
            coder.GenerateOffsetJump(function, forIterateHead - function.CurrentInstructionCount);
            coder.UpdateJumpAddress(function, forEndPatchIndex, function.CurrentInstructionCount - forEndPatchIndex);


            function.StatementHeadAddressStack.Pop();
            function.PatchBreakInstruction(forConditionHead, function.CurrentInstructionCount);
            currentContext.Lexer.ReadNextToken();
        }


        private void ParseWhileStatement()
        {
            var function = coder.GetFunction(currentParseFunctionName);
            var whileHeadAddress = function.CurrentInstructionCount;
            function.StatementHeadAddressStack.Push(whileHeadAddress);


            ref var token = ref currentContext.Lexer.LastReadToken;
            ThrowExceptionNotStartOpenSymbol(ref token, SrTokenKind.OpenParen, "(");
            currentContext.Lexer.ReadNextToken();
            ParseExpression();
            ThrowExceptionNotEndCloseSymbol(ref token, SrTokenKind.CloseParen, ")");



            var patchTargetIndex = coder.GenerateJumpTest(function, 0);



            currentContext.Lexer.ReadNextToken();
            while (token.Kind != SrTokenKind.End)
            {
                ParseBlock();
            }
            ThrowExceptionNotEndCloseSymbol(ref token, SrTokenKind.End, "end");


            coder.GenerateOffsetJump(function, whileHeadAddress - function.CurrentInstructionCount);
            var whileTailAddress = function.CurrentInstructionCount - patchTargetIndex;
            coder.UpdateJumpAddress(function, patchTargetIndex, whileTailAddress);


            function.StatementHeadAddressStack.Pop();
            function.PatchBreakInstruction(whileHeadAddress, function.CurrentInstructionCount);
            currentContext.Lexer.ReadNextToken();
        }


        private void ParseIfStatement()
        {
            var function = coder.GetFunction(currentParseFunctionName);
            ref var token = ref currentContext.Lexer.LastReadToken;
            ThrowExceptionNotStartOpenSymbol(ref token, SrTokenKind.OpenParen, "(");
            currentContext.Lexer.ReadNextToken();
            ParseExpression();
            ThrowExceptionNotEndCloseSymbol(ref token, SrTokenKind.CloseParen, ")");
            currentContext.Lexer.ReadNextToken();


            var endJumpAddressList = new List<int>();
            var jumpAddress = coder.GenerateJumpTest(function, 0);


            while (token.Kind != SrTokenKind.End)
            {
                ParseBlock();


                if (token.Kind == SrTokenKind.Else)
                {
                    endJumpAddressList.Add(coder.GenerateOffsetJump(function, 0));
                    coder.UpdateJumpAddress(function, jumpAddress, function.CurrentInstructionCount - jumpAddress);
                    currentContext.Lexer.ReadNextToken();


                    if (token.Kind == SrTokenKind.If)
                    {
                        currentContext.Lexer.ReadNextToken();
                        ThrowExceptionNotStartOpenSymbol(ref token, SrTokenKind.OpenParen, "(");
                        currentContext.Lexer.ReadNextToken();
                        ParseExpression();
                        ThrowExceptionNotEndCloseSymbol(ref token, SrTokenKind.CloseParen, ")");
                        currentContext.Lexer.ReadNextToken();


                        jumpAddress = coder.GenerateJumpTest(function, 0);
                    }
                    else
                    {
                        jumpAddress = -1;
                    }
                }
            }


            ThrowExceptionNotEndCloseSymbol(ref token, SrTokenKind.End, "end");
            if (jumpAddress != -1)
            {
                coder.UpdateJumpAddress(function, jumpAddress, function.CurrentInstructionCount - jumpAddress);
            }
            foreach (var endJumpAddress in endJumpAddressList)
            {
                coder.UpdateJumpAddress(function, endJumpAddress, function.CurrentInstructionCount - endJumpAddress);
            }
            currentContext.Lexer.ReadNextToken();
        }
        #endregion


        #region Expression
        private void ParseExpression()
        {
            var value = new CccBinaryCoder.ExpressionValue();
            value.FirstGenerate = true;
            ParseExpression(ref value, 0);


            if (value.FirstGenerate && value.Type != CccBinaryCoder.CccType.Void)
            {
                coder.GenerateSetSingleExpressionValue(coder.GetFunction(currentParseFunctionName), ref value);
            }
        }


        private int ParseExpression(ref CccBinaryCoder.ExpressionValue value, int currentOpPriority)
        {
            var function = coder.GetFunction(currentParseFunctionName);
            ref var token = ref currentContext.Lexer.LastReadToken;
            switch (token.Kind)
            {
                case SrTokenKind.DoubleMinus:
                case SrTokenKind.DoublePlus:
                case SrTokenKind.Plus:
                case SrTokenKind.Minus:
                    var unaryOp = token.Kind;
                    currentContext.Lexer.ReadNextToken();
                    ParseExpression(ref value, UnaryOperatorPriority);
                    coder.GenerateSetSingleExpressionValue(function, ref value);
                    coder.GenerateUnaryOperationCode(function, unaryOp, ref value);
                    value.FirstGenerate = false;
                    break;


                case SrTokenKind.OpenParen:
                    currentContext.Lexer.ReadNextToken();
                    ParseExpression(ref value, 0);
                    ThrowExceptionNotEndCloseSymbol(ref token, SrTokenKind.CloseParen, ")");
                    break;


                case SrTokenKind.Integer:
                    value.IdentifierKind = CccBinaryCoder.IdentifierKind.ConstantValue;
                    value.Type = CccBinaryCoder.CccType.Int;
                    value.Integer = token.Integer;
                    currentContext.Lexer.ReadNextToken();
                    break;


                case SrTokenKind.Number:
                    value.IdentifierKind = CccBinaryCoder.IdentifierKind.ConstantValue;
                    value.Type = CccBinaryCoder.CccType.Number;
                    value.Number = token.Number;
                    currentContext.Lexer.ReadNextToken();
                    break;


                case SrTokenKind.String:
                    value.IdentifierKind = CccBinaryCoder.IdentifierKind.ConstantValue;
                    value.Type = CccBinaryCoder.CccType.String;
                    value.Text = token.Text;
                    currentContext.Lexer.ReadNextToken();
                    break;


                case SrTokenKind.Identifier:
                    var identifierKind = coder.GetIdentifierKind(token.Text, currentParseFunctionName);
                    var identifierName = token.Text;
                    value.IdentifierKind = identifierKind;
                    switch (identifierKind)
                    {
                        case CccBinaryCoder.IdentifierKind.ConstantValue:
                            var constantInfo = coder.GetConstant(identifierName);
                            value.Type = constantInfo.Type;
                            value.Integer = constantInfo.IntegerValue;
                            value.Number = constantInfo.NumberValue;
                            value.Text = constantInfo.TextValue;
                            currentContext.Lexer.ReadNextToken();
                            break;


                        case CccBinaryCoder.IdentifierKind.PeripheralFunction:
                            ParsePeripheralFunctionCall();
                            var perFunction = coder.GetFunction(identifierName);
                            value.Type = perFunction.ReturnType;
                            value.Text = perFunction.Name;
                            break;


                        case CccBinaryCoder.IdentifierKind.StandardFunction:
                            ParseFunctionCall();
                            var stdFunction = coder.GetFunction(identifierName);
                            value.Type = stdFunction.ReturnType;
                            value.Text = stdFunction.Name;
                            break;


                        case CccBinaryCoder.IdentifierKind.ArgumentVariable:
                            var argumentInfo = coder.GetFunction(currentParseFunctionName).ArgumentTable[identifierName];
                            value.Type = argumentInfo.Type;
                            value.Integer = argumentInfo.Index;
                            value.Text = argumentInfo.Name;
                            currentContext.Lexer.ReadNextToken();
                            break;


                        case CccBinaryCoder.IdentifierKind.LocalVariable:
                            var variableInfo = coder.GetFunction(currentParseFunctionName).LocalVariableTable[identifierName];
                            value.Type = variableInfo.Type;
                            value.Integer = variableInfo.Address;
                            value.Text = variableInfo.Name;
                            currentContext.Lexer.ReadNextToken();
                            break;


                        case CccBinaryCoder.IdentifierKind.GlobalVariable:
                            var globalVariableInfo = coder.GetVariable(identifierName);
                            value.Type = globalVariableInfo.Type;
                            value.Integer = globalVariableInfo.Address;
                            value.Text = globalVariableInfo.Name;
                            currentContext.Lexer.ReadNextToken();
                            break;


                        case CccBinaryCoder.IdentifierKind.Unknown:
                            ThrowExceptionUnknownToken(ref token);
                            break;
                    }
                    break;


                default:
                    break;
            }


            var op = token.Kind;
            while (OperatorPriorityTable.ContainsKey(op) && OperatorPriorityTable[op].left > currentOpPriority)
            {
                currentContext.Lexer.ReadNextToken();
                var rightValue = new CccBinaryCoder.ExpressionValue();
                rightValue.FirstGenerate = value.FirstGenerate;
                var nextOperator = ParseExpression(ref rightValue, OperatorPriorityTable[op].right);
                value.FirstGenerate = rightValue.FirstGenerate;
                if (value.Type == CccBinaryCoder.CccType.Void && op == SrTokenKind.Equal)
                {
                    ThrowExceptionIfNotAssignableTarget(value.Text);
                }
                coder.GenerateOperationCode(function, op, ref value, ref rightValue);
                op = nextOperator;
            }


            return op;
        }
        #endregion
        #endregion


        #region exception thrower
        /// <summary>
        /// 再帰コンパイルしようとしている例外をスローします
        /// </summary>
        /// <param name="scriptName">再帰コンパイルを検知した直近のスクリプト名</param>
        /// <exception cref="InvalidOperationException">スクリプト '{scriptName}' にて再帰コンパイルを検出しました</exception>
        private void ThrowExceptionRecursiveScriptCompile(string scriptName)
        {
            // 再帰コンパイルしようとしている恐れのコンパイルエラーを吐く
            ThrowExceptionCompileError($"スクリプト '{scriptName}' にて再帰コンパイルを検出しました", 0);
        }


        private void ThrowExceptionUnknownDirective(string directiveName)
        {
            ThrowExceptionCompileError($"不明なディレクティグ '{directiveName}' です", 0);
        }


        private void ThrowExceptionUnknownToken(ref Token token)
        {
            ThrowExceptionCompileError($"不明なトークン '{token.Text}' です", 0);
        }


        private void ThrowExceptionIfUnknownLinkObjectName(ref Token token)
        {
            if (token.Kind != SrTokenKind.String)
            {
                ThrowExceptionCompileError($"リンクするオブジェクト名 '{token.Text}' が正しくありません", 0);
            }
        }


        private void ThrowExceptionUnknownIfCompileScriptName(ref Token token)
        {
            if (token.Kind != SrTokenKind.String)
            {
                ThrowExceptionCompileError($"コンパイルするスクリプト名 '{token.Text}' が正しくありません", 0);
            }
        }


        private void ThrowExceptionFunctionNotReturnExpression(string functionName)
        {
            ThrowExceptionCompileError($"関数 '{functionName}' は値を返すことはできません", 0);
        }


        private void ThrowExceptionFunctionNeedReturnExpression(string functionName)
        {
            ThrowExceptionCompileError($"関数 '{functionName}' は値を返す式が必要です", 0);
        }


        private void ThrowExceptionIfInvalidConstantName(ref Token token)
        {
            if (token.Kind != SrTokenKind.Identifier)
            {
                ThrowExceptionCompileError($"無効な定数名 '{token.Text}' です", 0);
            }
        }


        private void ThrowExceptionIfInvalidConstantNumberValue(ref Token token)
        {
            if (token.Kind == SrTokenKind.Integer || token.Kind == SrTokenKind.Number)
            {
                return;
            }


            ThrowExceptionCompileError("負の定数が使えるのは int型 または number型 のみです", 0);
        }


        private void ThrowExceptionIfInvalidConstantValue(ref Token token)
        {
            if (token.Kind == SrTokenKind.Integer || token.Kind == SrTokenKind.Number || token.Kind == SrTokenKind.String)
            {
                return;
            }


            ThrowExceptionCompileError($"無効な定数値 '{token.Text}' です", 0);
        }


        private void ThrowExceptionNotStartOpenSymbol(ref Token token, int kind, string symbol)
        {
            if (token.Kind != kind)
            {
                ThrowExceptionCompileError($"'{symbol}' から始める必要があります", 0);
            }
        }


        private void ThrowExceptionNotEndCloseSymbol(ref Token token, int kind, string symbol)
        {
            if (token.Kind != kind)
            {
                ThrowExceptionCompileError($"'{symbol}' で終了している必要があります", 0);
            }
        }


        private void ThrowExceptionIfInvalidPeripheralFunctionName(ref Token token)
        {
            if (token.Kind != SrTokenKind.Identifier)
            {
                ThrowExceptionCompileError($"無効な周辺関数名 '{token.Text}' です", 0);
            }
        }


        private void ThrowExceptionIfInvalidFunctionName(ref Token token)
        {
            if (token.Kind != SrTokenKind.Identifier)
            {
                ThrowExceptionCompileError($"無効な関数名 '{token.Text}' です", 0);
            }
        }


        private void ThrowExceptionIfInvalidArgumentName(ref Token token)
        {
            if (token.Kind != SrTokenKind.Identifier)
            {
                ThrowExceptionCompileError($"無効な引数名 '{token.Text}' です", 0);
            }
        }


        private void ThrowExceptionIfInvalidGlobalVariableName(ref Token token)
        {
            if (token.Kind != SrTokenKind.Identifier)
            {
                ThrowExceptionCompileError($"無効なグローバル変数名 '{token.Text}' です", 0);
            }
        }


        private void ThrowExceptionIfInvalidLocalVariableName(ref Token token)
        {
            if (token.Kind != SrTokenKind.Identifier)
            {
                ThrowExceptionCompileError($"無効なローカル変数名 '{token.Text}' です", 0);
            }
        }


        private void ThrowExceptionIfInvalidImportPeripheralFunctionName(ref Token token)
        {
            if (token.Kind != SrTokenKind.Identifier)
            {
                ThrowExceptionCompileError($"無効なインポート周辺関数名 '{token.Text}' です", 0);
            }
        }


        private void ThrowExceptionIfUnknownToken(ref Token token, int kind)
        {
            if (token.Kind != kind)
            {
                ThrowExceptionCompileError($"不明なトークン '{token.Text}' です", 0);
            }
        }


        private void ThrowExceptionIfNotReturnType(ref Token token)
        {
            if (token.Kind == SrTokenKind.TypeVoid || token.Kind == SrTokenKind.TypeInt || token.Kind == SrTokenKind.TypeNumber || token.Kind == SrTokenKind.TypeString)
            {
                return;
            }


            ThrowExceptionCompileError($"戻り値の型 '{token.Text}' が正しくありません。戻り値の型として使えるのは 'void' 'int' 'number' 'string' のいずれかです。", 0);
        }


        private void ThrowExceptionIfNotArgumentType(ref Token token)
        {
            if (token.Kind == SrTokenKind.TypeInt || token.Kind == SrTokenKind.TypeNumber || token.Kind == SrTokenKind.TypeString)
            {
                return;
            }


            ThrowExceptionCompileError($"引数の型 '{token.Text}' が正しくありません。引数の型として使えるのは 'int' 'number' 'string' のいずれかです。", 0);
        }


        private void ThrowExceptionIfNotVariableType(ref Token token)
        {
            if (token.Kind == SrTokenKind.TypeInt || token.Kind == SrTokenKind.TypeNumber || token.Kind == SrTokenKind.TypeString)
            {
                return;
            }


            ThrowExceptionCompileError($"変数の型 '{token.Text}' が正しくありません。変数の型として使えるのは 'int' 'number' 'string' のいずれかです。", 0);
        }


        private void ThrowExceptionIfInvalidPeripheralName(ref Token token)
        {
            if (token.Kind != SrTokenKind.Identifier)
            {
                ThrowExceptionCompileError($"不明な周辺機器名 '{token.Text}' です", 0);
            }
        }


        private void ThrowExceptionDifferArgumentCount(string functionName)
        {
            ThrowExceptionCompileError($"関数 '{functionName}' の引数の数が一致しません", 0);
        }


        private void ThrowExceptionIfNotAssignableTarget(string name)
        {
            var identifirKind = coder.GetIdentifierKind(name, currentParseFunctionName);
            if (identifirKind == CccBinaryCoder.IdentifierKind.LocalVariable ||
                identifirKind == CccBinaryCoder.IdentifierKind.ArgumentVariable ||
                identifirKind == CccBinaryCoder.IdentifierKind.GlobalVariable)
            {
                return;
            }


            ThrowExceptionCompileError($"識別子 '{name}' は代入可能な式ではありません", 0);
        }


        private void ThrowExceptionNotEndFinish(ref Token token)
        {
            if (token.Kind == TokenKind.EndOfToken)
            {
                ThrowExceptionCompileError("end で終了する必要があります", 0);
            }
        }


        /// <summary>
        /// 例外としてコンパイルエラーをスローします
        /// </summary>
        /// <param name="message">スローするメッセージ</param>
        private void ThrowExceptionCompileError(string message, uint errorCode)
        {
            // 現在のコンテキストから必要な情報を取り出してロガーにエラーを書き込む
            var scriptName = currentContext.ScriptName;
            var lexer = currentContext.Lexer;
            ref var token = ref lexer.LastReadToken;
            logger.Write(CccParserLogType.Error, scriptName, token.LineNumber, token.ColumnNumber, errorCode, message);


            // すべてのコンテキストを破棄する
            while (contextStack.Count > 0)
            {
                // コンテキストをポップして破棄
                contextStack.Pop().Lexer.Dispose();
            }


            // 例外を吐く
            throw new SyntaxErrorException(message);
        }
        #endregion



        #region ParserContext
        /// <summary>
        /// 構文解析コンテキストクラスです
        /// </summary>
        private class ParserContext
        {
            /// <summary>
            /// このコンテキストを保持している構文解析器
            /// </summary>
            public SrParser Parser { get; private set; }

            /// <summary>
            /// 構文解析している対象のスクリプト名を取得します
            /// </summary>
            public string ScriptName { get; private set; }

            /// <summary>
            /// このコンテキストにおける字句解析器を取得します
            /// </summary>
            public CccLexer Lexer { get; private set; }



            /// <summary>
            /// ParserContext クラスのインスタンスを初期化します
            /// </summary>
            /// <param name="scriptName">構文解析対象のスクリプト名</param>
            /// <param name="parser">このコンテキストを持つ構文解析器</param>
            public ParserContext(string scriptName, SrParser parser)
            {
                // 構文解析器を覚える
                Parser = parser ?? throw new ArgumentNullException(nameof(parser));


                // スクリプト名を覚えてレキサの初期化をする
                ScriptName = scriptName;
                Lexer = new CccLexer();


                // スクリプトを開いてレキサをリセットする
                Lexer.Reset(parser.storage.Open(scriptName));
            }
        }
        #endregion
    }
}
*/