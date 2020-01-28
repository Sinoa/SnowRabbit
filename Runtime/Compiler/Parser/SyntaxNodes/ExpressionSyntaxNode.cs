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
using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 式構文を表す構文ノードクラスです
    /// </summary>
    public class ExpressionSyntaxNode : SyntaxNode
    {
        private static readonly Dictionary<int, Action<SyntaxNode, byte, SyntaxNode, byte, SrRuntimeType, SrCompileContext>> operationTable;

        // メンバ変数定義
        private HashSet<byte> usedRegisterIndexs;
        private Stack<byte> freeRegisterStack;



        /// <summary>
        /// この式構文による結果を出力する先のレジスタインデックス
        /// </summary>
        public byte ResultRegisterIndex { get; private set; }


        /// <summary>
        /// この式構文による結果を出力したときの型
        /// </summary>
        public SrRuntimeType ResultType { get; private set; }



        static ExpressionSyntaxNode()
        {
            operationTable = new Dictionary<int, Action<SyntaxNode, byte, SyntaxNode, byte, SrRuntimeType, SrCompileContext>>()
            {
                { TokenKind.Equal, OpAssignment },
                { TokenKind.PlusEqual, OpPlusAssignment },
                { TokenKind.MinusEqual, OpMinusAssignment },
                { TokenKind.AsteriskEqual, OpMullAssignment },
                { TokenKind.SlashEqual, OpDivAssignment },
                { TokenKind.AndEqual, OpAndAssignment },
                { TokenKind.VerticalbarEqual, OpOrAssignment },
                { TokenKind.CircumflexEqual, OpExOrAssignment },
                { TokenKind.DoubleVerticalbar, OpConditionOr },
                { TokenKind.DoubleAnd, OpConditionAnd },
                { TokenKind.Verticalbar, OpLogicalOr },
                { TokenKind.Circumflex, OpLogicalExOr },
                { TokenKind.And, OpLogicalAnd },
                { TokenKind.DoubleEqual, OpEqual },
                { TokenKind.NotEqual, OpNotEqual },
                { TokenKind.OpenAngle, OpRelationLesser },
                { TokenKind.CloseAngle, OpRelationGrater },
                { TokenKind.LesserEqual, OpRelationLesserEqual },
                { TokenKind.GreaterEqual, OpRelationGraterEqual },
                { TokenKind.DoubleOpenAngle, OpLeftBitShift },
                { TokenKind.DoubleCloseAngle, OpRightBitShift },
                { TokenKind.Plus, OpAdd },
                { TokenKind.Minus, OpSub },
                { TokenKind.Asterisk, OpMull },
                { TokenKind.Slash, OpDiv },
            };
        }


        /// <summary>
        /// ExpressionSyntaxNode クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="token">対応するトークン</param>
        public ExpressionSyntaxNode(in Token token) : base(token)
        {
        }


        #region Register control utility
        private static Stack<byte> CreateFreeRegisterStack()
        {
            return new Stack<byte>(new byte[]
            {
                SrvmProcessor.RegisterR28Index,
                SrvmProcessor.RegisterR27Index,
                SrvmProcessor.RegisterR26Index,
                SrvmProcessor.RegisterR25Index,
                SrvmProcessor.RegisterR24Index,
                SrvmProcessor.RegisterR23Index,
                SrvmProcessor.RegisterR22Index,
                SrvmProcessor.RegisterR21Index,
                SrvmProcessor.RegisterR20Index,
                SrvmProcessor.RegisterR19Index,
                SrvmProcessor.RegisterR18Index,
                SrvmProcessor.RegisterR17Index,
                SrvmProcessor.RegisterR16Index,
                SrvmProcessor.RegisterR15Index,
                SrvmProcessor.RegisterR14Index,
                SrvmProcessor.RegisterR13Index,
                SrvmProcessor.RegisterR12Index,
                SrvmProcessor.RegisterR11Index,
                SrvmProcessor.RegisterR10Index,
                SrvmProcessor.RegisterR9Index,
                SrvmProcessor.RegisterR8Index,
                SrvmProcessor.RegisterDIndex,
                SrvmProcessor.RegisterCIndex,
                SrvmProcessor.RegisterBIndex,
                SrvmProcessor.RegisterAIndex,
            });
        }


        private void InitializeRegisterInformation()
        {
            if (Parent is ExpressionSyntaxNode parentExpression)
            {
                usedRegisterIndexs = parentExpression.usedRegisterIndexs;
                freeRegisterStack = parentExpression.freeRegisterStack;
                return;
            }


            usedRegisterIndexs = new HashSet<byte>();
            freeRegisterStack = CreateFreeRegisterStack();
        }


        private void FinalizeRegisterInformation(SrCompileContext context)
        {
            if (Parent is ExpressionSyntaxNode) return;
            var functionSymbol = context.AssemblyData.GetFunctionSymbol(context.CurrentCompileFunctionName);
            functionSymbol.UsedRegisterSet.UnionWith(usedRegisterIndexs);
        }


        private byte TakeFreeRegisterIndex()
        {
            if (freeRegisterStack.Count == 0)
            {
                // 空きレジスタがもうない
                throw new System.Exception();
            }


            var freeRegisterIndex = freeRegisterStack.Pop();
            usedRegisterIndexs.Add(freeRegisterIndex);
            return freeRegisterIndex;
        }


        private void ReleaseRegister(byte registerIndex)
        {
            freeRegisterStack.Push(registerIndex);
        }
        #endregion


        #region Load Store control
        private byte LoadFromExpression(SyntaxNode node, SrCompileContext context, out SrRuntimeType returnType)
        {
            switch (node)
            {
                case LiteralSyntaxNode x: return LoadFromLiteral(x, context, out returnType);
                case IdentifierSyntaxNode x: return LoadFromIdentifier(x, context, out returnType);
                case FunctionCallSyntaxNode x: return LoadFromFunctionCall(x, context, out returnType);
            }


            if (!(node is ExpressionSyntaxNode))
            {
                // 式ですら無いなら値の読み取りのしようがない
                throw new System.Exception();
            }


            node.Compile(context);
            returnType = ((ExpressionSyntaxNode)node).ResultType;
            return ((ExpressionSyntaxNode)node).ResultRegisterIndex;
        }


        private byte LoadFromLiteral(LiteralSyntaxNode literal, SrCompileContext context, out SrRuntimeType returnType)
        {
            var literalToken = literal.Token;
            var instruction = new SrInstruction();
            var targetRegisterIndex = TakeFreeRegisterIndex();
            switch (context.ToRuntimeType(literalToken.Kind))
            {
                case SrRuntimeType.Integer:
                    returnType = SrRuntimeType.Integer;
                    instruction.Set(OpCode.Movl, targetRegisterIndex, 0, 0, (int)literalToken.Integer);
                    context.AddBodyCode(instruction, false);
                    return targetRegisterIndex;


                case SrRuntimeType.Number:
                    returnType = SrRuntimeType.Number;
                    instruction.Set(OpCode.Movl, targetRegisterIndex, 0, 0, (float)literalToken.Number);
                    context.AddBodyCode(instruction, false);
                    return targetRegisterIndex;


                case SrRuntimeType.Boolean:
                    returnType = SrRuntimeType.Boolean;
                    var boolValue = literalToken.Text == "true" ? 1 : 0;
                    instruction.Set(OpCode.Movl, targetRegisterIndex, 0, 0, boolValue);
                    context.AddBodyCode(instruction, false);
                    return targetRegisterIndex;


                case SrRuntimeType.String:
                    returnType = SrRuntimeType.String;
                    var symbol = context.CreateOrGetStringSymbol(literalToken.Text);
                    instruction.Set(OpCode.Ldrl, targetRegisterIndex, 0, 0, symbol.InitialAddress);
                    context.AddBodyCode(instruction, true);
                    return targetRegisterIndex;


                case SrRuntimeType.Object:
                    returnType = SrRuntimeType.Object;
                    instruction.Set(OpCode.Movl, targetRegisterIndex, 0, 0, 0);
                    context.AddBodyCode(instruction, false);
                    return targetRegisterIndex;
            }


            // 何を処理すれば良いのか
            throw new System.Exception();
        }


        private byte LoadFromIdentifier(IdentifierSyntaxNode identifier, SrCompileContext context, out SrRuntimeType returnType)
        {
            var targetRegisterIndex = TakeFreeRegisterIndex();
            var identifierToken = identifier.Token;
            var variableSymbol = context.AssemblyData.GetVariableSymbol(identifierToken.Text, context.CurrentCompileFunctionName);
            if (variableSymbol == null)
            {
                // 不明な識別子
                throw new System.Exception();
            }


            returnType = variableSymbol.Type;
            var instruction = new SrInstruction();
            switch (variableSymbol)
            {
                case SrGlobalVariableSymbol globalVariableSymbol:
                    instruction.Set(OpCode.Ldrl, targetRegisterIndex, 0, 0, globalVariableSymbol.InitialAddress);
                    context.AddBodyCode(instruction, true);
                    break;


                case SrLocalVariableSymbol localVariableSymbol:
                    instruction.Set(OpCode.Ldr, targetRegisterIndex, SrvmProcessor.RegisterBPIndex, 0, -localVariableSymbol.Address);
                    context.AddBodyCode(instruction, false);
                    break;


                case SrParameterVariableSymbol parameterVariableSymbol:
                    instruction.Set(OpCode.Ldr, targetRegisterIndex, SrvmProcessor.RegisterBPIndex, 0, parameterVariableSymbol.Address + 1);
                    context.AddBodyCode(instruction, false);
                    break;
            }


            return targetRegisterIndex;
        }


        private byte LoadFromFunctionCall(FunctionCallSyntaxNode functionCall, SrCompileContext context, out SrRuntimeType returnType)
        {
            var functionSymbol = context.AssemblyData.GetFunctionSymbol(functionCall.Children[0].Token.Text);
            if (functionSymbol == null)
            {
                // 未定義の関数
                throw new System.Exception();
            }


            if (functionSymbol.ReturnType == SrRuntimeType.Void)
            {
                // void の関数は値として取り出せない
                throw new System.Exception();
            }


            functionCall.Compile(context);
            var targetRegisterIndex = TakeFreeRegisterIndex();
            var instruction = new SrInstruction();
            instruction.Set(OpCode.Mov, targetRegisterIndex, SrvmProcessor.RegisterR29Index);
            context.AddBodyCode(instruction, false);
            returnType = functionSymbol.ReturnType;
            return 0;
        }
        #endregion


        #region Main compile code
        public override void Compile(SrCompileContext context)
        {
            InitializeRegisterInformation();


            if (Children.Count == 1)
            {
                CompileUnaryExpression(Children[0], Token, context);
            }
            else if (Children.Count == 2)
            {
                CompileExpression(Children[0], Children[1], Token, context);
            }


            FinalizeRegisterInformation(context);
        }


        private void CompileUnaryExpression(SyntaxNode expression, in Token operation, SrCompileContext context)
        {
            var targetRegisterIndex = LoadFromExpression(expression, context, out var returnType);


            if (returnType == SrRuntimeType.String || returnType == SrRuntimeType.Object || returnType == SrRuntimeType.Void)
            {
                // 文字列 オブジェクト void に対する単項式は今の所未実装
                throw new System.Exception();
            }



            var instruction = new SrInstruction();
            switch (operation.Kind)
            {
                case TokenKind.Plus:
                    break;


                case TokenKind.Minus:
                    instruction.Set(OpCode.Neg, targetRegisterIndex, targetRegisterIndex);
                    context.AddBodyCode(instruction, false);
                    break;


                case TokenKind.Exclamation:
                    if (returnType != SrRuntimeType.Boolean)
                    {
                        // 論理反転は今の所booleanのみ対応
                        throw new System.Exception();
                    }
                    instruction.Set(OpCode.Neg, targetRegisterIndex, targetRegisterIndex);
                    context.AddBodyCode(instruction, false);
                    break;


                case TokenKind.DoublePlus:
                    if (returnType != SrRuntimeType.Integer)
                    {
                        // インクリメント操作は整数のみ対応
                        throw new System.Exception();
                    }
                    instruction.Set(OpCode.Addl, targetRegisterIndex, targetRegisterIndex, 0, 1);
                    context.AddBodyCode(instruction, false);
                    break;


                case TokenKind.DoubleMinus:
                    if (returnType != SrRuntimeType.Integer)
                    {
                        // デクリメント操作は整数のみ対応
                        throw new System.Exception();
                    }
                    instruction.Set(OpCode.Subl, targetRegisterIndex, targetRegisterIndex, 0, 1);
                    context.AddBodyCode(instruction, false);
                    break;
            }
        }


        private void CompileExpression(SyntaxNode leftExpression, SyntaxNode rightExpression, in Token operation, SrCompileContext context)
        {
            ResultRegisterIndex = LoadFromExpression(leftExpression, context, out var leftResultType);
            var rightRegisterIndex = LoadFromExpression(rightExpression, context, out var rightResultType);
            ResultType = CompileCastExpression(ResultRegisterIndex, leftResultType, rightRegisterIndex, rightResultType, context);


            operationTable[operation.Kind](leftExpression, ResultRegisterIndex, rightExpression, rightRegisterIndex, ResultType, context);
            ReleaseRegister(rightRegisterIndex);
        }


        private SrRuntimeType CompileCastExpression(byte leftRegister, SrRuntimeType leftType, byte rightRegister, SrRuntimeType rightType, SrCompileContext context)
        {
            var instruction = new SrInstruction();


            if (leftType != rightType)
            {
                if (leftType == SrRuntimeType.Number)
                {
                    if (rightType != SrRuntimeType.Integer)
                    {
                        // 整数以外は実数キャストは出来ない
                        throw new System.Exception();
                    }


                    instruction.Set(OpCode.Movitf, rightRegister, rightRegister);
                    context.AddBodyCode(instruction, false);
                    return SrRuntimeType.Number;
                }


                if (rightType == SrRuntimeType.Number)
                {
                    if (leftType != SrRuntimeType.Integer)
                    {
                        // 整数以外は実数キャストは出来ない
                        throw new System.Exception();
                    }


                    instruction.Set(OpCode.Movitf, leftRegister, leftRegister);
                    context.AddBodyCode(instruction, false);
                    return SrRuntimeType.Number;
                }
            }


            return leftType;
        }
        #endregion


        #region Operation functions
        private static void OpAssignment(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            var instruction = new SrInstruction();
            instruction.Set(OpCode.Mov, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpPlusAssignment(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                // 現在は整数または実数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Add : OpCode.Fadd, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpMinusAssignment(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                // 現在は整数または実数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Sub : OpCode.Fsub, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpMullAssignment(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                // 現在は整数または実数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Mul : OpCode.Fmul, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpDivAssignment(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                // 現在は整数または実数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Div : OpCode.Fdiv, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpAndAssignment(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (type != SrRuntimeType.Integer)
            {
                // 現在は整数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(OpCode.And, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpOrAssignment(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (type != SrRuntimeType.Integer)
            {
                // 現在は整数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(OpCode.Or, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpExOrAssignment(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (type != SrRuntimeType.Integer)
            {
                // 現在は整数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(OpCode.Xor, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpConditionOr(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            var instruction = new SrInstruction();
            if (type == SrRuntimeType.Object || type == SrRuntimeType.String)
            {
                instruction.Set(OpCode.Tonnull, leftRegister, leftRegister);
                context.AddBodyCode(instruction, false);
                instruction.Set(OpCode.Tonnull, rightRegister, rightRegister);
                context.AddBodyCode(instruction, false);
            }
            else
            {
                instruction.Set(OpCode.Tne, leftRegister, leftRegister, SrvmProcessor.RegisterZeroIndex);
                context.AddBodyCode(instruction, false);
                instruction.Set(OpCode.Tne, rightRegister, rightRegister, SrvmProcessor.RegisterZeroIndex);
                context.AddBodyCode(instruction, false);
            }


            instruction.Set(OpCode.Or, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpConditionAnd(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            var instruction = new SrInstruction();
            if (type == SrRuntimeType.Object || type == SrRuntimeType.String)
            {
                instruction.Set(OpCode.Tonnull, leftRegister, leftRegister);
                context.AddBodyCode(instruction, false);
                instruction.Set(OpCode.Tonnull, rightRegister, rightRegister);
                context.AddBodyCode(instruction, false);
            }
            else
            {
                instruction.Set(OpCode.Tne, leftRegister, leftRegister, SrvmProcessor.RegisterZeroIndex);
                context.AddBodyCode(instruction, false);
                instruction.Set(OpCode.Tne, rightRegister, rightRegister, SrvmProcessor.RegisterZeroIndex);
                context.AddBodyCode(instruction, false);
            }


            instruction.Set(OpCode.And, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpLogicalOr(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (type != SrRuntimeType.Integer)
            {
                // 現在は整数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(OpCode.Or, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpLogicalExOr(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (type != SrRuntimeType.Integer)
            {
                // 現在は整数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(OpCode.Xor, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpLogicalAnd(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (type != SrRuntimeType.Integer)
            {
                // 現在は整数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(OpCode.And, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpEqual(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            var instruction = new SrInstruction();
            if (type == SrRuntimeType.Object || type == SrRuntimeType.String)
            {
                instruction.Set(OpCode.Toeq, leftRegister, leftRegister, rightRegister);
                context.AddBodyCode(instruction, false);
            }
            else
            {
                instruction.Set(OpCode.Teq, leftRegister, leftRegister, rightRegister);
                context.AddBodyCode(instruction, false);
            }
        }


        private static void OpNotEqual(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            var instruction = new SrInstruction();
            if (type == SrRuntimeType.Object || type == SrRuntimeType.String)
            {
                instruction.Set(OpCode.Tone, leftRegister, leftRegister, rightRegister);
                context.AddBodyCode(instruction, false);
            }
            else
            {
                instruction.Set(OpCode.Tne, leftRegister, leftRegister, rightRegister);
                context.AddBodyCode(instruction, false);
            }
        }


        private static void OpRelationLesser(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                // 現在は整数または実数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Tl : OpCode.Ftl, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpRelationGrater(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                // 現在は整数または実数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Tg : OpCode.Ftg, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpRelationLesserEqual(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                // 現在は整数または実数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Tle : OpCode.Ftle, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpRelationGraterEqual(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                // 現在は整数または実数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Tge : OpCode.Ftge, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpLeftBitShift(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (type != SrRuntimeType.Integer)
            {
                // 現在は整数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(OpCode.Shl, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpRightBitShift(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (type != SrRuntimeType.Integer)
            {
                // 現在は整数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(OpCode.Shr, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpAdd(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                // 現在は整数または実数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Add : OpCode.Fadd, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpSub(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                // 現在は整数または実数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Sub : OpCode.Fsub, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpMull(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                // 現在は整数または実数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Mul : OpCode.Fmul, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpDiv(SyntaxNode leftSyntaxNode, byte leftRegister, SyntaxNode rightSyntaxNode, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                // 現在は整数または実数のみ対応
                throw new Exception();
            }


            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Div : OpCode.Fdiv, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }
        #endregion
    }
}