// zlib License
// 
// Copyright (c) 2020 Sinoa
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

using SnowRabbit.Compiler.Assembler.Symbols;

namespace SnowRabbit.Compiler.Parser.SyntaxNodes
{
    /// <summary>
    /// 周辺機器関数宣言構文を表す構文ノードクラスです
    /// </summary>
    public class PeripheralDeclareSyntaxNode : SyntaxNode
    {
        public override void Compile(SrCompileContext context)
        {
            var functionName = Children[0].Token.Text;
            var returnType = context.ToRuntimeType(Children[1].Token.Kind);
            var peripheralName = Children[2].Token.Text;
            var peripheralFuncName = Children[3].Token.Text;


            // 宣言されたパラメータ型の一覧を先に確定させる（既存宣言との照合と登録の両方に使用する）
            var parameterTypes = new System.Collections.Generic.List<SrRuntimeType>();
            if (Children[4] != null)
            {
                var typeList = (TypeListSyntaxNode)Children[4];
                foreach (var type in typeList.Children)
                {
                    var runtimeType = context.ToRuntimeType(type.Token.Kind);
                    if (!IsSupportType(runtimeType))
                    {
                        // 未サポートの型
                        throw context.ErrorReporter.NotSupportedType(type.Token, runtimeType);
                    }


                    parameterTypes.Add(runtimeType);
                }
            }


            var existingSymbol = context.AssemblyData.GetGlobalSymbol(functionName);
            if (existingSymbol != null)
            {
                // 同一シグネチャのペリフェラル宣言の再宣言は何もしない
                // （#link したオブジェクトや #compile したスクリプトが同じ using を持つケースを許容するため）
                if (existingSymbol is SrPeripheralFunctionSymbol existingPeripheral &&
                    existingPeripheral.SignatureEquals(returnType, peripheralName, peripheralFuncName, parameterTypes))
                {
                    return;
                }


                // 既に使用されている名前
                throw context.ErrorReporter.PredefinedSymbol(Children[0].Token, functionName);
            }


            context.CreateOrGetStringSymbol(peripheralName);
            context.CreateOrGetStringSymbol(peripheralFuncName);
            var symbol = context.CreatePeripheralFunctionSymbol(returnType, functionName, peripheralName, peripheralFuncName);
            context.CreateGlobalVariableSymbol(SrRuntimeType.Object, symbol.PeripheralGlobalVariableName, default);
            int typeCount = 0;
            foreach (var parameterType in parameterTypes)
            {
                symbol.AddOrGetParameter(typeCount++.ToString(), parameterType);
            }
        }


        private bool IsSupportType(SrRuntimeType type)
        {
            return
                type == SrRuntimeType.Integer ||
                type == SrRuntimeType.Number ||
                type == SrRuntimeType.String ||
                type == SrRuntimeType.Object ||
                type == SrRuntimeType.Boolean;
        }
    }
}
