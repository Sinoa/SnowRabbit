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
using SnowRabbit.Diagnostics.Logging;

namespace SnowRabbit.Compiler.Parser
{
    /// <summary>
    /// 構文解析器の状態を管理するコンテキストクラスです
    /// </summary>
    public class AnalyzerContext
    {
        // メンバ変数定義
        private readonly Stack<CompileUnitContext> compileUnitContexts = new Stack<CompileUnitContext>();



        /// <summary>
        /// 現在の翻訳単位コンテキスト
        /// </summary>
        public CompileUnitContext CurrentCompileUnitContext => compileUnitContexts.Peek();



        /// <summary>
        /// 新しい翻訳単位コンテキストをプッシュします。翻訳単位コンテキストがプッシュされた場合は現在の翻訳単位コンテキストの参照も変わります。
        /// </summary>
        /// <param name="context">プッシュする翻訳単位コンテキスト</param>
        /// <exception cref="ArgumentNullException">context が null です</exception>
        public void PushCompileUnitContext(CompileUnitContext context)
        {
            // 新しいコンテキストをプッシュする
            SrLogger.Trace(nameof(AnalyzerContext), $"Push new compile unit context.");
            compileUnitContexts.Push(context ?? throw new ArgumentNullException(nameof(context)));
        }


        /// <summary>
        /// 現在スタックに積まれている翻訳単位コンテキストをポップします
        /// </summary>
        /// <returns>ポップされた翻訳単位コンテキストを返しますが、スタックが空の場合は null を返します</returns>
        public CompileUnitContext PopCompileUnitContext()
        {
            // スタックが空なら null を返して、ポップ出来るならそのまま返す
            return compileUnitContexts.Count == 0 ? null : compileUnitContexts.Pop();
        }
    }
}
