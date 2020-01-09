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

using SnowRabbit.Compiler.Lexer;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 代入式構文を表す構文ノードクラスです
    /// </summary>
    public class AssignmentExpressionSyntaxNode : SyntaxNode
    {
        // メンバ変数定義
        private Token operation;



        /// <summary>
        /// この構文ノードが対応する構文ノードを生成します
        /// </summary>
        /// <param name="context">コンパイルする対象となる翻訳単位コンテキスト</param>
        /// <returns>構文ノードを生成出来た場合は構文ノードのインスタンスを、生成出来ない場合は null を返します</returns>
        public static SyntaxNode Create(LocalCompileContext context)
        {
            // トークンの参照を取得
            ref var token = ref context.Lexer.LastReadToken;


            // 条件論理和構文ノードの生成をして代入記号が存在する間ループ
            var expression = ConditionOrExpressionSyntaxNode.Create(context);
            while (IsAssignmentSimbol(ref token))
            {
                // この段階で実行するべき代入演算を覚える
                var operation = token;


                // 次のトークンを読み込んで条件論理和構文の生成をする
                context.Lexer.ReadNextToken();
                var rightExpression = ConditionOrExpressionSyntaxNode.Create(context);


                // 代入式構文ノードを生成する
                var assignmentExpression = new AssignmentExpressionSyntaxNode();
                assignmentExpression.operation = operation;
                assignmentExpression.Add(rightExpression);
                assignmentExpression.Add(expression);


                // 自身が左辺になる
                expression = assignmentExpression;
            }


            // 最終的に決定された式を返す
            return expression;
        }


        /// <summary>
        /// 指定されたトークンが代入記号かどうかを判断します
        /// </summary>
        /// <param name="token">判断するトークンへの参照</param>
        /// <returns>トークンが代入記号ならば true を、異なる場合は false を返します</returns>
        private static bool IsAssignmentSimbol(ref Token token)
        {
            // 代入記号のいづれかに一致するかを返す
            return
                token.Kind == TokenKind.Equal ||
                token.Kind == TokenKind.PlusEqual ||
                token.Kind == TokenKind.MinusEqual ||
                token.Kind == TokenKind.AsteriskEqual ||
                token.Kind == TokenKind.SlashEqual ||
                token.Kind == TokenKind.AndEqual ||
                token.Kind == TokenKind.VerticalbarEqual ||
                token.Kind == TokenKind.CircumflexEqual;
        }
    }
}