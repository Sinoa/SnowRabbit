# SnowRabbit への貢献

SnowRabbitへの貢献に興味を持っていただきありがとうございます！

## 貢献の方法

### バグ報告

1. [Issues](https://github.com/Sinoa/SnowRabbit/issues) で既存の報告がないか確認してください
2. 新しいIssueを作成し、以下の情報を含めてください：
   - 再現手順
   - 期待される動作
   - 実際の動作
   - 環境情報（OS、.NETバージョン、Unityバージョン等）

### 機能提案

1. Issueで提案内容を説明してください
2. ユースケースと期待される動作を明確にしてください
3. メンテナーと議論した後、実装を開始してください

### プルリクエスト

1. リポジトリをフォークしてください
2. `develop` ブランチから作業ブランチを作成してください
   ```bash
   git checkout develop
   git checkout -b feature/your-feature-name
   ```
3. 変更を加えてコミットしてください
4. テストが通ることを確認してください
   ```bash
   dotnet test .NativeTest\SnowRabbitTest.csproj
   ```
5. プルリクエストを作成してください

## 開発環境のセットアップ

### 必要なツール

- .NET 8.0 SDK以降
- Git
- Visual Studio 2022 / Visual Studio Code / Rider（お好みのIDE）

### ビルド手順

```bash
# リポジトリのクローン
git clone https://github.com/Sinoa/SnowRabbit.git
cd SnowRabbit

# ビルド
dotnet build SnowRabbit.sln

# テスト実行
dotnet test .NativeTest\SnowRabbitTest.csproj

# サンプル実行
dotnet run --project .SampleApplication\SampleApplication.csproj
```

### プロジェクト構成

| プロジェクト | ターゲット | 説明 |
|-------------|-----------|------|
| `Runtime/SnowRabbit.csproj` | .NET Standard 2.1 | メインライブラリ |
| `.NativeTest/SnowRabbitTest.csproj` | .NET 8.0 | テストプロジェクト |
| `.SampleApplication/SampleApplication.csproj` | .NET 8.0 | サンプルアプリ |

## コーディング規約

### 命名規則

- **クラス名**: `Sr` (SnowRabbit) または `Srvm` (SnowRabbit Virtual Machine) プレフィックスを使用
  - 例: `SrCompiler`, `SrvmMachine`, `SrProcess`
- **インターフェース**: `I` プレフィックス
  - 例: `ISrScriptStorage`
- **定数**: `UPPER_SNAKE_CASE`
- **プライベートフィールド**: `camelCase`（`_` プレフィックスなし）

### コードスタイル

- 明示的な型宣言を推奨（`var` よりも具体的な型名）
- インデント: スペース4つ
- ファイルエンコーディング: UTF-8 (BOMなし)
- 改行コード: CRLF (Windows)

### XMLドキュメント

- publicメンバーにはXMLドキュメントコメントを記述してください
- 日本語でのコメントを推奨します
- 特殊文字（`<`, `>`, `&`）は適切にエスケープしてください

```csharp
/// <summary>
/// トークンを読み取ります
/// </summary>
/// <param name="token">読み取ったトークンを受け取る参照</param>
/// <returns>トークンを読み取れた場合は true</returns>
public bool ReadNextToken(out Token token)
```

### ライセンスヘッダ

すべてのソースファイルには以下のライセンスヘッダを含めてください：

```csharp
// zlib/libpng License
//
// Copyright(c) 2019 - 2026 Sinoa
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
```

## テスト

### テストの実行

```bash
# 全テスト実行
dotnet test .NativeTest\SnowRabbitTest.csproj

# 特定のテストを実行
dotnet test .NativeTest\SnowRabbitTest.csproj --filter "FullyQualifiedName~SrLexerTest"

# 詳細出力
dotnet test .NativeTest\SnowRabbitTest.csproj --logger "console;verbosity=detailed"
```

### テストの追加

- 新機能には対応するテストを追加してください
- テストクラスは `SnowRabbitTest` 名前空間に配置
- テストメソッドには `[Test]` 属性を付与
- テストメソッド名は検証内容を明確に表す名前にしてください

```csharp
[TestFixture]
public class MyFeatureTest
{
    [Test]
    public void FeatureName_Condition_ExpectedResult()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

## Unity互換性

SnowRabbitはUnityでの使用を考慮して設計されています：

- ターゲットフレームワーク: .NET Standard 2.1
- `.meta` ファイルはビルドから除外されています
- `SnowRabbit.asmdef` を使用してUnity統合を行います

Unity固有の変更を行う場合は、.NET Core環境との互換性を維持してください。

## 質問・サポート

- GitHub Issuesで質問してください
- 作者への連絡: Sinoa <sinoans@gmail.com>

## 謝辞

貢献してくださるすべての方に感謝します！
