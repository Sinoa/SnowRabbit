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
using System.Collections.Generic;
using System.IO;

namespace TextProcessorLib
{
    /// <summary>
    /// 非常にシンプルな式処理クラスです
    /// </summary>
    public class SimpleExpressionProcessor
    {
        // メンバ変数定義
        private TokenReader lexer;
        private Stack<double> accumulatorStack;
        private Dictionary<string, double> variableTable;



        /// <summary>
        /// このクラスに対して、式に登場する変数の取得設定をします
        /// </summary>
        /// <param name="varName">式に登場する変数名</param>
        /// <returns>変数名に対して割り当てられた値を返しますが、変数が存在しない場合は 0.0 を返します</returns>
        public double this[string varName]
        {
            get { return variableTable.TryGetValue(varName, out var value) ? value : 0.0; }
            set => variableTable[varName] = value;
        }



        /// <summary>
        /// SimpleExpressionProcessor クラスのインスタンスを初期化します
        /// </summary>
        public SimpleExpressionProcessor()
        {
            // メンバ変数の初期化をする
            lexer = TokenReader.EmptyReader;
            accumulatorStack = new Stack<double>();
            variableTable = new Dictionary<string, double>();
        }


        /// <summary>
        /// 指示された式を実行します
        /// </summary>
        /// <param name="expression">計算したい式。"result = expression" または "expression" の文法が利用可能です</param>
        /// <returns>支持された式の結果を返します</returns>
        public double Calculate(string expression)
        {
            // 念の為スタックを空にする
            accumulatorStack.Clear();


            // 文字列リーダーを生成してレキサをリセット後最初の文字を読み取る
            lexer.Reset(new StringReader(expression));
            lexer.ReadNextToken();


            // 式解析関数を呼び出して最後に積まれた値を取り出して返す
            return accumulatorStack.Pop();
        }


        /// <summary>
        /// 加算演算（+ -）の構文解析を実行します
        /// </summary>
        private void ParseAddExpression()
        {
        }


        /// <summary>
        /// 乗算演算（* / %）の構文解析を実行します
        /// </summary>
        private void ParseMulExpression()
        {
        }


        /// <summary>
        /// 単項演算の構文解析を実行します
        /// </summary>
        private void ParseUnaryExpression()
        {
        }
    }



    /// <summary>
    /// SimpleExpressionProcessor にて構文エラーが発生した場合の例外です
    /// </summary>
    public class SimpleExpressionProcessorSyntaxErrorException : Exception
    {
    }
}