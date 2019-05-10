﻿// zlib/libpng License
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
        /// スタックポインタをデクリメントしてから、レジスタ内容をスタックへプッシュします：--SP; [SP] = Ra
        /// </summary>
        Push = 0x14,

        /// <summary>
        /// スタックポインタをデクリメントしてから、即値をスタックへプッシュします：--SP; [SP] = Imm
        /// </summary>
        Pushl = 0x15,

        /// <summary>
        /// スタックからレジスタ内容へポップし、スタックポインタをインクリメントします：Ra = [SP]; ++SP;
        /// </summary>
        Pop = 0x16,
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

        /// <summary>
        /// レジスタ間の等価テストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb == Rc ? 1 : 0
        /// </summary>
        Teq = 0x37,

        /// <summary>
        /// レジスタ間の否等価テストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb != Rc ? 1 : 0
        /// </summary>
        Tne = 0x38,

        /// <summary>
        /// レジスタ間の大なりテストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb > Rc ? 1 : 0
        /// </summary>
        Tg = 0x39,

        /// <summary>
        /// レジスタ間の大なり等価テストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb >= Rc ? 1 : 0
        /// </summary>
        Tge = 0x3A,

        /// <summary>
        /// レジスタ間の小なりテストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb < Rc ? 1 : 0
        /// </summary>
        Tl = 0x3B,

        /// <summary>
        /// レジスタ間の小なり等価テストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb <= Rc ? 1 : 0
        /// </summary>
        Tle = 0x3C,
        #endregion

        #region Flow Control
        /// <summary>
        /// 無条件でレジスタによって指定されたアドレスへ分岐します：IP = Ra
        /// </summary>
        Br = 0x80,

        /// <summary>
        /// 無条件で即値によって指定されたアドレスへ分岐します：IP = Imm
        /// </summary>
        Brl = 0x81,

        /// <summary>
        /// 指定されたレジスタが0でない場合に、レジスタによって指定されたアドレスへ分岐します：IP = Rb != 0 ? Ra : IP
        /// </summary>
        Bnz = 0x82,

        /// <summary>
        /// 指定されたレジスタが0でない場合に、即値によって指定されたアドレスへ分岐します：IP = Rb != 0 ? Imm : IP
        /// </summary>
        Bnzl = 0x83,

        /// <summary>
        /// 次に実行するべき命令のアドレスをプッシュして、レジスタによって指定されたアドレスへ分岐します：PUSH(IP+1); IP = Ra
        /// </summary>
        Call = 0x84,

        /// <summary>
        /// 次に実行するべき命令のアドレスをプッシュして、即値によって指定されたアドレスへ分岐します：PUSH(IP+1); IP = Imm
        /// </summary>
        Calll = 0x85,

        /// <summary>
        /// 指定されたレジスタが0でない場合に、次に実行するべき命令のアドレスをプッシュして、レジスタによって指定されたアドレスへ分岐します：if (Rb != 0) { PUSH(IP+1); IP = Ra; }
        /// </summary>
        Callnz = 0x86,

        /// <summary>
        /// 指定されたレジスタが0でない場合に、次に実行するべき命令のアドレスをプッシュして、即値によって指定されたアドレスへ分岐します：if (Rb != 0) { PUSH(IP+1); IP = Imm; }
        /// </summary>
        Callnzl = 0x87,

        /// <summary>
        /// スタックから値をポップしてポップした値のアドレスへ分岐します：IP = POP();
        /// </summary>
        Ret = 0x89,
        #endregion

        #region CSharp Host Control
        /// <summary>
        /// レジスタが示す周辺機器関数を呼び出します：Peripheral #Ra -> Call #Rb( ArgNum #Rc )
        /// [CallPeripheralFunction]
        /// </summary>
        Cpf = 0xF0,

        /// <summary>
        /// レジスタと即値が示す周辺機器関数を呼び出します：Peripheral #Ra -> Call #Rb( ArgNum Imm )
        /// [CallPeripheralFunction Literal argnum]
        /// </summary>
        Cpfl = 0xF1,

        /// <summary>
        /// レジスタが示す周辺機器名から周辺機器IDを取得します：Ra = GetPeripheralId([Rb])
        /// [GetPeripheralId]
        /// </summary>
        Gpid = 0xF2,

        /// <summary>
        /// 即値が示す周辺機器名から周辺機器IDを取得します：Ra = GetPeripheralId([Imm])
        /// [GetPeripheralId Literal peripheralName]
        /// </summary>
        Gpidl = 0xF3,

        /// <summary>
        /// レジスタが示す周辺機器IDの周辺機器関数名から周辺機器関数IDを取得します：Ra = GetFuncId(Rb, [Rc])
        /// [GetPeripheralFunctionId]
        /// </summary>
        Gpfid = 0xF4,

        /// <summary>
        /// レジスタと即値が示す周辺機器IDの周辺機器関数名から周辺機器関数IDを取得します：Ra = GetFuncId(Rb, [Imm])
        /// [GetPeripheralFunctionId Literal peripheralFunctionName]
        /// </summary>
        Gpfidl = 0xF5,
        #endregion
    }
}