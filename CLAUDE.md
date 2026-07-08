# Copilot Instructions for SnowRabbit

SnowRabbitは純粋なC#で実装されたスクリプトエンジンです。独自のスクリプト言語(.srs)、コンパイラ、レジスタマシン設計の仮想マシンを持ち、.NET単体およびUnityパッケージ(UPM)として動作します。

## ビルド・テストコマンド

開発には .NET 10 SDK が必要です(SampleApplication と SnowRabbitCompiler が net10.0 ターゲット。本体ライブラリは netstandard2.1、テストは net8.0)。

```bash
# ソリューション全体をビルド (構成はSDK既定の Debug / Release)
dotnet build SnowRabbit.slnx

# 構成を指定してビルド
dotnet build SnowRabbit.slnx -c Release

# 全テスト実行 (テストはnet8.0ターゲットだが RollForward=Major 設定により新しいランタイムでも動く)
dotnet test test/SnowRabbit.Tests/SnowRabbit.Tests.csproj

# 単一テストを名前で実行
dotnet test test/SnowRabbit.Tests/SnowRabbit.Tests.csproj --filter "FullyQualifiedName~MemoryBlockTest"

# サンプルアプリケーション実行
dotnet run --project src/SampleApplication/SampleApplication.csproj

# コマンドラインコンパイラ (snowrabbitc) 実行
dotnet run --project src/SnowRabbitCompiler/SnowRabbitCompiler.csproj -- script.srs -v
```

## アーキテクチャ

### コンパイラパイプライン (`src/SnowRabbit/Compiler/`)
コンパイラは`.srs`スクリプトを3段階で変換する:
1. **Lexer** (`Lexer/SrLexer`) → ソースコードをトークン化
2. **Parser** (`Parser/SrParser`) → 構文木を構築 (`Parser/SyntaxNodes/`)。コード生成も各SyntaxNodeの`Compile(SrCompileContext)`が担う
3. **Assembler** (`Assembler/SrAssembler`) → バイナリ実行コードを生成

エントリーポイント: `SrCompiler.Compile(path, outStream)` がパイプライン全体を統括する。
デバッグ用に `SrDisassembler.Disassemble(stream)` でバイナリを逆アセンブルできる。
`SrParser.cs` 冒頭のBNFコメントが文法の一次情報で、`vscodeextensions/syntax.md` と同期させること。

### 仮想マシン (`src/SnowRabbit/RuntimeEngine/`)
32本のレジスタを持つレジスタマシン設計:
- レジスタ: `rax`, `rbx`, `rcx`, `rdx`, `rsi`, `rdi`, `rbp`, `rsp`, `r8`-`r29`, `ip`(命令ポインタ), `zero`(常にゼロ)。定数定義は `SrvmProcessor` の `Register*Index`
- **SrvmMachine** - VMのメインオーケストレータ。プラガブルなパーツ(Processor, Memory, Firmware, Storage)で構成され、`SrvmMachinePartsFactory` 派生クラスで差し替える
- **SrvmProcessor** - `RuntimeEngine/OpCode.cs`で定義された全73命令を実行
- **SrProcess** - 実行中のスクリプトプロセスを表現
  - ライフサイクル: `Ready` → `Running` → `Suspended`/`Stopped`/`Panic` (`SrProcessStatus`。将来用の `ResumeRequested` も定義されているが現在未使用)
- **SrVirtualMemory** - `MemoryBlock`セグメントによるメモリ管理(プログラム領域、グローバル、ヒープ、スタックの4セグメント。仮想アドレスは上位12bit=セグメント/下位20bit=オフセット)

### ホスト連携 (Peripheralシステム)
C#ホスト関数は属性を通じてスクリプトに公開される:
```csharp
[SrPeripheral("PeripheralName")]
public class MyPeripheral {
    [SrHostFunction("FunctionName")]
    public ReturnType Method(int a, int b, [SrProcessID] int processId) { }
}
```
- `SrvmFirmware.AttachPeripheral(instance)`でペリフェラルを登録(通常は`SrvmDefaultMachinePartsFactory`派生の`CreateFirmware()`内で行う)
- `[SrProcessID]`属性を付けた`int`パラメータには呼び出し元プロセスIDが自動注入される(スクリプト側の`using`シグネチャには含めない)
- 戻り値に `Task`/`Task<T>` を使うと非同期ホスト関数になり、プロセスは完了まで `Suspended` になる

## スクリプト言語 (.srs)

**型**: `void`, `int`(64bit整数), `number`(浮動小数点), `string`, `object`, `bool`

```
// ホスト関数のインポート
using FuncAlias = ReturnType PeripheralName.FunctionName(paramTypes);

// 定数とグローバル変数 (globalの初期化子はリテラルのみ)
#const CONSTANT_NAME value
global Type variableName = initialValue;

// 関数は 'function...end' ブロック、ローカル変数は 'local' を使用 (localは式で初期化可)
function ReturnType FunctionName(Type param)
    local Type varName = value;
    return expression;
end
```

**制御構文**: `if`/`else`/`else if`, `for`, `while`, `break`, `return`
**リテラル**: 整数(10進/16進 `0x`)、実数、文字列(`"..."` と `'...'`、エスケープは `\n \t \\ \" \'` の5種)、`true`, `false`, `null`
**演算子**: 算術 `+ - * / %`、比較、論理、ビット演算、複合代入(`%=`は無し)。インクリメントは前置(`++a`)のみ
**ディレクティブ**: `#const`, `#compile`(他スクリプトの取り込み), `#link`(予約構文・未実装)

詳細仕様は `docs/LANGUAGE_REFERENCE.md` を参照。VS Code用の構文ハイライト拡張は `vscodeextensions/` にある(こちらの言語IDは `csf`・拡張子 `.csf`)。

## コーディング規約

- **ライセンスヘッダ**: 全ソースファイルにzlibライセンスヘッダを含める
- **命名規則**: 公開コアクラスは `Sr` (SnowRabbit) または `Srvm` (SnowRabbit Virtual Machine) をプレフィックスとする(例外: `SyntaxNode`系、`Token`/`TokenReader`/`TokenKind`、`CompileReport`等の内部型)
- **Unsafeコード**: 本体ライブラリはパフォーマンス重視の処理のため許可(`AllowUnsafeBlocks`)
- **XMLドキュメントコメント**: 日本語で記述する
- **Unity互換**: ルートの `package.json` がUPMパッケージマニフェスト(`.asmdef`/`.meta`は現在含まれない)
- **Disposableパターン**: コアクラスは`SrDisposable`基底クラスを継承
- **テスト**: NUnit 4 (`test/SnowRabbit.Tests/`)。コンパイル〜VM実行のE2E検証は `SrEndToEndExecutionTest` のパターンに倣う
