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
using CarrotAssemblerLib;

namespace CarrotAssemblerTokenViewer
{
    /// <summary>
    /// アプリケーションの起動クラスです
    /// </summary>
    internal static class ApplicationMain
    {
        /// <summary>
        /// アプリケーションのエントリポイントです
        /// </summary>
        /// <param name="args">アプリケーション引数</param>
        private static void Main(string[] args)
        {
            // コマンドライン引数に1つも渡されていないのなら
            if (args.Length < 1)
            {
                // ちょっとした説明を出して終了
                Console.WriteLine("トークンを読み取るためのスクリプトファイルを指定して下さい");
                return;
            }


            // 渡された値がファイル名であることを前提としてファイルの存在を確認する
            if (!File.Exists(args[0]))
            {
                // ファイルが見つからないことを表示して終了
                Console.WriteLine($"ファイル '{args[0]}' が見つかりません");
                return;
            }


            // ファイルを開いてトークンリーダーを生成する
            using (var reader = new CarrotAsmLexer(File.OpenRead(args[0])))
            {
                // 改行コードをトークンとして認める
                reader.AllowEndOfLineToken = true;


                // 全てのトークンを読み切るまでループ
                while (reader.ReadNextToken(out var token))
                {
                    // トークン内容を表示する
                    var lineNumber = token.LineNumber.ToString().PadLeft(4);
                    var columnNumber = token.ColumnNumber.ToString().PadLeft(4);
                    var kind = token.Kind.ToString().PadLeft(6);
                    var text = token.Text == "\n" ? string.Empty : token.Text;
                    Console.WriteLine($"({lineNumber},{columnNumber}) [{kind}] {text}");
                }
            }
        }
    }
}