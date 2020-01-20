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
using System.Collections.Generic;

namespace SnowRabbit.Compiler.Reporter
{
    /// <summary>
    /// コンソールに対してコンパイルレポートを表示するクラスです
    /// </summary>
    public class SrCompileReportConsolePrinter : ISrCompileReportPrinter
    {
        private static readonly Dictionary<CompileReportLevel, ConsoleColorData> colorTable;



        static SrCompileReportConsolePrinter()
        {
            colorTable = new Dictionary<CompileReportLevel, ConsoleColorData>()
            {
                { CompileReportLevel.Error, new ConsoleColorData(ConsoleColor.Gray, ConsoleColor.Red) }
            };
        }


        public void PrintReport(CompileReport report)
        {
            var colorData = colorTable[report.Level];
            Console.BackgroundColor = colorData.BackgroundColor;
            Console.ForegroundColor = colorData.ForegroundColor;
            Console.Write($"{report.Token.ReaderName} (Line:{report.Token.LineNumber} Column:{report.Token.ColumnNumber}) {report.Message}");
            Console.ResetColor();
        }



        private readonly struct ConsoleColorData
        {
            public readonly ConsoleColor BackgroundColor;
            public readonly ConsoleColor ForegroundColor;



            public ConsoleColorData(ConsoleColor background, ConsoleColor foreground)
            {
                BackgroundColor = background;
                ForegroundColor = foreground;
            }
        }
    }
}
