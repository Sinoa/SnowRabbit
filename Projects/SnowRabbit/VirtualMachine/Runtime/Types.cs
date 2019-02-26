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

using System.Runtime.InteropServices;

namespace SnowRabbit.VirtualMachine.Runtime
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



    /// <summary>
    /// 仮想マシンの変数型を扱う型でメモリレイアウトを定義する共用体です
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct SrTypeValue
    {
        #region Type of Number
        /// <summary>
        /// 1つの64bit浮動小数点
        /// </summary>
        [FieldOffset(0)]
        public fixed double Double[1];

        /// <summary>
        /// 2つの32bit浮動小数点
        /// </summary>
        [FieldOffset(0)]
        public fixed float Float[2];
        #endregion


        #region Type of Signed Integer
        /// <summary>
        /// 1つの符号付き64bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed long Long[1];

        /// <summary>
        /// 2つの符号付き32bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed int Int[2];

        /// <summary>
        /// 4つの符号付き16bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed short Short[4];

        /// <summary>
        /// 8つの符号付き8bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed sbyte Sbyte[8];
        #endregion


        #region Type of Unsigned Integer
        /// <summary>
        /// 1つの符号なし64bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed ulong Ulong[1];

        /// <summary>
        /// 2つの符号なし32bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed uint Uint[2];

        /// <summary>
        /// 4つの符号なし16bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed ushort Ushort[4];

        /// <summary>
        /// 8つの符号なし8bit整数
        /// </summary>
        [FieldOffset(0)]
        public fixed byte Byte[8];
        #endregion


        #region Type of General
        /// <summary>
        /// 4つのUnicode文字
        /// </summary>
        [FieldOffset(0)]
        public fixed char Char[4];

        /// <summary>
        /// 8つのブール
        /// </summary>
        [FieldOffset(0)]
        public fixed bool Bool[8];
        #endregion
    }



    /// <summary>
    /// 命令コード型のImmediate部の表現をした共用体です
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct InstructionImmediateValue
    {
        #region Immediate types
        /// <summary>
        /// 命令コードが使用するバイト配列即値です
        /// </summary>
        [FieldOffset(0)]
        public fixed byte ByteCode[4];

        /// <summary>
        /// 命令コードが使用する符号なし8bit整数即値です
        /// </summary>
        [FieldOffset(0)]
        public byte Byte;

        /// <summary>
        /// 命令コードが使用する符号付き8bit整数即値です
        /// </summary>
        [FieldOffset(0)]
        public sbyte Sbyte;

        /// <summary>
        /// 命令コードが使用する符号なし16bit整数即値です
        /// </summary>
        [FieldOffset(0)]
        public ushort Ushort;

        /// <summary>
        /// 命令コードが使用する符号付き16bit整数即値です
        /// </summary>
        [FieldOffset(0)]
        public short Short;

        /// <summary>
        /// 命令コードが使用する符号なし32bit整数即値です
        /// </summary>
        [FieldOffset(0)]
        public uint Uint;

        /// <summary>
        /// 命令コードが使用する符号付き32bit整数即値です
        /// </summary>
        [FieldOffset(0)]
        public int Int;

        /// <summary>
        /// 命令コードが使用する32bit浮動小数点即値です
        /// </summary>
        [FieldOffset(0)]
        public float Float;
        #endregion
    }



    /// <summary>
    /// 仮想マシンが実行できる命令コードを表現した共用体です
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct InstructionCode
    {
        #region Whole byte array
        /// <summary>
        /// 命令コード全体を表すバイト配列です
        /// </summary>
        [FieldOffset(0)]
        public fixed byte ByteCode[8];
        #endregion


        #region OpCode and register
        /// <summary>
        /// 実行する命令コード
        /// </summary>
        [FieldOffset(0)]
        public OpCode OpCode;

        /// <summary>
        /// 命令コードが使用するレジスタの位置を示す引数Aです
        /// </summary>
        [FieldOffset(1)]
        public byte Ra;

        /// <summary>
        /// 命令コードが使用するレジスタの位置を示す引数Bです
        /// </summary>
        [FieldOffset(2)]
        public byte Rb;

        /// <summary>
        /// 命令コードが使用するレジスタの位置を示す引数Cです
        /// </summary>
        [FieldOffset(3)]
        public byte Rc;
        #endregion


        #region Immediate region
        /// <summary>
        /// 命令コードが使用する即値です
        /// </summary>
        [FieldOffset(4)]
        public InstructionImmediateValue Immediate;
        #endregion
    }



    /// <summary>
    /// 確保したメモリブロックの情報を表現した構造体です
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryAllocationInfo
    {
        #region Info parameter
        /// <summary>
        /// このメモリブロックの確保済みブロック数（最上位ビットが1の場合は未使用ブロックとして処理されます）
        /// </summary>
        public uint AllocatedBlockCount;

        /// <summary>
        /// 1つ前の確保済みメモリブロック数
        /// </summary>
        public uint PrevAllocatedBlockCount;
        #endregion
    }



    /// <summary>
    /// 仮想マシンが扱うメモリレイアウトを定義する共用体です
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct SrValue
    {
        #region 8 byte Layouts
        /// <summary>
        /// 仮想マシンが扱う型毎の値です
        /// </summary>
        [FieldOffset(0)]
        public SrTypeValue Value;

        /// <summary>
        /// 仮想マシンが扱う命令コードの値です
        /// </summary>
        [FieldOffset(0)]
        public InstructionCode Instruction;

        /// <summary>
        /// このメモリブロックのアロケーション情報を持ちます
        /// </summary>
        [FieldOffset(0)]
        public MemoryAllocationInfo AllocationInfo;
        #endregion
    }
}