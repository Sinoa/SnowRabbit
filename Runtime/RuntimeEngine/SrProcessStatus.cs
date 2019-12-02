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

namespace SnowRabbit.RuntimeEngine
{
    /// <summary>
    /// SrProcess のプロセスステータスを列挙します
    /// </summary>
    [Flags]
    public enum SrProcessStatus : byte
    {
        /// <summary>
        /// プロセスは動作中です
        /// </summary>
        Running = 0x00,

        /// <summary>
        /// プロセスは一時停止中です
        /// </summary>
        Suspended = 0x01,

        /// <summary>
        /// プロセスの動作準備が出来ました
        /// </summary>
        Ready = 0x02,

        /// <summary>
        /// プロセスは停止しました
        /// </summary>
        Stopped = 0x04,

        /// <summary>
        /// プロセス実行中に不明なエラーが発生しました
        /// </summary>
        Panic = 0x10,
    }
}
