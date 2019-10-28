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

命令コードは、64bit固定長で内訳は以下の様に定義されます。

MSB                                                   LSB
+--------+----------+----------+------------------------+
|  8bit  |   8bit   |  16bit   |         32bit          |
+-------------------------------------------------------+
| OpCode | Reserved | Register | Immediate or Parameter |
+--------+----------+----------+------------------------+

各種要素の内容は以下の通りです。
- OpCode
  - SnowRabbit.RuntimeEngine.OpeCode 列挙型の値を持ちます。ここにどんな命令を実行するべきかを格納します。
- Reserved
  - 未定義の予約領域です。必ず 0 クリアされていなければなりません。
- Register
  - 命令で使用されるオペランドで、どのレジスタを使用するかを 5bit で 3つ 持つことが出来ます。より詳細な内訳は後述します。
- Immediate or Parameter
  - 命令で使用されるオペランドで、32bit長の即値や OpCode によって特別な使い方をする領域です。未使用の場合は 0 であるべきです。




Register のより詳細な内訳は以下の様に定義されます。

MSB                           LSB
+----------+------+------+------+
|   1bit   | 5bit | 5bit | 5bit |
+-------------------------------+
| Reserved |  R1  |  R2  |  R3  |
+----------+------+------+-------

- Reserved
  - 未定義の予約領域です。必ず 0 クリアされていなければなりません。
- R1
  - オペランド引数1のレジスタ指定番号です。
- R2
  - オペランド引数2のレジスタ指定番号です。
- R3
  - オペランド引数3のレジスタ指定番号です。

*/

using System.Runtime.InteropServices;

namespace SnowRabbit.RuntimeEngine
{
    /// <summary>
    /// 仮想マシンが実行する命令の構造を表現した構造体です。また構造はリトルエンディアンを前提としています。
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct SrInstruction
    {
        /// <summary>
        /// 命令全体を符号なし64bit整数としてアクセスします
        /// </summary>
        [FieldOffset(0)]
        public ulong Raw;


        /// <summary>
        /// 実行するオペコード
        /// </summary>
        [FieldOffset(7)]
        public OpCode OpCode;


        /// <summary>
        /// 予約領域です。0でなければいけません。
        /// </summary>
        [FieldOffset(6)]
        public byte Reserved;


        /// <summary>
        /// レジスタ番号指定データ
        /// </summary>
        [FieldOffset(4)]
        public ushort Register;


        /// <summary>
        /// 命令に組み込まれる符号付き32bit整数即値
        /// </summary>
        [FieldOffset(0)]
        public int Int;


        /// <summary>
        /// 命令に組み込まれる符号なし32bit整数即値
        /// </summary>
        [FieldOffset(0)]
        public uint Uint;


        /// <summary>
        /// 命令に組み込まれる32bit浮動小数点即値
        /// </summary>
        [FieldOffset(0)]
        public float Float;



        /// <summary>
        /// 命令を設定します
        /// </summary>
        /// <param name="opCode">実行するオペコード</param>
        public void Set(OpCode opCode)
        {
            // 共通設定関数を使用して即値は0にする
            SetCommon(opCode, 0, 0, 0);
            Uint = 0U;
        }


        /// <summary>
        /// 指定された値をもとに命令を設定します
        /// </summary>
        /// <param name="opCode">実行するオペコード</param>
        /// <param name="r1">オペランドで使用するレジスタ指定引数1</param>
        public void Set(OpCode opCode, byte r1)
        {
            // 共通設定関数を使用して即値は0にする
            SetCommon(opCode, r1, 0, 0);
            Uint = 0U;
        }


        /// <summary>
        /// 指定された値をもとに命令を設定します
        /// </summary>
        /// <param name="opCode">実行するオペコード</param>
        /// <param name="r1">オペランドで使用するレジスタ指定引数1</param>
        /// <param name="r2">オペランドで使用するレジスタ指定引数2</param>
        public void Set(OpCode opCode, byte r1, byte r2)
        {
            // 共通設定関数を使用して即値は0にする
            SetCommon(opCode, r1, r2, 0);
            Uint = 0U;
        }


        /// <summary>
        /// 指定された値をもとに命令を設定します
        /// </summary>
        /// <param name="opCode">実行するオペコード</param>
        /// <param name="r1">オペランドで使用するレジスタ指定引数1</param>
        /// <param name="r2">オペランドで使用するレジスタ指定引数2</param>
        /// <param name="r3">オペランドで使用するレジスタ指定引数3</param>
        public void Set(OpCode opCode, byte r1, byte r2, byte r3)
        {
            // 共通設定関数を使用して即値は0にする
            SetCommon(opCode, r1, r2, r3);
            Uint = 0U;
        }


        /// <summary>
        /// 指定された値をもとに命令を設定します
        /// </summary>
        /// <param name="opCode">実行するオペコード</param>
        /// <param name="r1">オペランドで使用するレジスタ指定引数1</param>
        /// <param name="r2">オペランドで使用するレジスタ指定引数2</param>
        /// <param name="r3">オペランドで使用するレジスタ指定引数3</param>
        /// <param name="immediate">オペランドで使用する符号付き32bit整数</param>
        public void Set(OpCode opCode, byte r1, byte r2, byte r3, int immediate)
        {
            // 共通設定関数を使用して即値を設定
            SetCommon(opCode, r1, r2, r3);
            Int = immediate;
        }


        /// <summary>
        /// 指定された値をもとに命令を設定します
        /// </summary>
        /// <param name="opCode">実行するオペコード</param>
        /// <param name="r1">オペランドで使用するレジスタ指定引数1</param>
        /// <param name="r2">オペランドで使用するレジスタ指定引数2</param>
        /// <param name="r3">オペランドで使用するレジスタ指定引数3</param>
        /// <param name="immediate">オペランドで使用する符号なし32bit整数</param>
        public void Set(OpCode opCode, byte r1, byte r2, byte r3, uint immediate)
        {
            // 共通設定関数を使用して即値を設定
            SetCommon(opCode, r1, r2, r3);
            Uint = immediate;
        }


        /// <summary>
        /// 指定された値をもとに命令を設定します
        /// </summary>
        /// <param name="opCode">実行するオペコード</param>
        /// <param name="r1">オペランドで使用するレジスタ指定引数1</param>
        /// <param name="r2">オペランドで使用するレジスタ指定引数2</param>
        /// <param name="r3">オペランドで使用するレジスタ指定引数3</param>
        /// <param name="immediate">オペランドで使用する32bit浮動小数点</param>
        public void Set(OpCode opCode, byte r1, byte r2, byte r3, float immediate)
        {
            // 共通設定関数を使用して即値を設定
            SetCommon(opCode, r1, r2, r3);
            Float = immediate;
        }


        /// <summary>
        /// 指定された値をもとに命令を設定します
        /// </summary>
        /// <param name="opCode">実行するオペコード</param>
        /// <param name="r1">オペランドで使用するレジスタ指定引数1</param>
        /// <param name="r2">オペランドで使用するレジスタ指定引数2</param>
        /// <param name="r3">オペランドで使用するレジスタ指定引数3</param>
        private void SetCommon(OpCode opCode, byte r1, byte r2, byte r3)
        {
            // オペコードとレジスタ値を設定して予約領域はゼロクリア
            // 16bit変数の内訳として、5bit長のレジスタ指定値が3つ入る
            // MSB[1bit:Reserved, 5bit:R1, 5bit:R2, 5bit:R3]LSB
            OpCode = opCode;
            Reserved = 0;
            Register = 0;
            Register = (ushort)((Register & ~0x7C00) | ((r1 & 0x1F) << 10));
            Register = (ushort)((Register & ~0x03E0) | ((r2 & 0x1F) << 5));
            Register = (ushort)((Register & ~0x001F) | ((r3 & 0x1F) << 0));
        }


        /// <summary>
        /// 命令に設定されているレジスタ指定引数を取得します
        /// </summary>
        /// <param name="r1">レジスタ指定引数1</param>
        /// <param name="r2">レジスタ指定引数2</param>
        /// <param name="r3">レジスタ指定引数3</param>
        public void GetRegisterNumber(out byte r1, out byte r2, out byte r3)
        {
            // 16bit変数の内訳として、5bit長のレジスタ指定値が3つ入る
            // MSB[1bit:Reserved, 5bit:R1, 5bit:R2, 5bit:R3]LSB
            r1 = (byte)((Register >> 10) & 0x1F);
            r2 = (byte)((Register >> 5) & 0x1F);
            r3 = (byte)((Register >> 0) & 0x1F);
        }
    }
}