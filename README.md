# SnowRabbit

純粋なC#で実装されたスクリプトエンジンです。独自のスクリプト言語、コンパイラ、レジスタマシン設計の仮想マシンを持ち、.NET環境およびUnityで動作します。

## 特徴

- **純粋なC#実装**: 外部依存なし、ポータブル
- **レジスタマシン設計**: 32本のレジスタを持つ高効率な仮想マシン
- **Unity対応**: .NET Standard 2.1準拠でUnityパッケージとして利用可能
- **ホスト連携**: C#メソッドをスクリプトから呼び出し可能（Peripheralシステム）
- **シンプルな言語仕様**: 学習しやすい独自スクリプト言語

## 始め方

### 必要条件

- .NET 10.0 SDK 以降（開発・ビルド用。`SampleApplication` と `SnowRabbitCompiler` が net10.0 をターゲットにしています）
- .NET Standard 2.1対応環境（ランタイム。本体ライブラリのターゲット）
- Unity 2021.3以降（Unity使用時）

※テストプロジェクトは net8.0 ターゲットですが、`RollForward=Major` を設定しているため
.NET 8 ランタイムが無い環境でもインストール済みの新しいランタイム（.NET 10 等）で自動的に実行されます。

### 導入方法

#### NuGetパッケージ（.NET）

```bash
# 今後公開予定
dotnet add package SnowRabbit
```

#### Unityパッケージ

Unity Package Manager経由でnpmパッケージとしてインストールできます（今後公開予定）。

#### ソースからビルド

```bash
git clone https://github.com/Sinoa/SnowRabbit.git
cd SnowRabbit
dotnet build SnowRabbit.slnx
```

### クイックスタート

#### 1. スクリプトファイルの作成 (hello.srs)

```
// ホスト関数のインポート
using Print = void Console.WriteLine(string);

// エントリーポイント（小文字のmain）
function void main()
    Print("Hello, SnowRabbit!");
end
```

#### 2. C#からの実行

```csharp
using SnowRabbit.Compiler;
using SnowRabbit.RuntimeEngine.VirtualMachine;
using SnowRabbit.RuntimeEngine.VirtualMachine.Peripheral;

// コンパイル
var compiler = new SrCompiler();
using var outStream = new FileStream("hello.bin", FileMode.Create);
compiler.Compile("hello.srs", outStream);
outStream.Close();

// 実行
var vm = new SrvmMachine(new MyFactory());
var process = vm.CreateProcess("hello.bin");
while (process.ProcessState != SrProcessStatus.Stopped)
{
    process.Run();
}
process.Dispose();

// Peripheralファクトリ
public class MyFactory : SrvmDefaultMachinePartsFactory
{
    public override SrvmFirmware CreateFirmware()
    {
        var firmware = base.CreateFirmware();
        firmware.AttachPeripheral(new ConsolePeripheral());
        return firmware;
    }
}

// ホスト関数を提供するPeripheral
[SrPeripheral("Console")]
public class ConsolePeripheral
{
    [SrHostFunction("WriteLine")]
    public void WriteLine(string text)
    {
        Console.WriteLine(text);
    }
}
```

## スクリプト言語仕様

### 型

| 型 | 説明 |
|---|---|
| `void` | 戻り値なし |
| `int` | 整数型 |
| `number` | 浮動小数点型 |
| `string` | 文字列型 |
| `object` | オブジェクト型 |
| `bool` | 真偽値型 |

### 構文

```
// 定数定義
#const MAX_VALUE 100

// グローバル変数
global int counter = 0;

// 関数定義
function int Add(int a, int b)
    return a + b;
end

// ローカル変数
function void Example()
    local int x = 10;
    local string s = "hello";
end

// 制御構文
function void Control()
    // if-else
    if (x > 0)
        x = 1;
    else
        x = 0;
    end

    // while
    while (x < 10)
        x = x + 1;
    end

    // for
    for (i = 0; i < 10; i = i + 1)
        sum = sum + i;
    end
end
```

### ホスト関数のインポート

```
// 書式: using エイリアス = 戻り型 Peripheral名.関数名(引数型リスト);
using MyFunc = int Sample.Calculate(int, int);
using Print = void Console.Write(string);
```

## アーキテクチャ

### コンパイラパイプライン

```
.srs ソース → Lexer → Parser → Assembler → .bin バイナリ
```

1. **Lexer** (`SrLexer`): ソースコードをトークン化
2. **Parser** (`SrParser`): 構文木を構築
3. **Assembler** (`SrAssembler`): バイナリ実行コードを生成

### 仮想マシン

- **32本のレジスタ**: `rax`, `rbx`, `rcx`, `rdx`, `rsi`, `rdi`, `rbp`, `rsp`, `r8`-`r29`, `ip`, `zero`
- **メモリ領域**: プログラム領域、グローバル、ヒープ、スタック
- **プロセスライフサイクル**: `Ready` → `Running` → `Suspended`/`Stopped`/`Panic`（`SrProcessStatus` には将来用の `ResumeRequested` も定義されています）

## ビルドとテスト

```bash
# ビルド
dotnet build SnowRabbit.slnx

# テスト実行
dotnet test test/SnowRabbit.Tests/SnowRabbit.Tests.csproj

# サンプル実行
dotnet run --project src/SampleApplication/SampleApplication.csproj

# コマンドラインコンパイラ (snowrabbitc) の実行例
dotnet run --project src/SnowRabbitCompiler/SnowRabbitCompiler.csproj -- script.srs -v
```

## ディレクトリ構成

```
SnowRabbit/
├── src/
│   ├── SnowRabbit/            # メインライブラリ
│   │   ├── Compiler/          # コンパイラ（Lexer, Parser, Assembler）
│   │   └── RuntimeEngine/     # 仮想マシン
│   ├── SnowRabbitCompiler/    # コマンドラインコンパイラ（snowrabbitc）
│   └── SampleApplication/     # サンプルアプリケーション
├── test/
│   └── SnowRabbit.Tests/      # テストプロジェクト（NUnit 4）
├── docs/                      # 言語リファレンス
├── vscodeextensions/          # VS Code 構文ハイライト拡張（言語ID: csf）
└── SnowRabbit.slnx
```

## SnowRabbitについて

### 作者

* Sinoa <sinoans@gmail.com>

### ライセンス

* zlib ライセンス - [詳細](LICENSE.md)

### コントリビューション

プルリクエストを歓迎します。大きな変更を行う場合は、まずIssueで議論してください。
