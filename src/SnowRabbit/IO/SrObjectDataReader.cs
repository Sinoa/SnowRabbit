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

using System;
using System.IO;
using SnowRabbit.Compiler.Assembler;
using SnowRabbit.Compiler.Assembler.Symbols;
using SnowRabbit.RuntimeEngine.Data;

namespace SnowRabbit.IO
{
    /// <summary>
    /// オブジェクトファイル (SROB) を読み込むクラスです
    /// </summary>
    public class SrObjectDataReader : SrDisposable
    {
        // 各要素数の常識的な上限
        private const int MaxElementCount = 1 << 20;

        // メンバ変数定義
        private bool disposed;
        private readonly SrBinaryIO binaryIO;



        /// <summary>
        /// SrObjectDataReader クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="stream">読み込み元のストリーム</param>
        /// <exception cref="ArgumentNullException">stream が null です</exception>
        /// <exception cref="ArgumentException">stream に読み込みが許可されていません</exception>
        public SrObjectDataReader(Stream stream)
        {
            if (!(stream ?? throw new ArgumentNullException(nameof(stream))).CanRead)
            {
                throw new ArgumentException("stream に読み込みが許可されていません");
            }


            binaryIO = new SrBinaryIO(stream);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposed) return;


            if (disposing)
            {
                binaryIO.Dispose();
            }


            disposed = true;
            base.Dispose(disposing);
        }


        /// <summary>
        /// オブジェクトファイルを読み込みます
        /// </summary>
        /// <returns>読み込まれたオブジェクトデータを返します</returns>
        /// <exception cref="SrMalformedObjectDataException">オブジェクトファイルの形式が不正です</exception>
        public SrObjectData Read()
        {
            try
            {
                return ReadCore();
            }
            catch (EndOfStreamException error)
            {
                throw new SrMalformedObjectDataException("オブジェクトファイルが途中で切り詰められています", error);
            }
            catch (InvalidDataException error)
            {
                throw new SrMalformedObjectDataException("オブジェクトファイルに不正な値が含まれています", error);
            }
        }


        private SrObjectData ReadCore()
        {
            var magicNumber = binaryIO.ReadUInt();
            if (magicNumber == SrExecutableData.MagicNumber)
            {
                // 実行形式を誤ってリンクしようとした場合は専用のメッセージで伝える
                throw new SrMalformedObjectDataException("実行形式 (SROF) はリンクできません。-c オプションで生成したオブジェクトファイル (.sro) を指定してください");
            }
            if (magicNumber != SrObjectData.MagicNumber)
            {
                throw new SrMalformedObjectDataException("マジックナンバーが一致しません。SnowRabbit のオブジェクトファイル (SROB) ではありません");
            }


            var version = binaryIO.ReadInt();
            if (version != SrObjectData.FormatVersion)
            {
                throw new SrMalformedObjectDataException($"オブジェクトファイルのバージョン '{version}' はサポートされていません（サポートバージョン: {SrObjectData.FormatVersion}）");
            }


            var symbolCount = binaryIO.ReadInt();
            var stringCount = binaryIO.ReadInt();
            var functionCodeCount = binaryIO.ReadInt();
            if (symbolCount < 0 || symbolCount > MaxElementCount) throw new SrMalformedObjectDataException($"シンボル数 '{symbolCount}' が不正です");
            if (stringCount < 0 || stringCount > MaxElementCount) throw new SrMalformedObjectDataException($"文字列数 '{stringCount}' が不正です");
            if (functionCodeCount < 0 || functionCodeCount > MaxElementCount) throw new SrMalformedObjectDataException($"関数コード数 '{functionCodeCount}' が不正です");


            var objectData = new SrObjectData();
            for (int i = 0; i < symbolCount; ++i)
            {
                objectData.Symbols.Add(ReadSymbol());
            }


            for (int i = 0; i < stringCount; ++i)
            {
                var stringData = new SrObjectStringData();
                stringData.Text = binaryIO.ReadString();
                stringData.InitialAddress = binaryIO.ReadInt();
                objectData.Strings.Add(stringData);
            }


            for (int i = 0; i < functionCodeCount; ++i)
            {
                var functionCode = new SrObjectFunctionCodeData();
                functionCode.FunctionName = binaryIO.ReadString();
                var codeLength = binaryIO.ReadInt();
                if (codeLength < 0 || codeLength > MaxElementCount)
                {
                    throw new SrMalformedObjectDataException($"関数 '{functionCode.FunctionName}' のコード長 '{codeLength}' が不正です");
                }
                for (int codeIndex = 0; codeIndex < codeLength; ++codeIndex)
                {
                    var instructionData = new SrObjectInstructionData();
                    instructionData.Raw = binaryIO.ReadULong();
                    instructionData.UnresolvedAddress = binaryIO.ReadByte() != 0;
                    functionCode.Codes.Add(instructionData);
                }
                objectData.FunctionCodes.Add(functionCode);
            }


            return objectData;
        }


        private SrObjectSymbolData ReadSymbol()
        {
            var symbol = new SrObjectSymbolData();
            symbol.Kind = (SrSymbolKind)binaryIO.ReadInt();
            symbol.Name = binaryIO.ReadString();
            symbol.InitialAddress = binaryIO.ReadInt();


            switch (symbol.Kind)
            {
                case SrSymbolKind.PeripheralFunction:
                    symbol.Type = (SrRuntimeType)binaryIO.ReadInt();
                    symbol.PeripheralName = binaryIO.ReadString();
                    symbol.PeripheralFunctionName = binaryIO.ReadString();
                    ReadTypedNames(symbol.Parameters);
                    return symbol;

                case SrSymbolKind.ScriptFunction:
                    symbol.Type = (SrRuntimeType)binaryIO.ReadInt();
                    ReadTypedNames(symbol.Parameters);
                    ReadTypedNames(symbol.LocalVariables);
                    var usedRegisterCount = binaryIO.ReadInt();
                    if (usedRegisterCount < 0 || usedRegisterCount > 32)
                    {
                        throw new SrMalformedObjectDataException($"関数 '{symbol.Name}' の使用レジスタ数 '{usedRegisterCount}' が不正です");
                    }
                    for (int i = 0; i < usedRegisterCount; ++i)
                    {
                        symbol.UsedRegisters.Add(binaryIO.ReadByte());
                    }
                    return symbol;

                case SrSymbolKind.GlobalVariable:
                case SrSymbolKind.Constant:
                    symbol.Type = (SrRuntimeType)binaryIO.ReadInt();
                    ReadLiteral(symbol);
                    return symbol;

                case SrSymbolKind.Label:
                    symbol.FunctionName = binaryIO.ReadString();
                    symbol.LabelAddress = binaryIO.ReadInt();
                    return symbol;
            }


            throw new SrMalformedObjectDataException($"不明なシンボル種別 '{symbol.Kind}' です");
        }


        private void ReadTypedNames(System.Collections.Generic.List<SrObjectTypedNameData> list)
        {
            var count = binaryIO.ReadInt();
            if (count < 0 || count > MaxElementCount)
            {
                throw new SrMalformedObjectDataException($"要素数 '{count}' が不正です");
            }


            for (int i = 0; i < count; ++i)
            {
                var typedName = new SrObjectTypedNameData();
                typedName.Name = binaryIO.ReadString();
                typedName.Type = (SrRuntimeType)binaryIO.ReadInt();
                list.Add(typedName);
            }
        }


        private void ReadLiteral(SrObjectSymbolData symbol)
        {
            symbol.HasLiteral = binaryIO.ReadByte() != 0;
            if (!symbol.HasLiteral) return;


            symbol.LiteralKind = binaryIO.ReadInt();
            symbol.LiteralText = binaryIO.ReadString();
            symbol.LiteralInteger = binaryIO.ReadLong();
            symbol.LiteralNumber = binaryIO.ReadDouble();
        }
    }
}
