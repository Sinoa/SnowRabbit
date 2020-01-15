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
using System.IO;
using SnowRabbit.Compiler.Assembler;
using SnowRabbit.Compiler.IO;
using SnowRabbit.Compiler.Parser;
using SnowRabbit.Compiler.Parser.SyntaxNodes;

namespace SnowRabbit.Compiler
{
    /// <summary>
    /// SnowRabbit が提供するコンパイラ機能を提供するクラスです
    /// </summary>
    public class SrCompiler : SrDisposable
    {
        // メンバ変数定義
        private readonly ISrScriptStorage scriptStorage;



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
        public SrCompiler(ISrScriptStorage storage)
        {
            // 参照を受け取る
            scriptStorage = storage ?? throw new ArgumentNullException(nameof(storage));
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
        /// パーサを使用してスクリプトから構文木を作ります
        /// </summary>
        /// <param name="path">構文解析する対象となるスクリプトのパス</param>
        /// <param name="node">生成された構文木を出力する先の参照</param>
        private void Parse(string path, out SyntaxNode node)
        {
            // パーサを生成して構文解析をする
            node = new SrParser(scriptStorage).Parse(path);
        }


        /// <summary>
        /// 構文木からアセンブリコードを作り出すためにコンパイルをします
        /// </summary>
        /// <param name="node">生成された構文木のルートノード</param>
        /// <param name="assemblyData">コンパイルされた結果のアセンブリデータを出力する先の参照</param>
        private void Compile(SyntaxNode node, out SrAssemblyData assemblyData)
        {
            // コンパイルしてアセンブリデータを渡す
            var compileContext = new SrCompileContext();
            node.Compile(compileContext);
            assemblyData = compileContext.AssemblyData;
        }


        /// <summary>
        /// アセンブリデータから最終的な実行コードを出力します
        /// </summary>
        /// <param name="assemblyData">コンパイルされたアセンブリデータ</param>
        /// <param name="outStream">実行コードを出力するストリーム</param>
        private void Assemble(SrAssemblyData assemblyData, Stream outStream)
        {
            // アセンブラを生成してアセンブル
            new SrAssembler().Assemble(assemblyData, outStream);
        }
    }
}