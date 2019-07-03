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

using System.Collections.Generic;
using System.IO;
using TextProcessorLib;

namespace CarrotAssemblerLib
{
    /// <summary>
    /// CarrotAssembler の追加トークン種別を保持するクラスです
    /// </summary>
    public class CarrotAsmTokenKind : TokenKind
    {
        #region Keyword
        /// <summary>
        /// キーワードのトークン定義オフセット値
        /// </summary>
        public const int KeywordOffset = UserDefineOffset + 0;

        /// <summary>
        /// 定数定義
        /// 'const'
        /// </summary>
        public const int Const = KeywordOffset + 0;

        /// <summary>
        /// グローバル
        /// 'global'
        /// </summary>
        public const int Global = KeywordOffset + 1;
        #endregion

        #region OpCode keyword
        /// <summary>
        /// OpCodeのトークン定義オフセット値
        /// </summary>
        public const int OpCodeKeywordOffset = UserDefineOffset + 100;

        /// <summary>
        /// OpCodeのトークンである開始値
        /// </summary>
        public const int OpCodeTokenBegin = OpHalt;

        /// <summary>
        /// halt命令
        /// 'halt'
        /// </summary>
        public const int OpHalt = OpCodeKeywordOffset + 0;

        /// <summary>
        /// mov命令
        /// 'mov'
        /// </summary>
        public const int OpMov = OpCodeKeywordOffset + 10;

        /// <summary>
        /// ldr命令
        /// 'ldr'
        /// </summary>
        public const int OpLdr = OpCodeKeywordOffset + 20;

        /// <summary>
        /// str命令
        /// 'str'
        /// </summary>
        public const int OpStr = OpCodeKeywordOffset + 30;

        /// <summary>
        /// push命令
        /// 'push'
        /// </summary>
        public const int OpPush = OpCodeKeywordOffset + 40;

        /// <summary>
        /// pop命令
        /// 'pop'
        /// </summary>
        public const int OpPop = OpCodeKeywordOffset + 50;

        /// <summary>
        /// fmov命令
        /// 'fmov'
        /// </summary>
        public const int OpFmov = OpCodeKeywordOffset + 55;

        /// <summary>
        /// fpush命令
        /// 'fpush'
        /// </summary>
        public const int OpFpush = OpCodeKeywordOffset + 56;

        /// <summary>
        /// movfti命令
        /// 'movfti'
        /// </summary>
        public const int OpMovfti = OpCodeKeywordOffset + 57;

        /// <summary>
        /// movitf命令
        /// 'movitf'
        /// </summary>
        public const int OpMovitf = OpCodeKeywordOffset + 58;

        /// <summary>
        /// add命令
        /// 'add'
        /// </summary>
        public const int OpAdd = OpCodeKeywordOffset + 60;

        /// <summary>
        /// sub命令
        /// 'sub'
        /// </summary>
        public const int OpSub = OpCodeKeywordOffset + 70;

        /// <summary>
        /// mul命令
        /// 'mul'
        /// </summary>
        public const int OpMul = OpCodeKeywordOffset + 80;

        /// <summary>
        /// div命令
        /// 'div'
        /// </summary>
        public const int OpDiv = OpCodeKeywordOffset + 90;

        /// <summary>
        /// mod命令
        /// 'mod'
        /// </summary>
        public const int OpMod = OpCodeKeywordOffset + 100;

        /// <summary>
        /// pow命令
        /// 'pow'
        /// </summary>
        public const int OpPow = OpCodeKeywordOffset + 110;

        /// <summary>
        /// or命令
        /// 'or'
        /// </summary>
        public const int OpOr = OpCodeKeywordOffset + 120;

        /// <summary>
        /// xor命令
        /// 'xor'
        /// </summary>
        public const int OpXor = OpCodeKeywordOffset + 130;

        /// <summary>
        /// and命令
        /// 'and'
        /// </summary>
        public const int OpAnd = OpCodeKeywordOffset + 140;

        /// <summary>
        /// not命令
        /// 'not'
        /// </summary>
        public const int OpNot = OpCodeKeywordOffset + 150;

        /// <summary>
        /// shl命令
        /// 'shl'
        /// </summary>
        public const int OpShl = OpCodeKeywordOffset + 160;

        /// <summary>
        /// shr命令
        /// 'shr'
        /// </summary>
        public const int OpShr = OpCodeKeywordOffset + 170;

        /// <summary>
        /// teq命令
        /// 'teq'
        /// </summary>
        public const int OpTeq = OpCodeKeywordOffset + 180;

        /// <summary>
        /// tne命令
        /// 'tne'
        /// </summary>
        public const int OpTne = OpCodeKeywordOffset + 190;

        /// <summary>
        /// tg命令
        /// 'tg'
        /// </summary>
        public const int OpTg = OpCodeKeywordOffset + 200;

        /// <summary>
        /// tge命令
        /// 'tge'
        /// </summary>
        public const int OpTge = OpCodeKeywordOffset + 210;

        /// <summary>
        /// tl命令
        /// 'tl'
        /// </summary>
        public const int OpTl = OpCodeKeywordOffset + 220;

        /// <summary>
        /// tle命令
        /// 'tle'
        /// </summary>
        public const int OpTle = OpCodeKeywordOffset + 230;

        /// <summary>
        /// fadd命令
        /// 'fadd'
        /// </summary>
        public const int OpFadd = OpCodeKeywordOffset + 231;

        /// <summary>
        /// fsub命令
        /// 'fsub'
        /// </summary>
        public const int OpFsub = OpCodeKeywordOffset + 232;

        /// <summary>
        /// fmul命令
        /// 'fmul'
        /// </summary>
        public const int OpFmul = OpCodeKeywordOffset + 233;

        /// <summary>
        /// fdiv命令
        /// 'fdiv'
        /// </summary>
        public const int OpFdiv = OpCodeKeywordOffset + 234;

        /// <summary>
        /// fmod命令
        /// 'fmod'
        /// </summary>
        public const int OpFmod = OpCodeKeywordOffset + 235;

        /// <summary>
        /// fpow命令
        /// 'fpow'
        /// </summary>
        public const int OpFpow = OpCodeKeywordOffset + 236;

        /// <summary>
        /// br命令
        /// 'br'
        /// </summary>
        public const int OpBr = OpCodeKeywordOffset + 240;

        /// <summary>
        /// bnz命令
        /// 'bnz'
        /// </summary>
        public const int OpBnz = OpCodeKeywordOffset + 250;

        /// <summary>
        /// call命令
        /// 'call'
        /// </summary>
        public const int OpCall = OpCodeKeywordOffset + 260;

        /// <summary>
        /// callnz命令
        /// 'callnz'
        /// </summary>
        public const int OpCallnz = OpCodeKeywordOffset + 270;

        /// <summary>
        /// ret命令
        /// 'ret'
        /// </summary>
        public const int OpRet = OpCodeKeywordOffset + 280;

        /// <summary>
        /// cpf命令
        /// 'cpf'
        /// </summary>
        public const int OpCpf = OpCodeKeywordOffset + 290;

        /// <summary>
        /// gpid命令
        /// 'gpid'
        /// </summary>
        public const int OpGpid = OpCodeKeywordOffset + 300;

        /// <summary>
        /// gpfid命令
        /// 'gpfid'
        /// </summary>
        public const int OpGpfid = OpCodeKeywordOffset + 310;

        /// <summary>
        /// OpCodeのトークンである終了値
        /// </summary>
        public const int OpCodeTokenEnd = OpGpfid;
        #endregion

        #region Register keyword
        /// <summary>
        /// レジスタ名トークンキーワードのオフセット値
        /// </summary>
        public const int RegisterKeywordOffset = UserDefineOffset + 800;

        /// <summary>
        /// レジスタ名トークンキーワードの開始値
        /// </summary>
        public const int RegisterNameTokenBegin = RegisterRax;

        /// <summary>
        /// アキュムレータレジスタ
        /// 'rax'
        /// </summary>
        public const int RegisterRax = RegisterKeywordOffset + 0;

        /// <summary>
        /// ベースレジスタ
        /// 'rbx'
        /// </summary>
        public const int RegisterRbx = RegisterKeywordOffset + 1;

        /// <summary>
        /// カウンタレジスタ
        /// 'rcx'
        /// </summary>
        public const int RegisterRcx = RegisterKeywordOffset + 2;

        /// <summary>
        /// データレジスタ
        /// 'rdx'
        /// </summary>
        public const int RegisterRdx = RegisterKeywordOffset + 3;

        /// <summary>
        /// ソースインデックスレジスタ
        /// 'rsi'
        /// </summary>
        public const int RegisterRsi = RegisterKeywordOffset + 4;

        /// <summary>
        /// ディスティネーションインデックスレジスタ
        /// 'rdi'
        /// </summary>
        public const int RegisterRdi = RegisterKeywordOffset + 5;

        /// <summary>
        /// ベースポインタレジスタ
        /// 'rbp'
        /// </summary>
        public const int RegisterRbp = RegisterKeywordOffset + 6;

        /// <summary>
        /// スタックポインタレジスタ
        /// 'rsp'
        /// </summary>
        public const int RegisterRsp = RegisterKeywordOffset + 7;

        /// <summary>
        /// 完全汎用レジスタ8番
        /// 'r8'
        /// </summary>
        public const int RegisterR8 = RegisterKeywordOffset + 8;

        /// <summary>
        /// 完全汎用レジスタ9番
        /// 'r9'
        /// </summary>
        public const int RegisterR9 = RegisterKeywordOffset + 9;

        /// <summary>
        /// 完全汎用レジスタ10番
        /// 'r10'
        /// </summary>
        public const int RegisterR10 = RegisterKeywordOffset + 10;

        /// <summary>
        /// 完全汎用レジスタ11番
        /// 'r11'
        /// </summary>
        public const int RegisterR11 = RegisterKeywordOffset + 11;

        /// <summary>
        /// 完全汎用レジスタ12番
        /// 'r12'
        /// </summary>
        public const int RegisterR12 = RegisterKeywordOffset + 12;

        /// <summary>
        /// 完全汎用レジスタ13番
        /// 'r13'
        /// </summary>
        public const int RegisterR13 = RegisterKeywordOffset + 13;

        /// <summary>
        /// 完全汎用レジスタ14番
        /// 'r14'
        /// </summary>
        public const int RegisterR14 = RegisterKeywordOffset + 14;

        /// <summary>
        /// 完全汎用レジスタ15番
        /// 'r15'
        /// </summary>
        public const int RegisterR15 = RegisterKeywordOffset + 15;

        /// <summary>
        /// レジスタ名トークンキーワードの終了値
        /// </summary>
        public const int RegisterNameTokenEnd = RegisterR15;
        #endregion



        #region Utility function
        /// <summary>
        /// 指定されたトークン種別がOpCodeトークンの種別かどうかを判断します
        /// </summary>
        /// <param name="kind">判断するトークン種別</param>
        /// <returns>OpCodeトークン種別の場合は true を、異なる場合は false を返します</returns>
        public static bool IsOpCodeKind(int kind)
        {
            // OpCodeトークンIDの範囲内ならOpCodeトークンであることを返す
            return OpCodeTokenBegin <= kind && kind <= OpCodeTokenEnd;
        }


        /// <summary>
        /// 指定されたトークン種別がRegisterNameトークンの種別かどうかを判断します
        /// </summary>
        /// <param name="kind">判断するトークン種別</param>
        /// <returns>RegisterNameトークン種別の場合は true を、異なる場合は false を返します</returns>
        public static bool IsRegisterNameKind(int kind)
        {
            // RegisterNameトークンIDの範囲内ならRegisterNameトークンであることを返す
            return RegisterNameTokenBegin <= kind && kind <= RegisterNameTokenEnd;
        }
        #endregion
    }



    /// <summary>
    /// CarrotAssember の追加レキサ実装クラスです
    /// </summary>
    public class CarrotAsmLexer : TokenReader
    {
        /// <summary>
        /// CarrotAsmLexer クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="textReader">字句解析対象となるテキストリーダー</param>
        public CarrotAsmLexer(TextReader textReader) : base(textReader)
        {
        }


        /// <summary>
        /// 既定トークンテーブルに対して、更に実装クラス側で追加が必要なトークンを追加します。
        /// </summary>
        /// <param name="tokenTable">追加するトークンを受け取るテーブル</param>
        protected override void SetupToken(Dictionary<string, int> tokenTable)
        {
            // キーワードトークンの追加
            tokenTable["const"] = CarrotAsmTokenKind.Const;
            tokenTable["global"] = CarrotAsmTokenKind.Global;


            // OpCodeキーワードトークンの追加
            tokenTable["halt"] = CarrotAsmTokenKind.OpHalt;
            tokenTable["mov"] = CarrotAsmTokenKind.OpMov;
            tokenTable["ldr"] = CarrotAsmTokenKind.OpLdr;
            tokenTable["str"] = CarrotAsmTokenKind.OpStr;
            tokenTable["push"] = CarrotAsmTokenKind.OpPush;
            tokenTable["pop"] = CarrotAsmTokenKind.OpPop;
            tokenTable["fmov"] = CarrotAsmTokenKind.OpFmov;
            tokenTable["fpush"] = CarrotAsmTokenKind.OpFpush;
            tokenTable["movfti"] = CarrotAsmTokenKind.OpMovfti;
            tokenTable["movitf"] = CarrotAsmTokenKind.OpMovitf;
            tokenTable["add"] = CarrotAsmTokenKind.OpAdd;
            tokenTable["sub"] = CarrotAsmTokenKind.OpSub;
            tokenTable["mul"] = CarrotAsmTokenKind.OpMul;
            tokenTable["div"] = CarrotAsmTokenKind.OpDiv;
            tokenTable["mod"] = CarrotAsmTokenKind.OpMod;
            tokenTable["pow"] = CarrotAsmTokenKind.OpPow;
            tokenTable["fadd"] = CarrotAsmTokenKind.OpFadd;
            tokenTable["fsub"] = CarrotAsmTokenKind.OpFsub;
            tokenTable["fmul"] = CarrotAsmTokenKind.OpFmul;
            tokenTable["fdiv"] = CarrotAsmTokenKind.OpFdiv;
            tokenTable["fmod"] = CarrotAsmTokenKind.OpFmod;
            tokenTable["fpow"] = CarrotAsmTokenKind.OpFpow;
            tokenTable["or"] = CarrotAsmTokenKind.OpOr;
            tokenTable["xor"] = CarrotAsmTokenKind.OpXor;
            tokenTable["and"] = CarrotAsmTokenKind.OpAnd;
            tokenTable["not"] = CarrotAsmTokenKind.OpNot;
            tokenTable["shl"] = CarrotAsmTokenKind.OpShl;
            tokenTable["shr"] = CarrotAsmTokenKind.OpShr;
            tokenTable["teq"] = CarrotAsmTokenKind.OpTeq;
            tokenTable["tne"] = CarrotAsmTokenKind.OpTne;
            tokenTable["tg"] = CarrotAsmTokenKind.OpTg;
            tokenTable["tge"] = CarrotAsmTokenKind.OpTge;
            tokenTable["tl"] = CarrotAsmTokenKind.OpTl;
            tokenTable["tle"] = CarrotAsmTokenKind.OpTle;
            tokenTable["br"] = CarrotAsmTokenKind.OpBr;
            tokenTable["bnz"] = CarrotAsmTokenKind.OpBnz;
            tokenTable["call"] = CarrotAsmTokenKind.OpCall;
            tokenTable["callnz"] = CarrotAsmTokenKind.OpCallnz;
            tokenTable["ret"] = CarrotAsmTokenKind.OpRet;
            tokenTable["cpf"] = CarrotAsmTokenKind.OpCpf;
            tokenTable["gpid"] = CarrotAsmTokenKind.OpGpid;
            tokenTable["gpfid"] = CarrotAsmTokenKind.OpGpfid;


            // レジスタ名キーワードトークンの追加
            tokenTable["rax"] = CarrotAsmTokenKind.RegisterRax;
            tokenTable["rbx"] = CarrotAsmTokenKind.RegisterRbx;
            tokenTable["rcx"] = CarrotAsmTokenKind.RegisterRcx;
            tokenTable["rdx"] = CarrotAsmTokenKind.RegisterRdx;
            tokenTable["rsi"] = CarrotAsmTokenKind.RegisterRsi;
            tokenTable["rdi"] = CarrotAsmTokenKind.RegisterRdi;
            tokenTable["rbp"] = CarrotAsmTokenKind.RegisterRbp;
            tokenTable["rsp"] = CarrotAsmTokenKind.RegisterRsp;
            tokenTable["r8"] = CarrotAsmTokenKind.RegisterR8;
            tokenTable["r9"] = CarrotAsmTokenKind.RegisterR9;
            tokenTable["r10"] = CarrotAsmTokenKind.RegisterR10;
            tokenTable["r11"] = CarrotAsmTokenKind.RegisterR11;
            tokenTable["r12"] = CarrotAsmTokenKind.RegisterR12;
            tokenTable["r13"] = CarrotAsmTokenKind.RegisterR13;
            tokenTable["r14"] = CarrotAsmTokenKind.RegisterR14;
            tokenTable["r15"] = CarrotAsmTokenKind.RegisterR15;
        }
    }
}