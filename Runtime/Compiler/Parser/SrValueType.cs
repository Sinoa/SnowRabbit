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

namespace SnowRabbit.Compiler.Parser
{
    /// <summary>
    /// SnowRabbit が扱うデータの型を表現します
    /// </summary>
    public enum SrValueType
    {
        /// <summary>
        /// ボイド型
        /// </summary>
        Void,

        /// <summary>
        /// 整数型
        /// </summary>
        Integer,

        /// <summary>
        /// 実数型
        /// </summary>
        Number,

        /// <summary>
        /// 文字列型
        /// </summary>
        String,

        /// <summary>
        /// ブーリアン型
        /// </summary>
        Boolean,

        /// <summary>
        /// オブジェクト型
        /// </summary>
        Object,
    }
}
