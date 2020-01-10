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

using System.Collections.Generic;
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.Compiler.Parser.SyntaxErrors;
using SnowRabbit.Diagnostics.Logging;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 1つの構文を表す構文ノード抽象クラスです
    /// </summary>
    public abstract class SyntaxNode
    {
        // メンバ変数定義
        private Token token;
        private List<SyntaxNode> children;



        /// <summary>
        /// 現れたトークンへの参照
        /// </summary>
        public ref Token Token => ref token;


        /// <summary>
        /// この構文にぶら下がる子構文ノードリスト
        /// </summary>
        public IReadOnlyList<SyntaxNode> Children { get; }



        /// <summary>
        /// SyntaxNode クラスのインスタンスを初期化します
        /// </summary>
        protected SyntaxNode()
        {
            // トークンは既定値を使用（既定値のトークンは不明トークン[Kind == 0]として扱われる）
            token = default;
            children = new List<SyntaxNode>();
            Children = children.AsReadOnly();
        }


        /// <summary>
        /// SyntaxNode クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="token">適応するトークンへの参照</param>
        protected SyntaxNode(in Token token)
        {
            // トークンを受け取る
            this.token = token;
            children = new List<SyntaxNode>();
            Children = children.AsReadOnly();
        }


        /// <summary>
        /// 子構文ノードを追加します
        /// </summary>
        /// <param name="node">追加する子構文ノード</param>
        public void Add(SyntaxNode node)
        {
            // 追加する
            children.Add(node);
        }


        /// <summary>
        /// 該当のトークンが登場しているかチェックして問題がなければ次のトークンを読み込みます
        /// </summary>
        /// <param name="tokenKind">チェックするトークン種別</param>
        /// <param name="context">現在のコンテキスト</param>
        protected static void CheckTokenAndReadNext(int tokenKind, LocalCompileContext context)
        {
            // 不明なトークン例外としてチェックする
            CheckTokenAndReadNext(tokenKind, context, new SrUnknownTokenSyntaxErrorException(ref context.Lexer.LastReadToken));
        }


        /// <summary>
        /// 該当のトークンが登場しているかチェックして問題がなければ次のトークンを読み込みます
        /// </summary>
        /// <param name="tokenKind">チェックするトークン種別</param>
        /// <param name="context">現在のコンテキスト</param>
        /// <param name="exception">不明なトークンエラー以外に使用する場合は対応するエラー例外オブジェクトを指定</param>
        protected static void CheckTokenAndReadNext(int tokenKind, LocalCompileContext context, SrSyntaxErrorException exception)
        {
            // 該当トークンで無いなら
            ref var token = ref context.Lexer.LastReadToken;
            if (token.Kind != tokenKind)
            {
                // 不明なトークンとしてコンパイルエラーとする
                context.ThrowSyntaxError(exception);
                return;
            }


            // 次のトークンを読み込む
            context.Lexer.ReadNextToken();
        }


        /// <summary>
        /// node が null でないなら追加し null の場合はコンパイルエラーを出します
        /// </summary>
        /// <param name="node">チェックする構文ノード</param>
        /// <param name="context">現在のコンテキスト</param>
        protected void CheckSyntaxAndAddNode(SyntaxNode node, LocalCompileContext context)
        {
            CheckSyntaxAndAddNode(node, context, new SrUnknownTokenSyntaxErrorException(ref context.Lexer.LastReadToken));
        }


        /// <summary>
        /// node が null でないなら parentNode に追加し null の場合はコンパイルエラーを出します
        /// </summary>
        /// <param name="node">チェックする構文ノード</param>
        /// <param name="context">現在のコンテキスト</param>
        /// <param name="exception">不明なトークンエラー以外に使用する場合は対応するエラー例外オブジェクトを指定</param>
        protected void CheckSyntaxAndAddNode(SyntaxNode node, LocalCompileContext context, SrSyntaxErrorException exception)
        {
            // ノードが null なら
            if (node == null)
            {
                // 不明なトークンとしてコンパイルエラーを出す
                context.ThrowSyntaxError(exception);
                return;
            }


            // null でないなら素直に追加
            Add(node);
        }


        /// <summary>
        /// この構文ノードに対応する構文を解釈します
        /// </summary>
        /// <param name="context">構文解釈する対象となっている翻訳単位コンテキスト</param>
        /// <returns>解釈した結果の構文ノードを返しますが、正しく解釈出来なかった場合は null を返します。</returns>
        public virtual SyntaxNode Compile(LocalCompileContext context)
        {
            // 既定は null を返す
            SrLogger.Trace(nameof(SyntaxNode), $"'{GetType().Name}' is not implemented 'Compile' method.");
            return null;
        }
    }
}
