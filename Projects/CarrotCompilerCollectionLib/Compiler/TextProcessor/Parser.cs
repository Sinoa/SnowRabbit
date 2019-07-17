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

using System;
using System.Collections.Generic;
using System.IO;
using CarrotCompilerCollection.IO;
using CarrotCompilerCollection.Utility;
using TextProcessorLib;

namespace CarrotCompilerCollection.Compiler
{
    /// <summary>
    /// Carrot スクリプトコードの構文解析及び実行コードを生成するクラスです
    /// </summary>
    public class CccParser
    {
        // メンバ変数定義
        private CccBinaryCoder coder;
        private IScriptStorage storage;
        private ICccParserLogger logger;
        private Stack<ParserContext> contextStack;
        private ParserContext currentContext;
        private string parseFunctionName;



        #region Constructor and initialize
        /// <summary>
        /// CccParser クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="logger">構文解析ログを出力するロガー</param>
        /// <exception cref="ArgumentNullException">storage が null です</exception>
        /// <exception cref="ArgumentNullException">logger が null です</exception>
        public CccParser(IScriptStorage storage, ICccParserLogger logger)
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
            contextStack.Pop();


            if (contextStack.Count > 0)
            {
                currentContext = contextStack.Peek();
                return;
            }


            coder.OutputExecuteCode();
        }
        #endregion


        #region Parse
        /// <summary>
        /// コンパイル単位のルートになる構文解析関数です
        /// </summary>
        private void ParseCompileUnit()
        {
            currentContext.Lexer.ReadNextToken();


            ParseDirective();
            ParsePeripheralDeclare();
            ParseGlobalVariableDeclare();
            ParseFunctionDeclare();
        }


        #region Parse directive
        private void ParseDirective()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            while (token.Kind == CccTokenKind.Sharp)
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
                case CccTokenKind.Link:
                    currentContext.Lexer.ReadNextToken();
                    ParseLinkDirective();
                    break;


                case CccTokenKind.Compile:
                    currentContext.Lexer.ReadNextToken();
                    ParseCompileDirective();
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
        #endregion


        #region Parse peripheral
        private void ParsePeripheralDeclare()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            while (token.Kind == CccTokenKind.Using)
            {
                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfInvalidPeripheralFunctionName(ref token);
                var functionName = token.Text;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfUnknownToken(ref token, CccTokenKind.Equal);


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfNotReturnType(ref token);
                var returnTypeKind = token.Kind;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfInvalidPeripheralName(ref token);
                var peripheralName = token.Text;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfUnknownToken(ref token, CccTokenKind.Period);


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfInvalidImportPeripheralFunctionName(ref token);
                var importFunctionName = token.Text;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionNotStartOpenSymbol(ref token, CccTokenKind.OpenParen, "(");
                var argumentList = new List<int>();


                currentContext.Lexer.ReadNextToken();
                ParsePeripheralDeclareTypeList(argumentList);
                ThrowExceptionNotEndCloseSymbol(ref token, CccTokenKind.CloseParen, ")");


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfUnknownToken(ref token, CccTokenKind.Semicolon);


                coder.RegisterPeripheralFunction(functionName, returnTypeKind, argumentList, peripheralName, importFunctionName);
                currentContext.Lexer.ReadNextToken();
            }
        }


        private void ParsePeripheralDeclareTypeList(IList<int> typeList)
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            if (token.Kind == CccTokenKind.CloseParen)
            {
                return;
            }


            while (true)
            {
                ThrowExceptionIfNotArgumentType(ref token);
                typeList.Add(token.Kind);


                currentContext.Lexer.ReadNextToken();
                if (token.Kind != CccTokenKind.Comma)
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
            while (token.Kind == CccTokenKind.Global)
            {
                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfNotVariableType(ref token);
                var variableType = token.Kind;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfInvalidGlobalVariableName(ref token);
                var varName = token.Text;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfUnknownToken(ref token, CccTokenKind.Semicolon);


                coder.RegisterVariable(varName, variableType, CccBinaryCoder.VariableType.Global);


                currentContext.Lexer.ReadNextToken();
            }
        }
        #endregion


        #region Parse function
        private void ParseFunctionDeclare()
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            while (token.Kind == CccTokenKind.Function)
            {
                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfNotReturnType(ref token);
                var returnType = token.Kind;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfInvalidFunctionName(ref token);
                var functionName = token.Text;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionNotStartOpenSymbol(ref token, CccTokenKind.OpenParen, "(");


                currentContext.Lexer.ReadNextToken();
                var argumentList = new List<CccBinaryCoder.CccArgumentInfo>();
                ParseFunctionArgumentList(argumentList);
                ThrowExceptionNotEndCloseSymbol(ref token, CccTokenKind.CloseParen, ")");


                // Register Function
                coder.RegisterFunction(functionName, returnType, argumentList);
                parseFunctionName = functionName;


                currentContext.Lexer.ReadNextToken();
                while (token.Kind != CccTokenKind.End)
                {
                    ParseBlock();
                }


                currentContext.Lexer.ReadNextToken();
            }
        }


        private void ParseFunctionArgumentList(IList<CccBinaryCoder.CccArgumentInfo> argumentList)
        {
            ref var token = ref currentContext.Lexer.LastReadToken;
            if (token.Kind == CccTokenKind.CloseParen)
            {
                return;
            }


            while (true)
            {
                ThrowExceptionIfNotArgumentType(ref token);
                var type =
                    token.Kind == CccTokenKind.TypeInt ? CccBinaryCoder.CccType.Int :
                    token.Kind == CccTokenKind.TypeNumber ? CccBinaryCoder.CccType.Number :
                    CccBinaryCoder.CccType.String;


                currentContext.Lexer.ReadNextToken();
                ThrowExceptionIfInvalidArgumentName(ref token);
                var name = token.Text;


                var argument = new CccBinaryCoder.CccArgumentInfo();
                argument.Name = name;
                argument.Type = type;
                argumentList.Add(argument);


                currentContext.Lexer.ReadNextToken();
                if (token.Kind != CccTokenKind.Comma)
                {
                    break;
                }


                currentContext.Lexer.ReadNextToken();
            }
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
                case CccTokenKind.Semicolon:
                    currentContext.Lexer.ReadNextToken();
                    return;


                case CccTokenKind.Local:
                    currentContext.Lexer.ReadNextToken();
                    ParseLocalVariableDeclare();
                    break;


                case CccTokenKind.For:
                    currentContext.Lexer.ReadNextToken();
                    ParseForStatement();
                    break;


                case CccTokenKind.While:
                    currentContext.Lexer.ReadNextToken();
                    ParseWhileStatement();
                    break;


                default:
                    ParseExpressionRoot();
                    break;
            }


            currentContext.Lexer.ReadNextToken();
        }


        private void ParseLocalVariableDeclare()
        {
        }


        private void ParseForStatement()
        {
        }


        private void ParseWhileStatement()
        {
        }
        #endregion


        #region Expression
        private void ParseExpressionRoot()
        {
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


        private void ThrowExceptionIfUnknownLinkObjectName(ref Token token)
        {
            if (token.Kind != CccTokenKind.String)
            {
                ThrowExceptionCompileError($"リンクするオブジェクト名 '{token.Text}' が正しくありません", 0);
            }
        }


        private void ThrowExceptionUnknownIfCompileScriptName(ref Token token)
        {
            if (token.Kind != CccTokenKind.String)
            {
                ThrowExceptionCompileError($"コンパイルするスクリプト名 '{token.Text}' が正しくありません", 0);
            }
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
            if (token.Kind != CccTokenKind.Identifier)
            {
                ThrowExceptionCompileError($"無効な周辺関数名 '{token.Text}' です", 0);
            }
        }


        private void ThrowExceptionIfInvalidFunctionName(ref Token token)
        {
            if (token.Kind != CccTokenKind.Identifier)
            {
                ThrowExceptionCompileError($"無効な関数名 '{token.Text}' です", 0);
            }
        }


        private void ThrowExceptionIfInvalidArgumentName(ref Token token)
        {
            if (token.Kind != CccTokenKind.Identifier)
            {
                ThrowExceptionCompileError($"無効な引数名 '{token.Text}' です", 0);
            }
        }


        private void ThrowExceptionIfInvalidGlobalVariableName(ref Token token)
        {
            if (token.Kind != CccTokenKind.Identifier)
            {
                ThrowExceptionCompileError($"無効なグローバル変数名 '{token.Text}' です", 0);
            }
        }


        private void ThrowExceptionIfInvalidImportPeripheralFunctionName(ref Token token)
        {
            if (token.Kind != CccTokenKind.Identifier)
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
            if (token.Kind == CccTokenKind.TypeVoid || token.Kind == CccTokenKind.TypeInt || token.Kind == CccTokenKind.TypeNumber || token.Kind == CccTokenKind.TypeString)
            {
                return;
            }


            ThrowExceptionCompileError($"戻り値の型 '{token.Text}' が正しくありません。戻り値の型として使えるのは 'void' 'int' 'number' 'string' のいずれかです。", 0);
        }


        private void ThrowExceptionIfNotArgumentType(ref Token token)
        {
            if (token.Kind == CccTokenKind.TypeInt || token.Kind == CccTokenKind.TypeNumber || token.Kind == CccTokenKind.TypeString)
            {
                return;
            }


            ThrowExceptionCompileError($"引数の型 '{token.Text}' が正しくありません。引数の型として使えるのは 'int' 'number' 'string' のいずれかです。", 0);
        }


        private void ThrowExceptionIfNotVariableType(ref Token token)
        {
            if (token.Kind == CccTokenKind.TypeInt || token.Kind == CccTokenKind.TypeNumber || token.Kind == CccTokenKind.TypeString)
            {
                return;
            }


            ThrowExceptionCompileError($"変数の型 '{token.Text}' が正しくありません。変数の型として使えるのは 'int' 'number' 'string' のいずれかです。", 0);
        }


        private void ThrowExceptionIfInvalidPeripheralName(ref Token token)
        {
            if (token.Kind != CccTokenKind.Identifier)
            {
                ThrowExceptionCompileError($"不明な周辺機器名 '{token.Text}' です", 0);
            }
        }


        /// <summary>
        /// 例外としてコンパイルエラーをスローします
        /// </summary>
        /// <param name="message">スローするメッセージ</param>
        private void ThrowExceptionCompileError(string message, uint errorCode)
        {
            // 現在のコンテキストから必要な情報を取り出してロガーにエラーを書き込んで例外を吐く
            var scriptName = currentContext.ScriptName;
            var lexer = currentContext.Lexer;
            ref var token = ref lexer.LastReadToken;
            logger.Write(CccParserLogType.Error, scriptName, token.LineNumber, token.ColumnNumber, errorCode, message);
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
            public CccParser Parser { get; private set; }

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
            public ParserContext(string scriptName, CccParser parser)
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