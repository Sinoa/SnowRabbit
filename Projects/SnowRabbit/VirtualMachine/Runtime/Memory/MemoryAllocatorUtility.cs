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
    /// メモリアロケータの実装で使用する機能をユーティリティ化したクラスです
    /// </summary>
    public static class MemoryAllocatorUtility
    {
        /// <summary>
        /// バイトサイズから配列の要素数を求めます
        /// </summary>
        /// <remarks>この関数は 8byte ブロックサイズを既定として計算されます。別のブロックサイズを使用する場合は ByteSizeToElementCount(int, int) を参照してください</remarks>
        /// <param name="size">計算するバイト数</param>
        /// <returns>バイト数から要素数に変換した値を返します</returns>
        public static int ByteSizeToElementCount(int size)
        {
            // パディング込みの計算式例（ビット演算ではなく汎用計算式の場合）
            // paddingSize = (blockSize - (dataSize % blockSize)) % blockSize
            // finalSize = dataSize + paddingSize
            // blockCount = finalSize / blockSize
            // サイズから確保するべき配列の要素数を求める（1ブロック8byteなので8byte倍数に収まるようにしてから求める）
            return (size + ((8 - (size & 7)) & 7)) >> 3;
        }


        /// <summary>
        /// 配列の要素数からバイトサイズを求めます
        /// </summary>
        /// <remarks>この関数は 8byte ブロックサイズを既定として計算されます。別のブロックサイズを使用する場合は ElementCountToByteSize(int, int) を参照してください</remarks>
        /// <param name="count">計算する要素数</param>
        /// <returns>要素数からバイト数に変換した値を返します</returns>
        public static int ElementCountToByteSize(int count)
        {
            // ブロックサイズは8byteなのでそのまま8倍にして返す
            return count << 3;
        }


        /// <summary>
        /// バイトサイズから配列の要素数を求めます
        /// </summary>
        /// <param name="size">計算するバイト数</param>
        /// <param name="blockSize">1要素のブロックサイズ</param>
        /// <returns>バイト数から要素数に変換した値を返します</returns>
        public static int ByteSizeToElementCount(int size, int blockSize)
        {
            // パディング込みの計算式例（ビット演算ではなく汎用計算式の場合）
            // paddingSize = (blockSize - (dataSize % blockSize)) % blockSize
            // finalSize = dataSize + paddingSize
            // blockCount = finalSize / blockSize
            // サイズから確保するべき配列の要素数を求める（1ブロック8byteなので8byte倍数に収まるようにしてから求める）
            return (size + ((blockSize - (size % blockSize)) % blockSize)) / blockSize;
        }


        /// <summary>
        /// 配列の要素数からバイトサイズを求めます
        /// </summary>
        /// <param name="count">計算する要素数</param>
        /// <param name="blockSize">1要素のブロックサイズ</param>
        /// <returns>要素数からバイト数に変換した値を返します</returns>
        public static int ElementCountToByteSize(int count, int blockSize)
        {
            // ブロックサイズの倍数をそのまま計算して返す
            return count * blockSize;
        }
    }
}