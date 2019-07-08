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
using System.Runtime.Serialization;

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


            // 式解析関数を呼び出して、スタックに値が積まれていれば取り出して返す
            ParseAddExpression();
            return accumulatorStack.Count > 0 ? accumulatorStack.Pop() : 0.0;
        }


        /// <summary>
        /// 加算演算（+ -）の構文解析を実行します
        /// </summary>
        private void ParseAddExpression()
        {
            // 優先度の高い乗算系の構文解析をする
            ParseMulExpression();


            // プラス, マイナス が見つかる間はループ
            ref var token = ref lexer.LastReadToken;
            int lastFetchKind = token.Kind;
            while (token.Kind == TokenKind.Plus || token.Kind == TokenKind.Minus)
            {
                // 次のトークンを読み出してから優先度の高い乗算系の構文解析をする
                lexer.ReadNextToken();
                ParseMulExpression();


                // 積まれた結果を計算する
                DoOperation(lastFetchKind);
            }
        }


        /// <summary>
        /// 乗算演算（* / %）の構文解析を実行します
        /// </summary>
        private void ParseMulExpression()
        {
            // 優先度の高い単項演算の構文解析をする
            ParseUnaryExpression();


            // アスタリスク, スラッシュ, パーセント が見つかる間はループ
            ref var token = ref lexer.LastReadToken;
            int lastFetchKind = token.Kind;
            while (token.Kind == TokenKind.Asterisk || token.Kind == TokenKind.Slash || token.Kind == TokenKind.Percent)
            {
                // 次のトークンを読み出してから単項演算の構文解析をする
                lexer.ReadNextToken();
                ParseUnaryExpression();


                // 積まれた結果を計算する
                DoOperation(lastFetchKind);
            }
        }


        /// <summary>
        /// 単項演算の構文解析を実行します
        /// </summary>
        /// <exception cref="SimpleExpressionProcessorSyntaxErrorException">'variableName' が未定義です</exception>
        /// <exception cref="SimpleExpressionProcessorSyntaxErrorException">'(' に対応する ')' が存在しません</exception>
        /// <exception cref="SimpleExpressionProcessorSyntaxErrorException">未定義のトークン 'token' です</exception>
        private void ParseUnaryExpression()
        {
            // 最後に読み取られたトークンの種類に応じて処理を変える
            ref var token = ref lexer.LastReadToken;
            switch (token.Kind)
            {
                // 整数なら実数として計算スタックに詰む
                case TokenKind.Integer:
                    accumulatorStack.Push(token.Integer);
                    break;


                // 実数なら実数として計算スタックに詰む
                case TokenKind.Number:
                    accumulatorStack.Push(token.Number);
                    break;


                // 識別子なら変数から実数として計算スタックに詰む
                case TokenKind.Identifier:
                    PushVariableValue(token.Text);
                    break;


                // オープンパーレンではなくクローズパーレンが先に来ているなら
                case TokenKind.CloseParen:
                    ThrowOpenParenNotFound();
                    break;


                // オープンパーレンなら次のトークンを読み取って加算演算のパースから始めて
                // 最後にクローズパーレンが来ていないなら構文エラー
                case TokenKind.OpenParen:
                    lexer.ReadNextToken();
                    ParseAddExpression();
                    ThrowIfCloseParenNotFound();
                    break;


                // 想定外のトークンが来たら例外を投げる
                default:
                    throw new SimpleExpressionProcessorSyntaxErrorException($"未定義のトークン '{token.Text}' です");
            }


            // 次のトークンを読み込んでおく
            lexer.ReadNextToken();
        }


        /// <summary>
        /// 指定された演算子の種類に応じて計算スタックから値を取り出して計算を行います
        /// </summary>
        /// <param name="operationTokenKind">計算する方法を示す演算子のトークン種別</param>
        /// <exception cref="SimpleExpressionProcessorSyntaxErrorException">0除算の実行を行おうとしました</exception>
        private void DoOperation(int operationTokenKind)
        {
            // 計算スタックから値を取り出す
            var rValue = accumulatorStack.Pop();
            var lValue = accumulatorStack.Pop();


            // 演算子が除算または剰余の場合に0除算になってしまうかどうかを判定
            if ((operationTokenKind == TokenKind.Slash || operationTokenKind == TokenKind.Percent) && rValue == 0.0)
            {
                // 0除算になってしまう場合は例外を送出する
                throw new SimpleExpressionProcessorSyntaxErrorException("0除算の実行を行おうとしました");
            }


            // 演算子によって処理を変える
            switch (operationTokenKind)
            {
                // 加算演算子なら加算してプッシュする
                case TokenKind.Plus:
                    accumulatorStack.Push(lValue + rValue);
                    break;


                // 減算演算子なら減算してプッシュする
                case TokenKind.Minus:
                    accumulatorStack.Push(lValue - rValue);
                    break;


                // 乗算演算子なら乗算してプッシュする
                case TokenKind.Asterisk:
                    accumulatorStack.Push(lValue * rValue);
                    break;


                // 除算演算子なら除算してプッシュする
                case TokenKind.Slash:
                    accumulatorStack.Push(lValue / rValue);
                    break;


                // 剰余演算子なら剰余してプッシュする
                case TokenKind.Percent:
                    accumulatorStack.Push(lValue % rValue);
                    break;
            }
        }


        /// <summary>
        /// 指定された変数名の値を計算スタックにプッシュします
        /// </summary>
        /// <param name="variableName"></param>
        /// <exception cref="SimpleExpressionProcessorSyntaxErrorException">'variableName' が未定義です</exception>
        private void PushVariableValue(string variableName)
        {
            // 変数の値の取得を試みて成功したのなら
            if (variableTable.TryGetValue(variableName, out var value))
            {
                // 値をプッシュして終了
                accumulatorStack.Push(value);
                return;
            }


            // 変数が見つからなかった例外を投げる
            throw new SimpleExpressionProcessorSyntaxErrorException($"'{variableName}' が未定義です");
        }


        /// <summary>
        /// オープンパーレンが見つからない例外をスローします
        /// </summary>
        /// <exception cref="SimpleExpressionProcessorSyntaxErrorException">')' に対応する '(' が存在しません</exception>
        private void ThrowOpenParenNotFound()
        {
            // 例外を投げる
            ref var token = ref lexer.LastReadToken;
            throw new SimpleExpressionProcessorSyntaxErrorException($"[{token.LineNumber}行 {token.ColumnNumber}列] ')' に対応する '(' が存在しません");
        }


        /// <summary>
        /// クローズパーレンが見つからない場合は例外をスローします
        /// </summary>
        /// <exception cref="SimpleExpressionProcessorSyntaxErrorException">'(' に対応する ')' が存在しません</exception>
        private void ThrowIfCloseParenNotFound()
        {
            // 最後に読み取ったものがクローズパーレン出ない場合
            ref var token = ref lexer.LastReadToken;
            if (token.Kind != TokenKind.CloseParen)
            {
                // 例外を投げる
                throw new SimpleExpressionProcessorSyntaxErrorException($"[{token.LineNumber}行 {token.ColumnNumber}列] '(' に対応する ')' が存在しません");
            }
        }
    }



    /// <summary>
    /// SimpleExpressionProcessor にて構文エラーが発生した場合の例外です
    /// </summary>
    public class SimpleExpressionProcessorSyntaxErrorException : Exception
    {
        /// <summary>
        /// SimpleExpressionProcessorSyntaxErrorException のインスタンスを初期化します
        /// </summary>
        public SimpleExpressionProcessorSyntaxErrorException() : base()
        {
        }


        /// <summary>
        /// SimpleExpressionProcessorSyntaxErrorException のインスタンスを初期化します
        /// </summary>
        /// <param name="message">発生した例外のメッセージ</param>
        public SimpleExpressionProcessorSyntaxErrorException(string message) : base(message)
        {
        }


        /// <summary>
        /// SimpleExpressionProcessorSyntaxErrorException のインスタンスを初期化します
        /// </summary>
        /// <param name="message">発生した例外のメッセージ</param>
        /// <param name="innerException">この例外を発生させた原因の例外</param>
        public SimpleExpressionProcessorSyntaxErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }


        /// <summary>
        /// シリアル化したデータから SimpleExpressionProcessorSyntaxErrorException のインスタンスを初期化します
        /// </summary>
        /// <param name="info">シリアル化されたオブジェクト情報</param>
        /// <param name="context">シリアルデータの転送コンテキスト</param>
        protected SimpleExpressionProcessorSyntaxErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}