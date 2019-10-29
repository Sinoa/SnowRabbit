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

# SnowRabbit v1 命令表

| 命令番号 | 命令番号（Hex） | ニーモニック | 説明 |
| --- | --- | --- | --- |
| 0 | 0x00 | DON'T USED!!!!! | Don't used. Zero op code is cpu execute exception. |
| 1 | 0x01 | Halt | CPUを無条件に停止します |
| 2 | 0x02 |  |  |
| 3 | 0x03 |  |  |
| 4 | 0x04 |  |  |
| 5 | 0x05 |  |  |
| 6 | 0x06 |  |  |
| 7 | 0x07 |  |  |
| 8 | 0x08 |  |  |
| 9 | 0x09 |  |  |
| 10 | 0x0A |  |  |
| 11 | 0x0B |  |  |
| 12 | 0x0C |  |  |
| 13 | 0x0D |  |  |
| 14 | 0x0E |  |  |
| 15 | 0x0F |  |  |
| 16 | 0x10 | Mov | レジスタ間のコピーをします：Ra = Rb |
| 17 | 0x11 | Movl | レジスタへ即値をコピーします：Ra = Imm |
| 18 | 0x12 | Ldr | メモリからレジスタへロードします：Ra = \[Rb+Imm\] |
| 19 | 0x13 | Ldrl | メモリからレジスタへロードします：Ra = \[Imm\] |
| 20 | 0x14 | Str | レジスタからメモリへストアします：\[Rb+Imm\] = Ra |
| 21 | 0x15 | Strl | レジスタからメモリへストアします：\[Imm\] = Ra |
| 22 | 0x16 | Push | スタックポインタをデクリメントしてから、レジスタ内容をスタックへプッシュします：--SP; \[SP\] = Ra |
| 23 | 0x17 | Pushl | スタックポインタをデクリメントしてから、即値をスタックへプッシュします：--SP; \[SP\] = Imm |
| 24 | 0x18 | Pop | スタックからレジスタ内容へポップし、スタックポインタをインクリメントします：Ra = \[SP\]; ++SP; |
| 25 | 0x19 | Fmovl | レジスタへ浮動小数点即値をコピーします：Ra = Imm |
| 26 | 0x1A | Fpushl | スタックポインタをデクリメントしてから、浮動小数点即値をスタックへプッシュします：--SP; \[SP\] = Imm |
| 27 | 0x1B | Movfti | 指定された浮動小数点レジスタから整数レジスタへ変換して転送します：Ra = (long)Rb |
| 28 | 0x1C | Movitf | 指定された整数レジスタから浮動小数点レジスタへ変換して転送します：Ra = (float)Rb |
| 29 | 0x1D |  |  |
| 30 | 0x1E |  |  |
| 31 | 0x1F |  |  |
| 32 | 0x20 | Add | レジスタ間の加算をします：Ra = Rb + Rc |
| 33 | 0x21 | Addl | レジスタと即値の加算をします：Ra = Rb + Imm |
| 34 | 0x22 | Sub | レジスタ間の減算をします：Ra = Rb - Rc |
| 35 | 0x23 | Subl | レジスタと即値の減算をします：Ra = Rb - Imm |
| 36 | 0x24 | Mul | レジスタ間の乗算をします：Ra = Rb * Rc |
| 37 | 0x25 | Mull | レジスタと即値の乗算をします：Ra = Rb * Imm |
| 38 | 0x26 | Div | レジスタ間の除算をします：Ra = Rb / Rc |
| 39 | 0x27 | Divl | レジスタと即値の除算をします：Ra = Rb / Imm |
| 40 | 0x28 | Mod | レジスタ間の剰余を求めます：Ra = Rb % Rc |
| 41 | 0x29 | Modl | レジスタと即値の剰余を求めます：Ra = Rb % Imm |
| 42 | 0x2A | Pow | レジスタ間の累乗を求めます：Ra = Pow(Rb, Rc) |
| 43 | 0x2B | Powl | レジスタと即値の累乗を求めます：Ra = Pow(Rb, Imm) |
| 44 | 0x2C | Neg | レジスタの値を符号反転します：Ra = -Rb |
| 45 | 0x2D | Negl | 即値を符号反転します：Ra = -Imm |
| 46 | 0x2E |  |  |
| 47 | 0x2F |  |  |
| 48 | 0x30 | Or | レジスタ間の論理和を求めます：Ra = Rb | Rc |
| 49 | 0x31 | Xor | レジスタ間の排他的論理和を求めます：Ra = Rb ^ Rc |
| 50 | 0x32 | And | レジスタ間の論理積を求めます：Ra = Rb & Rc |
| 51 | 0x33 | Not | レジスタの論理否定を求めます：Ra = ~Ra |
| 52 | 0x34 | Shl | レジスタ間の左方向ビットシフトをします：Ra = Rb << Rc |
| 53 | 0x35 | Shr | レジスタ間の右方向ビットシフトをします：Ra = Rb >> Rc |
| 54 | 0x36 | Teq | レジスタ間の等価テストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb == Rc ? 1 : 0 |
| 55 | 0x37 | Tne | レジスタ間の否等価テストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb != Rc ? 1 : 0 |
| 56 | 0x38 | Tg | レジスタ間の大なりテストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb > Rc ? 1 : 0 |
| 57 | 0x39 | Tge | レジスタ間の大なり等価テストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb >= Rc ? 1 : 0 |
| 58 | 0x3A | Tl | レジスタ間の小なりテストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb < Rc ? 1 : 0 |
| 59 | 0x3B | Tle | レジスタ間の小なり等価テストを行い、真であれば1を偽であれば0をレジスタに設定します：Ra = Rb <= Rc ? 1 : 0 |
| 60 | 0x3C |  |  |
| 61 | 0x3D |  |  |
| 62 | 0x3E |  |  |
| 63 | 0x3F |  |  |
| 64 | 0x40 | Fadd | レジスタ間の浮動小数点加算をします：Ra = Rb + Rc |
| 65 | 0x41 | Faddl | レジスタと即値の浮動小数点加算をします：Ra = Rb + Imm |
| 66 | 0x42 | Fsub | レジスタ間の浮動小数点減算をします：Ra = Rb - Rc |
| 67 | 0x43 | Fsubl | レジスタと即値の浮動小数点減算をします：Ra = Rb - Imm |
| 68 | 0x44 | Fmul | レジスタ間の浮動小数点乗算をします：Ra = Rb * Rc |
| 69 | 0x45 | Fmull | レジスタと即値の浮動小数点乗算をします：Ra = Rb * Imm |
| 70 | 0x46 | Fdiv | レジスタ間の浮動小数点除算をします：Ra = Rb / Rc |
| 71 | 0x47 | Fdivl | レジスタと即値の浮動小数点除算をします：Ra = Rb / Imm |
| 72 | 0x48 | Fmod | レジスタ間の浮動小数点剰余を求めます：Ra = Rb % Rc |
| 73 | 0x49 | Fmodl | レジスタと即値の浮動小数点剰余を求めます：Ra = Rb % Imm |
| 74 | 0x4A | Fpow | レジスタ間の累乗を求めます：Ra = Pow(Rb, Rc) |
| 75 | 0x4B | Fpowl | レジスタと即値の累乗を求めます：Ra = Pow(Rb, Imm) |
| 76 | 0x4C | Fneg | レジスタの値を符号反転します：Ra = -Rb |
| 77 | 0x4D | Fnegl | 即値を符号反転します：Ra = -Rb |
| 78 | 0x4E |  |  |
| 79 | 0x4F |  |  |
| 80 | 0x50 |  |  |
| 81 | 0x51 |  |  |
| 82 | 0x52 |  |  |
| 83 | 0x53 |  |  |
| 84 | 0x54 |  |  |
| 85 | 0x55 |  |  |
| 86 | 0x56 |  |  |
| 87 | 0x57 |  |  |
| 88 | 0x58 |  |  |
| 89 | 0x59 |  |  |
| 90 | 0x5A |  |  |
| 91 | 0x5B |  |  |
| 92 | 0x5C |  |  |
| 93 | 0x5D |  |  |
| 94 | 0x5E |  |  |
| 95 | 0x5F |  |  |
| 96 | 0x60 |  |  |
| 97 | 0x61 |  |  |
| 98 | 0x62 |  |  |
| 99 | 0x63 |  |  |
| 100 | 0x64 |  |  |
| 101 | 0x65 |  |  |
| 102 | 0x66 |  |  |
| 103 | 0x67 |  |  |
| 104 | 0x68 |  |  |
| 105 | 0x69 |  |  |
| 106 | 0x6A |  |  |
| 107 | 0x6B |  |  |
| 108 | 0x6C |  |  |
| 109 | 0x6D |  |  |
| 110 | 0x6E |  |  |
| 111 | 0x6F |  |  |
| 112 | 0x70 |  |  |
| 113 | 0x71 |  |  |
| 114 | 0x72 |  |  |
| 115 | 0x73 |  |  |
| 116 | 0x74 |  |  |
| 117 | 0x75 |  |  |
| 118 | 0x76 |  |  |
| 119 | 0x77 |  |  |
| 120 | 0x78 |  |  |
| 121 | 0x79 |  |  |
| 122 | 0x7A |  |  |
| 123 | 0x7B |  |  |
| 124 | 0x7C |  |  |
| 125 | 0x7D |  |  |
| 126 | 0x7E |  |  |
| 127 | 0x7F |  |  |
| 128 | 0x80 | Br | 無条件でレジスタと即値によって指定されたアドレスへ分岐します：IP = Ra + Imm |
| 129 | 0x81 | Brl | 無条件で即値によって指定されたアドレスへ分岐します：IP = Imm |
| 130 | 0x82 | Bnz | 指定されたレジスタが0でない場合に、レジスタと即値によって指定されたアドレスへ分岐します：IP = Rb != 0 ? Ra + Imm : IP |
| 131 | 0x83 | Bnzl | 指定されたレジスタが0でない場合に、即値によって指定されたアドレスへ分岐します：IP = Rb != 0 ? Imm : IP |
| 132 | 0x84 | Call | 次に実行するべき命令のアドレスをプッシュして、レジスタと即値によって指定されたアドレスへ分岐します：PUSH(IP+1); IP = Ra + Imm |
| 133 | 0x85 | Calll | 次に実行するべき命令のアドレスをプッシュして、即値によって指定されたアドレスへ分岐します：PUSH(IP+1); IP = Imm |
| 134 | 0x86 | Callnz | 指定されたレジスタが0でない場合に、次に実行するべき命令のアドレスをプッシュして、レジスタと即値によって指定されたアドレスへ分岐します：if (Rb != 0) { PUSH(IP+1); IP = Ra + Imm; } |
| 135 | 0x87 | Callnzl | 指定されたレジスタが0でない場合に、次に実行するべき命令のアドレスをプッシュして、即値によって指定されたアドレスへ分岐します：if (Rb != 0) { PUSH(IP+1); IP = Imm; } |
| 136 | 0x88 | Ret | スタックから値をポップしてポップした値のアドレスへ分岐します：IP = POP(); |
| 137 | 0x89 |  |  |
| 138 | 0x8A |  |  |
| 139 | 0x8B |  |  |
| 140 | 0x8C |  |  |
| 141 | 0x8D |  |  |
| 142 | 0x8E |  |  |
| 143 | 0x8F |  |  |
| 144 | 0x90 |  |  |
| 145 | 0x91 |  |  |
| 146 | 0x92 |  |  |
| 147 | 0x93 |  |  |
| 148 | 0x94 |  |  |
| 149 | 0x95 |  |  |
| 150 | 0x96 |  |  |
| 151 | 0x97 |  |  |
| 152 | 0x98 |  |  |
| 153 | 0x99 |  |  |
| 154 | 0x9A |  |  |
| 155 | 0x9B |  |  |
| 156 | 0x9C |  |  |
| 157 | 0x9D |  |  |
| 158 | 0x9E |  |  |
| 159 | 0x9F |  |  |
| 160 | 0xA0 |  |  |
| 161 | 0xA1 |  |  |
| 162 | 0xA2 |  |  |
| 163 | 0xA3 |  |  |
| 164 | 0xA4 |  |  |
| 165 | 0xA5 |  |  |
| 166 | 0xA6 |  |  |
| 167 | 0xA7 |  |  |
| 168 | 0xA8 |  |  |
| 169 | 0xA9 |  |  |
| 170 | 0xAA |  |  |
| 171 | 0xAB |  |  |
| 172 | 0xAC |  |  |
| 173 | 0xAD |  |  |
| 174 | 0xAE |  |  |
| 175 | 0xAF |  |  |
| 176 | 0xB0 |  |  |
| 177 | 0xB1 |  |  |
| 178 | 0xB2 |  |  |
| 179 | 0xB3 |  |  |
| 180 | 0xB4 |  |  |
| 181 | 0xB5 |  |  |
| 182 | 0xB6 |  |  |
| 183 | 0xB7 |  |  |
| 184 | 0xB8 |  |  |
| 185 | 0xB9 |  |  |
| 186 | 0xBA |  |  |
| 187 | 0xBB |  |  |
| 188 | 0xBC |  |  |
| 189 | 0xBD |  |  |
| 190 | 0xBE |  |  |
| 191 | 0xBF |  |  |
| 192 | 0xC0 |  |  |
| 193 | 0xC1 |  |  |
| 194 | 0xC2 |  |  |
| 195 | 0xC3 |  |  |
| 196 | 0xC4 |  |  |
| 197 | 0xC5 |  |  |
| 198 | 0xC6 |  |  |
| 199 | 0xC7 |  |  |
| 200 | 0xC8 |  |  |
| 201 | 0xC9 |  |  |
| 202 | 0xCA |  |  |
| 203 | 0xCB |  |  |
| 204 | 0xCC |  |  |
| 205 | 0xCD |  |  |
| 206 | 0xCE |  |  |
| 207 | 0xCF |  |  |
| 208 | 0xD0 |  |  |
| 209 | 0xD1 |  |  |
| 210 | 0xD2 |  |  |
| 211 | 0xD3 |  |  |
| 212 | 0xD4 |  |  |
| 213 | 0xD5 |  |  |
| 214 | 0xD6 |  |  |
| 215 | 0xD7 |  |  |
| 216 | 0xD8 |  |  |
| 217 | 0xD9 |  |  |
| 218 | 0xDA |  |  |
| 219 | 0xDB |  |  |
| 220 | 0xDC |  |  |
| 221 | 0xDD |  |  |
| 222 | 0xDE |  |  |
| 223 | 0xDF |  |  |
| 224 | 0xE0 |  |  |
| 225 | 0xE1 |  |  |
| 226 | 0xE2 |  |  |
| 227 | 0xE3 |  |  |
| 228 | 0xE4 |  |  |
| 229 | 0xE5 |  |  |
| 230 | 0xE6 |  |  |
| 231 | 0xE7 |  |  |
| 232 | 0xE8 |  |  |
| 233 | 0xE9 |  |  |
| 234 | 0xEA |  |  |
| 235 | 0xEB |  |  |
| 236 | 0xEC |  |  |
| 237 | 0xED |  |  |
| 238 | 0xEE |  |  |
| 239 | 0xEF |  |  |
| 240 | 0xF0 | Cpf | レジスタが示す周辺機器関数を呼び出します：Peripheral #Ra -> Call #Rb( ArgNum #Rc ) |
| 241 | 0xF1 | Cpfl | レジスタと即値が示す周辺機器関数を呼び出します：Peripheral #Ra -> Call #Rb( ArgNum Imm ) |
| 242 | 0xF2 | Gpid | レジスタが示す周辺機器名から周辺機器IDを取得します：Ra = GetPeripheralId(\[Rb\]) |
| 243 | 0xF3 | Gpidl | 即値が示す周辺機器名から周辺機器IDを取得します：Ra = GetPeripheralId(\[Imm\]) |
| 244 | 0xF4 | Gpfid | レジスタが示す周辺機器IDの周辺機器関数名から周辺機器関数IDを取得します：Ra = GetFuncId(Rb, \[Rc\]) |
| 245 | 0xF5 | Gpfidl | レジスタと即値が示す周辺機器IDの周辺機器関数名から周辺機器関数IDを取得します：Ra = GetFuncId(Rb, \[Imm\]) |
| 246 | 0xF6 |  |  |
| 247 | 0xF7 |  |  |
| 248 | 0xF8 |  |  |
| 249 | 0xF9 |  |  |
| 250 | 0xFA |  |  |
| 251 | 0xFB |  |  |
| 252 | 0xFC |  |  |
| 253 | 0xFD |  |  |
| 254 | 0xFE |  |  |
| 255 | 0xFF |  |  |

*/

namespace SnowRabbit.RuntimeEngine
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
        /// レジスタの値を符号反転します：Ra = -Rb
        /// </summary>
        Neg = 0x2C,

        /// <summary>
        /// 即値を符号反転します：Ra = -Imm
        /// </summary>
        Negl = 0x2D,

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

        /// <summary>
        /// レジスタの値を符号反転します：Ra = -Rb
        /// </summary>
        Fneg = 0x4C,

        /// <summary>
        /// 即値を符号反転します：Ra = -Rb
        /// </summary>
        Fnegl = 0x4D,
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
        /// 無条件でレジスタと即値によって指定されたアドレスへ分岐します：IP = Ra + Imm
        /// </summary>
        Br = 0x80,

        /// <summary>
        /// 無条件で即値によって指定されたアドレスへ分岐します：IP = Imm
        /// </summary>
        Brl = 0x81,

        /// <summary>
        /// 指定されたレジスタが0でない場合に、レジスタと即値によって指定されたアドレスへ分岐します：IP = Rb != 0 ? Ra + Imm : IP
        /// </summary>
        Bnz = 0x82,

        /// <summary>
        /// 指定されたレジスタが0でない場合に、即値によって指定されたアドレスへ分岐します：IP = Rb != 0 ? Imm : IP
        /// </summary>
        Bnzl = 0x83,

        /// <summary>
        /// 次に実行するべき命令のアドレスをプッシュして、レジスタと即値によって指定されたアドレスへ分岐します：PUSH(IP+1); IP = Ra + Imm
        /// </summary>
        Call = 0x84,

        /// <summary>
        /// 次に実行するべき命令のアドレスをプッシュして、即値によって指定されたアドレスへ分岐します：PUSH(IP+1); IP = Imm
        /// </summary>
        Calll = 0x85,

        /// <summary>
        /// 指定されたレジスタが0でない場合に、次に実行するべき命令のアドレスをプッシュして、レジスタと即値によって指定されたアドレスへ分岐します：if (Rb != 0) { PUSH(IP+1); IP = Ra + Imm; }
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