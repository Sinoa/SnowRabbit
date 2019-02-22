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

namespace SnowRabbit.VirtualMachine
{
    /// <summary>
    /// 仮想マシンが理解するオペレーションコードです
    /// </summary>
    public enum OpCode : byte
    {
        #region Data Transfer
        /// <summary>
        /// レジスタ間または即値のコピーをします：Ra = Rb, Ra = Imm
        /// </summary>
        Mov = 0x10,

        /// <summary>
        /// メモリからレジスタへロードします：Ra = [Rb]
        /// </summary>
        Ldr = 0x11,

        /// <summary>
        /// レジスタからメモリへストアします：[Ra] = Rb
        /// </summary>
        Str = 0x12,
        #endregion

        #region Arithmetic
        /// <summary>
        /// レジスタ間または即値の加算をします：Ra = Rb + Rc, Ra = Rb + Imm
        /// </summary>
        Add = 0x20,

        /// <summary>
        /// レジスタ間または即値の減算をします：Ra = Rb - Rc, Ra = Rb - Imm
        /// </summary>
        Sub = 0x21,

        /// <summary>
        /// レジスタ間または即値の乗算をします：Ra = Rb * Rc, Ra = Rb * Imm
        /// </summary>
        Mul = 0x22,

        /// <summary>
        /// レジスタ間または即値の除算をします：Ra = Rb / Rc, Ra = Rb / Imm
        /// </summary>
        Div = 0x23,

        /// <summary>
        /// レジスタ間または即値の剰余を求めます：Ra = Rb % Rc, Ra = Rb % Imm
        /// </summary>
        Mod = 0x24,

        /// <summary>
        /// レジスタ間または即値の累乗を求めます：Ra = Pow(Rb, Rc), Ra = Pow(Rb, Imm)
        /// </summary>
        Pow = 0x25,
        #endregion

        #region Logic
        /// <summary>
        /// レジスタ間または即値の論理和を求めます：Ra = Rb | Rc, Ra = Rb | Imm
        /// </summary>
        Or = 0x30,

        /// <summary>
        /// レジスタ間または即値の排他的論理和を求めます：Ra = Rb ^ Rc, Ra = Rb | Imm
        /// </summary>
        Xor = 0x31,

        /// <summary>
        /// レジスタ間または即値の論理積を求めます：Ra = Rb & Rc, Ra = Rb & Imm
        /// </summary>
        And = 0x32,

        /// <summary>
        /// レジスタまたは即値の論理否定を求めます：Ra = !Ra, Ra = !Imm
        /// </summary>
        Not = 0x33,

        /// <summary>
        /// レジスタ間または即値の左方向ビットシフトをします：Ra = Rb << Rc, Ra = Rb << Imm
        /// </summary>
        Shl = 0x34,

        /// <summary>
        /// レジスタ間または即値の右方向ビットシフトをします：Ra = Rb >> Rc, Ra = Rb >> Imm
        /// </summary>
        Shr = 0x35,
        #endregion

        #region Flow Control
        /// <summary>
        /// 無条件で指定されたアドレスへ分岐します：PC = Ra, PC = Imm
        /// </summary>
        B = 0x40,

        /// <summary>
        /// ステータスレジスタのZフラグがOffの場合指に指定されたアドレスへ分岐します：PC = Z == 0 ? Ra : PC, PC = Z == 0 ? Imm : PC
        /// </summary>
        Bne = 0x41,

        /// <summary>
        /// ステータスレジスタのZフラグがOnまたはPフラグがOnの場合に指定されたアドレスへ分岐します：PC = (Z == 1 || P == 1) ? Ra : PC, PC = (Z == 1 || P == 1) ? Imm : PC
        /// </summary>
        Bge = 0x42,

        /// <summary>
        /// ステータスレジスタのZフラグがOnまたはNフラグがOnの場合に指定されたアドレスへ分岐します：PC = (Z == 1 || N == 1) ? Ra : PC, PC = (Z == 1 || N == 1) ? Imm : PC
        /// </summary>
        Ble = 0x43,

        /// <summary>
        /// ステータスレジスタのPフラグがOnの場合に指定されたアドレスへ分岐します：PC = P == 1 ? Ra : PC, PC = P == 1 ? Imm : PC
        /// </summary>
        Bg = 0x44,

        /// <summary>
        /// ステータスレジスタのNフラグがOnの場合に指定されたアドレスへ分岐します：PC = N == 1 ? Ra : PC, PC = N == 1 ? Imm : PC
        /// </summary>
        Bl = 0x45,

        /// <summary>
        /// 現在のプログラムカウンタの次の命令アドレスをプッシュして指定されたアドレスへ分岐します：Push PC; PC = Ra, Push PC; PC = Imm
        /// </summary>
        Call = 0x46,

        /// <summary>
        /// 現在のスタックトップからポップしたアドレスへ分岐します：Pop Ra; PC = Ra
        /// </summary>
        Ret = 0x47,
        #endregion

        #region CSharp Host Control
        /// <summary>
        /// 指定されたレジスタが示すホスト関数アドレスを呼び出します
        /// </summary>
        HostCall = 0x50,
        #endregion
    }
}