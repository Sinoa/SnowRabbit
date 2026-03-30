# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

雪兎スクリプト（CSF）向けの VS Code 構文ハイライト拡張機能。TypeScript/JavaScript コードは含まず、TextMate 文法定義のみで構成される純粋なシンタックスハイライト拡張。

## 開発・テスト方法

- **デバッグ実行**: VS Code で F5 を押すと Extension Development Host が起動し、`.csf` ファイルでハイライトを確認できる
- **サンプルファイル**: `samples/example.csf` でハイライトの動作確認が可能
- **ビルドシステムなし**: npm scripts、TypeScript コンパイル、バンドラーは不要
- **パッケージング**: `vsce package` で VSIX ファイルを生成

## アーキテクチャ

拡張機能は3つのファイルで構成される:

- **`package.json`** — 拡張機能マニフェスト。言語ID `csf`、ファイル拡張子 `.csf`、文法スコープ `source.csf` を登録
- **`syntaxes/csf.tmLanguage.json`** — TextMate 形式の文法定義。正規表現ベースのトークナイズルールを定義。スコープ名は `*.csf` サフィックスを使用
- **`language-configuration.json`** — 括弧マッチング、コメント (`//`)、インデントルール（`function`/`for`/`while`/`if` でインデント増、`end`/`else` で減）

## CSF 言語の特徴

- ブロック終端は `end` キーワード（中括弧ではない）
- プリプロセッサディレクティブ: `#compile`, `#link`, `#const`
- 型: `void`, `int`, `number`, `string`, `object`, `bool`
- 宣言: `function`, `using`（ペリフェラル）, `global`, `local`
- 言語仕様の詳細は `syntax.md`（BNF 形式）を参照

## 文法ファイル編集時の注意

- TextMate 文法の正規表現は oniguruma 形式
- 新しいトークンを追加する際は `repository` セクションにパターンを定義し、`patterns` から `include` で参照する
- スコープ名は VS Code の標準命名規則（`keyword.control`, `entity.name.function` 等）に従い、末尾に `.csf` を付ける
