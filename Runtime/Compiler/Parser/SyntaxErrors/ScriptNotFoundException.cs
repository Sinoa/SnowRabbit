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
using System.Runtime.Serialization;

namespace SnowRabbit.Compiler.Parser.SyntaxErrors
{
    /// <summary>
    /// スクリプトが見つけられなかった場合の例外クラスです
    /// </summary>
    [Serializable]
    public class ScriptNotFoundException : SnowRabbitException
    {
        /// <summary>
        /// 見つけられなかったスクリプトの対象パス
        /// </summary>
        public string TargetPath { get; }


        /// <summary>
        /// 例外メッセージ
        /// </summary>
        public override string Message => $"スクリプト '{TargetPath}' が見つかりませんでした。";



        /// <summary>
        /// ScriptNotFoundException クラスのインスタンスを初期化します
        /// </summary>
        public ScriptNotFoundException()
        {
        }


        /// <summary>
        /// ScriptNotFoundException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="path">見つけられなかったスクリプトのパス</param>
        public ScriptNotFoundException(string path) : base(path)
        {
            // 対象パスを覚えておく
            TargetPath = path;
        }


        /// <summary>
        /// ScriptNotFoundException クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="path">見つけられなかったスクリプトのパス</param>
        /// <param name="inner">この例外を発生させる原因となった例外</param>
        public ScriptNotFoundException(string path, Exception inner) : base(path, inner)
        {
            // 対象パスを覚えておく
            TargetPath = path;
        }


        /// <summary>
        /// シリアル化したデータを使用して ScriptNotFoundException クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">スローされている例外に関するシリアル化済みオブジェクトデータを保持している SerializationInfo</param>
        /// <param name="context">転送元または転送先についてのコンテキスト情報を含む StreamingContext</param>
        protected ScriptNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
