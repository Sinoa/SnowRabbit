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
        // メンバ変数定義
        private Dictionary<uint, string> constStringTable;
        private Dictionary<string, int> globalVariableTable;
        private Dictionary<string, int> labelTable;
        private List<InstructionCode> instructionList;
        private List<Token> argumentList;
        private int nextGlobalVariableIndex;
        private int opCodeTokenKind;



        /// <summary>
        /// CarrotBinaryCoder クラスのインスタンスを初期化します
        /// </summary>
        public CarrotBinaryCoder()
        {
            // メンバ変数の初期化をする
            constStringTable = new Dictionary<uint, string>();
            globalVariableTable = new Dictionary<string, int>();
            labelTable = new Dictionary<string, int>();
            instructionList = new List<InstructionCode>();
            argumentList = new List<Token>();
            nextGlobalVariableIndex = -1;
        }


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
        internal int RegisterLable(string name)
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
        /// 現在のコンテキストでマシン語を生成します
        /// </summary>
        /// <param name="message">マシン語の生成に失敗した場合のメッセージを格納する文字列への参照</param>
        /// <returns>マシン語の生成に成功した場合は true を、失敗した場合は false を返します</returns>
        internal bool GenerateCode(out string message)
        {
            // マシン語の用意
            var instructionCode = default(InstructionCode);


            // 命令のトークン種別毎に実装を変える
            switch (opCodeTokenKind)
            {
                // halt命令
                case CarrotAsmTokenKind.OpHalt:
                    // 引数の数が1つ以上あるなら
                    if (argumentList.Count >= 1)
                    {
                        // 引数の数が一致しないエラーメッセージを設定して失敗を返す
                        message = "halt 命令にはオペランドは必要ありません";
                        return false;
                    }
                    // 命令だけ設定する
                    instructionCode.OpCode = OpCode.Halt;
                    break;


                // mov命令
                case CarrotAsmTokenKind.OpMov:
                    // 引数の数が2つではないなら
                    if (argumentList.Count != 2)
                    {
                        // 引数の数が一致しないエラーメッセージを設定して失敗を返す
                        message = "mov 命令は 2つのオペランドが必要です";
                    }
                    // 命令を設定
                    instructionCode.OpCode = OpCode.Mov;
                    break;


                case CarrotAsmTokenKind.OpLdr:
                case CarrotAsmTokenKind.OpStr:
                case CarrotAsmTokenKind.OpPush:
                case CarrotAsmTokenKind.OpPop:
                case CarrotAsmTokenKind.OpAdd:
                case CarrotAsmTokenKind.OpSub:
                case CarrotAsmTokenKind.OpMul:
                case CarrotAsmTokenKind.OpDiv:
                case CarrotAsmTokenKind.OpMod:
                case CarrotAsmTokenKind.OpPow:
                case CarrotAsmTokenKind.OpOr:
                case CarrotAsmTokenKind.OpXor:
                case CarrotAsmTokenKind.OpAnd:
                case CarrotAsmTokenKind.OpNot:
                case CarrotAsmTokenKind.OpShl:
                case CarrotAsmTokenKind.OpShr:
                case CarrotAsmTokenKind.OpTeq:
                case CarrotAsmTokenKind.OpTne:
                case CarrotAsmTokenKind.OpTg:
                case CarrotAsmTokenKind.OpTge:
                case CarrotAsmTokenKind.OpTl:
                case CarrotAsmTokenKind.OpTle:
                case CarrotAsmTokenKind.OpBr:
                case CarrotAsmTokenKind.OpBnz:
                case CarrotAsmTokenKind.OpCall:
                case CarrotAsmTokenKind.OpCallnz:
                case CarrotAsmTokenKind.OpRet:
                case CarrotAsmTokenKind.OpCpf:
                case CarrotAsmTokenKind.OpGpid:
                case CarrotAsmTokenKind.OpGpfid:
                    break;
            }


            // 命令コードを追加して引数リストをクリアする
            instructionList.Add(instructionCode);
            argumentList.Clear();
            message = string.Empty;
            return true;
        }


        internal void OutputExecuteCode()
        {
        }
    }
    #endregion



    #region OpCoder base class
    /// <summary>
    /// 特定のオペコードの符号化を行う抽象クラスです
    /// </summary>
    public abstract class OpCoderBase
    {
        /// <summary>
        /// この符号器が担当するOpCodeトークン種別
        /// </summary>
        public abstract int OpCodeTokenKind { get; }


        /// <summary>
        /// この符号器が担当する命令の名前
        /// </summary>
        public abstract string OpCodeName { get; }



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
        /// <param name="arg1">第一オペランドの確認するべきトークン種別</param>
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
            if (operand[0].Kind != arg1)
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
        /// <param name="arg1">第一オペランドの確認するべきトークン種別</param>
        /// <param name="arg2">第二オペランドの確認するべきトークン種別</param>
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
            if (operand[0].Kind != arg1 || operand[1].Kind != arg2)
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
        /// <param name="arg1">第一オペランドの確認するべきトークン種別</param>
        /// <param name="arg2">第二オペランドの確認するべきトークン種別</param>
        /// <param name="arg3">第三オペランドの確認するべきトークン種別</param>
        /// <returns>オペランドの引数パターンが一致した場合は true を、一致しない場合は false を返します</returns>
        protected bool TestOperandPattern(List<Token> operand, out string message, int arg1, int arg2, int arg3)
        {
            // もし引数が3で無いなら
            if (operand.Count != 2)
            {
                // 引数は3つでなければならないエラーメッセージを設定して失敗を返す
                message = $"'{OpCodeName}'命令のオペランドは3つです";
                return false;
            }


            // 引数の型チェックに引っかかる場合は
            if (operand[0].Kind != arg1 || operand[1].Kind != arg2 || operand[2].Kind != arg3)
            {
                // 引数の型が一致しないエラーメッセージを設定して失敗を返す
                message = $"'{OpCodeName}'命令のいくつかのオペランドに指定出来ない型が含まれています";
                return false;
            }


            // 型チェックも引っかからない場合は成功を返す
            message = string.Empty;
            return true;
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
    #endregion
}