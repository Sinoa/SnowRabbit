# Copilot Instructions for SnowRabbit

SnowRabbitは純粋なC#で実装されたスクリプトエンジンです。独自のスクリプト言語、コンパイラ、レジスタマシン設計の仮想マシンを持ち、.NET Core単体およびUnityパッケージとして動作します。

## ビルド・テストコマンド

```bash
# ソリューション全体をビルド
dotnet build SnowRabbit.sln

# 構成を指定してビルド (Debug, Release, Trace)
dotnet build SnowRabbit.sln -c Release

# 全テスト実行
dotnet test .NativeTest\SnowRabbitTest.csproj

# 単一テストを名前で実行
dotnet test .NativeTest\SnowRabbitTest.csproj --filter "FullyQualifiedName~MemoryBlockTest"

# サンプルアプリケーション実行
dotnet run --project .SampleApplication\SampleApplication.csproj
```

## アーキテクチャ

### コンパイラパイプライン (`Runtime\Compiler\`)
コンパイラは`.srs`スクリプトを3段階で変換する:
1. **Lexer** (`SrLexer`) → ソースコードをトークン化
2. **Parser** (`SrParser`) → 構文木を構築 (`SyntaxNodes\`)
3. **Assembler** (`SrAssembler`) → バイナリ実行コードを生成

エントリーポイント: `SrCompiler.Compile(path, outStream)` がパイプライン全体を統括する。
デバッグ用に `SrDisassembler` でバイナリを逆アセンブルできる。

### 仮想マシン (`Runtime\RuntimeEngine\`)
32本のレジスタを持つレジスタマシン設計:
- レジスタ: `rax`, `rbx`, `rcx`, `rdx`, `rsi`, `rdi`, `rbp`, `rsp`, `r8`-`r29`, `ip`(命令ポインタ), `zero`(常にゼロ)
- **SrvmMachine** - VMのメインオーケストレータ。プラガブルなパーツ(Processor, Memory, Firmware, Storage)で構成
- **SrvmProcessor** - `OpCode.cs`で定義された命令を実行
- **SrProcess** - 実行中のスクリプトプロセスを表現
  - ライフサイクル: `Ready` → `Running` → `Suspended`/`Stopped`/`Panic`
- **SrVirtualMemory** - `MemoryBlock`セグメントによるメモリ管理(プログラム領域、グローバル、ヒープ、スタック)

### ホスト連携 (Peripheralシステム)
C#ホスト関数は属性を通じてスクリプトに公開される:
```csharp
[SrPeripheral("PeripheralName")]
public class MyPeripheral {
    [SrHostFunction("FunctionName")]
    public ReturnType Method(int a, int b, [SrProcessID] int processId) { }
}
```
- `SrvmFirmware.AttachPeripheral()`でペリフェラルを登録
- `[SrProcessID]`属性を付けたパラメータには呼び出し元プロセスIDが自動注入される

## スクリプト言語 (.srs)

**型**: `void`, `int`, `number`(浮動小数点), `string`, `object`, `bool`

```
// ホスト関数のインポート
using FuncAlias = ReturnType PeripheralName.FunctionName(paramTypes);

// 定数とグローバル変数
#const CONSTANT_NAME value
global Type variableName = initialValue;

// 関数は 'function...end' ブロック、ローカル変数は 'local' を使用
function ReturnType FunctionName(Type param)
    local Type varName = value;
    return expression;
end
```

**制御構文**: `if`/`else`, `for`, `while`, `break`, `return`
**リテラル**: `true`, `false`, `null`

## コーディング規約

- **ライセンスヘッダ**: 全ソースファイルにzlib/libpngライセンスヘッダを含める
- **命名規則**: クラス名は `Sr` (SnowRabbit) または `Srvm` (SnowRabbit Virtual Machine) をプレフィックスとする
- **Unsafeコード**: パフォーマンス重視の処理のため全構成で許可
- **明示的な型**: `.editorconfig`で`var`より明示的な型宣言を推奨
- **Unity互換**: `.meta`ファイルはビルドから除外。Unity統合には`SnowRabbit.asmdef`を使用
- **Disposableパターン**: コアクラスは`SrDisposable`基底クラスを継承
