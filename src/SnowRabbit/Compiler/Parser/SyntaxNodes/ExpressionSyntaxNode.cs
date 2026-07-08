// zlib License
//
// Copyright (c) 2019 Sinoa
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
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 式構文を表す構文ノードクラスです。
    /// レジスタの取得と返却は SrCompileContext が所有するプールに対して行い、
    /// プールのリセットは文の境界（CompileAsStatement または CompileStatementExpressionValue）でのみ行われます。
    /// </summary>
    public class ExpressionSyntaxNode : SyntaxNode
    {
        /// <summary>
        /// 二項演算のコード生成処理を表すデリゲートです
        /// </summary>
        private delegate void OperationHandler(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context);

        private static readonly Dictionary<int, OperationHandler> operationTable;


        /// <summary>
        /// この式構文による結果を出力する先のレジスタインデックス
        /// </summary>
        public byte ResultRegisterIndex { get; protected set; }

        /// <summary>
        /// この式構文による結果を出力したときの型
        /// </summary>
        public SrRuntimeType ResultType { get; protected set; }


        static ExpressionSyntaxNode()
        {
            operationTable = new Dictionary<int, OperationHandler>()
            {
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
                { TokenKind.Percent, OpMod },
            };
        }


        /// <summary>
        /// ExpressionSyntaxNode クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="token">対応するトークン</param>
        public ExpressionSyntaxNode(in Token token) : base(token)
        {
        }


        #region Expression value compile utility
        /// <summary>
        /// 文の境界から式の値をコンパイルします。レジスタプールをリセットしてから式を評価し、
        /// 結果が格納されたレジスタ番号と結果の型を返します。
        /// </summary>
        /// <param name="node">コンパイルする式のノード</param>
        /// <param name="context">コンパイルコンテキスト</param>
        /// <param name="resultType">式の結果の型</param>
        /// <returns>式の結果が格納されたレジスタ番号を返します</returns>
        internal static byte CompileStatementExpressionValue(SyntaxNode node, SrCompileContext context, out SrRuntimeType resultType)
        {
            // 文をまたいで生存するレジスタ値は存在しないため、文の境界でプールをリセットする
            context.ResetRegisterPool();
            return CompileExpressionValue(node, context, out resultType);
        }


        /// <summary>
        /// 制御構文の条件式をコンパイルします。条件として使用可能な型（真偽値・整数・実数）であることを検査します。
        /// </summary>
        /// <param name="node">条件式のノード</param>
        /// <param name="context">コンパイルコンテキスト</param>
        /// <returns>条件の結果が格納されたレジスタ番号を返します</returns>
        internal static byte CompileConditionExpressionValue(SyntaxNode node, SrCompileContext context)
        {
            var conditionRegisterIndex = CompileStatementExpressionValue(node, context, out var conditionType);
            if (conditionType != SrRuntimeType.Boolean && conditionType != SrRuntimeType.Integer && conditionType != SrRuntimeType.Number)
            {
                // 条件式として評価できない型
                throw context.ErrorReporter.TypeMismatch(node.Token, SrRuntimeType.Boolean, conditionType);
            }


            return conditionRegisterIndex;
        }


        /// <summary>
        /// 式の値をコンパイルし、結果が格納されたレジスタ番号と結果の型を返します。
        /// 式の途中（ネストした部分式や引数）から呼び出す場合はレジスタプールをリセットしません。
        /// </summary>
        /// <param name="node">コンパイルする式のノード</param>
        /// <param name="context">コンパイルコンテキスト</param>
        /// <param name="resultType">式の結果の型</param>
        /// <returns>式の結果が格納されたレジスタ番号を返します</returns>
        internal static byte CompileExpressionValue(SyntaxNode node, SrCompileContext context, out SrRuntimeType resultType)
        {
            switch (node)
            {
                case LiteralSyntaxNode x: return LoadFromLiteral(x, context, out resultType);
                case IdentifierSyntaxNode x: return LoadFromIdentifier(x, context, out resultType);
                case FunctionCallSyntaxNode x: return LoadFromFunctionCall(x, context, out resultType);
            }


            if (node is ExpressionSyntaxNode expressionNode)
            {
                expressionNode.Compile(context);
                resultType = expressionNode.ResultType;
                return expressionNode.ResultRegisterIndex;
            }


            // 式として評価できないノード
            throw context.ErrorReporter.UnknownExpression(node.Token);
        }


        /// <summary>
        /// 値を対象の型へ暗黙変換できる場合は、必要な変換命令を出力して true を返します。
        /// 許可される暗黙変換は「同一型」「整数から実数への昇格」「文字列とオブジェクトの相互受け渡し」のみです。
        /// </summary>
        /// <param name="valueRegisterIndex">変換対象の値が格納されたレジスタ番号</param>
        /// <param name="fromType">値の型</param>
        /// <param name="targetType">変換先の型</param>
        /// <param name="context">コンパイルコンテキスト</param>
        /// <returns>暗黙変換が可能な場合は true を、不可能な場合は false を返します</returns>
        internal static bool TryEmitImplicitConversion(byte valueRegisterIndex, SrRuntimeType fromType, SrRuntimeType targetType, SrCompileContext context)
        {
            if (fromType == targetType) return true;


            // 整数から実数への昇格
            if (fromType == SrRuntimeType.Integer && targetType == SrRuntimeType.Number)
            {
                var instruction = new SrInstruction();
                instruction.Set(OpCode.Movitf, valueRegisterIndex, valueRegisterIndex);
                context.AddBodyCode(instruction, false);
                return true;
            }


            // 文字列とオブジェクトは相互に受け渡し可能（null リテラルはオブジェクト型のため）
            if ((fromType == SrRuntimeType.String && targetType == SrRuntimeType.Object) ||
                (fromType == SrRuntimeType.Object && targetType == SrRuntimeType.String))
            {
                return true;
            }


            return false;
        }
        #endregion


        #region Load Store control
        private static byte LoadFromLiteral(LiteralSyntaxNode literal, SrCompileContext context, out SrRuntimeType returnType)
        {
            var literalToken = literal.Token;
            var instruction = new SrInstruction();
            var targetRegisterIndex = context.TakeFreeRegisterIndex(literalToken);
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


            // 何をロードすれば良いのか不明
            throw context.ErrorReporter.UnknownLiteralType(literalToken);
        }


        private static byte LoadFromIdentifier(IdentifierSyntaxNode identifier, SrCompileContext context, out SrRuntimeType returnType)
        {
            var identifierToken = identifier.Token;
            var variableSymbol = context.AssemblyData.GetVariableSymbol(identifierToken.Text, context.CurrentCompileFunctionName);
            if (variableSymbol == null)
            {
                // 不明な識別子
                throw context.ErrorReporter.UnknownSymbol(identifierToken);
            }


            var targetRegisterIndex = context.TakeFreeRegisterIndex(identifierToken);
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

                case SrConstantSymbol constantSymbol:
                    switch (constantSymbol.Type)
                    {
                        case SrRuntimeType.Integer:
                            instruction.Set(OpCode.Movl, targetRegisterIndex, 0, 0, (int)constantSymbol.ConstantValue.Integer);
                            context.AddBodyCode(instruction, false);
                            break;

                        case SrRuntimeType.Number:
                            instruction.Set(OpCode.Movl, targetRegisterIndex, 0, 0, (float)constantSymbol.ConstantValue.Number);
                            context.AddBodyCode(instruction, false);
                            break;

                        case SrRuntimeType.Boolean:
                            var boolValue = constantSymbol.ConstantValue.Text == "true" ? 1 : 0;
                            instruction.Set(OpCode.Movl, targetRegisterIndex, 0, 0, boolValue);
                            context.AddBodyCode(instruction, false);
                            break;

                        case SrRuntimeType.String:
                            var stringSymbol = context.CreateOrGetStringSymbol(constantSymbol.ConstantValue.Text);
                            instruction.Set(OpCode.Ldrl, targetRegisterIndex, 0, 0, stringSymbol.InitialAddress);
                            context.AddBodyCode(instruction, true);
                            break;

                        default:
                            throw context.ErrorReporter.NotSupportedType(identifierToken, constantSymbol.Type);
                    }
                    break;
            }


            return targetRegisterIndex;
        }


        private static byte LoadFromFunctionCall(FunctionCallSyntaxNode functionCall, SrCompileContext context, out SrRuntimeType returnType)
        {
            var functionNameToken = functionCall.Children[0].Token;
            var functionName = functionNameToken.Text;
            var functionSymbol = context.AssemblyData.GetFunctionSymbol(functionName);
            if (functionSymbol == null)
            {
                // 未定義の関数
                throw context.ErrorReporter.UnknownSymbol(functionNameToken);
            }


            if (functionSymbol.ReturnType == SrRuntimeType.Void)
            {
                // void の関数は値として取り出せない
                throw context.ErrorReporter.NotSupporteReturnVoid(functionNameToken, functionName);
            }


            // 呼び出しの戻り値は r29 に載るため、プールから取得したレジスタへ直ちに取り込む
            functionCall.Compile(context);
            var targetRegisterIndex = context.TakeFreeRegisterIndex(functionNameToken);
            var instruction = new SrInstruction();
            instruction.Set(OpCode.Mov, targetRegisterIndex, SrvmProcessor.RegisterR29Index);
            context.AddBodyCode(instruction, false);
            returnType = functionSymbol.ReturnType;
            return targetRegisterIndex;
        }


        /// <summary>
        /// 指定された識別子の変数へレジスタの値を格納するコードを出力します
        /// </summary>
        /// <param name="identifierNode">格納先変数の識別子ノード</param>
        /// <param name="srcRegisterIndex">格納する値が入ったレジスタ番号</param>
        /// <param name="context">コンパイルコンテキスト</param>
        internal static void StoreResult(SyntaxNode identifierNode, byte srcRegisterIndex, SrCompileContext context)
        {
            if (!(identifierNode is IdentifierSyntaxNode))
            {
                throw context.ErrorReporter.InvalidIdentifier(identifierNode.Token);
            }


            var name = identifierNode.Token.Text;
            var variableSymbol = context.AssemblyData.GetVariableSymbol(name, context.CurrentCompileFunctionName);
            if (variableSymbol == null)
            {
                throw context.ErrorReporter.NotVariable(identifierNode.Token, name);
            }


            SrInstruction instruction = default;
            switch (variableSymbol)
            {
                case SrGlobalVariableSymbol globalSymbol:
                    instruction.Set(OpCode.Strl, srcRegisterIndex, 0, 0, globalSymbol.InitialAddress);
                    context.AddBodyCode(instruction, true);
                    return;

                case SrLocalVariableSymbol localSymbol:
                    instruction.Set(OpCode.Str, srcRegisterIndex, SrvmProcessor.RegisterBPIndex, 0, -localSymbol.Address);
                    context.AddBodyCode(instruction, false);
                    return;

                case SrParameterVariableSymbol parameterSymbol:
                    instruction.Set(OpCode.Str, srcRegisterIndex, SrvmProcessor.RegisterBPIndex, 0, parameterSymbol.Address + 1);
                    context.AddBodyCode(instruction, false);
                    return;
            }


            throw context.ErrorReporter.NotVariable(identifierNode.Token, name);
        }
        #endregion


        #region Main compile code
        public override void Compile(SrCompileContext context)
        {
            if (Children.Count == 0)
            {
                // 子を持たない式はリテラルまたは識別子の葉ノードのみ
                // （LoadFrom系は具象型で先に判定されるため、ここで再帰することはない）
                switch (this)
                {
                    case LiteralSyntaxNode literal:
                        ResultRegisterIndex = LoadFromLiteral(literal, context, out var literalType);
                        ResultType = literalType;
                        return;

                    case IdentifierSyntaxNode identifier:
                        ResultRegisterIndex = LoadFromIdentifier(identifier, context, out var identifierType);
                        ResultType = identifierType;
                        return;
                }


                throw context.ErrorReporter.UnknownExpression(Token);
            }


            if (Children.Count == 1)
            {
                CompileUnaryExpression(Children[0], Token, context);
                return;
            }


            if (IsAssignmentOperation(Token.Kind))
            {
                CompileAssignmentExpression(Children[0], Children[1], Token, context);
                return;
            }


            CompileBinaryExpression(Children[0], Children[1], Token, context);
        }


        private static bool IsAssignmentOperation(int tokenKind)
        {
            return
                tokenKind == TokenKind.Equal ||
                tokenKind == TokenKind.PlusEqual ||
                tokenKind == TokenKind.MinusEqual ||
                tokenKind == TokenKind.AsteriskEqual ||
                tokenKind == TokenKind.SlashEqual ||
                tokenKind == TokenKind.AndEqual ||
                tokenKind == TokenKind.VerticalbarEqual ||
                tokenKind == TokenKind.CircumflexEqual;
        }


        private void CompileUnaryExpression(SyntaxNode expression, in Token operation, SrCompileContext context)
        {
            var targetRegisterIndex = CompileExpressionValue(expression, context, out var operandType);

            var instruction = new SrInstruction();
            switch (operation.Kind)
            {
                case TokenKind.Plus:
                    if (operandType != SrRuntimeType.Integer && operandType != SrRuntimeType.Number)
                    {
                        throw context.ErrorReporter.InvalidUnaryOperation(operation, "+", operandType);
                    }
                    break;

                case TokenKind.Minus:
                    if (operandType != SrRuntimeType.Integer && operandType != SrRuntimeType.Number)
                    {
                        throw context.ErrorReporter.InvalidUnaryOperation(operation, "-", operandType);
                    }
                    instruction.Set(operandType == SrRuntimeType.Integer ? OpCode.Neg : OpCode.Fneg, targetRegisterIndex, targetRegisterIndex);
                    context.AddBodyCode(instruction, false);
                    break;

                case TokenKind.Exclamation:
                    if (operandType != SrRuntimeType.Boolean)
                    {
                        throw context.ErrorReporter.InvalidUnaryOperation(operation, "!", operandType);
                    }
                    // ゼロとの等価判定で論理値を反転する（算術否定では true(1) が -1 になり反転しない）
                    instruction.Set(OpCode.Teq, targetRegisterIndex, targetRegisterIndex, SrvmProcessor.RegisterZeroIndex);
                    context.AddBodyCode(instruction, false);
                    break;

                case TokenKind.DoublePlus:
                    if (operandType != SrRuntimeType.Integer)
                    {
                        throw context.ErrorReporter.InvalidUnaryOperation(operation, "++", operandType);
                    }
                    instruction.Set(OpCode.Addl, targetRegisterIndex, targetRegisterIndex, 0, 1);
                    context.AddBodyCode(instruction, false);
                    StoreResult(expression, targetRegisterIndex, context);
                    break;

                case TokenKind.DoubleMinus:
                    if (operandType != SrRuntimeType.Integer)
                    {
                        throw context.ErrorReporter.InvalidUnaryOperation(operation, "--", operandType);
                    }
                    instruction.Set(OpCode.Subl, targetRegisterIndex, targetRegisterIndex, 0, 1);
                    context.AddBodyCode(instruction, false);
                    StoreResult(expression, targetRegisterIndex, context);
                    break;
            }

            ResultType = operandType;
            ResultRegisterIndex = targetRegisterIndex;
        }


        private void CompileAssignmentExpression(SyntaxNode leftExpression, SyntaxNode rightExpression, in Token operation, SrCompileContext context)
        {
            // 代入先は変数の識別子であるべき
            if (!(leftExpression is IdentifierSyntaxNode))
            {
                throw context.ErrorReporter.InvalidIdentifier(leftExpression.Token);
            }


            var variableName = leftExpression.Token.Text;
            var variableSymbol = context.AssemblyData.GetVariableSymbol(variableName, context.CurrentCompileFunctionName);
            if (variableSymbol == null)
            {
                throw context.ErrorReporter.NotVariable(leftExpression.Token, variableName);
            }


            var variableType = variableSymbol.Type;
            if (operation.Kind == TokenKind.Equal)
            {
                // 単純代入は右辺のみを評価して変数の型へ暗黙変換してから格納する
                var valueRegisterIndex = CompileExpressionValue(rightExpression, context, out var valueType);
                if (!TryEmitImplicitConversion(valueRegisterIndex, valueType, variableType, context))
                {
                    throw context.ErrorReporter.InvalidCast(operation, valueType, variableType);
                }


                StoreResult(leftExpression, valueRegisterIndex, context);
                ResultRegisterIndex = valueRegisterIndex;
                ResultType = variableType;
                return;
            }


            // 複合代入は変数の現在値と右辺を評価してから演算して書き戻す
            var leftRegisterIndex = CompileExpressionValue(leftExpression, context, out _);
            var rightRegisterIndex = CompileExpressionValue(rightExpression, context, out var rightType);
            if (!TryEmitImplicitConversion(rightRegisterIndex, rightType, variableType, context))
            {
                // 変数の型へ変換できない右辺（int変数へのnumber代入などの縮小変換を含む）
                throw context.ErrorReporter.InvalidCast(operation, rightType, variableType);
            }


            var instruction = new SrInstruction();
            instruction.Set(SelectCompoundAssignmentOpCode(operation, variableType, context), leftRegisterIndex, leftRegisterIndex, rightRegisterIndex);
            context.AddBodyCode(instruction, false);
            StoreResult(leftExpression, leftRegisterIndex, context);
            context.ReleaseRegisterIndex(rightRegisterIndex);
            ResultRegisterIndex = leftRegisterIndex;
            ResultType = variableType;
        }


        private static OpCode SelectCompoundAssignmentOpCode(in Token operation, SrRuntimeType variableType, SrCompileContext context)
        {
            var isInteger = variableType == SrRuntimeType.Integer;
            var isNumber = variableType == SrRuntimeType.Number;
            switch (operation.Kind)
            {
                case TokenKind.PlusEqual:
                    if (isInteger) return OpCode.Add;
                    if (isNumber) return OpCode.Fadd;
                    break;

                case TokenKind.MinusEqual:
                    if (isInteger) return OpCode.Sub;
                    if (isNumber) return OpCode.Fsub;
                    break;

                case TokenKind.AsteriskEqual:
                    if (isInteger) return OpCode.Mul;
                    if (isNumber) return OpCode.Fmul;
                    break;

                case TokenKind.SlashEqual:
                    if (isInteger) return OpCode.Div;
                    if (isNumber) return OpCode.Fdiv;
                    break;

                case TokenKind.AndEqual:
                    if (isInteger) return OpCode.And;
                    break;

                case TokenKind.VerticalbarEqual:
                    if (isInteger) return OpCode.Or;
                    break;

                case TokenKind.CircumflexEqual:
                    if (isInteger) return OpCode.Xor;
                    break;
            }


            throw context.ErrorReporter.InvalidBinaryOperation(operation, operation.Text, variableType);
        }


        private void CompileBinaryExpression(SyntaxNode leftExpression, SyntaxNode rightExpression, in Token operation, SrCompileContext context)
        {
            ResultRegisterIndex = CompileExpressionValue(leftExpression, context, out var leftResultType);
            var rightRegisterIndex = CompileExpressionValue(rightExpression, context, out var rightResultType);
            var operationType = CompileCastExpression(ResultRegisterIndex, leftResultType, rightRegisterIndex, rightResultType, operation, context);


            if (!operationTable.TryGetValue(operation.Kind, out var operationHandler))
            {
                // 演算子として処理できないトークン
                throw context.ErrorReporter.UnknownExpression(operation);
            }


            operationHandler(operation, ResultRegisterIndex, rightRegisterIndex, operationType, context);
            context.ReleaseRegisterIndex(rightRegisterIndex);
            ResultType = operationType;


            // 比較・等価・条件演算の結果型は、オペランドの型ではなく真偽値になる
            switch (operation.Kind)
            {
                case TokenKind.DoubleEqual:
                case TokenKind.NotEqual:
                case TokenKind.OpenAngle:
                case TokenKind.CloseAngle:
                case TokenKind.LesserEqual:
                case TokenKind.GreaterEqual:
                case TokenKind.DoubleVerticalbar:
                case TokenKind.DoubleAnd:
                    ResultType = SrRuntimeType.Boolean;
                    break;
            }
        }


        private static SrRuntimeType CompileCastExpression(byte leftRegister, SrRuntimeType leftType, byte rightRegister, SrRuntimeType rightType, in Token operation, SrCompileContext context)
        {
            if (leftType == rightType) return leftType;


            var instruction = new SrInstruction();
            if (leftType == SrRuntimeType.Number && rightType == SrRuntimeType.Integer)
            {
                // 右辺の整数を実数へ昇格
                instruction.Set(OpCode.Movitf, rightRegister, rightRegister);
                context.AddBodyCode(instruction, false);
                return SrRuntimeType.Number;
            }


            if (rightType == SrRuntimeType.Number && leftType == SrRuntimeType.Integer)
            {
                // 左辺の整数を実数へ昇格
                instruction.Set(OpCode.Movitf, leftRegister, leftRegister);
                context.AddBodyCode(instruction, false);
                return SrRuntimeType.Number;
            }


            // 文字列とオブジェクトの混在はオブジェクトとして扱う（null 比較のため）
            if ((leftType == SrRuntimeType.String && rightType == SrRuntimeType.Object) ||
                (leftType == SrRuntimeType.Object && rightType == SrRuntimeType.String))
            {
                return SrRuntimeType.Object;
            }


            // それ以外の型の混在は演算できない
            throw context.ErrorReporter.InvalidCast(operation, rightType, leftType);
        }
        #endregion


        #region Operation functions
        private static void OpConditionOr(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
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


        private static void OpConditionAnd(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
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


        private static void OpLogicalOr(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (type != SrRuntimeType.Integer)
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, "|", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(OpCode.Or, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpLogicalExOr(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (type != SrRuntimeType.Integer)
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, "^", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(OpCode.Xor, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpLogicalAnd(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (type != SrRuntimeType.Integer)
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, "&", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(OpCode.And, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpEqual(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
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


        private static void OpNotEqual(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
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


        private static void OpRelationLesser(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, "<", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Tl : OpCode.Ftl, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpRelationGrater(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, ">", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Tg : OpCode.Ftg, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpRelationLesserEqual(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, "<=", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Tle : OpCode.Ftle, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpRelationGraterEqual(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, ">=", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Tge : OpCode.Ftge, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpLeftBitShift(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (type != SrRuntimeType.Integer)
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, "<<", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(OpCode.Shl, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpRightBitShift(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (type != SrRuntimeType.Integer)
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, ">>", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(OpCode.Shr, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpAdd(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, "+", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Add : OpCode.Fadd, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpSub(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, "-", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Sub : OpCode.Fsub, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpMull(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, "*", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Mul : OpCode.Fmul, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpDiv(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, "/", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Div : OpCode.Fdiv, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }


        private static void OpMod(in Token operation, byte leftRegister, byte rightRegister, SrRuntimeType type, SrCompileContext context)
        {
            if (!(type == SrRuntimeType.Integer || type == SrRuntimeType.Number))
            {
                throw context.ErrorReporter.InvalidBinaryOperation(operation, "%", type);
            }

            var instruction = new SrInstruction();
            instruction.Set(type == SrRuntimeType.Integer ? OpCode.Mod : OpCode.Fmod, leftRegister, leftRegister, rightRegister);
            context.AddBodyCode(instruction, false);
        }
        #endregion
    }
}
