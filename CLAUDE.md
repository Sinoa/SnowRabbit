# Copilot Instructions for SnowRabbit

SnowRabbitは純粋なC#で実装されたスクリプトエンジンです。独自のスクリプト言語、コンパイラ、レジスタ仮想マシンを持ち、.NET Core単体およびUnityパッケージとして動作します。

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
1. **Lexer** → ソースコードをトークン化
2. **Parser** (`SrParser`) → 構文木を構築 (`SyntaxNodes\`)
3. **Assembler** (`SrAssembler`) → バイナリ実行コードを生成

エントリーポイント: `SrCompiler.Compile(path, outStream)` がパイプライン全体を統括する。

### 仮想マシン (`Runtime\RuntimeEngine\`)
レジスタマシン設計のVM:
- **SrvmMachine** - VMのメインオーケストレータ。プラガブルなパーツ(Processor, Memory, Firmware, Storage)で構成
- **SrvmProcessor** - `OpCode.cs`で定義された命令を実行
- **SrProcess** - 実行中のスクリプトプロセスを表現 (`SrProcessStatus`でライフサイクル管理)
- **SrVirtualMemory** - `MemoryBlock`セグメントによるメモリ管理

### ホスト連携 (Peripheralシステム)
C#ホスト関数は属性を通じてスクリプトに公開される:
```csharp
[SrPeripheral("PeripheralName")]
public class MyPeripheral {
    [SrHostFunction("FunctionName")]
    public ReturnType Method(params) { }
}
```
`SrvmFirmware.AttachPeripheral()`でペリフェラルを登録する。

## スクリプト言語 (.srs)

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

## コーディング規約

- **ライセンスヘッダ**: 全ソースファイルにzlib/libpngライセンスヘッダを含める
- **命名規則**: クラス名は `Sr` (SnowRabbit) または `Srvm` (SnowRabbit Virtual Machine) をプレフィックスとする
- **Unsafeコード**: パフォーマンス重視の処理のため全構成で許可
- **明示的な型**: `.editorconfig`で`var`より明示的な型宣言を推奨
- **Unity互換**: `.meta`ファイルはビルドから除外。Unity統合には`SnowRabbit.asmdef`を使用
- **Disposableパターン**: コアクラスは`SrDisposable`基底クラスを継承
