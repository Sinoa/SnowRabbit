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

namespace SnowRabbit.VirtualMachine.Runtime
{
    /// <summary>
    /// UnityおよびC#でも共通としてアサートを呼べるようにしたアサーションクラスです
    /// </summary>
    public static class SrAssertions
    {
        /// <summary>
        /// condition が false の場合、指定されたメッセージを出力してアサートログを表示します
        /// </summary>
        /// <param name="condition">本来正常とされるべき評価する式の結果</param>
        /// <param name="message">アサートした場合の出力するメッセージ</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
#if UNITY_ASSERTIONS
            // Unityアサートが対応している場合はUnityアサーションを利用する
            UnityEngine.Assertions.Assert.IsTrue(condition, message);
#else
            // Unity以外からの場合のアサート実装を利用する
            System.Diagnostics.Debug.Assert(condition, message);
#endif
        }
    }
}
