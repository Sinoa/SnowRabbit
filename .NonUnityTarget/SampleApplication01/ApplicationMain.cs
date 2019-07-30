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
using System.IO;
using CarrotCompilerCollection.Compiler;
using CarrotCompilerCollection.IO;
using CarrotCompilerCollection.Utility;
using SnowRabbit.Machine;
using SnowRabbit.Runtime;

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
            machine.AttachPeripheral(new SamplePeripheral());


            // コンパイルされた実行コードを受け取るメモリストリームを生成する
            using (var programStream = new MemoryStream())
            {
                // コンパイルする
                compiler.Compile("Assets/Sample.csf", programStream);


                // ストリーム位置を先頭に移動してディスアセンブル
                programStream.Seek(0, SeekOrigin.Begin);
                Console.Write(compiler.Disassemble(programStream));


                // ストリームの先頭に移動して実行
                programStream.Seek(0, SeekOrigin.Begin);
                machine.CreateProcess(programStream, out var process);
                process.Run();
                process.Terminate();
            }
        }
    }



    /// <summary>
    /// 非常にシンプルな周辺機器クラスです
    /// </summary>
    internal class SamplePeripheral : SrvmPeripheral
    {
        /// <summary>
        /// 周辺機器名
        /// </summary>
        public override string PeripheralName => "Sample";



        /// <summary>
        /// この周辺機器が提供する関数をセットアップします
        /// </summary>
        /// <param name="registryHandler">関数を登録する関数</param>
        protected override void SetupFunction(Action<string, Func<SrStackFrame, HostFunctionResult>> registryHandler)
        {
            // 関数の登録をしていく
            registryHandler(nameof(ConsoleWriteLine), ConsoleWriteLine);
            registryHandler(nameof(Stop), Stop);
        }


        /// <summary>
        /// スクリプトから文字列を受け取ってコンソールに書き込みます
        /// </summary>
        /// <param name="stackFrame">この関数用のスタックフレーム</param>
        /// <returns>継続を返します</returns>
        private HostFunctionResult ConsoleWriteLine(SrStackFrame stackFrame)
        {
            // 第一引数の文字列を出す
            Console.WriteLine((string)stackFrame.GetObject(0));
            return HostFunctionResult.Continue;
        }


        /// <summary>
        /// スクリプトの動作を停止させます
        /// </summary>
        /// <param name="stackFrame">この関数用のスタックフレーム</param>
        /// <returns>停止を返します</returns>
        private HostFunctionResult Stop(SrStackFrame stackFrame)
        {
            // 停止を要求する
            return HostFunctionResult.Pause;
        }
    }
}