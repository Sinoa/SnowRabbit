# 雪兎仮想マシンの実行データフォーマット v1（草案）

## フォーマット名称
- フル名称
    - Carrot Executable and Linkable Format
- 略式
    - CELF（[C]arrot [E]xecutable and [L]inkable [F]ormat）

## エンディアン
リトルエンディアンでのみ対応、ビッグエンディアンCPUなどは  
プログラムローダで適切にメモリにロードする事で対応。


# データ構造

## CELFヘッダ

全体サイズ：16 byte

| 名称 | 型 | サイズ | 説明 |
| --- | --- | --- | --- |
| Magic Signature | byte[4] | 4 | CELFデータである証明となる、シグネチャ '0xCC','0xEE','0x11','0xFF' |
| Object Version | ushort | 2 | オブジェクトファイルのバージョン上位8bitをメジャーバージョン下位8bitをマイナーバージョン 例1.0'0x0100' 1.3'0x0103' |
| Runtime Version | ushort | 2 | 実行されるランタイムの想定バージョン（フォーマットは ObjectVersion と同じ） |
| CELF Type | ushort | 2 | CELFの格納タイプ'未定義ファイル=0x0000',実行可能ファイル=0x0001','共有ライブラリファイル=0x0002' |
| CELF Header Size | ushort | 2 | CELFヘッダ自体のサイズ |
| Section Header Table Offset | uint | 4 | セクションヘッダテーブルが存在するCELFデータの先頭からのオフセット |

## セクションヘッダテーブル

全体サイズ：16 + 32 * n byte

| 名称 | 型 | サイズ | 説明 |
| --- | --- | --- | --- |
| Section Header Count | ushort | 2 | テーブルに書き込まれているセクションヘッダの数 |
| Program Code Section Index | ushort | 2 | 実行可能なプログラムコードを持つセクションがあるインデックス |
| String Table Section Index | ushort | 2 | 文字列テーブルを持つセクションがあるインデックス |
| Reserved | ushort | 2 | 予約領域（0で埋めなければならない） |
| Reserved | ulong | 8 | 予約領域（0で埋めなければならない） |
| Section Headers | SectionHeader[n] | 32 * n | 複数セクションヘッダ |

## セクションヘッダ

全体サイズ：32 byte

| 名称 | 型 | サイズ | 説明 |
| --- | --- | --- | --- |
| Section Name | byte[16] | 16 | セクション名（16文字までのASCIIコード） |
| Section Offset | uint | 4 | セクションの実体が格納されているCELFデータの先頭からのオフセット |
| Section Size | uint | 4 | セクションの実体サイズ |
| Reserved | ulong | 8 | 予約領域（0で埋めなければならない） |

## プログラムコードセクション

全体サイズ：4 + 8 * n byte

| 名称 | 型 | サイズ | 説明 |
| --- | --- | --- | --- |
| Code Count | uint | 4 | 格納されている命令の数（サイズではなく数） |
| Codes | InstructionCode[n] | 8 * n | 実際の命令配列 |

## 文字列テーブルセクション

全体サイズ：4 + (8 + strData) * n

| 名称 | 型 | サイズ | 説明 |
| --- | --- | --- | --- |
| String Count | uint | 4 | 格納されている文字列の数 |
| Strings | StringData[n] | (8 + strData) * n | 実際の文字列データ配列 |

## 文字列データ

全体サイズ：8 + dataLength

| 名称 | 型 | サイズ | 説明 |
| --- | --- | --- | --- |
| Load Destination Index | uint | 4 | 文字列のロード先インデックス |
| String Size | uint | 4 | UTF-8データサイズ |
| UTF-8 Data | byte[n] | n | ロードするべき、UTF-8エンコード済み文字列データ |
