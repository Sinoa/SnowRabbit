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
using SnowRabbit.Compiler.Builder;
using SnowRabbit.Compiler.Lexer;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 1つの構文を表す構文ノード抽象クラスです
    /// </summary>
    public abstract class SyntaxNode
    {
        // メンバ変数定義
        private readonly List<SyntaxNode> children;



        /// <summary>
        /// 現れたトークンのコピーを受け取ります
        /// </summary>
        public Token Token { get; private set; }


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
            Token = default;
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
            Token = token;
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
        /// この構文ノードから生成されるアセンブリコードをコンパイルします
        /// </summary>
        /// <param name="builder">アセンブリコードを書き出す処理をするビルダー</param>
        public virtual void Compile(SrBuilder builder)
        {
        }
    }
}
