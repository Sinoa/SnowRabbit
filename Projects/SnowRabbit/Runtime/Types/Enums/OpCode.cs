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

/*

===== OpCode Assigned Table =====
0x00  :  Don't used. Zero op code is cpu execute exception.
0x01  :  Halt
0x02  :  
0x03  :  
0x04  :  
0x05  :  
0x06  :  
0x07  :  
0x08  :  
0x09  :  
0x0A  :  
0x0B  :  
0x0C  :  
0x0D  :  
0x0E  :  
0x0F  :  
0x10  :  Mov
0x11  :  Movl
0x12  :  Ldr
0x13  :  Ldrl
0x14  :  Str
0x15  :  Strl
0x16  :  Push
0x17  :  Pushl
0x18  :  Pop
0x19  :  Fmovl
0x1A  :  Fpushl
0x1B  :  Movfti
0x1C  :  Movitf
0x1D  :  
0x1E  :  
0x1F  :  
0x20  :  Add
0x21  :  Addl
0x22  :  Sub
0x23  :  Subl
0x24  :  Mul
0x25  :  Mull
0x26  :  Div
0x27  :  Divl
0x28  :  Mod
0x29  :  Modl
0x2A  :  Pow
0x2B  :  Powl
0x2C  :  
0x2D  :  
0x2E  :  
0x2F  :  
0x30  :  Or
0x31  :  Xor
0x32  :  And
0x33  :  Not
0x34  :  Shl
0x35  :  Shr
0x36  :  Teq
0x37  :  Tne
0x38  :  Tg
0x39  :  Tge
0x3A  :  Tl
0x3B  :  Tle
0x3C  :  
0x3D  :  
0x3E  :  
0x3F  :  
0x40  :  Fadd
0x41  :  Faddl
0x42  :  Fsub
0x43  :  Fsubl
0x44  :  Fmul
0x45  :  Fmull
0x46  :  Fdiv
0x47  :  Fdivl
0x48  :  Fmod
0x49  :  Fmodl
0x4A  :  Fpow
0x4B  :  Fpowl
0x4C  :  
0x4D  :  
0x4E  :  
0x4F  :  
0x50  :  
0x51  :  
0x52  :  
0x53  :  
0x54  :  
0x55  :  
0x56  :  
0x57  :  
0x58  :  
0x59  :  
0x5A  :  
0x5B  :  
0x5C  :  
0x5D  :  
0x5E  :  
0x5F  :  
0x60  :  
0x61  :  
0x62  :  
0x63  :  
0x64  :  
0x65  :  
0x66  :  
0x67  :  
0x68  :  
0x69  :  
0x6A  :  
0x6B  :  
0x6C  :  
0x6D  :  
0x6E  :  
0x6F  :  
0x70  :  
0x71  :  
0x72  :  
0x73  :  
0x74  :  
0x75  :  
0x76  :  
0x77  :  
0x78  :  
0x79  :  
0x7A  :  
0x7B  :  
0x7C  :  
0x7D  :  
0x7E  :  
0x7F  :  
0x80  :  Br
0x81  :  Brl
0x82  :  Bnz
0x83  :  Bnzl
0x84  :  Call
0x85  :  Calll
0x86  :  Callnz
0x87  :  Callnzl
0x88  :  Ret
0x89  :  
0x8A  :  
0x8B  :  
0x8C  :  
0x8D  :  
0x8E  :  
0x8F  :  
0x90  :  
0x91  :  
0x92  :  
0x93  :  
0x94  :  
0x95  :  
0x96  :  
0x97  :  
0x98  :  
0x99  :  
0x9A  :  
0x9B  :  
0x9C  :  
0x9D  :  
0x9E  :  
0x9F  :  
0xA0  :  
0xA1  :  
0xA2  :  
0xA3  :  
0xA4  :  
0xA5  :  
0xA6  :  
0xA7  :  
0xA8  :  
0xA9  :  
0xAA  :  
0xAB  :  
0xAC  :  
0xAD  :  
0xAE  :  
0xAF  :  
0xB0  :  
0xB1  :  
0xB2  :  
0xB3  :  
0xB4  :  
0xB5  :  
0xB6  :  
0xB7  :  
0xB8  :  
0xB9  :  
0xBA  :  
0xBB  :  
0xBC  :  
0xBD  :  
0xBE  :  
0xBF  :  
0xC0  :  
0xC1  :  
0xC2  :  
0xC3  :  
0xC4  :  
0xC5  :  
0xC6  :  
0xC7  :  
0xC8  :  
0xC9  :  
0xCA  :  
0xCB  :  
0xCC  :  
0xCD  :  
0xCE  :  
0xCF  :  
0xD0  :  
0xD1  :  
0xD2  :  
0xD3  :  
0xD4  :  
0xD5  :  
0xD6  :  
0xD7  :  
0xD8  :  
0xD9  :  
0xDA  :  
0xDB  :  
0xDC  :  
0xDD  :  
0xDE  :  
0xDF  :  
0xE0  :  
0xE1  :  
0xE2  :  
0xE3  :  
0xE4  :  
0xE5  :  
0xE6  :  
0xE7  :  
0xE8  :  
0xE9  :  
0xEA  :  
0xEB  :  
0xEC  :  
0xED  :  
0xEE  :  
0xEF  :  
0xF0  :  Cpf
0xF1  :  Cpfl
0xF2  :  Gpid
0xF3  :  Gpidl
0xF4  :  Gpfid
0xF5  :  Gpfidl
0xF6  :  
0xF7  :  
0xF8  :  
0xF9  :  
0xFA  :  
0xFB  :  
0xFC  :  
0xFD  :  
0xFE  :  
0xFF  :  

*/

namespace SnowRabbit.Runtime
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
        Halt = 0x01,
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
        /// メモリからレジスタへロードします：Ra = [Imm]
        /// </summary>
        Ldrl = 0x13,

        /// <summary>
        /// レジスタからメモリへストアします：[Rb+Imm] = Ra
        /// </summary>
        Str = 0x14,

        /// <summary>
        /// レジスタからメモリへストアします：[Imm] = Ra
        /// </summary>
        Strl = 0x15,

        /// <summary>
        /// スタックポインタをデクリメントしてから、レジスタ内容をスタックへプッシュします：--SP; [SP] = Ra
        /// </summary>
        Push = 0x16,

        /// <summary>
        /// スタックポインタをデクリメントしてから、即値をスタックへプッシュします：--SP; [SP] = Imm
        /// </summary>
        Pushl = 0x17,

        /// <summary>
        /// スタックからレジスタ内容へポップし、スタックポインタをインクリメントします：Ra = [SP]; ++SP;
        /// </summary>
        Pop = 0x18,

        /// <summary>
        /// レジスタへ浮動小数点即値をコピーします：Ra = Imm
        /// </summary>
        Fmovl = 0x19,

        /// <summary>
        /// スタックポインタをデクリメントしてから、浮動小数点即値をスタックへプッシュします：--SP; [SP] = Imm
        /// </summary>
        Fpushl = 0x1A,

        /// <summary>
        /// 指定された浮動小数点レジスタから整数レジスタへ変換して転送します：Ra = (long)Rb
        /// </summary>
        Movfti = 0x1B,

        /// <summary>
        /// 指定された整数レジスタから浮動小数点レジスタへ変換して転送します：Ra = (float)Rb
        /// </summary>
        Movitf = 0x1C,
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

        /// <summary>
        /// レジスタ間の浮動小数点加算をします：Ra = Rb + Rc
        /// </summary>
        Fadd = 0x40,

        /// <summary>
        /// レジスタと即値の浮動小数点加算をします：Ra = Rb + Imm
        /// </summary>
        Faddl = 0x41,

        /// <summary>
        /// レジスタ間の浮動小数点減算をします：Ra = Rb - Rc
        /// </summary>
        Fsub = 0x42,

        /// <summary>
        /// レジスタと即値の浮動小数点減算をします：Ra = Rb - Imm
        /// </summary>
        Fsubl = 0x43,

        /// <summary>
        /// レジスタ間の浮動小数点乗算をします：Ra = Rb * Rc
        /// </summary>
        Fmul = 0x44,

        /// <summary>
        /// レジスタと即値の浮動小数点乗算をします：Ra = Rb * Imm
        /// </summary>
        Fmull = 0x45,

        /// <summary>
        /// レジスタ間の浮動小数点除算をします：Ra = Rb / Rc
        /// </summary>
        Fdiv = 0x46,

        /// <summary>
        /// レジスタと即値の浮動小数点除算をします：Ra = Rb / Imm
        /// </summary>
        Fdivl = 0x47,

        /// <summary>
        /// レジスタ間の浮動小数点剰余を求めます：Ra = Rb % Rc
        /// </summary>
        Fmod = 0x48,

        /// <summary>
        /// レジスタと即値の浮動小数点剰余を求めます：Ra = Rb % Imm
        /// </summary>
        Fmodl = 0x49,

        /// <summary>
        /// レジスタ間の累乗を求めます：Ra = Pow(Rb, Rc)
        /// </summary>
        Fpow = 0x4A,

        /// <summary>
        /// レジスタと即値の累乗を求めます：Ra = Pow(Rb, Imm)
        /// </summary>
        Fpowl = 0x4B,
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
        /// レジスタの論理否定を求めます：Ra = ~Ra
        /// </summary>
        Not = 0x33,

        /// <summary>
        /// レジスタ間の左方向ビットシフトをします：Ra = Rb << Rc
        /// </summary>
        Shl = 0x34,

        /// <summary>
        /// レジスタ間の右方向ビットシフトをします：Ra = Rb >> Rc
        /// </summary>
        Shr = 0x35,

        /// <summary>
        /// レジスタ間の等価テストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb == Rc ? 1 : 0
        /// </summary>
        Teq = 0x36,

        /// <summary>
        /// レジスタ間の否等価テストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb != Rc ? 1 : 0
        /// </summary>
        Tne = 0x37,

        /// <summary>
        /// レジスタ間の大なりテストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb > Rc ? 1 : 0
        /// </summary>
        Tg = 0x38,

        /// <summary>
        /// レジスタ間の大なり等価テストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb >= Rc ? 1 : 0
        /// </summary>
        Tge = 0x39,

        /// <summary>
        /// レジスタ間の小なりテストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb < Rc ? 1 : 0
        /// </summary>
        Tl = 0x3A,

        /// <summary>
        /// レジスタ間の小なり等価テストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb <= Rc ? 1 : 0
        /// </summary>
        Tle = 0x3B,
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
        Ret = 0x88,
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