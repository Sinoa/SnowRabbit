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

using System.IO;

namespace SnowRabbit.Machine
{
    /// <summary>
    /// 非常に単純な構成で実装された仮想マシンストレージクラスです
    /// </summary>
    public class SrvmSimplyStorage : SrvmStorage
    {
        // メンバ変数定義
        private DirectoryInfo baseDirectoryInfo;



        /// <summary>
        /// SrvmSimplyStorage クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="baseDirectoryPath">基準となる、スクリプトを格納しているディレクトリパス</param>
        public SrvmSimplyStorage(string baseDirectoryPath)
        {
            // ディレクトリ情報を生成
            baseDirectoryInfo = new DirectoryInfo(baseDirectoryPath);
        }


        /// <summary>
        /// 指定されたストリームを閉じます
        /// </summary>
        /// <param name="stream">閉じるストリーム</param>
        protected internal override void Close(Stream stream)
        {
            // リソースを破棄するだけ
            stream.Dispose();
        }


        /// <summary>
        /// 指定されたパスのスクリプトを開きます
        /// </summary>
        /// <param name="path">開くスクリプトパス</param>
        /// <returns>開いたスクリプトのストリームを返しますが、見つからなければ null を返します</returns>
        protected internal override Stream Open(string path)
        {
            // 開くファイルパスの再生成
            path = Path.Combine(baseDirectoryInfo.FullName, path);


            // そもそもディレクトリまたはファイルが存在しないなら
            baseDirectoryInfo.Refresh();
            if (baseDirectoryInfo.Exists || !File.Exists(Path.Combine(baseDirectoryInfo.FullName, path)))
            {
                // null を返す
                return null;
            }


            // ファイルを開いて返す
            return File.OpenRead(path);
        }
    }
}