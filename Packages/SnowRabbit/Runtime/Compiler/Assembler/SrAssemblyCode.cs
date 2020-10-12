// zlib/libpng License
//
// Copyright(c) 2020 Sinoa
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
using SnowRabbit.RuntimeEngine;

namespace SnowRabbit.Compiler.Assembler
{
    /// <summary>
    /// アセンブリデータに含まれるアセンブリコードを表す構造体です
    /// </summary>
    public struct SrAssemblyCode : IEquatable<SrAssemblyCode>
    {
        /// <summary>
        /// VMが解釈できる命令コードの実体
        /// </summary>
        public SrInstruction Instruction;


        /// <summary>
        /// 命令コードに含まれるアドレスはまだ未解決か否か
        /// </summary>
        public bool UnresolvedAddress;



        /// <summary>
        /// SrAssemblyCode 構造体のインスタンスを初期化します
        /// </summary>
        /// <param name="instruction">命令コード</param>
        /// <param name="unresolved">アドレスが未解決か否か</param>
        public SrAssemblyCode(in SrInstruction instruction, bool unresolved)
        {
            // そのまま受け取る
            Instruction = instruction;
            UnresolvedAddress = unresolved;
        }


        /// <summary>
        /// SrAssemblyCode の等価確認をします
        /// </summary>
        /// <param name="other">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public bool Equals(SrAssemblyCode other) => Instruction == other.Instruction && UnresolvedAddress == other.UnresolvedAddress;


        /// <summary>
        /// object の等価オーバーロードです
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns>等価の場合は true を、非等価の場合は false を返します</returns>
        public override bool Equals(object obj) => obj is SrAssemblyCode ? Equals((SrAssemblyCode)obj) : false;


        /// <summary>
        /// ハッシュコードを取得します
        /// </summary>
        /// <returns>ハッシュコードを返します</returns>
        public override int GetHashCode() => base.GetHashCode();


        /// <summary>
        /// 等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>等価の結果を返します</returns>
        public static bool operator ==(SrAssemblyCode left, SrAssemblyCode right) => left.Equals(right);


        /// <summary>
        /// 非等価演算子のオーバーロードです
        /// </summary>
        /// <param name="left">左の値</param>
        /// <param name="right">右の値</param>
        /// <returns>非等価の結果を返します</returns>
        public static bool operator !=(SrAssemblyCode left, SrAssemblyCode right) => !left.Equals(right);
    }
}
