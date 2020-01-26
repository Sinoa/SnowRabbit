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

namespace SnowRabbit.Compiler.Lexer
{
    /// <summary>
    /// トークンの種別を表現した定数を保持しているクラスです。
    /// </summary>
    /// <remarks>
    /// 独自の追加トークンを定義する場合は、このクラスを継承して追加の定義を行って下さい。
    /// ただし、必ず UserDefineOffset 以上の正の整数を用いて下さい
    /// </remarks>
    public abstract class TokenKind
    {
        #region Special [0 - -99]
        /// <summary>
        /// 追加のトークン定義を行うためのオフセット
        /// </summary>
        public const int UserDefineOffset = 1000;

        /// <summary>
        /// 不明なトークン(通常は無効値として扱われ無効な識別子としても扱います)
        /// invalid token kind and invalid identifier.
        /// </summary>
        public const int Unknown = 0;

        /// <summary>
        /// これ以上のトークンは存在しないトークン
        /// </summary>
        public const int EndOfToken = -1;

        /// <summary>
        /// 行末を示すトークン
        /// </summary>
        public const int EndOfLine = -2;
        #endregion

        #region Typical [-100 - -499]
        /// <summary>
        /// 識別子
        /// identifier | hogemoge | _identifier | hoge123
        /// </summary>
        public const int Identifier = -100;

        /// <summary>
        /// 文字列
        /// "a" or "abc" or 'a' or 'abc'
        /// </summary>
        public const int String = -101;

        /// <summary>
        /// 整数
        /// 1234 | 0xABCD
        /// </summary>
        public const int Integer = -102;

        /// <summary>
        /// 実数
        /// 1234.0 | 1234.56
        /// </summary>
        public const int Number = -103;
        #endregion

        #region SingleSymbol [-500 - -599]
        /// <summary>
        /// オープンパーレン
        /// (
        /// </summary>
        public const int OpenParen = -500;

        /// <summary>
        /// クローズパーレン
        /// )
        /// </summary>
        public const int CloseParen = -501;

        /// <summary>
        /// オープンアングル
        /// <
        /// </summary>
        public const int OpenAngle = -502;

        /// <summary>
        /// クローズアングル
        /// >
        /// </summary>
        public const int CloseAngle = -503;

        /// <summary>
        /// オープンブラケット
        /// [
        /// </summary>
        public const int OpenBracket = -504;

        /// <summary>
        /// クローズブラケット
        /// ]
        /// </summary>
        public const int CloseBracket = -505;

        /// <summary>
        /// オープンブレス
        /// {
        /// </summary>
        public const int OpenBrace = -506;

        /// <summary>
        /// クローズブレス
        /// }
        /// </summary>
        public const int CloseBrace = -507;

        /// <summary>
        /// コロン
        /// :
        /// </summary>
        public const int Colon = -508;

        /// <summary>
        /// セミコロン
        /// ;
        /// </summary>
        public const int Semicolon = -509;

        /// <summary>
        /// シャープ
        /// #
        /// </summary>
        public const int Sharp = -510;

        /// <summary>
        /// カンマ
        /// ,
        /// </summary>
        public const int Comma = -511;

        /// <summary>
        /// ピリオド
        /// .
        /// </summary>
        public const int Period = -512;

        /// <summary>
        /// イコール
        /// =
        /// </summary>
        public const int Equal = -513;

        /// <summary>
        /// プラス
        /// +
        /// </summary>
        public const int Plus = -514;

        /// <summary>
        /// マイナス
        /// -
        /// </summary>
        public const int Minus = -515;

        /// <summary>
        /// アスタリスク
        /// *
        /// </summary>
        public const int Asterisk = -516;

        /// <summary>
        /// スラッシュ
        /// /
        /// </summary>
        public const int Slash = -517;

        /// <summary>
        /// パーセント
        /// %
        /// </summary>
        public const int Percent = -518;

        /// <summary>
        /// エクスクラメーション
        /// !
        /// </summary>
        public const int Exclamation = -519;

        /// <summary>
        /// クエスチョン
        /// ?
        /// </summary>
        public const int Question = -520;

        /// <summary>
        /// バーティカルバー
        /// |
        /// </summary>
        public const int Verticalbar = -521;

        /// <summary>
        /// アンド
        /// &
        /// </summary>
        public const int And = -522;

        /// <summary>
        /// ドル
        /// $
        /// </summary>
        public const int Dollar = -523;

        /// <summary>
        /// サーカムフレックス
        /// ^
        /// </summary>
        public const int Circumflex = -524;

        /// <summary>
        /// チルダ
        /// ~
        /// </summary>
        public const int Tilde = -525;

        /// <summary>
        /// アットサイン
        /// @
        /// </summary>
        public const int AtSign = -526;
        #endregion

        #region DoubleSymbol [-600 - -699]
        /// <summary>
        /// ダブルイコール
        /// ==
        /// </summary>
        public const int DoubleEqual = -600;

        /// <summary>
        /// ノットイコール
        /// !=
        /// </summary>
        public const int NotEqual = -601;

        /// <summary>
        /// レッサーイコール
        /// <=
        /// </summary>
        public const int LesserEqual = -602;

        /// <summary>
        /// グレイターイコール
        /// >=
        /// </summary>
        public const int GreaterEqual = -603;

        /// <summary>
        /// プラスイコール
        /// +=
        /// </summary>
        public const int PlusEqual = -604;

        /// <summary>
        /// マイナスイコール
        /// -=
        /// </summary>
        public const int MinusEqual = -605;

        /// <summary>
        /// アスタリスクイコール
        /// *=
        /// </summary>
        public const int AsteriskEqual = -606;

        /// <summary>
        /// スラッシュイコール
        /// /=
        /// </summary>
        public const int SlashEqual = -607;

        /// <summary>
        /// アンドイコール
        /// &=
        /// </summary>
        public const int AndEqual = -608;

        /// <summary>
        /// バーティカルバーイコール
        /// |=
        /// </summary>
        public const int VerticalbarEqual = -609;

        /// <summary>
        /// サーカムフレックスイコール
        /// ^=
        /// </summary>
        public const int CircumflexEqual = -610;

        /// <summary>
        /// 右矢印
        /// ->
        /// </summary>
        public const int RightArrow = -612;

        /// <summary>
        /// 左矢印
        /// <-
        /// </summary>
        public const int LeftArrow = -613;

        /// <summary>
        /// ダブルアンド
        /// &&
        /// </summary>
        public const int DoubleAnd = -614;

        /// <summary>
        /// ダブルバーティカルバー
        /// ||
        /// </summary>
        public const int DoubleVerticalbar = -615;

        /// <summary>
        /// ダブルオープンアングル
        /// <<
        /// </summary>
        public const int DoubleOpenAngle = -616;

        /// <summary>
        /// ダブルクローズアングル
        /// >>
        /// </summary>
        public const int DoubleCloseAngle = -617;

        /// <summary>
        /// ダブルプラス
        /// ++
        /// </summary>
        public const int DoublePlus = -618;

        /// <summary>
        /// ダブルマイナス
        /// --
        /// </summary>
        public const int DoubleMinus = -619;
        #endregion
    }
}
