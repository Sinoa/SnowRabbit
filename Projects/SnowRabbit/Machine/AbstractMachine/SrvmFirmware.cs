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
using System.Text;
using SnowRabbit.Runtime;

namespace SnowRabbit.Machine
{
    /// <summary>
    /// 仮想マシンが実装する仮想マシンファームウェアの抽象クラスです
    /// </summary>
    public abstract class SrvmFirmware : SrvmMachineParts
    {
        // 定数定義
        private const int ReadBufferSize = 1 << 10;
        private const byte CarrotObjectFormatSignature0 = 0xCC;
        private const byte CarrotObjectFormatSignature1 = 0xEE;
        private const byte CarrotObjectFormatSignature2 = 0x11;
        private const byte CarrotObjectFormatSignature3 = 0xFF;

        // メンバ変数定義
        private bool disposed;
        private int nextPeripheralID;
        private Dictionary<string, int> peripheralIDTable;
        private Dictionary<int, SrvmPeripheral> peripheralTable;
        private int nextProcessID;
        private byte[] readBuffer; // Span<byte> readBuffer = stackalloc byte[n];が目標（UnityがSpan<T>対応してくれれば考える）
        private Encoding encoding;



        #region Constructor and Dispose
        /// <summary>
        /// SrvmFirmware のインスタンスを初期化します
        /// </summary>
        public SrvmFirmware()
        {
            // 読み込みバッファを予め生成しておく（可能ならばスタック上に確保したいがUnity側都合などで一旦フィールドに置いてしまう）
            readBuffer = new byte[ReadBufferSize];


            // BOM無しUTF-8エンコーディングを生成しておく
            encoding = new UTF8Encoding(false);


            // 次の決定された周辺機器ID
            peripheralIDTable = new Dictionary<string, int>();
            peripheralTable = new Dictionary<int, SrvmPeripheral>();
            nextPeripheralID = 0;


            // 次に生成するべきプロセスIDは1から
            nextProcessID = 1;
        }


        /// <summary>
        /// インスタンスのリソースを解放します
        /// </summary>
        /// <param name="disposing">マネージドを含む解放の場合は true を、アンマネージドのみの場合は false を指定</param>
        protected override void Dispose(bool disposing)
        {
            // 既に解放済みなら
            if (disposed)
            {
                // 終了
                return;
            }


            // マネージ解放なら
            if (disposing)
            {
                // 周辺機器の数だけ回る
                foreach (var peripheralRecord in peripheralTable)
                {
                    // 周辺機器の解放も呼び出す
                    peripheralRecord.Value.Dispose();
                }


                // 周辺機器テーブルをクリアする
                peripheralTable.Clear();
                peripheralIDTable.Clear();
            }


            // 解放済みマーク
            disposed = true;


            // 基本クラスのDisposeも呼ぶ
            base.Dispose(disposing);
        }
        #endregion


        #region Program load functions
        /// <summary>
        /// 指定されたプログラムパスからプロセスを生成します
        /// </summary>
        /// <param name="programPath">プロセスを生成する元になるプログラムのパス</param>
        /// <param name="process">生成されたプロセスを設定する</param>
        internal void CreateProcess(string programPath, out SrProcess process)
        {
            throw new NotImplementedException();
        }


        internal void TerminateProcess(ref SrProcess process)
        {
        }


        /// <summary>
        /// 指定されたプロセスにプログラムをロードします
        /// </summary>
        /// <param name="programPath">ロードするプログラム</param>
        /// <param name="process">ロードする先のプロセス</param>
        /// <exception cref="SrProgramNotFoundException">指定されたパス '{path}' のプログラムを見つけられませんでした</exception>
        internal void LoadProgram(string programPath, ref SrProcess process)
        {
            // ストレージからプログラムのリソースをオープンする
            var programStream = Machine.Storage.Open(programPath);
            if (programStream == null)
            {
                // プログラムを見つけられなかった例外を吐く
                throw new SrProgramNotFoundException(programPath);
            }


            try
            {
                // ストリームによるプログラムロードを行う
                LoadProgram(programStream, ref process);
            }
            catch
            {
                // 例外が起きたら再スロー
                throw;
            }
            finally
            {
                // ストリームを閉じる
                Machine.Storage.Close(programStream);
            }
        }


        /// <summary>
        /// 指定されたプロセスにプログラムをロードします
        /// </summary>
        /// <param name="programData">ロードするプログラムのバイト配列</param>
        /// <param name="process">ロードする先のプロセス</param>
        /// <exception cref="ArgumentNullException">programData が null です</exception>
        internal void LoadProgram(byte[] programData, ref SrProcess process)
        {
            // バイト配列からメモリストリームを生成する
            using (var programStream = new MemoryStream(programData ?? throw new ArgumentNullException(nameof(programData))))
            {
                // ストリームによるプログラムロードを行う
                LoadProgram(programStream, ref process);
            }
        }


        /// <summary>
        /// 指定されたプロセスにプログラムをロードします
        /// </summary>
        /// <param name="programStream">ロードするプログラムのストリーム</param>
        /// <param name="process">ロードする先のプロセス</param>
        /// <exception cref="ArgumentNullException">programStream が null です</exception>
        internal void LoadProgram(Stream programStream, ref SrProcess process)
        {
            // シグネチャをチェックして不一致なら
            if (!CheckMagicSignature(programStream ?? throw new ArgumentNullException(nameof(programStream))))
            {
                // 例外を吐く
                throw new InvalidOperationException("プログラムのシグネチャが不正です");
            }


            // プログラムコードのサイズ（命令数）、大域変数の数、最低確保オブジェクト数を読み込む
            var instructionNumber = ReadInt(programStream);
            var globalVariableNumber = ReadInt(programStream);
            var minimumObjectNumber = ReadInt(programStream);
            var constStringNumber = ReadInt(programStream);


            // プロセスに必要なサイズとオブジェクトメモリに必要なサイズを計算する
            var processMemorySize = (instructionNumber + minimumObjectNumber) * 8 + globalVariableNumber + (4 << 10);
            var objectMemorySize = minimumObjectNumber + constStringNumber + 100;


            // メモリを確保する
            process.ProcessMemory = Machine.Memory.AllocateValue(processMemorySize, AllocationType.Process);
            process.ObjectMemory = Machine.Memory.AllocateObject(objectMemorySize, AllocationType.Process);


            // プログラムコードを読み込む
            ReadProgramCode(programStream, ref process.ProcessMemory, instructionNumber);
            ReadConstStringData(programStream, ref process.ObjectMemory, constStringNumber);
        }


        /// <summary>
        /// 対象のストリームから Carrot Object Format である証明をするためのシグネチャをチェックします
        /// TODO:BinaryReader/Writerのようにリトルエンディアンのみではなくビッグエンディアンにも対応した読み書きクラスを実装する
        /// </summary>
        /// <param name="stream">開かれたプログラムストリーム</param>
        /// <returns>シグネチャが正しく一致した場合は true を、一致しなかった場合は false を返します</returns>
        /// <exception cref="EndOfStreamException"></exception>
        private bool CheckMagicSignature(Stream stream)
        {
            // 4byte分ロードするが4byteも読み込めていなかったら
            var readSize = stream.Read(readBuffer, 0, 4);
            if (readSize < 4)
            {
                // 末尾に達してしまった
                throw new EndOfStreamException();
            }


            // 読み取ったデータに、1つでもシグネチャと一致しないのなら
            if (readBuffer[0] != CarrotObjectFormatSignature0 || readBuffer[1] != CarrotObjectFormatSignature1 ||
                readBuffer[2] != CarrotObjectFormatSignature2 || readBuffer[3] != CarrotObjectFormatSignature3)
            {
                // シグネチャと一致しないことを返す
                return false;
            }


            // ここまで来たら一致したという事で返す
            return true;
        }


        /// <summary>
        /// ストリームから int を読み込みます。
        /// TODO:BinaryReader/Writerのようにリトルエンディアンのみではなくビッグエンディアンにも対応した読み書きクラスを実装する
        /// </summary>
        /// <param name="stream">読み込むストリーム</param>
        /// <returns>読み込まれた int を返します</returns>
        /// <exception cref="EndOfStreamException"></exception>
        private int ReadInt(Stream stream)
        {
            var readSize = stream.Read(readBuffer, 0, 4);
            if (readSize < 4)
            {
                // 末尾に達してしまった
                throw new EndOfStreamException();
            }


            // もしビッグエンディアンCPU環境なら
            if (!BitConverter.IsLittleEndian)
            {
                // データを反転
                Array.Reverse(readBuffer, 0, 4);
            }


            // intとして返す
            return BitConverter.ToInt32(readBuffer, 0);
        }


        private void ReadProgramCode(Stream stream, ref MemoryBlock<SrValue> processMemory, int instructionNumber)
        {
            for (int i = 0; i < instructionNumber; ++i)
            {
                stream.Read(readBuffer, 0, 4);
                var instructionCode = new InstructionCode();
                instructionCode.OpCode = (OpCode)readBuffer[0];
                instructionCode.Ra = readBuffer[1];
                instructionCode.Rb = readBuffer[2];
                instructionCode.Rc = readBuffer[3];
                instructionCode.Immediate.Int = ReadInt(stream);
                processMemory[i].Instruction = instructionCode;
            }
        }


        private void ReadConstStringData(Stream stream, ref MemoryBlock<SrObject> objectMemory, int constStringNumber)
        {
            var stringReadBuffer = readBuffer;


            for (int i = 0; i < constStringNumber; ++i)
            {
                var dataSize = ReadInt(stream);
                var index = ReadInt(stream);
                stringReadBuffer = stringReadBuffer.Length >= dataSize ? stringReadBuffer : new byte[dataSize];
                stream.Read(stringReadBuffer, 0, dataSize);
                objectMemory[index].Value = encoding.GetString(stringReadBuffer, 0, dataSize);
            }
        }
        #endregion


        #region Peripheral control functions
        /// <summary>
        /// 周辺機器をファームウェアに追加します。追加された周辺機器はこのインスタンスが解放される時、一緒に解放されます。
        /// </summary>
        /// <param name="peripheral">追加する周辺機器のインスタンス</param>
        /// <exception cref="ArgumentNullException">peripheral が null です</exception>
        /// <exception cref="ArgumentException">追加しようとした周辺機器の名前が null または 空白 です</exception>
        internal void AddPeripheral(SrvmPeripheral peripheral)
        {
            // null を渡されたら
            if (peripheral == null)
            {
                // 追加することは出来ないので例外を吐く
                throw new ArgumentNullException(nameof(peripheral));
            }


            // 追加する周辺機器名がnullだったり空白のみである場合は
            if (string.IsNullOrWhiteSpace(peripheral.PeripheralName))
            {
                // 周辺機器名が正しくない
                throw new ArgumentException("追加しようとした周辺機器の名前が null または 空白 です", nameof(peripheral));
            }


            // 周辺機器IDを作って周辺機器テーブルに追加
            var peripheralName = peripheral.PeripheralName;
            var peripheralId = nextPeripheralID++;
            peripheralIDTable[peripheralName] = peripheralId;
            peripheralTable[peripheralId] = peripheral;
        }


        /// <summary>
        /// 周辺機器名から周辺機器IDを取得します
        /// </summary>
        /// <param name="name">IDを取得する周辺機器名</param>
        /// <returns>周辺機器IDが取得できた場合はその値を返しますが、取得が出来なかった場合は -1 を返します</returns>
        internal int GetPeripheralID(string name)
        {
            // 名前から取得を試みて成功したら返して、駄目だったら -1 を返す
            return peripheralIDTable.TryGetValue(name, out var id) ? id : -1;
        }


        /// <summary>
        /// 周辺機器IDから周辺機器を取得します
        /// </summary>
        /// <param name="id">取得したい周辺機器</param>
        /// <returns>正常に周辺機器の取得ができた場合は参照を返しますが、取得ができなかった場合は null を返します</returns>
        internal SrvmPeripheral GetPeripheral(int id)
        {
            // IDから取得を試みて成功したら返して、駄目だったら null を返す
            return peripheralTable.TryGetValue(id, out var peripheral) ? peripheral : null;
        }
        #endregion
    }
}