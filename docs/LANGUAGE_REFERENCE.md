# SnowRabbit スクリプト言語リファレンス

SnowRabbitスクリプト言語（.srs）の完全な仕様書です。

## 目次

- [基本構文](#基本構文)
- [型システム](#型システム)
- [リテラル](#リテラル)
- [変数](#変数)
- [関数](#関数)
- [制御構文](#制御構文)
- [演算子](#演算子)
- [ホスト連携](#ホスト連携)
- [プリプロセッサディレクティブ](#プリプロセッサディレクティブ)
- [文法仕様（BNF）](#文法仕様bnf)

---

## 基本構文

### コメント

```
// 行コメント（この行の終わりまで）
```

※ブロックコメント（`/* */`）は現在サポートされていません。

### ファイル構造

スクリプトファイル（.srs）は以下の要素で構成されます：

1. プリプロセッサディレクティブ（`#const`, `#compile`, `#link`。`#link` はコンパイル済みオブジェクトファイルのリンク）
2. ホスト関数インポート（`using`）
3. グローバル変数宣言（`global`）
4. 関数定義（`function...end`）

```
// 1. プリプロセッサディレクティブ
#const MAX_VALUE 100

// 2. ホスト関数インポート
using Print = void Console.WriteLine(string);

// 3. グローバル変数
global int counter = 0;

// 4. 関数定義（mainがエントリーポイント）
function void main()
    Print("Hello!");
end
```

### エントリーポイント

スクリプトの実行は `main` 関数から開始されます（小文字）。

```
function void main()
    // ここから実行開始
end
```

---

## 型システム

SnowRabbitは以下の組み込み型をサポートします：

| 型 | 説明 | C#での対応 |
|---|---|---|
| `void` | 戻り値なし | `void` |
| `int` | 64ビット整数 | `long` |
| `number` | 64ビット浮動小数点 | `double` |
| `string` | 文字列 | `string` |
| `object` | オブジェクト参照 | `object` |
| `bool` | 真偽値 | `bool` |

---

## リテラル

### 整数リテラル

10進数と16進数（`0x` / `0X` プレフィックス）が使用できます。

```
0
123
-456
0xFF
0x1f
```

※`-456` は「単項マイナス + 整数リテラル `456`」として解釈されます。

### 浮動小数点リテラル

```
0.0
3.14
-2.5
```

※指数表記（`1e5`）はサポートされていません。また `123.` のように小数部を省略することはできません。

### 文字列リテラル

ダブルクォート（`"..."`）とシングルクォート（`'...'`）のどちらでも記述できます。

```
"hello"
'hello'
"日本語テスト"
"escape: \n \t \\"
```

使用できるエスケープシーケンスは以下の5種類のみです。それ以外のエスケープはコンパイルエラーになります。

| エスケープ | 意味 |
|-----------|------|
| `\n` | 改行 |
| `\t` | タブ |
| `\\` | バックスラッシュ |
| `\"` | ダブルクォート |
| `\'` | シングルクォート |

### 真偽値リテラル

```
true
false
```

### Nullリテラル

```
null
```

---

## 変数

### グローバル変数

スクリプト全体でアクセス可能な変数です。

```
// 初期値あり
global int counter = 0;
global string message = "hello";
global number pi = 3.14159;

// 初期値なし（デフォルト値で初期化）
global int value;
```

### ローカル変数

関数内でのみ有効な変数です。

```
function void Example()
    // 初期値あり
    local int x = 10;
    local string s = "test";
    
    // 初期値なし
    local number n;
    
    // 式で初期化
    local int sum = x + 20;
end
```

---

## 関数

### 関数定義

```
function 戻り型 関数名(パラメータリスト)
    // 関数本体
end
```

### 例

```
// 引数なし、戻り値なし
function void SayHello()
    // ...
end

// 引数あり、戻り値あり
function int Add(int a, int b)
    return a + b;
end

// 複数の引数
function number Calculate(int x, number y, string s)
    // ...
    return 0.0;
end
```

### 関数呼び出し

```
function void main()
    local int result = Add(1, 2);
    SayHello();
end
```

---

## 制御構文

### if文

```
if (条件式)
    // 条件が真の場合に実行
end
```

### if-else文

```
if (条件式)
    // 条件が真の場合
else
    // 条件が偽の場合
end
```

### if-else if-else文

```
if (条件1)
    // 条件1が真
else if (条件2)
    // 条件2が真
else
    // どちらも偽
end
```

### while文

```
while (条件式)
    // 条件が真の間ループ
end
```

### for文

```
for (初期化式; 条件式; 更新式)
    // ループ本体
end
```

例：
```
local int sum = 0;
local int i = 0;
for (i = 0; i < 10; i = i + 1)
    sum = sum + i;
end
```

空のfor文（無限ループ）：
```
for (;;)
    // 無限ループ
    break;  // breakで脱出
end
```

### break文

ループを中断します。

```
while (true)
    if (condition)
        break;
    end
end
```

### return文

関数から値を返します。

```
function int GetValue()
    return 42;
end

function void DoNothing()
    return;  // void関数では省略可能
end
```

---

## 演算子

### 演算子優先順位（高い順）

| 優先度 | 演算子 | 説明 |
|--------|--------|------|
| 1 | `()` | 括弧 |
| 2 | `+` `-` `!` `++` `--` | 単項演算子 |
| 3 | `*` `/` `%` | 乗除算・剰余 |
| 4 | `+` `-` | 加減算 |
| 5 | `<<` `>>` | ビットシフト |
| 6 | `<` `>` `<=` `>=` | 比較 |
| 7 | `==` `!=` | 等価 |
| 8 | `&` | ビットAND |
| 9 | `^` | ビットXOR |
| 10 | `\|` | ビットOR |
| 11 | `&&` | 論理AND |
| 12 | `\|\|` | 論理OR |
| 13 | `=` `+=` `-=` `*=` `/=` `&=` `\|=` `^=` | 代入 |

### 算術演算子

```
a + b   // 加算
a - b   // 減算
a * b   // 乗算
a / b   // 除算
a % b   // 剰余
-a      // 符号反転
+a      // 符号維持
```

### 比較演算子

```
a == b  // 等しい
a != b  // 等しくない
a < b   // より小さい
a > b   // より大きい
a <= b  // 以下
a >= b  // 以上
```

### 論理演算子

```
a && b  // 論理AND
a || b  // 論理OR
!a      // 論理NOT
```

`&&` と `||` は**短絡評価**されます。左辺で式全体の結果が確定した場合（`&&` の左辺が偽、`||` の左辺が真）、
右辺は評価されません（右辺に関数呼び出しなどの副作用がある場合、その副作用は発生しません）。

### ビット演算子

```
a & b   // ビットAND
a | b   // ビットOR
a ^ b   // ビットXOR
a << n  // 左シフト
a >> n  // 右シフト
```

### 文字列連結

`+` 演算子は両辺が文字列の場合、文字列連結を行います。文字列変数への `+=` も使用できます。

```
local string name = "World";
local string message = "Hello, " + name + "!";
message += "!!";
```

※文字列と数値の混合連結（`"a" + 1`）はサポートされていません。数値はホスト関数などで事前に文字列化してください。
※`==` / `!=` による文字列比較は参照比較です。連結で生成された文字列は内容が同じでも別インスタンスになるため、
`("a" + "b") == "ab"` が真になることは保証されません。

### 代入演算子

```
a = b       // 代入
a += b      // 加算代入 (a = a + b)
a -= b      // 減算代入
a *= b      // 乗算代入
a /= b      // 除算代入
a &= b      // ビットAND代入
a |= b      // ビットOR代入
a ^= b      // ビットXOR代入
```

### インクリメント/デクリメント

```
++a     // 前置インクリメント（式の値は変更後の値）
--a     // 前置デクリメント（式の値は変更後の値）
a++     // 後置インクリメント（式の値は変更前の値）
a--     // 後置デクリメント（式の値は変更前の値）
```

前置は変更後の値を、後置は変更前の値を式の値として返します。対象は整数型の変数のみです。

```
local int a = 1;
local int b = a++;  // a = 2, b = 1
local int c = ++a;  // a = 3, c = 3
```

---

## ホスト連携

### Peripheral（ペリフェラル）システム

C#で定義したメソッドをスクリプトから呼び出すことができます。

#### C#側の定義

```csharp
[SrPeripheral("Sample")]
public class SamplePeripheral
{
    [SrHostFunction("Add")]
    public int Add(int a, int b)
    {
        return a + b;
    }
    
    [SrHostFunction("Print")]
    public void Print(string text)
    {
        Console.WriteLine(text);
    }
    
    // プロセスIDを受け取る場合
    [SrHostFunction("GetProcessInfo")]
    public void GetProcessInfo([SrProcessID] int processId)
    {
        // processIdには呼び出し元のプロセスIDが自動注入される
    }
}
```

#### スクリプト側のインポート

```
// 書式: using エイリアス = 戻り型 Peripheral名.関数名(引数型リスト);
using Add = int Sample.Add(int, int);
using Print = void Sample.Print(string);
using GetInfo = void Sample.GetProcessInfo();  // SrProcessID引数は不要
```

#### 使用例

```
using Add = int Sample.Add(int, int);
using Print = void Sample.Print(string);

function void main()
    local int result = Add(10, 20);
    Print("Result: ");
end
```

### 対応する型マッピング

| スクリプト型 | C#型 |
|-------------|------|
| `int` | `sbyte`, `byte`, `char`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`（`int`, `long` 推奨） |
| `number` | `float`, `double` |
| `string` | `string` |
| `object` | `object`（上記以外の型も `object` として受け渡し可能） |
| `bool` | `bool` |
| `void` | `void` |

### 非同期ホスト関数（Task対応）

ホスト関数の戻り値には `Task` / `Task<T>` を使用できます。スクリプトが非同期ホスト関数を呼び出すと、
呼び出し元プロセスは `Suspended` 状態になり、タスク完了後の `Run()` 呼び出しで結果を受け取って再開します。

```csharp
[SrHostFunction("Wait")]
public Task Wait(int millisecond)
{
    return Task.Delay(millisecond);
}

[SrHostFunction("LoadText")]
public async Task<string> LoadText(string path)
{
    return await File.ReadAllTextAsync(path);
}
```

スクリプト側からは同期関数と同じように呼び出せます（`Task<string>` は `string` としてインポートします）。

```
using Wait = void Sample.Wait(int);
using LoadText = string Sample.LoadText(string);
```

---

## プリプロセッサディレクティブ

### 定数定義 (#const)

コンパイル時定数を定義します。

```
#const MAX_VALUE 100
#const PI 3.14159
#const MESSAGE "Hello"
```

使用例：
```
#const MAX_COUNT 10

function void main()
    local int i = 0;
    for (i = 0; i < MAX_COUNT; i = i + 1)
        // ...
    end
end
```

### スクリプトコンパイル (#compile)

他のスクリプトファイルをコンパイル対象に含めます。

```
#compile "utils.srs"
#compile "math.srs"
```

### リンク (#link)

事前にコンパイルされた**オブジェクトファイル（`.sro`）**をリンクして、その中の関数・グローバル変数・定数・`using` 宣言を使用できるようにします。

```
#link "library.sro"
```

#### オブジェクトファイルの作り方

`snowrabbitc` の `-c` / `--object` オプションで、スクリプトをオブジェクトファイルとしてコンパイルします。
オブジェクトとしてコンパイルする場合、`main` 関数は不要です。

```bash
snowrabbitc library.srs -c        # library.sro を出力
snowrabbitc main.srs              # main.srs 内の #link "library.sro" が解決され main.bin を出力
```

#### リンクの規則

- リンクされるもの: 関数、グローバル変数（初期化子はリンク後の起動コードで適用される）、`#const` 定数、`using` 宣言、文字列
- 同名シンボル（関数・グローバル変数・定数）が既に存在する場合はコンパイルエラー
- **同一シグネチャの `using` 宣言は共有されます**（ライブラリとメインが同じ `using` を書いてもエラーになりません。シグネチャが異なる場合はエラー）
- 同一パスの `#link` が複数回現れた場合は2回目以降がスキップされます（複数のスクリプトが同じライブラリをリンクする構成を許容）
- オブジェクト作成時の `#link` は**静的に取り込まれます**。取り込まれたライブラリを利用側が重ねてリンクするとシンボル衝突になります
- 実行バイナリ（`.bin` / SROF形式）はリンクできません。オブジェクトファイル（`.sro` / SROB形式）のみリンク可能です

---

## 文法仕様（BNF）

### リテラルと型

```bnf
literal
    : <integer>
    | <number>
    | <string>
    | 'true'
    | 'false'
    | 'null'

type
    : 'void'
    | 'int'
    | 'number'
    | 'string'
    | 'object'
    | 'bool'
```

### 宣言

```bnf
compile_unit
    : { directives }
    | { peripheral_declare }
    | { global_variable_declare }
    | { function_declare }

peripheral_declare
    : 'using' <identifier> '=' type <identifier> '.' <identifier> '(' [type_list] ')' ';'

global_variable_declare
    : 'global' type <identifier> [ '=' literal ] ';'

local_variable_declare
    : 'local' type <identifier> [ '=' expression ] ';'

function_declare
    : 'function' type <identifier> '(' [parameter_list] ')' { block } 'end'

parameter_list
    : parameter { ',' parameter }

parameter
    : type <identifier>
```

### 文

```bnf
block
    : statement

statement
    : empty_statement
    | local_variable_declare
    | for_statement
    | while_statement
    | if_statement
    | break_statement
    | return_statement
    | expression ';'

empty_statement
    : ';'

for_statement
    : 'for' '(' [ expression ] ';' [ expression ] ';' [ expression ] ')' { block } 'end'

while_statement
    : 'while' '(' expression ')' { block } 'end'

if_statement
    : 'if' '(' expression ')' { block } 'end'
    | 'if' '(' expression ')' { block } else_statement

else_statement
    : 'else' if_statement
    | 'else' { block } 'end'

break_statement
    : 'break' ';'

return_statement
    : 'return' [ expression ] ';'
```

### 式

```bnf
expression
    : assignment_expression

assignment_expression
    : condition_or_expression
    | assignment_expression '=' expression
    | assignment_expression '+=' expression
    | assignment_expression '-=' expression
    | assignment_expression '*=' expression
    | assignment_expression '/=' expression
    | assignment_expression '&=' expression
    | assignment_expression '|=' expression
    | assignment_expression '^=' expression

condition_or_expression
    : condition_and_expression
    | condition_or_expression '||' condition_and_expression

condition_and_expression
    : logical_or_expression
    | condition_and_expression '&&' logical_or_expression

logical_or_expression
    : logical_exclusive_or_expression
    | logical_or_expression '|' logical_exclusive_or_expression

logical_exclusive_or_expression
    : logical_and_expression
    | logical_exclusive_or_expression '^' logical_and_expression

logical_and_expression
    : equality_expression
    | logical_and_expression '&' equality_expression

equality_expression
    : relational_expression
    | equality_expression '==' relational_expression
    | equality_expression '!=' relational_expression

relational_expression
    : shift_expression
    | relational_expression '<' shift_expression
    | relational_expression '>' shift_expression
    | relational_expression '<=' shift_expression
    | relational_expression '>=' shift_expression

shift_expression
    : addsub_expression
    | shift_expression '<<' addsub_expression
    | shift_expression '>>' addsub_expression

addsub_expression
    : muldiv_expression
    | addsub_expression '+' muldiv_expression
    | addsub_expression '-' muldiv_expression

muldiv_expression
    : unary_expression
    | muldiv_expression '*' unary_expression
    | muldiv_expression '/' unary_expression
    | muldiv_expression '%' unary_expression

unary_expression
    : post_unary_expression
    | '+' unary_expression
    | '-' unary_expression
    | '!' unary_expression
    | '++' unary_expression
    | '--' unary_expression

post_unary_expression
    : primary_expression
    | primary_expression '(' [ argument_list ] ')'
    | post_unary_expression '++'
    | post_unary_expression '--'

primary_expression
    : literal
    | <identifier>
    | '(' expression ')'

argument_list
    : argument { ',' argument }

argument
    : expression
```

### ディレクティブ

```bnf
directives
    : '#' script_compile_directive
    | '#' link_object_directive
    | '#' constant_define_directive

script_compile_directive
    : 'compile' <string>

link_object_directive
    : 'link' <string>

constant_define_directive
    : 'const' <identifier> literal
```

---

## 制限事項

- ブロックコメント（`/* */`）は未サポート
- 剰余の複合代入（`%=`）およびシフトの複合代入（`<<=`, `>>=`）は未サポート
- 文字列と数値の混合連結（`"a" + 1`）は未サポート
- 配列は未サポート
- クラス/構造体の定義は未サポート
- 例外処理（try-catch）は未サポート

---

## サンプルプログラム

### フィボナッチ数列

```
function int Fibonacci(int n)
    if (n <= 1)
        return n;
    end
    local int previous1 = Fibonacci(n - 1);
    local int previous2 = Fibonacci(n - 2);
    return previous1 + previous2;
end

function void main()
    local int i = 0;
    for (i = 0; i < 10; i = i + 1)
        local int fib = Fibonacci(i);
        // Print(fib); // 要Peripheral
    end
end
```

### FizzBuzz

```
using Print = void Console.WriteLine(string);
using PrintInt = void Console.WriteInt(int);

function void main()
    local int i = 0;
    for (i = 1; i <= 100; i = i + 1)
        if (i % 15 == 0)
            Print("FizzBuzz");
        else if (i % 3 == 0)
            Print("Fizz");
        else if (i % 5 == 0)
            Print("Buzz");
        else
            PrintInt(i);
        end
    end
end
```
