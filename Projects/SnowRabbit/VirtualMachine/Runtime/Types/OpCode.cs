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

namespace SnowRabbit.VirtualMachine.Runtime
{
    /// <summary>
    /// 仮想マシンが理解するオペレーションコードです
    /// </summary>
    public enum OpCode : byte
    {
        #region CPU Control
        /// <summary>
        /// CPUを無条件に停止します
        /// </summary>
        Halt = 0x00,
        #endregion

        #region Data Transfer
        /// <summary>
        /// レジスタ間のコピーをします：Ra = Rb
        /// </summary>
        Mov = 0x10,

        /// <summary>
        /// レジスタへ即値をコピーします：Ra = Imm
        /// </summary>
        Movl = 0x11,

        /// <summary>
        /// メモリからレジスタへロードします：Ra = [Rb+Imm]
        /// </summary>
        Ldr = 0x12,

        /// <summary>
        /// レジスタからメモリへストアします：[Rb+Imm] = Ra
        /// </summary>
        Str = 0x13,

        /// <summary>
        /// スタックポインタをデクリメントしてから、レジスタ内容をスタックへプッシュします：--SP; Stack = Ra
        /// </summary>
        Push = 0x14,

        /// <summary>
        /// スタックからレジスタ内容へポップし、スタックポインタをインクリメントします：Ra = Stack; ++SP;
        /// </summary>
        Pop = 0x15,
        #endregion

        #region Arithmetic
        /// <summary>
        /// レジスタ間の加算をします：Ra = Rb + Rc
        /// </summary>
        Add = 0x20,

        /// <summary>
        /// レジスタと即値の加算をします：Ra = Rb + Imm
        /// </summary>
        Addl = 0x21,

        /// <summary>
        /// レジスタ間の減算をします：Ra = Rb - Rc
        /// </summary>
        Sub = 0x22,

        /// <summary>
        /// レジスタと即値の減算をします：Ra = Rb - Imm
        /// </summary>
        Subl = 0x23,

        /// <summary>
        /// レジスタ間の乗算をします：Ra = Rb * Rc
        /// </summary>
        Mul = 0x24,

        /// <summary>
        /// レジスタと即値の乗算をします：Ra = Rb * Imm
        /// </summary>
        Mull = 0x25,

        /// <summary>
        /// レジスタ間の除算をします：Ra = Rb / Rc
        /// </summary>
        Div = 0x26,

        /// <summary>
        /// レジスタと即値の除算をします：Ra = Rb / Imm
        /// </summary>
        Divl = 0x27,

        /// <summary>
        /// レジスタ間の剰余を求めます：Ra = Rb % Rc
        /// </summary>
        Mod = 0x28,

        /// <summary>
        /// レジスタと即値の剰余を求めます：Ra = Rb % Imm
        /// </summary>
        Modl = 0x29,

        /// <summary>
        /// レジスタ間の累乗を求めます：Ra = Pow(Rb, Rc)
        /// </summary>
        Pow = 0x2A,

        /// <summary>
        /// レジスタと即値の累乗を求めます：Ra = Pow(Rb, Imm)
        /// </summary>
        Powl = 0x2B,
        #endregion

        #region Logic
        /// <summary>
        /// レジスタ間の論理和を求めます：Ra = Rb | Rc
        /// </summary>
        Or = 0x30,

        /// <summary>
        /// レジスタ間の排他的論理和を求めます：Ra = Rb ^ Rc
        /// </summary>
        Xor = 0x31,

        /// <summary>
        /// レジスタ間の論理積を求めます：Ra = Rb & Rc
        /// </summary>
        And = 0x32,

        /// <summary>
        /// レジスタのビット反転を求めます：Ra = ~Ra
        /// </summary>
        Not = 0x33,

        /// <summary>
        /// レジスタの論理否定を求めます：Ra = !Ra
        /// </summary>
        Lnot = 0x34,

        /// <summary>
        /// レジスタ間の左方向ビットシフトをします：Ra = Rb << Rc
        /// </summary>
        Shl = 0x35,

        /// <summary>
        /// レジスタ間の右方向ビットシフトをします：Ra = Rb >> Rc
        /// </summary>
        Shr = 0x36,
        #endregion

        #region Flow Control
        /// <summary>
        /// 無条件で指定されたアドレスへ分岐します：PC = Ra
        /// </summary>
        Br = 0x40,

        /// <summary>
        /// 現在の命令ポインタの次の命令アドレスをリンクレジスタへ設定してから、無条件で指定されたアドレスへ分岐します：LR = PC + 1; PC = Ra
        /// </summary>
        Blr = 0x41,

        /// <summary>
        /// リンクレジスタに設定されているアドレスへ分岐します：PC = LR
        /// </summary>
        Ret = 0x42,

        /// <summary>
        /// ステータスレジスタのZフラグがOffの場合指に、現在の命令ポインタの次の命令アドレスをリンクレジスタへ設定してから、指定されたアドレスへ分岐します：PC = Z == 0 ? Ra : PC
        /// </summary>
        Bne = 0x43,

        /// <summary>
        /// ステータスレジスタのZフラグがOnまたはPフラグがOnの場合に、現在の命令ポインタの次の命令アドレスをリンクレジスタへ設定してから、指定されたアドレスへ分岐します：PC = (Z == 1 || P == 1) ? Ra : PC
        /// </summary>
        Bge = 0x44,

        /// <summary>
        /// ステータスレジスタのZフラグがOnまたはNフラグがOnの場合に、現在の命令ポインタの次の命令アドレスをリンクレジスタへ設定してから、指定されたアドレスへ分岐します：PC = (Z == 1 || N == 1) ? Ra : PC
        /// </summary>
        Bls = 0x45,

        /// <summary>
        /// ステータスレジスタのPフラグがOnの場合に、現在の命令ポインタの次の命令アドレスをリンクレジスタへ設定してから、指定されたアドレスへ分岐します：PC = P == 1 ? Ra : PC
        /// </summary>
        Bgt = 0x46,

        /// <summary>
        /// ステータスレジスタのNフラグがOnの場合に、現在の命令ポインタの次の命令アドレスをリンクレジスタへ設定してから、指定されたアドレスへ分岐します：PC = N == 1 ? Ra : PC
        /// </summary>
        Blt = 0x47,

        /// <summary>
        /// カウンタレジスタが1以上の場合に、カウンタレジスタをデクリメントしてから、指定されたアドレスへ分岐します：PC = CReg >= 1 ? {--CReg; Ra;} : PC
        /// </summary>
        Blp = 0x48,
        #endregion

        #region CSharp Host Control
        /// <summary>
        /// レジスタが示す周辺機器関数を呼び出します：Peripheral #Ra -> Call #Rb( ArgNum #Rc )
        /// [CallPeripheralFunction]
        /// </summary>
        Cpf = 0x50,

        /// <summary>
        /// レジスタと即値が示す周辺機器関数を呼び出します：Peripheral #Ra -> Call #Rb( ArgNum Imm )
        /// [CallPeripheralFunction Literal argnum]
        /// </summary>
        Cpfl = 0x51,

        /// <summary>
        /// レジスタが示す周辺機器名から周辺機器IDを取得します：Ra = GetPeripheralId([Rb])
        /// [GetPeripheralId]
        /// </summary>
        Gpid = 0x52,

        /// <summary>
        /// 即値が示す周辺機器名から周辺機器IDを取得します：Ra = GetPeripheralId([Imm])
        /// [GetPeripheralId Literal peripheralName]
        /// </summary>
        Gpidl = 0x53,

        /// <summary>
        /// レジスタが示す周辺機器IDの周辺機器関数名から周辺機器関数IDを取得します：Ra = GetFuncId(Rb, [Rc])
        /// [GetPeripheralFunctionId]
        /// </summary>
        Gpfid = 0x54,

        /// <summary>
        /// レジスタと即値が示す周辺機器IDの周辺機器関数名から周辺機器関数IDを取得します：Ra = GetFuncId(Rb, [Imm])
        /// [GetPeripheralFunctionId Literal peripheralFunctionName]
        /// </summary>
        Gpfidl = 0x55,
        #endregion
    }
}