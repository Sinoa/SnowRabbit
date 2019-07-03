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
using BenchmarkDotNet.Attributes;

namespace TextProcessorLibBenchmark.Benchmark
{
    /// <summary>
    /// SimpleLexer の性能評価を行うクラスです
    /// </summary>
    [MemoryDiagnoser]
    public class SimpleLexerBenchmark
    {
        // メンバ変数定義
        private TextReader reader;
        private SimpleLexer lexer;



        /// <summary>
        /// SimpleLexerBenchmark クラスのインスタンスを初期化します
        /// </summary>
        public SimpleLexerBenchmark()
        {
            // レキサのインスタンスを生成
            lexer = new SimpleLexer();
        }


        /// <summary>
        /// BenchParse ベンチを行う前の初期化をします
        /// </summary>
        [IterationSetup(Target = nameof(BenchParse))]
        public void Setup()
        {
            // 読み取るべきサンプルコードのテキストリーダとレキサの状態をリセットをする
            //reader = new StringReader("if ifelse << >> += = - + ( ) () {} [] {{}} # identifier");
            reader = new StreamReader("Assets/SampleCode.csf");
            lexer.Reset(reader);
        }


        /// <summary>
        /// BenchParse ベンチを行った後の解放をします
        /// </summary>
        [IterationCleanup(Target = nameof(BenchParse))]
        public void Cleanup()
        {
            // リーダーの解放
            reader.Dispose();
        }


        /// <summary>
        /// レキサのトークン読み込みのベンチマークを行います
        /// </summary>
        [Benchmark]
        public void BenchParse()
        {
            // すべてのトークンを読み切るまでループ
            while (lexer.ReadNextToken(out var token))
            {
            }
        }
    }
}