// zlib License
// 
// Copyright (c) 2020 Sinoa
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

using System;
using System.Collections.Generic;
using System.IO;
using SnowRabbit.Compiler.Assembler;
using SnowRabbit.Compiler.IO;
using SnowRabbit.Compiler.Parser;
using SnowRabbit.Compiler.Parser.SyntaxNodes;
using SnowRabbit.Compiler.Reporter;
using SnowRabbit.IO;

namespace SnowRabbit.Compiler
{
    /// <summary>
    /// SnowRabbit が提供するコンパイラ機能を提供するクラスです
    /// </summary>
    public class SrCompiler : SrDisposable
    {
        // メンバ変数定義
        private readonly ISrScriptStorage scriptStorage;
        private readonly ISrObjectStorage objectStorage;
        private readonly ISrCompileReportPrinter reportPrinter;



        public bool IsContainSymbolInfo { get; set; }



        /// <summary>
        /// SrCompiler クラスのインタンスを初期化します
        /// </summary>
        public SrCompiler() : this(new SrFileSystemScriptStorage())
        {
        }


        /// <summary>
        /// SrCompiler クラスのインタンスを初期化します
        /// </summary>
        /// <param name="storage">スクリプトを保持しているストレージ</param>
        /// <exception cref="ArgumentNullException">storage が null です</exception>
        public SrCompiler(ISrScriptStorage storage) : this(storage, new SrCompileReportConsolePrinter())
        {
        }


        public SrCompiler(ISrScriptStorage storage, ISrCompileReportPrinter printer) : this(storage, new SrFileSystemObjectStorage(), printer)
        {
        }


        /// <summary>
        /// SrCompiler クラスのインタンスを初期化します
        /// </summary>
        /// <param name="storage">スクリプトを保持しているストレージ</param>
        /// <param name="objectStorage">#link が参照するオブジェクトファイルを保持しているストレージ</param>
        /// <param name="printer">コンパイルレポートを出力するプリンタ</param>
        /// <exception cref="ArgumentNullException">storage または objectStorage または printer が null です</exception>
        public SrCompiler(ISrScriptStorage storage, ISrObjectStorage objectStorage, ISrCompileReportPrinter printer)
        {
            // 参照を受け取る
            scriptStorage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.objectStorage = objectStorage ?? throw new ArgumentNullException(nameof(objectStorage));
            reportPrinter = printer ?? throw new ArgumentNullException(nameof(printer));
        }


        /// <summary>
        /// 指定されたパスのスクリプトをコンパイルします
        /// </summary>
        /// <param name="path">コンパイルするスクリプトのパス</param>
        /// <param name="outStream">コンパイルした結果の実行コードを出力するストリーム</param>
        /// <exception cref="ArgumentException">path が null または 空文字列 または 空白文字列 です</exception>
        /// <exception cref="ArgumentNullException">outStream が null です</exception>
        public void Compile(string path, Stream outStream)
        {
            // パース、コンパイル、アセンブルとやっていく
            Parse(path, out var node);
            Compile(node, out var assemblyData);
            Assemble(assemblyData, outStream);
        }


        /// <summary>
        /// 指定されたパスのスクリプトを、リンク可能なオブジェクトファイル (SROB) としてコンパイルします。
        /// オブジェクトとしてコンパイルする場合 main 関数は不要で、スタートアップコードは生成されません。
        /// </summary>
        /// <param name="path">コンパイルするスクリプトのパス</param>
        /// <param name="outStream">オブジェクトファイルを出力するストリーム</param>
        /// <exception cref="ArgumentException">path が null または 空文字列 または 空白文字列 です</exception>
        /// <exception cref="ArgumentNullException">outStream が null です</exception>
        public void CompileObject(string path, Stream outStream)
        {
            if (outStream == null) throw new ArgumentNullException(nameof(outStream));


            // パースとオブジェクトモードでのコンパイルをして、アドレス解決前のアセンブリデータをそのまま書き込む
            Parse(path, out var node);
            Compile(node, out var assemblyData, true);
            using (var writer = new SrObjectDataWriter(outStream, true))
            {
                writer.Write(assemblyData);
            }
        }


        /// <summary>
        /// パーサを使用してスクリプトから構文木を作ります
        /// </summary>
        /// <param name="path">構文解析する対象となるスクリプトのパス</param>
        /// <param name="node">生成された構文木を出力する先の参照</param>
        public void Parse(string path, out SyntaxNode node)
        {
            // パーサを生成して構文解析をする
            node = new SrParser(scriptStorage, reportPrinter).Parse(path);
        }


        /// <summary>
        /// 構文木からアセンブリコードを作り出すためにコンパイルをします
        /// </summary>
        /// <param name="node">生成された構文木のルートノード</param>
        /// <param name="assemblyData">コンパイルされた結果のアセンブリデータを出力する先の参照</param>
        public void Compile(SyntaxNode node, out SrAssemblyData assemblyData)
        {
            // 通常（実行コード生成）モードでコンパイルする
            Compile(node, out assemblyData, false);
        }


        /// <summary>
        /// 構文木からアセンブリコードを作り出すためにコンパイルをします
        /// </summary>
        /// <param name="node">生成された構文木のルートノード</param>
        /// <param name="assemblyData">コンパイルされた結果のアセンブリデータを出力する先の参照</param>
        /// <param name="isObjectCompileMode">リンク可能なオブジェクトとしてコンパイルする場合は true</param>
        public void Compile(SyntaxNode node, out SrAssemblyData assemblyData, bool isObjectCompileMode)
        {
            // #link されたオブジェクトを先にインポートしてからコンパイルする
            // （関数呼び出しなどのシンボル解決はコンパイル時に行われるため、シンボルが先に揃っている必要がある）
            var compileContext = new SrCompileContext(reportPrinter, isObjectCompileMode);
            ImportLinkObjects(node, compileContext, new HashSet<string>());
            node.Compile(compileContext);
            assemblyData = compileContext.AssemblyData;
        }


        /// <summary>
        /// 構文木から #link ディレクティブを収集して、参照されたオブジェクトファイルをインポートします。
        /// #compile で取り込まれたスクリプトの #link も対象になります。同一パスの重複リンクはスキップされます。
        /// </summary>
        /// <param name="node">走査する構文木のノード</param>
        /// <param name="context">インポート先のコンパイルコンテキスト</param>
        /// <param name="linkedPaths">既にリンクされたパスの集合</param>
        private void ImportLinkObjects(SyntaxNode node, SrCompileContext context, HashSet<string> linkedPaths)
        {
            if (!(node is CompileUnitSyntaxNode)) return;


            foreach (var child in node.Children)
            {
                if (child is LinkObjectDirectiveSyntaxNode linkDirective)
                {
                    ImportLinkObject(linkDirective, context, linkedPaths);
                }
                else if (child is CompileUnitSyntaxNode)
                {
                    // #compile で展開されたスクリプトの中の #link も対象にする
                    ImportLinkObjects(child, context, linkedPaths);
                }
            }
        }


        private void ImportLinkObject(LinkObjectDirectiveSyntaxNode linkDirective, SrCompileContext context, HashSet<string> linkedPaths)
        {
            // 同一パスの重複リンクはスキップする（複数のスクリプトが同じライブラリをリンクする構成を許容する）
            var path = linkDirective.Token.Text;
            if (!linkedPaths.Add(path)) return;


            var stream = objectStorage.OpenRead(path);
            if (stream == null)
            {
                throw context.ErrorReporter.LinkObjectNotFound(linkDirective.Token, path);
            }


            try
            {
                SrObjectData objectData;
                using (var reader = new SrObjectDataReader(stream))
                {
                    objectData = reader.Read();
                }
                SrObjectImporter.Import(objectData, context, linkDirective.Token, path);
            }
            catch (SrMalformedObjectDataException error)
            {
                // 壊れたオブジェクトはコンパイルエラーとして報告する
                throw context.ErrorReporter.InvalidLinkObject(linkDirective.Token, path, error.Message);
            }
        }


        /// <summary>
        /// アセンブリデータから最終的な実行コードを出力します
        /// </summary>
        /// <param name="assemblyData">コンパイルされたアセンブリデータ</param>
        /// <param name="outStream">実行コードを出力するストリーム</param>
        public void Assemble(SrAssemblyData assemblyData, Stream outStream)
        {
            // アセンブラを生成してアセンブル
            new SrAssembler() { IsContainSymbolInfo = IsContainSymbolInfo }.Assemble(assemblyData, outStream);
        }
    }
}