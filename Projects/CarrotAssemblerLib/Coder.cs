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

using System;
using System.Collections.Generic;
using System.IO;
using SnowRabbit.VirtualMachine.Runtime;
using TextProcessorLib;

namespace CarrotAssemblerLib
{
    #region Coder
    /// <summary>
    /// SnowRabbit 仮想マシン用の実行コードを生成するクラスです
    /// </summary>
    public class CarrotBinaryCoder
    {
        // クラス変数宣言
        private static readonly Dictionary<int, OpCoderBase> opCoderTable;

        // メンバ変数定義
        private Dictionary<uint, string> constStringTable;
        private Dictionary<string, int> globalVariableTable;
        private Dictionary<string, int> labelTable;
        private List<InstructionCode> instructionList;
        private List<Token> argumentList;
        private List<int> unresolveAddressInstructionList;
        private Dictionary<int, string> unresolveLabelAddressTable;
        private int nextGlobalVariableIndex;
        private int opCodeTokenKind;
        private Stream outputStream;



        /// <summary>
        /// CarrotBinaryCoder クラスの初期化をします
        /// </summary>
        static CarrotBinaryCoder()
        {
            // 生成するOpCoderの配列を用意
            var opCoders = new OpCoderBase[]
            {
                new OpCoderHalt(),
                new OpCoderMov(), new OpCoderLdr(), new OpCoderStr(), new OpCoderPush(), new OpCoderPop(),
                new OpCoderAdd(), new OpCoderSub(), new OpCoderMul(), new OpCoderDiv(), new OpCoderMod(), new OpCoderPow(),
                new OpCoderOr(), new OpCoderXor(), new OpCoderAnd(), new OpCoderNot(), new OpCoderShl(), new OpCoderShr(),
                new OpCoderTeq(), new OpCoderTne(), new OpCoderTg(), new OpCoderTge(), new OpCoderTl(), new OpCoderTle(),
                new OpCoderBr(), new OpCoderBnz(), new OpCoderCall(), new OpCoderCallnz(), new OpCoderRet(),
                new OpCoderCpf(), new OpCoderGpid(), new OpCoderGpfid(),
            };


            // OpCoderのテーブルを構築する
            opCoderTable = new Dictionary<int, OpCoderBase>();
            foreach (var coder in opCoders)
            {
                // テーブルにコーダーを追加する
                opCoderTable[coder.OpCodeTokenKind] = coder;
            }
        }


        /// <summary>
        /// CarrotBinaryCoder クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="outputStream">生成された実行コードを出力する先のストリーム</param>
        public CarrotBinaryCoder(Stream outputStream)
        {
            // メンバ変数の初期化をする
            constStringTable = new Dictionary<uint, string>();
            globalVariableTable = new Dictionary<string, int>();
            labelTable = new Dictionary<string, int>();
            instructionList = new List<InstructionCode>();
            argumentList = new List<Token>();
            unresolveAddressInstructionList = new List<int>();
            unresolveLabelAddressTable = new Dictionary<int, string>();
            nextGlobalVariableIndex = -1;
            this.outputStream = outputStream;
        }


        #region Register function
        /// <summary>
        /// 文字列定数を登録します
        /// </summary>
        /// <param name="index">登録する文字列のインデックス値</param>
        /// <param name="text">登録する文字列</param>
        /// <returns>正常に登録された場合は true を、指定されたインデックスが既に登録されている場合は false を返します</returns>
        internal bool RegisterConstString(uint index, string text)
        {
            // 既に指定されたインデックスが登録済みなら
            if (constStringTable.ContainsKey(index))
            {
                // 登録済みであることを返す
                return false;
            }


            // 登録して成功を返す
            constStringTable[index] = text;
            return true;
        }


        /// <summary>
        /// 指定されたグローバル変数名を登録します
        /// </summary>
        /// <param name="name">登録する変数名</param>
        /// <returns>正常に変数名が登録された場合は登録した変数インデックスを、既に登録済みの場合は 0 を返します</returns>
        internal int RegisterGlobalVariable(string name)
        {
            // 既に同じ名前の変数またはラベルが登録済みなら
            if (globalVariableTable.ContainsKey(name) || labelTable.ContainsKey(name))
            {
                // 登録済みであることを返す
                return 0;
            }


            // グローバル変数名を登録して登録したインデックスを返す（次に登録するインデックスも更新する）
            var registerdIndex = nextGlobalVariableIndex--;
            globalVariableTable[name] = registerdIndex;
            return registerdIndex;
        }


        /// <summary>
        /// 指定されたラベル名を登録します
        /// </summary>
        /// <param name="name">登録するラベル名</param>
        /// <returns>正常にラベル名が登録された場合はラベルが位置する命令アドレスを返しますが、既に登録済みの場合は -1 を返します</returns>
        internal int RegisterLabel(string name)
        {
            // 既に同じ名前の変数またはラベルが登録済みなら
            if (globalVariableTable.ContainsKey(name) || labelTable.ContainsKey(name))
            {
                // 登録済みであることを返す
                return -1;
            }


            // 現在の命令アドレスをテーブルに登録して返す
            labelTable[name] = instructionList.Count;
            return instructionList.Count;
        }


        /// <summary>
        /// 指定された名前のラベルを未解決ラベルとして登録します
        /// </summary>
        /// <param name="name">未解決のラベル名</param>
        /// <returns>未解決ラベル名の仮アドレスを返します</returns>
        internal int RegisterUnresolveLabel(string name)
        {
            // 仮アドレスを作って登録してから返す
            var tempAddress = -(unresolveLabelAddressTable.Count + 1);
            unresolveLabelAddressTable[tempAddress] = name;
            return tempAddress;
        }
        #endregion


        #region Contain function
        /// <summary>
        /// 指定された名前が変数名として存在するかどうかを調べます
        /// </summary>
        /// <param name="name">確認する名前</param>
        /// <returns>存在する場合は true を、存在しない場合は false を返します</returns>
        internal bool ContainGlobalVariable(string name)
        {
            // グローバル変数テーブルに含まれているか確認した結果を返す
            return globalVariableTable.ContainsKey(name);
        }


        /// <summary>
        /// 指定された名前がラベル名として存在するかどうかを確認します
        /// </summary>
        /// <param name="name">確認する名前</param>
        /// <returns>存在する場合は true を、存在しない場合は false を返します</returns>
        internal bool ContainLable(string name)
        {
            // ラベルテーブルに含まれているか確認した結果を返す
            return labelTable.ContainsKey(name);
        }
        #endregion


        #region Get function
        /// <summary>
        /// 指定されたグローバル変数名から仮アドレスを取得します
        /// </summary>
        /// <param name="name">仮アドレスを取得するグローバル変数名</param>
        /// <returns>指定されたグローバル変数名に対応する仮アドレスを返しますが、存在しない場合は 0 を返します</returns>
        internal int GetGlobalVariableTempAddress(string name)
        {
            // 指定したグローバル変数名がそもそも無いなら
            if (!ContainGlobalVariable(name))
            {
                // 0を返してそもそも無いことを伝える
                return 0;
            }


            // 仮アドレスを返す
            return globalVariableTable[name];
        }


        /// <summary>
        /// 指定されたラベルのアドレスを取得します
        /// </summary>
        /// <param name="name">アドレスを取得するラベル名</param>
        /// <returns>指定されたラベル名に対応するアドレスを返しますが、存在しない場合は -1 を返します</returns>
        internal int GetLabelAddress(string name)
        {
            // 指定されたラベル名がそもそも存在しないなら
            if (!ContainLable(name))
            {
                // -1を返してそもそも無いことを伝える
                return -1;
            }


            // ラベルのアドレスを返す
            return labelTable[name];
        }
        #endregion


        #region Setup function
        /// <summary>
        /// 指定されたOpCodeトークン種別を設定します
        /// </summary>
        /// <param name="tokenKind">設定するとトークン種別</param>
        internal void SetOpCodeTokenKind(int tokenKind)
        {
            // ここではそのまま受け入れる
            opCodeTokenKind = tokenKind;
        }


        /// <summary>
        /// 指定されたトークンを引数として追加します
        /// </summary>
        /// <param name="token">追加するトークン</param>
        internal void AddArgumentToken(ref Token token)
        {
            // 引数として使うトークンを追加する
            argumentList.Add(token);
        }


        /// <summary>
        /// 現在生成中の命令はアドレスの解決が必要であるマークを設定します
        /// </summary>
        internal void SetResolveAddressInstructionMark()
        {
            // 現在の命令の位置をアドレス解決が必要な命令アドレスであることを設定する
            unresolveAddressInstructionList.Add(instructionList.Count);
        }
        #endregion


        #region Build function
        /// <summary>
        /// 現在のコンテキストでマシン語を生成します
        /// </summary>
        /// <param name="message">マシン語の生成に失敗した場合のメッセージを格納する文字列への参照</param>
        /// <returns>マシン語の生成に成功した場合は true を、失敗した場合は false を返します</returns>
        internal bool GenerateCode(out string message)
        {
            // マシン語の用意をして命令の符号器を取得する
            InstructionCode instructionCode;
            var opCoder = opCoderTable[opCodeTokenKind];
            opCoder.SetCoder(this);


            // 符号器に命令の符号をしてもらい失敗したら
            if (!opCoder.Encode(argumentList, out instructionCode, out message))
            {
                // 引数リストを初期化して失敗を返す
                argumentList.Clear();
                return false;
            }


            // 命令コードを追加して引数リストをクリアする
            instructionList.Add(instructionCode);
            argumentList.Clear();
            message = string.Empty;
            return true;
        }


        /// <summary>
        /// 最終的な実行コードを出力します
        /// </summary>
        internal void OutputExecuteCode()
        {
            // 未解決のアドレスを解決する
            ResolveAddress();
        }


        /// <summary>
        /// 命令リストに含まれる未解決アドレスのオペランドを解決します
        /// </summary>
        private void ResolveAddress()
        {
            // 未解決アドレスを持つ命令の数分ループする
            for (int i = 0; i < unresolveAddressInstructionList.Count; ++i)
            {
                // 未解決命令を取得
                var unresolveIndex = unresolveAddressInstructionList[i];
                var instruction = instructionList[unresolveIndex];


                // 解決する命令によって解決関数を選択する
                switch (instruction.OpCode)
                {
                    // グローバル変数アドレス解決系命令ならグローバル変数アドレス解決を行う
                    case OpCode.Ldrl:
                    case OpCode.Strl:
                        ResolveGlobalVariableAddress(ref instruction);
                        break;


                    // ラベルアドレス解決系命令ならラベルアドレス解決を行う
                    case OpCode.Brl:
                    case OpCode.Bnzl:
                    case OpCode.Calll:
                    case OpCode.Callnzl:
                        ResolveLabelAddress(ref instruction);
                        break;


                    // 解決関数が未定義の命令が指定されたら
                    default:
                        throw new InvalidOperationException($"アドレスの解決関数が未定義の命令が指示されました '{instruction.OpCode}'");
                }


                // 解決した命令を書き戻す
                instructionList[unresolveIndex] = instruction;
            }
        }


        /// <summary>
        /// グローバル変数アドレスの解決を行います
        /// </summary>
        /// <param name="instructionCode">解決を行う命令への参照</param>
        private void ResolveGlobalVariableAddress(ref InstructionCode instructionCode)
        {
            // グローバル変数は即値の値にグローバル変数定義開始オフセット-1が
            // 負の値として入っているので反転してオフセットを加算して1を加算するだけで良い
            // つまり（プログラムコードの長さ - immediate + 1）
            instructionCode.Immediate.Int = instructionList.Count - instructionCode.Immediate.Int + 1;
        }


        /// <summary>
        /// ラベルアドレスの解決を行います
        /// </summary>
        /// <param name="instructionCode">解決を行う命令への参照</param>
        private void ResolveLabelAddress(ref InstructionCode instructionCode)
        {
            // ラベルは仮アドレスが即値に入っているので、仮アドレスから本来解決するべきラベル名を取得する
            var labelName = unresolveLabelAddressTable[instructionCode.Immediate.Int];


            // ラベル名から本当のラベルアドレスを取得するが、この時点でも取得出来ないなら
            if (!ContainLable(labelName))
            {
                // 未定義のラベルであることがここで判明する
                throw new InvalidOperationException($"未定義のラベルです '{labelName}'");
            }


            // ラベルアドレスを設定する
            instructionCode.Immediate.Int = GetLabelAddress(labelName);
        }
        #endregion
    }
    #endregion



    #region OpCoder base class
    /// <summary>
    /// 特定のオペコードの符号化を行う抽象クラスです
    /// </summary>
    public abstract class OpCoderBase
    {
        // メンバ変数定義
        protected CarrotBinaryCoder currentCoder;



        /// <summary>
        /// この符号器が担当するOpCodeトークン種別
        /// </summary>
        public abstract int OpCodeTokenKind { get; }


        /// <summary>
        /// この符号器が担当する命令の名前
        /// </summary>
        public abstract string OpCodeName { get; }



        /// <summary>
        /// このクラスのインスタンスを操作する符号器を設定します
        /// </summary>
        /// <param name="coder">操作する符号器</param>
        public void SetCoder(CarrotBinaryCoder coder)
        {
            // 参照を覚える
            currentCoder = coder;
        }


        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public abstract bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message);


        /// <summary>
        /// オペランドが0個のオペランドパターンをテストします
        /// </summary>
        /// <param name="operand">確認するオペランドのリスト</param>
        /// <param name="message">パターンに一致しない場合のエラーメッセージを設定する文字列への参照</param>
        /// <returns>オペランドの引数パターンが一致した場合は true を、一致しない場合は false を返します</returns>
        protected bool TestOperandPattern(List<Token> operand, out string message)
        {
            // もし引数が0で無いなら
            if (operand.Count != 0)
            {
                // 引数は不要であるエラーメッセージを設定して失敗を返す
                message = $"'{OpCodeName}'命令にはオペランドは不要です";
                return false;
            }


            // 引数がないなら空文字列を設定して成功を返す
            message = string.Empty;
            return true;
        }


        /// <summary>
        /// オペランドが1個のオペランドパターンをテストします
        /// </summary>
        /// <param name="operand">確認するオペランドのリスト</param>
        /// <param name="message">パターンに一致しない場合のエラーメッセージを設定する文字列への参照</param>
        /// <param name="arg1">第一オペランドの確認するべきトークン種別。-1の場合は不特定のレジスタとして表現します</param>
        /// <returns>オペランドの引数パターンが一致した場合は true を、一致しない場合は false を返します</returns>
        protected bool TestOperandPattern(List<Token> operand, out string message, int arg1)
        {
            // もし引数が1で無いなら
            if (operand.Count != 1)
            {
                // 引数は1つでなければならないエラーメッセージを設定して失敗を返す
                message = $"'{OpCodeName}'命令のオペランドは1つです";
                return false;
            }


            // 引数の型チェックに引っかかる場合は
            if (!TestOperandType(arg1, operand[0].Kind))
            {
                // 引数の型が一致しないエラーメッセージを設定して失敗を返す
                message = $"'{OpCodeName}'命令のいくつかのオペランドに指定出来ない型が含まれています";
                return false;
            }


            // 型チェックも引っかからない場合は成功を返す
            message = string.Empty;
            return true;
        }


        /// <summary>
        /// オペランドが2個のオペランドパターンをテストします
        /// </summary>
        /// <param name="operand">確認するオペランドのリスト</param>
        /// <param name="message">パターンに一致しない場合のエラーメッセージを設定する文字列への参照</param>
        /// <param name="arg1">第一オペランドの確認するべきトークン種別。-1の場合は不特定のレジスタとして表現します</param>
        /// <param name="arg2">第二オペランドの確認するべきトークン種別。-1の場合は不特定のレジスタとして表現します</param>
        /// <returns>オペランドの引数パターンが一致した場合は true を、一致しない場合は false を返します</returns>
        protected bool TestOperandPattern(List<Token> operand, out string message, int arg1, int arg2)
        {
            // もし引数が2で無いなら
            if (operand.Count != 2)
            {
                // 引数は2つでなければならないエラーメッセージを設定して失敗を返す
                message = $"'{OpCodeName}'命令のオペランドは2つです";
                return false;
            }


            // 引数の型チェックに引っかかる場合は
            if (!TestOperandType(arg1, operand[0].Kind) || !TestOperandType(arg2, operand[1].Kind))
            {
                // 引数の型が一致しないエラーメッセージを設定して失敗を返す
                message = $"'{OpCodeName}'命令のいくつかのオペランドに指定出来ない型が含まれています";
                return false;
            }


            // 型チェックも引っかからない場合は成功を返す
            message = string.Empty;
            return true;
        }


        /// <summary>
        /// オペランドが3個のオペランドパターンをテストします
        /// </summary>
        /// <param name="operand">確認するオペランドのリスト</param>
        /// <param name="message">パターンに一致しない場合のエラーメッセージを設定する文字列への参照</param>
        /// <param name="arg1">第一オペランドの確認するべきトークン種別。-1の場合は不特定のレジスタとして表現します</param>
        /// <param name="arg2">第二オペランドの確認するべきトークン種別。-1の場合は不特定のレジスタとして表現します</param>
        /// <param name="arg3">第三オペランドの確認するべきトークン種別。-1の場合は不特定のレジスタとして表現します</param>
        /// <returns>オペランドの引数パターンが一致した場合は true を、一致しない場合は false を返します</returns>
        protected bool TestOperandPattern(List<Token> operand, out string message, int arg1, int arg2, int arg3)
        {
            // もし引数が3で無いなら
            if (operand.Count != 3)
            {
                // 引数は3つでなければならないエラーメッセージを設定して失敗を返す
                message = $"'{OpCodeName}'命令のオペランドは3つです";
                return false;
            }


            // 引数の型チェックに引っかかる場合は
            if (!TestOperandType(arg1, operand[0].Kind) || !TestOperandType(arg2, operand[1].Kind) || !TestOperandType(arg3, operand[2].Kind))
            {
                // 引数の型が一致しないエラーメッセージを設定して失敗を返す
                message = $"'{OpCodeName}'命令のいくつかのオペランドに指定出来ない型が含まれています";
                return false;
            }


            // 型チェックも引っかからない場合は成功を返す
            message = string.Empty;
            return true;
        }


        /// <summary>
        /// 指定されたオペランドタイプが想定のタイプかどうかを判定します。
        /// </summary>
        /// <param name="expectedType">想定している型。-1の場合は不特定のレジスタとして扱われます</param>
        /// <param name="actualType">実際の型</param>
        /// <returns>想定通りの型の場合は true を、異なる場合は false を返します</returns>
        private bool TestOperandType(int expectedType, int actualType)
        {
            // もし-1が想定の型なら不特定のレジスタであることを想定する
            if (expectedType == -1)
            {
                // レジスタ名の指示かどうかを返す
                return CarrotAsmTokenKind.IsRegisterNameKind(actualType);
            }


            // 特定の指示ならそのまま等価式の結果を返す
            return expectedType == actualType;
        }


        /// <summary>
        /// 指定されたトークン種別からレジスタ番号へ変換します
        /// </summary>
        /// <param name="tokenKind">変換元トークン種別</param>
        /// <returns>変換されたレジスタ番号を返します</returns>
        protected byte TokenKindToRegisterNumber(int tokenKind)
        {
            // トークン種別にレジスタトークンオフセット値を引くだけで完了
            return (byte)(tokenKind - CarrotAsmTokenKind.RegisterKeywordOffset);
        }
    }
    #endregion



    #region OpCoder class
    /// <summary>
    /// halt命令符号器クラスです
    /// </summary>
    public class OpCoderHalt : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpHalt;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "halt";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Halt;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// mov命令符号器クラスです
    /// </summary>
    public class OpCoderMov : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpMov;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "mov";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Mov;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Movl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Immediate.Int = (int)operand[1].Integer;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// ldr命令符号器クラスです
    /// </summary>
    public class OpCoderLdr : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpLdr;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "ldr";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Ldr;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Immediate.Int = 0;
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Ldr;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Immediate.Int = (int)operand[2].Integer;
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Ldrl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Immediate.Int = (int)operand[1].Integer;
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, CarrotAsmTokenKind.Identifier))
            {
                // グローバル変数名から仮アドレスを取得するがそもそも取得出来ないなら
                var tempAdress = currentCoder.GetGlobalVariableTempAddress(operand[1].Text);
                if (tempAdress == 0)
                {
                    // 未定義のグローバル変数名であるエラーを設定して終了
                    message = $"指示された '{operand[1].Text}' はグローバル変数として定義されていません";
                    return false;
                }


                // 現在の命令はドレス解決が必要であることを設定する
                currentCoder.SetResolveAddressInstructionMark();


                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Ldrl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Immediate.Int = tempAdress;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// str命令符号器クラスです
    /// </summary>
    public class OpCoderStr : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpStr;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "str";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Str;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Immediate.Int = 0;
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Str;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Immediate.Int = (int)operand[2].Integer;
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Strl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Immediate.Int = (int)operand[1].Integer;
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, CarrotAsmTokenKind.Identifier))
            {
                // グローバル変数名から仮アドレスを取得するがそもそも取得出来ないなら
                var tempAdress = currentCoder.GetGlobalVariableTempAddress(operand[1].Text);
                if (tempAdress == 0)
                {
                    // 未定義のグローバル変数名であるエラーを設定して終了
                    message = $"指示された '{operand[1].Text}' はグローバル変数として定義されていません";
                    return false;
                }


                // 現在の命令はドレス解決が必要であることを設定する
                currentCoder.SetResolveAddressInstructionMark();


                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Strl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Immediate.Int = tempAdress;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// push命令符号器クラスです
    /// </summary>
    public class OpCoderPush : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpPush;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "push";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Push;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Pushl;
                instructionCode.Immediate.Int = (int)operand[0].Integer;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// pop命令符号器クラスです
    /// </summary>
    public class OpCoderPop : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpPop;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "pop";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Pop;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// add命令符号器クラスです
    /// </summary>
    public class OpCoderAdd : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpAdd;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "add";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Add;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Addl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Immediate.Int = (int)operand[2].Integer;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// sub命令符号器クラスです
    /// </summary>
    public class OpCoderSub : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpSub;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "sub";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Sub;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Subl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Immediate.Int = (int)operand[2].Integer;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// mul命令符号器クラスです
    /// </summary>
    public class OpCoderMul : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpMul;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "mul";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Mul;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Mull;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Immediate.Int = (int)operand[2].Integer;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// div命令符号器クラスです
    /// </summary>
    public class OpCoderDiv : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpDiv;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "div";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Div;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Divl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Immediate.Int = (int)operand[2].Integer;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// mod命令符号器クラスです
    /// </summary>
    public class OpCoderMod : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpMod;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "mod";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Mod;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Modl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Immediate.Int = (int)operand[2].Integer;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// pow命令符号器クラスです
    /// </summary>
    public class OpCoderPow : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpPow;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "pow";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Pow;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Powl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Immediate.Int = (int)operand[2].Integer;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// or命令符号器クラスです
    /// </summary>
    public class OpCoderOr : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpOr;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "or";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Or;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// xor命令符号器クラスです
    /// </summary>
    public class OpCoderXor : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpXor;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "xor";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Xor;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// and命令符号器クラスです
    /// </summary>
    public class OpCoderAnd : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpAnd;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "and";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.And;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// not命令符号器クラスです
    /// </summary>
    public class OpCoderNot : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpNot;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "not";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Not;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// shl命令符号器クラスです
    /// </summary>
    public class OpCoderShl : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpShl;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "shl";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Shl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// shr命令符号器クラスです
    /// </summary>
    public class OpCoderShr : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpShr;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "shr";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Shr;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// teq命令符号器クラスです
    /// </summary>
    public class OpCoderTeq : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpTeq;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "teq";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Teq;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// tne命令符号器クラスです
    /// </summary>
    public class OpCoderTne : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpTne;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "tne";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Tne;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// tg命令符号器クラスです
    /// </summary>
    public class OpCoderTg : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpTg;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "tg";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Tg;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// tge命令符号器クラスです
    /// </summary>
    public class OpCoderTge : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpTge;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "tge";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Tge;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// tl命令符号器クラスです
    /// </summary>
    public class OpCoderTl : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpTl;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "tl";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Tl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// tle命令符号器クラスです
    /// </summary>
    public class OpCoderTle : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpTle;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "tle";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Tle;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// br命令符号器クラスです
    /// </summary>
    public class OpCoderBr : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpBr;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "br";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Br;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Brl;
                instructionCode.Immediate.Int = (int)operand[0].Integer;
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, CarrotAsmTokenKind.Identifier))
            {
                // ラベルアドレスを取得するが見つけられなかったら
                var address = currentCoder.GetLabelAddress(operand[0].Text);
                if (address == -1)
                {
                    // ラベルの仮アドレスを発行して一時しのぎ後アドレスの未解決マークをする
                    address = currentCoder.RegisterUnresolveLabel(operand[0].Text);
                    currentCoder.SetResolveAddressInstructionMark();
                }


                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Brl;
                instructionCode.Immediate.Int = address;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// bnz命令符号器クラスです
    /// </summary>
    public class OpCoderBnz : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpBnz;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "bnz";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Bnz;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[0].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Bnzl;
                instructionCode.Rb = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Immediate.Int = (int)operand[1].Integer;
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, CarrotAsmTokenKind.Identifier))
            {
                // ラベルアドレスを取得するが見つけられなかったら
                var address = currentCoder.GetLabelAddress(operand[1].Text);
                if (address == -1)
                {
                    // ラベルの仮アドレスを発行して一時しのぎ後アドレスの未解決マークをする
                    address = currentCoder.RegisterUnresolveLabel(operand[1].Text);
                    currentCoder.SetResolveAddressInstructionMark();
                }


                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Bnzl;
                instructionCode.Rb = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Immediate.Int = address;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// call命令符号器クラスです
    /// </summary>
    public class OpCoderCall : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpCall;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "call";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Call;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Calll;
                instructionCode.Immediate.Int = (int)operand[0].Integer;
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, CarrotAsmTokenKind.Identifier))
            {
                // ラベルアドレスを取得するが見つけられなかったら
                var address = currentCoder.GetLabelAddress(operand[0].Text);
                if (address == -1)
                {
                    // ラベルの仮アドレスを発行して一時しのぎ後アドレスの未解決マークをする
                    address = currentCoder.RegisterUnresolveLabel(operand[0].Text);
                    currentCoder.SetResolveAddressInstructionMark();
                }


                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Calll;
                instructionCode.Immediate.Int = address;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// callnz命令符号器クラスです
    /// </summary>
    public class OpCoderCallnz : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpCallnz;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "callnz";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Callnz;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[0].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Callnzl;
                instructionCode.Rb = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Immediate.Int = (int)operand[1].Integer;
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, CarrotAsmTokenKind.Identifier))
            {
                // ラベルアドレスを取得するが見つけられなかったら
                var address = currentCoder.GetLabelAddress(operand[1].Text);
                if (address == -1)
                {
                    // ラベルの仮アドレスを発行して一時しのぎ後アドレスの未解決マークをする
                    address = currentCoder.RegisterUnresolveLabel(operand[1].Text);
                    currentCoder.SetResolveAddressInstructionMark();
                }


                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Callnzl;
                instructionCode.Rb = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Immediate.Int = address;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// ret命令符号器クラスです
    /// </summary>
    public class OpCoderRet : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpRet;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "ret";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Ret;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// cpf命令符号器クラスです
    /// </summary>
    public class OpCoderCpf : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpCpf;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "cpf";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Cpf;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Cpfl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Immediate.Int = (int)operand[2].Integer;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// gpid命令符号器クラスです
    /// </summary>
    public class OpCoderGpid : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpGpid;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "gpid";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Gpid;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Gpidl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Immediate.Int = (int)operand[1].Integer;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }



    /// <summary>
    /// gpfid命令符号器クラスです
    /// </summary>
    public class OpCoderGpfid : OpCoderBase
    {
        /// <summary>
        /// 担当するOpCodeトークン種別
        /// </summary>
        public override int OpCodeTokenKind => CarrotAsmTokenKind.OpGpfid;


        /// <summary>
        /// 担当する符号器名
        /// </summary>
        public override string OpCodeName => "gpfid";



        /// <summary>
        /// 命令のエンコードを行います
        /// </summary>
        /// <param name="operand">エンコードする命令に渡すオペランド</param>
        /// <param name="instructionCode">エンコードした命令を設定する命令コード構造体への参照</param>
        /// <param name="message">エンコードに何かしらの問題が発生したときに設定するメッセージへの参照</param>
        /// <returns>正しくエンコードが出来た場合は true を、失敗した場合は false を返します</returns>
        public override bool Encode(List<Token> operand, out InstructionCode instructionCode, out string message)
        {
            // 命令コードの初期化
            instructionCode = default;


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, -1))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Gpfid;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Rc = TokenKindToRegisterNumber(operand[2].Kind);
                return true;
            }


            // 引数テストに成功したら
            if (TestOperandPattern(operand, out message, -1, -1, CarrotAsmTokenKind.Integer))
            {
                // 命令を設定して成功を返す
                instructionCode.OpCode = OpCode.Gpfidl;
                instructionCode.Ra = TokenKindToRegisterNumber(operand[0].Kind);
                instructionCode.Rb = TokenKindToRegisterNumber(operand[1].Kind);
                instructionCode.Immediate.Int = (int)operand[2].Integer;
                return true;
            }


            // ここまで来てしまったら失敗を返す
            return false;
        }
    }
    #endregion
}