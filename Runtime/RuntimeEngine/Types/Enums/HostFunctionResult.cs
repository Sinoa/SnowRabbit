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

namespace SnowRabbit.Runtime
{
    /// <summary>
    /// ホストマシンから仮想マシンへ処理を返す際の結果を返します
    /// </summary>
    public enum HostFunctionResult
    {
        /// <summary>
        /// 仮想マシンの処理がそのまま継続して動作します。
        /// </summary>
        Continue,

        /// <summary>
        /// 仮想マシンの処理を一時停止します。仮想マシンのCPUは、次の実行するべき命令の位置で停止します。
        /// </summary>
        Pause,
    }
}