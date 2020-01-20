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
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.Compiler.Parser.SyntaxErrors;

namespace SnowRabbit.Compiler.Reporter
{
    public class SyntaxErrorReporter
    {
        private ISrCompileReportPrinter reportPrinter;



        public SyntaxErrorReporter(ISrCompileReportPrinter printer)
        {
            reportPrinter = printer ?? throw new ArgumentNullException(nameof(printer));
        }


        private SrSyntaxErrorException CreateAndReportErrorMessage(in Token token, string message)
        {
            reportPrinter.PrintReport(new CompileReport(token, message, CompileReportLevel.Error));
            return new SrSyntaxErrorException(message);
        }


        public SrSyntaxErrorException NotSymbolNotPair(in Token token, string startSymbol, string endSymbol)
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
    }
}
