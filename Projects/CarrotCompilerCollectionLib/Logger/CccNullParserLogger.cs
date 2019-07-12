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

namespace CarrotCompilerCollection.Utility
{
    /// <summary>
    /// 何も処理を行わない構文解析ロガークラスです
    /// </summary>
    public class CccNullParserLogger : ICccParserLogger
    {
        /// <summary>
        /// この関数は何も行いません
        /// </summary>
        /// <param name="type">無視されます</param>
        /// <param name="lineNumber">無視されます</param>
        /// <param name="columnNumber">無視されます</param>
        /// <param name="code">無視されます</param>
        /// <param name="message">無視されます</param>
        public void Write(CccParserLogType type, int lineNumber, int columnNumber, uint code, string message)
        {
        }
    }
}