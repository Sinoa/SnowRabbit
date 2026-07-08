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

using SnowRabbit.RuntimeEngine;
using SnowRabbit.RuntimeEngine.VirtualMachine;

namespace SnowRabbit.Tests;

/// <summary>
/// SrvmProcessor クラスに対するテストクラスです
/// </summary>
[TestFixture]
public class SrvmProcessorTest
{
    // レジスタ別名定数定義（テストコードの可読性向上のため）
    private const byte RegA = SrvmProcessor.RegisterAIndex;
    private const byte RegB = SrvmProcessor.RegisterBIndex;
    private const byte RegC = SrvmProcessor.RegisterCIndex;
    private const byte RegD = SrvmProcessor.RegisterDIndex;
    private const byte RegSI = SrvmProcessor.RegisterSIIndex;
    private const byte RegDI = SrvmProcessor.RegisterDIIndex;
    private const byte RegSP = SrvmProcessor.RegisterSPIndex;
    private const byte RegR8 = SrvmProcessor.RegisterR8Index;
    private const byte RegR9 = SrvmProcessor.RegisterR9Index;
    private const byte RegR10 = SrvmProcessor.RegisterR10Index;
    private const byte RegR11 = SrvmProcessor.RegisterR11Index;
    private const byte RegR12 = SrvmProcessor.RegisterR12Index;
    private const byte RegR13 = SrvmProcessor.RegisterR13Index;
    private const byte RegR14 = SrvmProcessor.RegisterR14Index;
    private const byte RegR15 = SrvmProcessor.RegisterR15Index;
    private const byte RegR16 = SrvmProcessor.RegisterR16Index;
    private const byte RegR17 = SrvmProcessor.RegisterR17Index;
    private const byte RegR18 = SrvmProcessor.RegisterR18Index;
    private const byte RegR19 = SrvmProcessor.RegisterR19Index;
    private const byte RegR20 = SrvmProcessor.RegisterR20Index;
    private const byte RegR21 = SrvmProcessor.RegisterR21Index;
    private const byte RegR22 = SrvmProcessor.RegisterR22Index;
    private const byte RegIP = SrvmProcessor.RegisterIPIndex;
    private const byte RegZero = SrvmProcessor.RegisterZeroIndex;

    // メモリレイアウト定数定義
    private const int GlobalMemorySize = 10;
    private const int HeapMemorySize = 10;
    private const int StackMemorySize = 10;
    private const int StackLowerBound = (int)SrVirtualMemory.StackOffset;
    private const int InitialStackPointer = StackLowerBound + StackMemorySize;

    // メンバ変数定義
    private TestProcessor processor = null!;
    private SrValue[] rawMemory = null!;
    private MemoryBlock<SrValue> globalMemory;
    private MemoryBlock<SrValue> heapMemory;
    private MemoryBlock<SrValue> stackMemory;
    private MemoryBlock<SrValue> processorContext;



    /// <summary>
    /// テストのセットアップをします
    /// </summary>
    [OneTimeSetUp]
    public void Setup()
    {
        // テスト用のプロセッサのインスタンスを生成する
        processor = new TestProcessor();


        // 実行用メモリ領域を確保してメモリブロックを割り当てる
        rawMemory = new SrValue[GlobalMemorySize + HeapMemorySize + StackMemorySize + SrvmProcessor.TotalRegisterCount];
        globalMemory = new MemoryBlock<SrValue>(rawMemory, 0, GlobalMemorySize);
        heapMemory = new MemoryBlock<SrValue>(rawMemory, GlobalMemorySize, HeapMemorySize);
        stackMemory = new MemoryBlock<SrValue>(rawMemory, GlobalMemorySize + HeapMemorySize, StackMemorySize);
        processorContext = new MemoryBlock<SrValue>(rawMemory, GlobalMemorySize + HeapMemorySize + StackMemorySize, SrvmProcessor.TotalRegisterCount);


        // 生メモリをクリア
        Array.Clear(rawMemory, 0, rawMemory.Length);
    }


    /// <summary>
    /// テストのクリーンアップをします
    /// </summary>
    [OneTimeTearDown]
    public void TearDown()
    {
        processor?.Dispose();
    }


    /// <summary>
    /// 指定されたプロセスIDとプログラムコードからプロセスを生成します
    /// </summary>
    /// <param name="processID">プロセスに割り当てるプロセスID</param>
    /// <param name="programCode">プロセスが実行するプログラムコード</param>
    /// <returns>生成されたプロセスを返します</returns>
    private SrProcess CreateProcess(int processID, SrValue[] programCode)
    {
        // メモリをクリアして、事前に割り当てたメモリブロックを使ってプロセスを生成後、コンテキストの初期化
        Array.Clear(rawMemory, 0, rawMemory.Length);
        MemoryBlock<SrValue> programMemory = new MemoryBlock<SrValue>(programCode, 0, programCode.Length);
        SrProcess process = new SrProcess(processID, programMemory, globalMemory, heapMemory, stackMemory, processorContext, null);
        SrvmProcessor.InitializeProcessorContext(process);
        return process;
    }


    /// <summary>
    /// 指定された命令列からプログラムコードを構築してプロセスを生成します
    /// </summary>
    /// <param name="instructions">プロセスが実行する命令列</param>
    /// <returns>生成されたプロセスを返します</returns>
    private SrProcess CreateProcess(params SrInstruction[] instructions)
    {
        // 既定のプロセスIDでプロセスを生成する
        return CreateProcess(1, BuildProgram(instructions));
    }


    /// <summary>
    /// 指定された命令列からプログラムコード配列を構築します
    /// </summary>
    /// <param name="instructions">プログラムコードにする命令列</param>
    /// <returns>構築されたプログラムコード配列を返します</returns>
    private static SrValue[] BuildProgram(params SrInstruction[] instructions)
    {
        // 命令列を順番に SrValue へ詰め替える
        SrValue[] programCode = new SrValue[instructions.Length];
        for (int i = 0; i < instructions.Length; ++i)
        {
            programCode[i] = instructions[i];
        }
        return programCode;
    }


    /// <summary>
    /// 指定された内容で整数即値の命令を生成します
    /// </summary>
    /// <param name="opCode">実行するオペコード</param>
    /// <param name="r1">オペランドで使用するレジスタ指定引数1</param>
    /// <param name="r2">オペランドで使用するレジスタ指定引数2</param>
    /// <param name="r3">オペランドで使用するレジスタ指定引数3</param>
    /// <param name="imm">オペランドで使用する符号付き32bit整数即値</param>
    /// <returns>生成された命令を返します</returns>
    private static SrInstruction Inst(OpCode opCode, byte r1 = 0, byte r2 = 0, byte r3 = 0, int imm = 0)
    {
        // 命令を組み立てて返す
        SrInstruction instruction = default;
        instruction.Set(opCode, r1, r2, r3, imm);
        return instruction;
    }


    /// <summary>
    /// 指定された内容で浮動小数点即値の命令を生成します
    /// </summary>
    /// <param name="opCode">実行するオペコード</param>
    /// <param name="r1">オペランドで使用するレジスタ指定引数1</param>
    /// <param name="r2">オペランドで使用するレジスタ指定引数2</param>
    /// <param name="r3">オペランドで使用するレジスタ指定引数3</param>
    /// <param name="imm">オペランドで使用する32bit浮動小数点即値</param>
    /// <returns>生成された命令を返します</returns>
    private static SrInstruction InstF(OpCode opCode, byte r1, byte r2, byte r3, float imm)
    {
        // 命令を組み立てて返す
        SrInstruction instruction = default;
        instruction.Set(opCode, r1, r2, r3, imm);
        return instruction;
    }


    /// <summary>
    /// 指定されたレジスタの符号付き64bit整数値を取得します
    /// </summary>
    /// <param name="registerIndex">取得するレジスタ番号</param>
    /// <returns>レジスタの符号付き64bit整数値を返します</returns>
    private long Reg(byte registerIndex) => processorContext[registerIndex].Primitive.Long;


    /// <summary>
    /// 指定されたレジスタの32bit浮動小数点値を取得します
    /// </summary>
    /// <param name="registerIndex">取得するレジスタ番号</param>
    /// <returns>レジスタの32bit浮動小数点値を返します</returns>
    private float RegF(byte registerIndex) => processorContext[registerIndex].Primitive.Float;


    /// <summary>
    /// 指定されたレジスタのオブジェクト参照を取得します
    /// </summary>
    /// <param name="registerIndex">取得するレジスタ番号</param>
    /// <returns>レジスタのオブジェクト参照を返します</returns>
    private object RegObj(byte registerIndex) => processorContext[registerIndex].Object;


    /// <summary>
    /// 指定されたレジスタへ符号付き64bit整数値を設定します
    /// </summary>
    /// <param name="registerIndex">設定するレジスタ番号</param>
    /// <param name="value">設定する値</param>
    private void SetReg(byte registerIndex, long value) => processorContext[registerIndex].Primitive.Long = value;


    /// <summary>
    /// 指定されたレジスタへ32bit浮動小数点値を設定します
    /// </summary>
    /// <param name="registerIndex">設定するレジスタ番号</param>
    /// <param name="value">設定する値</param>
    private void SetRegF(byte registerIndex, float value) => processorContext[registerIndex].Primitive.Float = value;


    /// <summary>
    /// 指定されたレジスタへオブジェクト参照を設定します
    /// </summary>
    /// <param name="registerIndex">設定するレジスタ番号</param>
    /// <param name="value">設定するオブジェクト参照</param>
    private void SetRegObj(byte registerIndex, object value) => processorContext[registerIndex].Object = value;


    #region Data Transfer
    /// <summary>
    /// Mov 命令がプリミティブ値とオブジェクト参照の両方をコピーすることをテストします
    /// </summary>
    [Test]
    public void OpMovTest()
    {
        // 転送元レジスタに整数値とオブジェクト参照を設定して Mov を実行する
        SrProcess process = CreateProcess(
            Inst(OpCode.Mov, RegA, RegB),
            Inst(OpCode.Halt));
        SetReg(RegB, 42);
        SetRegObj(RegB, "source");
        processor.Execute(process);


        // 値とオブジェクト参照が丸ごとコピーされ、転送元は保持されていることを確認する
        Assert.That(Reg(RegA), Is.EqualTo(42));
        Assert.That(RegObj(RegA), Is.SameAs("source"));
        Assert.That(Reg(RegB), Is.EqualTo(42));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Stopped));
    }


    /// <summary>
    /// Movl 命令の即値転送と、負の即値が32bit無符号としてゼロ拡張されることをテストします
    /// </summary>
    [Test]
    public void OpMovlTest()
    {
        // 正の即値と負の即値をそれぞれ転送する（転送先には事前にゴミ値を設定しておく）
        SrProcess process = CreateProcess(
            Inst(OpCode.Movl, RegA, imm: 123456),
            Inst(OpCode.Movl, RegB, imm: -1),
            Inst(OpCode.Halt));
        SetReg(RegA, -999);
        SetRegObj(RegA, "garbage");
        processor.Execute(process);


        // 即値が転送されオブジェクト参照もクリアされること、負の値はゼロ拡張されることを確認する
        Assert.That(Reg(RegA), Is.EqualTo(123456));
        Assert.That(RegObj(RegA), Is.Null);
        Assert.That(Reg(RegB), Is.EqualTo(4294967295L));
    }


    /// <summary>
    /// Ldr / Ldrl 命令による各メモリセグメントからのロードをテストします
    /// </summary>
    [Test]
    public void OpLoadTest()
    {
        // グローバル・ヒープ・スタックの各セグメントへ値を用意してロードする
        SrProcess process = CreateProcess(
            Inst(OpCode.Ldr, RegA, RegB),                   // global[3] (ベースレジスタのみ)
            Inst(OpCode.Ldr, RegC, RegB, imm: 1),           // global[4] (正の変位)
            Inst(OpCode.Ldr, RegD, RegB, imm: -2),          // global[1] (負の変位)
            Inst(OpCode.Ldrl, RegSI, imm: 0x00200004),      // heap[4] (絶対アドレス)
            Inst(OpCode.Ldrl, RegDI, imm: 0x00300005),      // stack[5] (絶対アドレス)
            Inst(OpCode.Halt));
        SetReg(RegB, 0x00100003);
        globalMemory[3] = 111;
        globalMemory[3].Object = "g3";
        globalMemory[4] = 222;
        globalMemory[1] = 333;
        heapMemory[4] = 444;
        stackMemory[5] = 555;
        processor.Execute(process);


        // 各セグメントの値がロードされていることを確認する（オブジェクト参照も含めてロードされる）
        Assert.That(Reg(RegA), Is.EqualTo(111));
        Assert.That(RegObj(RegA), Is.SameAs("g3"));
        Assert.That(Reg(RegC), Is.EqualTo(222));
        Assert.That(Reg(RegD), Is.EqualTo(333));
        Assert.That(Reg(RegSI), Is.EqualTo(444));
        Assert.That(Reg(RegDI), Is.EqualTo(555));
    }


    /// <summary>
    /// Str / Strl 命令による各メモリセグメントへのストアをテストします
    /// </summary>
    [Test]
    public void OpStoreTest()
    {
        // ヒープ・スタック・グローバルの各セグメントへストアする
        SrProcess process = CreateProcess(
            Inst(OpCode.Str, RegA, RegB, imm: 2),           // heap[2] (ベース+変位)
            Inst(OpCode.Str, RegA, RegC, imm: 0),           // stack[0] (ベースのみ)
            Inst(OpCode.Strl, RegA, imm: 0x00100001),       // global[1] (絶対アドレス)
            Inst(OpCode.Halt));
        SetReg(RegA, 777);
        SetRegObj(RegA, "stored");
        SetReg(RegB, 0x00200000);
        SetReg(RegC, 0x00300000);
        processor.Execute(process);


        // 各セグメントへ値が書き込まれていることを確認する（オブジェクト参照も含めてストアされる）
        Assert.That(heapMemory[2].Primitive.Long, Is.EqualTo(777));
        Assert.That(stackMemory[0].Primitive.Long, Is.EqualTo(777));
        Assert.That(globalMemory[1].Primitive.Long, Is.EqualTo(777));
        Assert.That(globalMemory[1].Object, Is.SameAs("stored"));
    }


    /// <summary>
    /// Push / Pop 命令の往復とスタックポインタの進行方向をテストします
    /// </summary>
    [Test]
    public void OpPushPopTest()
    {
        // 2つの値をプッシュしてから2つポップする
        SrProcess process = CreateProcess(
            Inst(OpCode.Push, RegA),
            Inst(OpCode.Push, RegB),
            Inst(OpCode.Pop, RegC),
            Inst(OpCode.Pop, RegD),
            Inst(OpCode.Halt));
        SetReg(RegA, 10);
        SetReg(RegB, 20);
        processor.Execute(process);


        // LIFO順にポップされ、SPが初期位置に復元されていることを確認する
        Assert.That(Reg(RegC), Is.EqualTo(20));
        Assert.That(Reg(RegD), Is.EqualTo(10));
        Assert.That(Reg(RegSP), Is.EqualTo(InitialStackPointer));


        // スタックがメモリ末尾から先頭方向（降順）へ伸びていることを残留値で確認する
        Assert.That(stackMemory[StackMemorySize - 1].Primitive.Long, Is.EqualTo(10));
        Assert.That(stackMemory[StackMemorySize - 2].Primitive.Long, Is.EqualTo(20));
    }


    /// <summary>
    /// Pushl 命令の即値ゼロ拡張と Fpushl 命令の浮動小数点即値プッシュをテストします
    /// </summary>
    [Test]
    public void OpImmediatePushTest()
    {
        // 正の即値・負の即値・浮動小数点即値をプッシュしてポップで取り出す
        SrProcess process = CreateProcess(
            Inst(OpCode.Pushl, imm: 5),
            Inst(OpCode.Pushl, imm: -1),
            InstF(OpCode.Fpushl, 0, 0, 0, 2.5f),
            Inst(OpCode.Pop, RegA),
            Inst(OpCode.Pop, RegB),
            Inst(OpCode.Pop, RegC),
            Inst(OpCode.Halt));
        processor.Execute(process);


        // 浮動小数点はビットパターンそのまま、負の即値は32bit無符号としてゼロ拡張されることを確認する
        Assert.That(RegF(RegA), Is.EqualTo(2.5f));
        Assert.That(Reg(RegB), Is.EqualTo(4294967295L));
        Assert.That(Reg(RegC), Is.EqualTo(5));
        Assert.That(Reg(RegSP), Is.EqualTo(InitialStackPointer));
    }


    /// <summary>
    /// Fmovl / Movfti / Movitf 命令の浮動小数点と整数の相互転送をテストします
    /// </summary>
    [Test]
    public void OpFloatTransferTest()
    {
        // 浮動小数点即値転送と、float→long / long→float の変換転送を実行する
        // （Movitf は転送先 r1 と転送元 r2 が異なるレジスタのケースを必ず検証する）
        SrProcess process = CreateProcess(
            InstF(OpCode.Fmovl, RegA, 0, 0, 3.5f),
            Inst(OpCode.Movfti, RegC, RegB),
            Inst(OpCode.Movfti, RegSI, RegD),
            Inst(OpCode.Movitf, RegR9, RegR8),
            Inst(OpCode.Halt));
        SetRegF(RegB, 3.9f);
        SetRegF(RegD, -2.75f);
        SetReg(RegR8, 42);
        SetReg(RegR9, 999);   // 転送先の事前値（r1 を読み取る取り違えバグの検出用）
        processor.Execute(process);


        // 各変換転送の結果を確認する（float→long は0方向への切り捨て）
        Assert.That(RegF(RegA), Is.EqualTo(3.5f));
        Assert.That(Reg(RegC), Is.EqualTo(3));
        Assert.That(Reg(RegSI), Is.EqualTo(-2));
        Assert.That(RegF(RegR9), Is.EqualTo(42.0f));
    }
    #endregion


    #region Integer Arithmetic
    /// <summary>
    /// レジスタ間整数算術命令 (Add/Sub/Mul/Div/Mod/Pow/Neg) をテストします
    /// </summary>
    [Test]
    public void OpIntegerArithmeticTest()
    {
        // RB=7, RC=3 として各算術命令を実行する
        SrProcess process = CreateProcess(
            Inst(OpCode.Add, RegA, RegB, RegC),
            Inst(OpCode.Sub, RegD, RegB, RegC),
            Inst(OpCode.Mul, RegSI, RegB, RegC),
            Inst(OpCode.Div, RegDI, RegB, RegC),
            Inst(OpCode.Mod, RegR8, RegB, RegC),
            Inst(OpCode.Pow, RegR9, RegB, RegC),
            Inst(OpCode.Neg, RegR10, RegB),
            Inst(OpCode.Halt));
        SetReg(RegB, 7);
        SetReg(RegC, 3);
        processor.Execute(process);


        // 各演算結果を確認する
        Assert.That(Reg(RegA), Is.EqualTo(10));
        Assert.That(Reg(RegD), Is.EqualTo(4));
        Assert.That(Reg(RegSI), Is.EqualTo(21));
        Assert.That(Reg(RegDI), Is.EqualTo(2));
        Assert.That(Reg(RegR8), Is.EqualTo(1));
        Assert.That(Reg(RegR9), Is.EqualTo(343));
        Assert.That(Reg(RegR10), Is.EqualTo(-7));
    }


    /// <summary>
    /// 即値整数算術命令 (Addl/Subl/Mull/Divl/Modl/Powl/Negl) と負の即値の符号拡張をテストします
    /// </summary>
    [Test]
    public void OpIntegerImmediateArithmeticTest()
    {
        // RB=10 として各即値算術命令を実行する（負の即値は符号拡張される）
        SrProcess process = CreateProcess(
            Inst(OpCode.Addl, RegA, RegB, imm: -4),
            Inst(OpCode.Subl, RegC, RegB, imm: -5),
            Inst(OpCode.Mull, RegD, RegB, imm: -3),
            Inst(OpCode.Divl, RegSI, RegB, imm: 3),
            Inst(OpCode.Modl, RegDI, RegB, imm: 3),
            Inst(OpCode.Powl, RegR8, RegB, imm: 2),
            Inst(OpCode.Negl, RegR9, imm: 123),
            Inst(OpCode.Negl, RegR10, imm: -9),
            Inst(OpCode.Halt));
        SetReg(RegB, 10);
        processor.Execute(process);


        // 各演算結果を確認する
        Assert.That(Reg(RegA), Is.EqualTo(6));
        Assert.That(Reg(RegC), Is.EqualTo(15));
        Assert.That(Reg(RegD), Is.EqualTo(-30));
        Assert.That(Reg(RegSI), Is.EqualTo(3));
        Assert.That(Reg(RegDI), Is.EqualTo(1));
        Assert.That(Reg(RegR8), Is.EqualTo(100));
        Assert.That(Reg(RegR9), Is.EqualTo(-123));
        Assert.That(Reg(RegR10), Is.EqualTo(9));
    }


    /// <summary>
    /// Div/Divl/Mod/Modl 命令のゼロ除算で例外が送出されプロセスがパニックすることをテストします
    /// </summary>
    [Test]
    public void OpDivideByZeroTest()
    {
        // Div (除数レジスタが0)
        SrProcess process = CreateProcess(
            Inst(OpCode.Div, RegA, RegB, RegC),
            Inst(OpCode.Halt));
        SetReg(RegB, 1);
        Assert.Throws<DivideByZeroException>(() => processor.Execute(process));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));


        // Divl (即値0除算)
        process = CreateProcess(
            Inst(OpCode.Divl, RegA, RegB, imm: 0),
            Inst(OpCode.Halt));
        SetReg(RegB, 1);
        Assert.Throws<DivideByZeroException>(() => processor.Execute(process));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));


        // Mod (除数レジスタが0)
        process = CreateProcess(
            Inst(OpCode.Mod, RegA, RegB, RegC),
            Inst(OpCode.Halt));
        SetReg(RegB, 1);
        Assert.Throws<DivideByZeroException>(() => processor.Execute(process));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));


        // Modl (即値0除算)
        process = CreateProcess(
            Inst(OpCode.Modl, RegA, RegB, imm: 0),
            Inst(OpCode.Halt));
        SetReg(RegB, 1);
        Assert.Throws<DivideByZeroException>(() => processor.Execute(process));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));
    }
    #endregion


    #region Float Arithmetic
    /// <summary>
    /// レジスタ間浮動小数点算術命令 (Fadd/Fsub/Fmul/Fdiv/Fmod/Fpow/Fneg) をテストします
    /// </summary>
    [Test]
    public void OpFloatArithmeticTest()
    {
        // RB=7.5f, RC=2.0f として各浮動小数点算術命令を実行する
        SrProcess process = CreateProcess(
            Inst(OpCode.Fadd, RegA, RegB, RegC),
            Inst(OpCode.Fsub, RegD, RegB, RegC),
            Inst(OpCode.Fmul, RegSI, RegB, RegC),
            Inst(OpCode.Fdiv, RegDI, RegB, RegC),
            Inst(OpCode.Fmod, RegR8, RegB, RegC),
            Inst(OpCode.Fpow, RegR9, RegB, RegC),
            Inst(OpCode.Fneg, RegR10, RegB),
            Inst(OpCode.Halt));
        SetRegF(RegB, 7.5f);
        SetRegF(RegC, 2.0f);
        processor.Execute(process);


        // 各演算結果を確認する（二進で正確に表現できる値のみ使用しているため厳密比較でよい）
        Assert.That(RegF(RegA), Is.EqualTo(9.5f));
        Assert.That(RegF(RegD), Is.EqualTo(5.5f));
        Assert.That(RegF(RegSI), Is.EqualTo(15.0f));
        Assert.That(RegF(RegDI), Is.EqualTo(3.75f));
        Assert.That(RegF(RegR8), Is.EqualTo(1.5f));
        Assert.That(RegF(RegR9), Is.EqualTo(56.25f));
        Assert.That(RegF(RegR10), Is.EqualTo(-7.5f));
    }


    /// <summary>
    /// 即値浮動小数点算術命令 (Faddl/Fsubl/Fmull/Fdivl/Fmodl/Fpowl/Fnegl) をテストします
    /// </summary>
    [Test]
    public void OpFloatImmediateArithmeticTest()
    {
        // RB=6.0f として各浮動小数点即値算術命令を実行する
        SrProcess process = CreateProcess(
            InstF(OpCode.Faddl, RegA, RegB, 0, 1.5f),
            InstF(OpCode.Fsubl, RegC, RegB, 0, 0.5f),
            InstF(OpCode.Fmull, RegD, RegB, 0, 2.5f),
            InstF(OpCode.Fdivl, RegSI, RegB, 0, 4.0f),
            InstF(OpCode.Fmodl, RegDI, RegB, 0, 3.5f),
            InstF(OpCode.Fpowl, RegR8, RegB, 0, 2.0f),
            InstF(OpCode.Fnegl, RegR9, 0, 0, 1.25f),
            Inst(OpCode.Halt));
        SetRegF(RegB, 6.0f);
        processor.Execute(process);


        // 各演算結果を確認する
        Assert.That(RegF(RegA), Is.EqualTo(7.5f));
        Assert.That(RegF(RegC), Is.EqualTo(5.5f));
        Assert.That(RegF(RegD), Is.EqualTo(15.0f));
        Assert.That(RegF(RegSI), Is.EqualTo(1.5f));
        Assert.That(RegF(RegDI), Is.EqualTo(2.5f));
        Assert.That(RegF(RegR8), Is.EqualTo(36.0f));
        Assert.That(RegF(RegR9), Is.EqualTo(-1.25f));
    }
    #endregion


    #region String
    /// <summary>
    /// Sadd 命令の文字列連結（通常・片側null・両側null）をテストします
    /// </summary>
    [Test]
    public void OpSaddTest()
    {
        // RB="Hello", RC="World" とし、RD と RSI は null のままにして連結する
        // （結果レジスタ RA には事前にゴミのプリミティブ値を設定しておく）
        SrProcess process = CreateProcess(
            Inst(OpCode.Sadd, RegA, RegB, RegC),
            Inst(OpCode.Sadd, RegR8, RegB, RegD),
            Inst(OpCode.Sadd, RegR9, RegD, RegSI),
            Inst(OpCode.Halt));
        SetRegObj(RegB, "Hello");
        SetRegObj(RegC, "World");
        SetReg(RegA, 0x123456);
        processor.Execute(process);


        // 連結結果と、結果レジスタのプリミティブ部が0クリアされることを確認する
        // （null の文字列は空文字列として扱われる）
        Assert.That(RegObj(RegA), Is.EqualTo("HelloWorld"));
        Assert.That(Reg(RegA), Is.EqualTo(0));
        Assert.That(RegObj(RegR8), Is.EqualTo("Hello"));
        Assert.That(RegObj(RegR9), Is.EqualTo(string.Empty));
    }
    #endregion


    #region Logic
    /// <summary>
    /// ビット演算命令 (Or/Xor/And/Not) をテストします
    /// </summary>
    [Test]
    public void OpBitwiseLogicTest()
    {
        // RB=0b1100, RC=0b1010 として各ビット演算命令を実行する（Not は r1 に対するインプレース演算）
        SrProcess process = CreateProcess(
            Inst(OpCode.Or, RegA, RegB, RegC),
            Inst(OpCode.Xor, RegD, RegB, RegC),
            Inst(OpCode.And, RegSI, RegB, RegC),
            Inst(OpCode.Not, RegDI),
            Inst(OpCode.Halt));
        SetReg(RegB, 0b1100);
        SetReg(RegC, 0b1010);
        processorContext[RegDI].Primitive.Ulong = 0xFFFF0000FFFF0000UL;
        processor.Execute(process);


        // 各演算結果を確認する
        Assert.That(Reg(RegA), Is.EqualTo(0b1110));
        Assert.That(Reg(RegD), Is.EqualTo(0b0110));
        Assert.That(Reg(RegSI), Is.EqualTo(0b1000));
        Assert.That(processorContext[RegDI].Primitive.Ulong, Is.EqualTo(0x0000FFFF0000FFFFUL));
    }


    /// <summary>
    /// シフト命令 (Shl/Shr) とシフト量が自動的に63でマスクされる挙動をテストします
    /// </summary>
    [Test]
    public void OpShiftTest()
    {
        // 通常のシフトと、63を超えるシフト量（68 → 68&63=4）のマスク挙動を実行する
        SrProcess process = CreateProcess(
            Inst(OpCode.Shl, RegA, RegB, RegC),
            Inst(OpCode.Shr, RegR8, RegD, RegSI),
            Inst(OpCode.Shl, RegR10, RegB, RegR9),
            Inst(OpCode.Shr, RegR11, RegD, RegR9),
            Inst(OpCode.Halt));
        SetReg(RegB, 1);
        SetReg(RegC, 4);
        SetReg(RegD, -16);
        SetReg(RegSI, 2);
        SetReg(RegR9, 68);
        processor.Execute(process);


        // 各シフト結果を確認する（Shr は算術右シフト、シフト量はC#準拠で下位6bitのみ有効）
        Assert.That(Reg(RegA), Is.EqualTo(16));
        Assert.That(Reg(RegR8), Is.EqualTo(-4));
        Assert.That(Reg(RegR10), Is.EqualTo(16));
        Assert.That(Reg(RegR11), Is.EqualTo(-1));
    }
    #endregion


    #region Comparison
    /// <summary>
    /// 整数比較命令 (Teq/Tne/Tg/Tge/Tl/Tle) の真偽両方の結果をテストします
    /// </summary>
    [Test]
    public void OpIntegerCompareTest()
    {
        // RB=5, RC=3, RD=5 として各比較命令を真偽両方のオペランドで実行する
        SrProcess process = CreateProcess(
            Inst(OpCode.Teq, RegA, RegB, RegD),
            Inst(OpCode.Teq, RegSI, RegB, RegC),
            Inst(OpCode.Tne, RegDI, RegB, RegC),
            Inst(OpCode.Tne, RegR8, RegB, RegD),
            Inst(OpCode.Tg, RegR9, RegB, RegC),
            Inst(OpCode.Tg, RegR10, RegC, RegB),
            Inst(OpCode.Tge, RegR11, RegB, RegD),
            Inst(OpCode.Tge, RegR12, RegC, RegB),
            Inst(OpCode.Tl, RegR13, RegC, RegB),
            Inst(OpCode.Tl, RegR14, RegB, RegC),
            Inst(OpCode.Tle, RegR15, RegB, RegD),
            Inst(OpCode.Tle, RegR16, RegB, RegC),
            Inst(OpCode.Halt));
        SetReg(RegB, 5);
        SetReg(RegC, 3);
        SetReg(RegD, 5);


        // 偽(0)が書き込まれることを確認するため、期待値0のレジスタへ事前に非0値を設定しておく
        SetReg(RegSI, 123);
        SetReg(RegR8, 123);
        SetReg(RegR10, 123);
        SetReg(RegR12, 123);
        SetReg(RegR14, 123);
        SetReg(RegR16, 123);
        processor.Execute(process);


        // 各比較結果を確認する
        Assert.That(Reg(RegA), Is.EqualTo(1));      // 5 == 5
        Assert.That(Reg(RegSI), Is.EqualTo(0));     // 5 == 3
        Assert.That(Reg(RegDI), Is.EqualTo(1));     // 5 != 3
        Assert.That(Reg(RegR8), Is.EqualTo(0));     // 5 != 5
        Assert.That(Reg(RegR9), Is.EqualTo(1));     // 5 > 3
        Assert.That(Reg(RegR10), Is.EqualTo(0));    // 3 > 5
        Assert.That(Reg(RegR11), Is.EqualTo(1));    // 5 >= 5
        Assert.That(Reg(RegR12), Is.EqualTo(0));    // 3 >= 5
        Assert.That(Reg(RegR13), Is.EqualTo(1));    // 3 < 5
        Assert.That(Reg(RegR14), Is.EqualTo(0));    // 5 < 3
        Assert.That(Reg(RegR15), Is.EqualTo(1));    // 5 <= 5
        Assert.That(Reg(RegR16), Is.EqualTo(0));    // 5 <= 3
    }


    /// <summary>
    /// 浮動小数点比較命令 (Ftg/Ftge/Ftl/Ftle) の真偽両方と NaN 比較が全て偽になることをテストします
    /// </summary>
    [Test]
    public void OpFloatCompareTest()
    {
        // RB=2.5f, RC=1.5f, RD=2.5f, RSI=NaN として各浮動小数点比較命令を実行する
        SrProcess process = CreateProcess(
            Inst(OpCode.Ftg, RegA, RegB, RegC),
            Inst(OpCode.Ftg, RegR8, RegC, RegB),
            Inst(OpCode.Ftge, RegR9, RegB, RegD),
            Inst(OpCode.Ftge, RegR10, RegC, RegB),
            Inst(OpCode.Ftl, RegR11, RegC, RegB),
            Inst(OpCode.Ftl, RegR12, RegB, RegC),
            Inst(OpCode.Ftle, RegR13, RegB, RegD),
            Inst(OpCode.Ftle, RegR14, RegB, RegC),
            Inst(OpCode.Ftg, RegR15, RegSI, RegC),
            Inst(OpCode.Ftge, RegR16, RegSI, RegC),
            Inst(OpCode.Ftl, RegR17, RegSI, RegC),
            Inst(OpCode.Ftle, RegR18, RegSI, RegC),
            Inst(OpCode.Ftge, RegR19, RegB, RegSI),
            Inst(OpCode.Halt));
        SetRegF(RegB, 2.5f);
        SetRegF(RegC, 1.5f);
        SetRegF(RegD, 2.5f);
        SetRegF(RegSI, float.NaN);


        // 偽(0)が書き込まれることを確認するため、期待値0のレジスタへ事前に非0値を設定しておく
        SetReg(RegR8, 123);
        SetReg(RegR10, 123);
        SetReg(RegR12, 123);
        SetReg(RegR14, 123);
        SetReg(RegR15, 123);
        SetReg(RegR16, 123);
        SetReg(RegR17, 123);
        SetReg(RegR18, 123);
        SetReg(RegR19, 123);
        processor.Execute(process);


        // 各比較結果を確認する
        Assert.That(Reg(RegA), Is.EqualTo(1));      // 2.5 > 1.5
        Assert.That(Reg(RegR8), Is.EqualTo(0));     // 1.5 > 2.5
        Assert.That(Reg(RegR9), Is.EqualTo(1));     // 2.5 >= 2.5
        Assert.That(Reg(RegR10), Is.EqualTo(0));    // 1.5 >= 2.5
        Assert.That(Reg(RegR11), Is.EqualTo(1));    // 1.5 < 2.5
        Assert.That(Reg(RegR12), Is.EqualTo(0));    // 2.5 < 1.5
        Assert.That(Reg(RegR13), Is.EqualTo(1));    // 2.5 <= 2.5
        Assert.That(Reg(RegR14), Is.EqualTo(0));    // 2.5 <= 1.5


        // NaN が絡む比較は全て偽になることを確認する
        Assert.That(Reg(RegR15), Is.EqualTo(0));    // NaN > 1.5
        Assert.That(Reg(RegR16), Is.EqualTo(0));    // NaN >= 1.5
        Assert.That(Reg(RegR17), Is.EqualTo(0));    // NaN < 1.5
        Assert.That(Reg(RegR18), Is.EqualTo(0));    // NaN <= 1.5
        Assert.That(Reg(RegR19), Is.EqualTo(0));    // 2.5 >= NaN
    }


    /// <summary>
    /// オブジェクト比較命令 (Toeq/Tone/Tonull/Tonnull) の参照等価テストをテストします
    /// </summary>
    [Test]
    public void OpObjectCompareTest()
    {
        // RB と RC には同一参照、RD には別インスタンス、RSI は null のままにして比較する
        SrProcess process = CreateProcess(
            Inst(OpCode.Toeq, RegA, RegB, RegC),
            Inst(OpCode.Toeq, RegR8, RegB, RegD),
            Inst(OpCode.Tone, RegR9, RegB, RegD),
            Inst(OpCode.Tone, RegR10, RegB, RegC),
            Inst(OpCode.Tonull, RegR11, RegSI),
            Inst(OpCode.Tonull, RegR12, RegB),
            Inst(OpCode.Tonnull, RegR13, RegB),
            Inst(OpCode.Tonnull, RegR14, RegSI),
            Inst(OpCode.Halt));
        object sharedObject = new object();
        SetRegObj(RegB, sharedObject);
        SetRegObj(RegC, sharedObject);
        SetRegObj(RegD, new object());


        // 偽(0)が書き込まれることを確認するため、期待値0のレジスタへ事前に非0値を設定しておく
        SetReg(RegR8, 123);
        SetReg(RegR10, 123);
        SetReg(RegR12, 123);
        SetReg(RegR14, 123);
        processor.Execute(process);


        // 各比較結果を確認する（比較は参照の等価判定）
        Assert.That(Reg(RegA), Is.EqualTo(1));      // 同一参照
        Assert.That(Reg(RegR8), Is.EqualTo(0));     // 別インスタンス
        Assert.That(Reg(RegR9), Is.EqualTo(1));     // 別インスタンス
        Assert.That(Reg(RegR10), Is.EqualTo(0));    // 同一参照
        Assert.That(Reg(RegR11), Is.EqualTo(1));    // null
        Assert.That(Reg(RegR12), Is.EqualTo(0));    // 非null
        Assert.That(Reg(RegR13), Is.EqualTo(1));    // 非null
        Assert.That(Reg(RegR14), Is.EqualTo(0));    // null
    }
    #endregion


    #region Flow Control
    /// <summary>
    /// 無条件分岐命令 (Br/Brl) の分岐先計算をテストします
    /// </summary>
    [Test]
    public void OpUnconditionalBranchTest()
    {
        // Brl の絶対分岐と、Br のレジスタ+即値分岐（ゼロレジスタ基準の絶対分岐含む）を実行する
        SrProcess process = CreateProcess(
            Inst(OpCode.Brl, imm: 2),                       // [0] → 2 へ分岐
            Inst(OpCode.Movl, RegA, imm: 111),              // [1] スキップされる
            Inst(OpCode.Movl, RegB, imm: 1),                // [2] Br の基準値を設定
            Inst(OpCode.Br, RegB, imm: 4),                  // [3] → 1+4=5 へ分岐
            Inst(OpCode.Movl, RegC, imm: 333),              // [4] スキップされる
            Inst(OpCode.Br, RegZero, imm: 7),               // [5] → 0+7=7 へ分岐
            Inst(OpCode.Movl, RegD, imm: 444),              // [6] スキップされる
            Inst(OpCode.Halt));                             // [7]
        processor.Execute(process);


        // スキップされた命令が実行されていないことと、最終的な命令ポインタを確認する
        Assert.That(Reg(RegA), Is.EqualTo(0));
        Assert.That(Reg(RegB), Is.EqualTo(1));
        Assert.That(Reg(RegC), Is.EqualTo(0));
        Assert.That(Reg(RegD), Is.EqualTo(0));
        Assert.That(processorContext[RegIP].Primitive.Int, Is.EqualTo(8));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Stopped));
    }


    /// <summary>
    /// 条件分岐命令 (Bnz/Bnzl) の分岐成立・不成立の両方をテストします
    /// </summary>
    [Test]
    public void OpConditionalBranchTest()
    {
        // R20=1(真), R21=0(偽) として、成立時は分岐し、不成立時は次の命令へ進むことを実行する
        SrProcess process = CreateProcess(
            Inst(OpCode.Bnz, RegZero, RegR20, imm: 2),      // [0] 成立 → 2 へ分岐
            Inst(OpCode.Movl, RegA, imm: 111),              // [1] スキップされる
            Inst(OpCode.Bnz, RegZero, RegR21, imm: 5),      // [2] 不成立 → 3 へ
            Inst(OpCode.Movl, RegB, imm: 222),              // [3] 実行される
            Inst(OpCode.Bnzl, 0, RegR20, imm: 6),           // [4] 成立 → 6 へ分岐
            Inst(OpCode.Movl, RegC, imm: 333),              // [5] スキップされる
            Inst(OpCode.Bnzl, 0, RegR21, imm: 8),           // [6] 不成立 → 7 へ
            Inst(OpCode.Movl, RegD, imm: 444),              // [7] 実行される
            Inst(OpCode.Halt));                             // [8]
        SetReg(RegR20, 1);
        processor.Execute(process);


        // 分岐成立側はスキップされ、不成立側は実行されていることを確認する
        Assert.That(Reg(RegA), Is.EqualTo(0));
        Assert.That(Reg(RegB), Is.EqualTo(222));
        Assert.That(Reg(RegC), Is.EqualTo(0));
        Assert.That(Reg(RegD), Is.EqualTo(444));
    }


    /// <summary>
    /// Call/Calll/Ret 命令の呼び出しと復帰の往復、戻りアドレスの記録をテストします
    /// </summary>
    [Test]
    public void OpCallRetTest()
    {
        // Calll による絶対アドレス呼び出しと Ret による復帰を実行する
        SrProcess process = CreateProcess(
            Inst(OpCode.Calll, imm: 3),                     // [0] 戻りアドレス1をプッシュして 3 へ
            Inst(OpCode.Movl, RegB, imm: 222),              // [1] 復帰後に実行される
            Inst(OpCode.Halt),                              // [2]
            Inst(OpCode.Movl, RegA, imm: 111),              // [3] サブルーチン本体
            Inst(OpCode.Ret));                              // [4] → 1 へ復帰
        processor.Execute(process);


        // サブルーチンと復帰後のコードが実行され、SPが復元されていることを確認する
        Assert.That(Reg(RegA), Is.EqualTo(111));
        Assert.That(Reg(RegB), Is.EqualTo(222));
        Assert.That(Reg(RegSP), Is.EqualTo(InitialStackPointer));


        // スタックへ積まれた戻りアドレスが正しいことを残留値で確認する
        Assert.That(stackMemory[StackMemorySize - 1].Primitive.Int, Is.EqualTo(1));


        // Call によるレジスタ+即値アドレス呼び出しと Ret による復帰を実行する
        process = CreateProcess(
            Inst(OpCode.Call, RegC, imm: 2),                // [0] 戻りアドレス1をプッシュして 3+2=5 へ
            Inst(OpCode.Movl, RegB, imm: 222),              // [1] 復帰後に実行される
            Inst(OpCode.Halt),                              // [2]
            Inst(OpCode.Movl, RegR21, imm: 999),            // [3] 実行されない
            Inst(OpCode.Movl, RegR22, imm: 999),            // [4] 実行されない
            Inst(OpCode.Movl, RegA, imm: 111),              // [5] サブルーチン本体
            Inst(OpCode.Ret));                              // [6] → 1 へ復帰
        SetReg(RegC, 3);
        processor.Execute(process);


        // サブルーチンと復帰後のコードが実行され、間の命令は実行されていないことを確認する
        Assert.That(Reg(RegA), Is.EqualTo(111));
        Assert.That(Reg(RegB), Is.EqualTo(222));
        Assert.That(Reg(RegR21), Is.EqualTo(0));
        Assert.That(Reg(RegR22), Is.EqualTo(0));
        Assert.That(Reg(RegSP), Is.EqualTo(InitialStackPointer));
        Assert.That(stackMemory[StackMemorySize - 1].Primitive.Int, Is.EqualTo(1));
    }


    /// <summary>
    /// 条件付き呼び出し命令 (Callnz/Callnzl) の条件成立・不成立の両方をテストします
    /// </summary>
    [Test]
    public void OpConditionalCallTest()
    {
        // Callnzl 条件成立（R20=1）: サブルーチンが呼び出される
        SrProcess process = CreateProcess(
            Inst(OpCode.Callnzl, 0, RegR20, imm: 3),
            Inst(OpCode.Movl, RegB, imm: 222),
            Inst(OpCode.Halt),
            Inst(OpCode.Movl, RegA, imm: 111),
            Inst(OpCode.Ret));
        SetReg(RegR20, 1);
        processor.Execute(process);
        Assert.That(Reg(RegA), Is.EqualTo(111));
        Assert.That(Reg(RegB), Is.EqualTo(222));
        Assert.That(Reg(RegSP), Is.EqualTo(InitialStackPointer));
        Assert.That(stackMemory[StackMemorySize - 1].Primitive.Int, Is.EqualTo(1));


        // Callnzl 条件不成立（R21=0）: サブルーチンは呼び出されずスタックも積まれない
        process = CreateProcess(
            Inst(OpCode.Callnzl, 0, RegR21, imm: 3),
            Inst(OpCode.Movl, RegB, imm: 222),
            Inst(OpCode.Halt),
            Inst(OpCode.Movl, RegA, imm: 111),
            Inst(OpCode.Ret));
        processor.Execute(process);
        Assert.That(Reg(RegA), Is.EqualTo(0));
        Assert.That(Reg(RegB), Is.EqualTo(222));
        Assert.That(Reg(RegSP), Is.EqualTo(InitialStackPointer));
        Assert.That(stackMemory[StackMemorySize - 1].Primitive.Long, Is.EqualTo(0));


        // Callnz 条件成立（R20=1）: レジスタ+即値のアドレスへ呼び出される
        process = CreateProcess(
            Inst(OpCode.Callnz, RegZero, RegR20, imm: 3),
            Inst(OpCode.Movl, RegB, imm: 222),
            Inst(OpCode.Halt),
            Inst(OpCode.Movl, RegA, imm: 111),
            Inst(OpCode.Ret));
        SetReg(RegR20, 1);
        processor.Execute(process);
        Assert.That(Reg(RegA), Is.EqualTo(111));
        Assert.That(Reg(RegB), Is.EqualTo(222));
        Assert.That(Reg(RegSP), Is.EqualTo(InitialStackPointer));


        // Callnz 条件不成立（R21=0）: サブルーチンは呼び出されない
        process = CreateProcess(
            Inst(OpCode.Callnz, RegZero, RegR21, imm: 3),
            Inst(OpCode.Movl, RegB, imm: 222),
            Inst(OpCode.Halt),
            Inst(OpCode.Movl, RegA, imm: 111),
            Inst(OpCode.Ret));
        processor.Execute(process);
        Assert.That(Reg(RegA), Is.EqualTo(0));
        Assert.That(Reg(RegB), Is.EqualTo(222));
        Assert.That(Reg(RegSP), Is.EqualTo(InitialStackPointer));
    }
    #endregion


    #region CPU Control
    /// <summary>
    /// Halt 命令によるプロセス停止と停止イベントの発火をテストします
    /// </summary>
    [Test]
    public void OpHaltTest()
    {
        // イベント記録をリセットしてから Halt で終了するプログラムを実行する
        processor.ResetEventRecord();
        SrProcess process = CreateProcess(
            Inst(OpCode.Movl, RegA, imm: 42),
            Inst(OpCode.Halt));
        processor.Execute(process);


        // プロセスが停止状態になり、停止イベントが1回発火していることを確認する
        Assert.That(Reg(RegA), Is.EqualTo(42));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Stopped));
        Assert.That(processor.StoppedEventCount, Is.EqualTo(1));
        Assert.That(processor.LastStoppedProcess, Is.SameAs(process));
        Assert.That(processorContext[RegIP].Primitive.Int, Is.EqualTo(2));


        // 停止済みプロセスを再実行しても何も起きないことを確認する
        processor.Execute(process);
        Assert.That(processor.StoppedEventCount, Is.EqualTo(1));
        Assert.That(processorContext[RegIP].Primitive.Int, Is.EqualTo(2));
    }


    /// <summary>
    /// 不明なオペコードの実行で例外が送出されプロセスがパニックすることをテストします
    /// </summary>
    [Test]
    public void OpUnknownInstructionTest()
    {
        // 未定義のオペコードを実行する
        SrProcess process = CreateProcess(
            Inst((OpCode)0x7F),
            Inst(OpCode.Halt));
        Assert.Throws<SrUnknownInstructionException>(() => processor.Execute(process));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));


        // パニック済みプロセスの再実行は何も起きないことを確認する
        Assert.DoesNotThrow(() => processor.Execute(process));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));


        // ゼロ埋め（未初期化）のプログラムメモリの実行も不明命令として扱われることを確認する
        process = CreateProcess(1, new SrValue[1]);
        Assert.Throws<SrUnknownInstructionException>(() => processor.Execute(process));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));
    }
    #endregion


    #region Stack Guard
    /// <summary>
    /// プッシュ系命令 (Push/Pushl/Fpushl/Call/Calll/Callnz/Callnzl) のスタックオーバーフロー検出をテストします
    /// </summary>
    [Test]
    public void StackOverflowTest()
    {
        // スタックを使い切った状態で各プッシュ系命令を実行するとオーバーフロー例外になることを確認する
        (string Name, SrInstruction Instruction)[] overflowCases = new (string, SrInstruction)[]
        {
            ("Push", Inst(OpCode.Push, RegA)),
            ("Pushl", Inst(OpCode.Pushl, imm: 123)),
            ("Fpushl", InstF(OpCode.Fpushl, 0, 0, 0, 1.5f)),
            ("Call", Inst(OpCode.Call, RegZero, imm: 0)),
            ("Calll", Inst(OpCode.Calll, imm: 0)),
            ("Callnz", Inst(OpCode.Callnz, RegZero, RegC, imm: 0)),
            ("Callnzl", Inst(OpCode.Callnzl, 0, RegC, imm: 0)),
        };
        foreach ((string name, SrInstruction instruction) in overflowCases)
        {
            // スタックを丁度使い切ってから対象の命令を実行する
            SrInstruction[] instructions = new SrInstruction[StackMemorySize + 2];
            for (int i = 0; i < StackMemorySize; ++i)
            {
                instructions[i] = Inst(OpCode.Pushl, imm: 0);
            }
            instructions[StackMemorySize] = instruction;
            instructions[StackMemorySize + 1] = Inst(OpCode.Halt);


            // Callnz/Callnzl の条件を真にしてから実行して、例外とパニック状態とSPの位置を確認する
            SrProcess process = CreateProcess(1, BuildProgram(instructions));
            SetReg(RegC, 1);
            Assert.Throws<SrStackOverflowException>(() => processor.Execute(process), name);
            Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic), name);
            Assert.That(Reg(RegSP), Is.EqualTo(StackLowerBound), name);
        }


        // スタックを丁度使い切るまでは正常に動作することを確認する
        SrInstruction[] okInstructions = new SrInstruction[StackMemorySize + 1];
        for (int i = 0; i < StackMemorySize; ++i)
        {
            okInstructions[i] = Inst(OpCode.Pushl, imm: 9);
        }
        okInstructions[StackMemorySize] = Inst(OpCode.Halt);
        SrProcess okProcess = CreateProcess(1, BuildProgram(okInstructions));
        processor.Execute(okProcess);
        Assert.That(okProcess.ProcessState, Is.EqualTo(SrProcessStatus.Stopped));
        Assert.That(Reg(RegSP), Is.EqualTo(StackLowerBound));
        Assert.That(stackMemory[0].Primitive.Long, Is.EqualTo(9));
    }


    /// <summary>
    /// Subl 命令によるスタックポインタのフレーム確保がスタック下限を割った場合の検出をテストします
    /// </summary>
    [Test]
    public void SublStackGuardTest()
    {
        // スタックサイズを超えるフレーム確保はオーバーフロー例外になることを確認する
        SrProcess process = CreateProcess(
            Inst(OpCode.Subl, RegSP, RegSP, imm: StackMemorySize + 1),
            Inst(OpCode.Halt));
        Assert.Throws<SrStackOverflowException>(() => processor.Execute(process));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));


        // スタックを丁度使い切るフレーム確保は正常に動作することを確認する
        process = CreateProcess(
            Inst(OpCode.Subl, RegSP, RegSP, imm: StackMemorySize),
            Inst(OpCode.Halt));
        processor.Execute(process);
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Stopped));
        Assert.That(Reg(RegSP), Is.EqualTo(StackLowerBound));
    }


    /// <summary>
    /// Pop / Ret 命令のスタックアンダーフロー検出をテストします
    /// </summary>
    [Test]
    public void StackUnderflowTest()
    {
        // 空のスタックからのポップはアンダーフロー例外になることを確認する
        SrProcess process = CreateProcess(
            Inst(OpCode.Pop, RegA),
            Inst(OpCode.Halt));
        Assert.Throws<SrStackUnderflowException>(() => processor.Execute(process));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));
        Assert.That(Reg(RegSP), Is.EqualTo(InitialStackPointer));


        // 空のスタックからの Ret もアンダーフロー例外になることを確認する
        process = CreateProcess(
            Inst(OpCode.Ret));
        Assert.Throws<SrStackUnderflowException>(() => processor.Execute(process));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));


        // プッシュした数を超えるポップもアンダーフロー例外になることを確認する
        process = CreateProcess(
            Inst(OpCode.Pushl, imm: 7),
            Inst(OpCode.Pop, RegA),
            Inst(OpCode.Pop, RegB),
            Inst(OpCode.Halt));
        Assert.Throws<SrStackUnderflowException>(() => processor.Execute(process));
        Assert.That(Reg(RegA), Is.EqualTo(7));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));
    }


#if DEBUG
    /// <summary>
    /// ゼロレジスタへの書き込み（ISA契約違反）がデバッグビルドの検証で検出されることをテストします
    /// </summary>
    [Test]
    public void ZeroRegisterWriteValidationTest()
    {
        // ゼロレジスタへのプリミティブ値の書き込みは検証例外になることを確認する
        SrProcess process = CreateProcess(
            Inst(OpCode.Movl, RegZero, imm: 1),
            Inst(OpCode.Halt));
        Assert.Throws<InvalidOperationException>(() => processor.Execute(process));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));


        // ゼロレジスタへのオブジェクト参照の書き込みも検証例外になることを確認する
        process = CreateProcess(
            Inst(OpCode.Sadd, RegZero, RegB, RegC),
            Inst(OpCode.Halt));
        SetRegObj(RegB, "a");
        Assert.Throws<InvalidOperationException>(() => processor.Execute(process));
        Assert.That(process.ProcessState, Is.EqualTo(SrProcessStatus.Panic));
    }
#endif
    #endregion
}



/// <summary>
/// 実行コードを正しく実行しているか確認するためのプロセッサクラスです
/// </summary>
public class TestProcessor : SrvmProcessor
{
    /// <summary>
    /// プロセス停止イベントが発火した回数
    /// </summary>
    public int StoppedEventCount { get; private set; }


    /// <summary>
    /// 最後に停止イベントが発火したプロセス
    /// </summary>
    public SrProcess LastStoppedProcess { get; private set; }



    /// <summary>
    /// イベントの記録をリセットします
    /// </summary>
    public void ResetEventRecord()
    {
        StoppedEventCount = 0;
        LastStoppedProcess = null;
    }


    /// <summary>
    /// プロセスが動作を停止した時のイベントを記録します
    /// </summary>
    /// <param name="process">停止したプロセス</param>
    protected override void OnProcessStopped(SrProcess process)
    {
        StoppedEventCount += 1;
        LastStoppedProcess = process;
    }
}
