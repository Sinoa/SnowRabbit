// zlib License
// 
// Copyright (c) 2026 Sinoa
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

using System.CommandLine;
using SnowRabbit.Compiler;

namespace SnowRabbitCompiler;

internal class ApplicationMain
{
    private static int Main(string[] args)
    {
        RootCommand rootCommand = new("SnowRabbit Script Compiler - .srsファイルをバイナリにコンパイルします");

        Argument<string[]> inputArgument = new(
            name: "input",
            description: "コンパイルする.srsファイル（複数指定可、ワイルドカード対応）")
        {
            Arity = ArgumentArity.OneOrMore
        };

        Option<string?> outputOption = new(
            aliases: ["-o", "--output"],
            description: "出力ファイルパス（単一ファイルの場合のみ有効）");

        Option<bool> symbolsOption = new(
            aliases: ["-s", "--symbols"],
            description: "シンボル情報を出力に含める",
            getDefaultValue: () => false);

        Option<bool> objectOption = new(
            aliases: ["-c", "--object"],
            description: "実行バイナリではなく #link 可能なオブジェクトファイル (.sro) を出力する",
            getDefaultValue: () => false);

        Option<bool> verboseOption = new(
            aliases: ["-v", "--verbose"],
            description: "詳細な出力を表示",
            getDefaultValue: () => false);

        rootCommand.AddArgument(inputArgument);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(symbolsOption);
        rootCommand.AddOption(objectOption);
        rootCommand.AddOption(verboseOption);

        rootCommand.SetHandler(CompileFiles, inputArgument, outputOption, symbolsOption, objectOption, verboseOption);

        // Invoke の戻り値 (void ハンドラでは常に 0) が Environment.ExitCode を上書きしてしまうため、
        // ハンドラ内で設定した ExitCode を失敗として反映する
        int invokeResult = rootCommand.Invoke(args);
        return invokeResult != 0 ? invokeResult : Environment.ExitCode;
    }

    private static void CompileFiles(string[] inputs, string? output, bool symbols, bool objectMode, bool verbose)
    {
        List<string> files = ExpandInputFiles(inputs);

        if (files.Count == 0)
        {
            Console.Error.WriteLine("エラー: コンパイル対象のファイルが見つかりません。");
            Environment.ExitCode = 1;
            return;
        }

        if (output != null && files.Count > 1)
        {
            Console.Error.WriteLine("エラー: 複数ファイルをコンパイルする場合、-o オプションは使用できません。");
            Environment.ExitCode = 1;
            return;
        }

        if (objectMode && symbols && verbose)
        {
            Console.WriteLine("注記: オブジェクトファイルは常に完全なシンボル情報を含むため、-s オプションは無視されます。");
        }

        int successCount = 0;
        int failCount = 0;

        foreach (string file in files)
        {
            string outputPath = output ?? Path.ChangeExtension(file, objectMode ? ".sro" : ".bin");

            if (verbose)
            {
                Console.WriteLine($"コンパイル中: {file} -> {outputPath}");
            }

            bool success = CompileSingleFile(file, outputPath, symbols, objectMode, verbose);

            if (success)
            {
                successCount++;
                if (verbose)
                {
                    Console.WriteLine($"成功: {file}");
                }
            }
            else
            {
                failCount++;
            }
        }

        if (verbose || files.Count > 1)
        {
            Console.WriteLine();
            Console.WriteLine($"コンパイル完了: 成功 {successCount}, 失敗 {failCount}");
        }

        if (failCount > 0)
        {
            Environment.ExitCode = 1;
        }
    }

    private static List<string> ExpandInputFiles(string[] inputs)
    {
        List<string> files = [];

        foreach (string input in inputs)
        {
            if (input.Contains('*') || input.Contains('?'))
            {
                string? directory = Path.GetDirectoryName(input);
                string pattern = Path.GetFileName(input);

                if (string.IsNullOrEmpty(directory))
                {
                    directory = ".";
                }

                try
                {
                    string[] matchedFiles = Directory.GetFiles(directory, pattern);
                    files.AddRange(matchedFiles);
                }
                catch (DirectoryNotFoundException)
                {
                    Console.Error.WriteLine($"警告: ディレクトリが見つかりません: {directory}");
                }
            }
            else
            {
                if (File.Exists(input))
                {
                    files.Add(input);
                }
                else
                {
                    Console.Error.WriteLine($"警告: ファイルが見つかりません: {input}");
                }
            }
        }

        return files;
    }

    private static bool CompileSingleFile(string inputPath, string outputPath, bool symbols, bool objectMode, bool verbose)
    {
        // 失敗時に既存の出力ファイルを壊したり書きかけのファイルを残したりしないよう、
        // 一時ファイルへ書き込んでから成功時にのみ出力先へ置き換える
        string temporaryPath = outputPath + ".tmp";

        try
        {
            using SrCompiler compiler = new();
            compiler.IsContainSymbolInfo = symbols;

            string? outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            using (FileStream outputStream = new(temporaryPath, FileMode.Create))
            {
                if (objectMode)
                {
                    compiler.CompileObject(inputPath, outputStream);
                }
                else
                {
                    compiler.Compile(inputPath, outputStream);
                }
            }

            File.Move(temporaryPath, outputPath, overwrite: true);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"エラー: {inputPath} のコンパイルに失敗しました。");
            if (verbose)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            else
            {
                Console.Error.WriteLine($"  {ex.Message}");
            }

            try
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
            catch (IOException)
            {
                // 一時ファイルの削除失敗はコンパイル結果に影響しないため無視する
            }

            return false;
        }
    }
}