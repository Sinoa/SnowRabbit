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

using System.IO;
using CarrotCompilerCollection.Compiler;
using CarrotCompilerCollection.IO;
using CarrotCompilerCollection.Utility;
using SnowRabbit.Machine;

namespace SampleApplication01
{
    /// <summary>
    /// アプリケーションの起動クラスです
    /// </summary>
    internal static class ApplicationMain
    {
        /// <summary>
        /// アプリケーションのエントリポイントです
        /// </summary>
        /// <param name="args">アプリケーションのコマンドライン引数</param>
        private static void Main(string[] args)
        {
            // コンパイラの起動準備をする
            var consoleCompilerLogger = new CccConsoleParserLogger();
            var fileSystemStorage = new FileSystemScriptStorage();
            var compiler = new CccParser(fileSystemStorage, consoleCompilerLogger);


            // 仮想マシンの起動準備をする
            var machine = new SrvmSimplyMachine();


            // コンパイルされた実行コードを受け取るメモリストリームを生成する
            using (var programStream = new MemoryStream())
            {
                // コンパイルしてストリーム位置を先頭に移動してからプロセスを生成して実行
                compiler.Compile("Assets/Sample.csf", programStream);
                programStream.Seek(0, SeekOrigin.Begin);
                machine.CreateProcess(programStream, out var process);
                machine.ExecuteProcess(ref process);
            }
        }
    }
}