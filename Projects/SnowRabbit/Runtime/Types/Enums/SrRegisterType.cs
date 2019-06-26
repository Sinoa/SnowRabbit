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
    /// 仮想マシンのレジスタがどの型かを示します
    /// </summary>
    public enum SrRegisterType : byte
    {
        /// <summary>
        /// 符号付き64bit整数
        /// </summary>
        Long = 0x00,

        /// <summary>
        /// 符号付き32bit整数
        /// </summary>
        Int = 0x10,

        /// <summary>
        /// 符号付き16bit整数
        /// </summary>
        Short = 0x20,

        /// <summary>
        /// 符号付き8bit整数
        /// </summary>
        Sbyte = 0x30,

        /// <summary>
        /// 符号なし64bit整数
        /// </summary>
        Ulong = 0x40,

        /// <summary>
        /// 符号なし32bit整数
        /// </summary>
        Uint = 0x50,

        /// <summary>
        /// 符号なし16bit整数
        /// </summary>
        Ushort = 0x60,

        /// <summary>
        /// 符号なし8bit整数
        /// </summary>
        Byte = 0x70,

        /// <summary>
        /// 倍精度浮動小数点
        /// </summary>
        Double = 0x80,

        /// <summary>
        /// 単精度浮動小数点
        /// </summary>
        Float = 0x90,

        /// <summary>
        /// Unicode文字
        /// </summary>
        Char = 0xA0,

        /// <summary>
        /// ブーリアン
        /// </summary>
        Bool = 0xB0,

        /// <summary>
        /// 参照型
        /// </summary>
        Object = 0xC0,

        /// <summary>
        /// 予約定義（使ってはいけません）
        /// </summary>
        Reserved1 = 0xD0,

        /// <summary>
        /// 予約定義（使ってはいけません）
        /// </summary>
        Reserved2 = 0xE0,

        /// <summary>
        /// 特殊レジスタへの参照
        /// </summary>
        SpecialRegister = 0xF0,
    }
}