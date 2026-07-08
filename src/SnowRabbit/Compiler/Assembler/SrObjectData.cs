// zlib License
//
// Copyright (c) 2026 Sinoa
//
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
// claim that you wrote the original software. If you use this software
// in a product, an acknowledgment in the product documentation would be
// appreciated but is not required.
//
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
//
// 3. This notice may not be removed or altered from any source
// distribution.

#nullable disable

using System.Collections.Generic;
using SnowRabbit.Compiler.Assembler.Symbols;

namespace SnowRabbit.Compiler.Assembler
{
    /// <summary>
    /// リンク可能なオブジェクトファイル (SROB) の内容を表すデータクラスです。
    /// アドレス解決前のアセンブリデータ（負の仮想アドレスと未解決フラグ）をそのまま保持します。
    /// InitialAddress はファイルローカルなシンボルIDとして機能し、リンク時に再採番されます。
    /// </summary>
    public class SrObjectData
    {
        /// <summary>
        /// オブジェクトファイルのマジックナンバー（リトルエンディアンでディスク上 "SROB" になる）
        /// </summary>
        public const uint MagicNumber = (byte)'B' << 24 | (byte)'O' << 16 | (byte)'R' << 8 | (byte)'S';

        /// <summary>
        /// オブジェクトファイル形式のバージョン
        /// </summary>
        public const int FormatVersion = 1;


        /// <summary>
        /// グローバルシンボル表の内容（文字列シンボルを除く）
        /// </summary>
        public List<SrObjectSymbolData> Symbols { get; } = new List<SrObjectSymbolData>();

        /// <summary>
        /// 文字列シンボル表の内容
        /// </summary>
        public List<SrObjectStringData> Strings { get; } = new List<SrObjectStringData>();

        /// <summary>
        /// 関数コード表の内容
        /// </summary>
        public List<SrObjectFunctionCodeData> FunctionCodes { get; } = new List<SrObjectFunctionCodeData>();
    }


    /// <summary>
    /// オブジェクトファイル内の1つのシンボルを表すデータクラスです
    /// </summary>
    public class SrObjectSymbolData
    {
        /// <summary>
        /// シンボルの種別（ScriptFunction / PeripheralFunction / GlobalVariable / Constant / Label のいずれか）
        /// </summary>
        public SrSymbolKind Kind { get; set; }

        /// <summary>
        /// シンボル名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ファイルローカルなシンボルID（元の負の仮想アドレス）
        /// </summary>
        public int InitialAddress { get; set; }

        /// <summary>
        /// 関数の戻り値型、または変数・定数の型
        /// </summary>
        public SrRuntimeType Type { get; set; }

        /// <summary>
        /// ペリフェラル関数の周辺機器名
        /// </summary>
        public string PeripheralName { get; set; }

        /// <summary>
        /// ペリフェラル関数の周辺機器関数名
        /// </summary>
        public string PeripheralFunctionName { get; set; }

        /// <summary>
        /// 関数のパラメータリスト（位置順）
        /// </summary>
        public List<SrObjectTypedNameData> Parameters { get; } = new List<SrObjectTypedNameData>();

        /// <summary>
        /// スクリプト関数のローカル変数リスト（割り当て順）
        /// </summary>
        public List<SrObjectTypedNameData> LocalVariables { get; } = new List<SrObjectTypedNameData>();

        /// <summary>
        /// スクリプト関数が使用するレジスタ番号の集合
        /// </summary>
        public List<byte> UsedRegisters { get; } = new List<byte>();

        /// <summary>
        /// グローバル変数の初期化リテラル、または定数の値を持つかどうか
        /// </summary>
        public bool HasLiteral { get; set; }

        /// <summary>
        /// リテラルのトークン種別
        /// </summary>
        public int LiteralKind { get; set; }

        /// <summary>
        /// リテラルのテキスト
        /// </summary>
        public string LiteralText { get; set; }

        /// <summary>
        /// リテラルの整数値
        /// </summary>
        public long LiteralInteger { get; set; }

        /// <summary>
        /// リテラルの実数値
        /// </summary>
        public double LiteralNumber { get; set; }

        /// <summary>
        /// ラベルが属する関数名
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// ラベルの関数コード内相対オフセット
        /// </summary>
        public int LabelAddress { get; set; }
    }


    /// <summary>
    /// 名前と型の組を表すデータクラスです（パラメータ・ローカル変数用）
    /// </summary>
    public class SrObjectTypedNameData
    {
        /// <summary>
        /// 名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 型
        /// </summary>
        public SrRuntimeType Type { get; set; }
    }


    /// <summary>
    /// オブジェクトファイル内の1つの文字列シンボルを表すデータクラスです
    /// </summary>
    public class SrObjectStringData
    {
        /// <summary>
        /// 文字列の内容
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// ファイルローカルなシンボルID（元の負の仮想アドレス）
        /// </summary>
        public int InitialAddress { get; set; }
    }


    /// <summary>
    /// オブジェクトファイル内の1つの関数コードを表すデータクラスです
    /// </summary>
    public class SrObjectFunctionCodeData
    {
        /// <summary>
        /// 関数名
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// 命令列（生の命令値と未解決アドレスフラグの組）
        /// </summary>
        public List<SrObjectInstructionData> Codes { get; } = new List<SrObjectInstructionData>();
    }


    /// <summary>
    /// オブジェクトファイル内の1つの命令を表すデータクラスです
    /// </summary>
    public struct SrObjectInstructionData
    {
        /// <summary>
        /// 命令の生の値
        /// </summary>
        public ulong Raw { get; set; }

        /// <summary>
        /// 即値が未解決のシンボルID（InitialAddress）を指しているかどうか
        /// </summary>
        public bool UnresolvedAddress { get; set; }
    }
}
