// zlib/libpng License
//
// Copyright(c) 2020 Sinoa
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
using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.Compiler.Parser.SyntaxErrors;

namespace SnowRabbit.Compiler.Reporter
{
    public class SyntaxErrorReporter
    {
        private readonly ISrCompileReportPrinter reportPrinter;



        public SyntaxErrorReporter(ISrCompileReportPrinter printer)
        {
            reportPrinter = printer ?? throw new ArgumentNullException(nameof(printer));
        }


        private SrSyntaxErrorException CreateAndReportErrorMessage(in Token token, string message)
        {
            reportPrinter.PrintReport(new CompileReport(token, message, CompileReportLevel.Error));
            return new SrSyntaxErrorException(message);
        }


        public SrSyntaxErrorException NotSymbolPair(in Token token, string startSymbol, string endSymbol)
        {
            return CreateAndReportErrorMessage(token, $"'{startSymbol}' に対応する '{endSymbol}' がありません。");
        }


        public SrSyntaxErrorException UnknownDirective(in Token token, string directiveName)
        {
            return CreateAndReportErrorMessage(token, $"不明なディレクティブ '{directiveName}' です。");
        }


        public SrSyntaxErrorException UnknownToken(in Token token)
        {
            return CreateAndReportErrorMessage(token, $"不明なトークン '{token.Text}' です。");
        }


        public SrSyntaxErrorException UnknownSymbol(in Token token)
        {
            return CreateAndReportErrorMessage(token, $"不明なシンボル '{token.Text}' です。");
        }


        public SrSyntaxErrorException MainFunctionNotFound(in Token token)
        {
            return CreateAndReportErrorMessage(token, "main関数が見つかりませんでした。");
        }


        public SrSyntaxErrorException NotSymbolEnd(in Token token, string symbolText)
        {
            return CreateAndReportErrorMessage(token, $"'{symbolText}' で終了していません。");
        }


        public SrSyntaxErrorException PredefinedSymbol(in Token token, string symbolName)
        {
            return CreateAndReportErrorMessage(token, $"'{symbolName}' は、既に定義済みのシンボルです。");
        }


        public SrSyntaxErrorException InvalidIdentifier(in Token token)
        {
            return CreateAndReportErrorMessage(token, $"無効な識別子です");
        }


        public SrSyntaxErrorException NotVariable(in Token token, string identifierName)
        {
            return CreateAndReportErrorMessage(token, $"'{identifierName}' は、変数ではありません。");
        }


        public SrSyntaxErrorException NotSupportedType(in Token token, SrRuntimeType type)
        {
            return CreateAndReportErrorMessage(token, $"型 '{type}' は、サポートされていません。");
        }


        public SrSyntaxErrorException NotSupporteReturnVoid(in Token token, string functionName)
        {
            return CreateAndReportErrorMessage(token, $"関数 '{functionName}' は void を返すため有効な戻り値として使用できません。");
        }


        public SrSyntaxErrorException InvalidArgumentCount(in Token token, string functionName, int parameterCount, int argumentCount)
        {
            return CreateAndReportErrorMessage(token, $"関数 '{functionName}' のパラメータは '{parameterCount}' 個ですが、引数の数が '{argumentCount}' 個です。");
        }


        public SrSyntaxErrorException InvalidParameterStoreType(in Token token, SrRuntimeType fromType, SrRuntimeType toType)
        {
            return CreateAndReportErrorMessage(token, $"パラメータの値渡しの型が '{fromType}' から '{toType}' へ一致しません。事前にキャストするか変換を行ってください。");
        }
    }
}
