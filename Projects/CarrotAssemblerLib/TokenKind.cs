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

namespace CarrotAssemblerLib.Common
{
    /// <summary>
    /// トークンの種別を表現します
    /// </summary>
    public enum TokenKind
    {
        /// <summary>
        /// 不明なトークン(通常は無効値として扱われ無効な識別子としても扱います)
        /// invalid token kind and invalid identifier.
        /// </summary>
        Unknown,

        /// <summary>
        /// 識別子
        /// identifier or hogemoge
        /// </summary>
        Identifier,

        /// <summary>
        /// 整数
        /// 1234
        /// </summary>
        Integer,

        /// <summary>
        /// 文字列
        /// "a" or "abc" or 'a' or 'abc'
        /// </summary>
        String,

        /// <summary>
        /// コロン
        /// :
        /// </summary>
        Coron,

        /// <summary>
        /// シャープ
        /// #
        /// </summary>
        Sharp,

        /// <summary>
        /// これ以上のトークンは存在しないトークン
        /// </summary>
        EndOfToken,
    }
}