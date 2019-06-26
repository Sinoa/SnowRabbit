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
using SnowRabbit.Machine;
using SnowRabbit.Runtime;
using UnityEngine;

namespace SrUnityFramework.Base
{
    /// <summary>
    /// Unity向け雪兎仮想マシンのマシンクラスです
    /// </summary>
    public class SrvmUnityMachine : SrvmMachine
    {
        /// <summary>
        /// SrvmUnityMachine クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="objectMemorySize">オブジェクトメモリに割り当てるメモリのバイトサイズ</param>
        /// <param name="valueMemorySize">メインメモリに割り当てるメモリのバイトサイズ</param>
        public SrvmUnityMachine(int objectMemorySize, int valueMemorySize)
            : base(new SrvmUnityProcessor(), new SrvmUnityFirmware(), new SrvmUnityMemory(objectMemorySize, valueMemorySize), new SrvmUnityStorage())
        {
        }
    }



    /// <summary>
    /// Unity向け雪兎仮想マシンのプロセッサクラスです
    /// </summary>
    public class SrvmUnityProcessor : SrvmProcessor
    {
    }



    /// <summary>
    /// Unity向け雪兎仮想マシンのファームウェアクラスです
    /// </summary>
    public class SrvmUnityFirmware : SrvmFirmware
    {
    }



    /// <summary>
    /// Unity向け雪兎仮想マシンのメモリクラスです
    /// </summary>
    public class SrvmUnityMemory : SrvmMemory
    {
        // メンバ変数定義
        private StandardSrObjectAllocator objectAllocator;
        private StandardSrValueAllocator valueAllocator;



        /// <summary>
        /// SrvmUnityMemory クラスのインスタンスを初期化します
        /// </summary>
        /// <param name="objectMemorySize">オブジェクトメモリに割り当てるメモリのバイトサイズ</param>
        /// <param name="valueMemorySize">メインメモリに割り当てるメモリのバイトサイズ</param>
        public SrvmUnityMemory(int objectMemorySize, int valueMemorySize)
        {
            // アロケータを生成する
            objectAllocator = new StandardSrObjectAllocator(objectMemorySize);
            valueAllocator = new StandardSrValueAllocator(valueMemorySize);
        }


        /// <summary>
        /// オブジェクトメモリを確保します
        /// </summary>
        /// <param name="count">確保するオブジェクトの数（数でありサイズではありません）</param>
        /// <param name="type">確保する理由となった確保タイプ</param>
        /// <returns>確保したオブジェクトメモリブロックを返します</returns>
        public override MemoryBlock<SrObject> AllocateObject(int count, AllocationType type)
        {
            // オブジェクトメモリアロケータからメモリを確保して返す
            return objectAllocator.Allocate(count, type);
        }


        /// <summary>
        /// メインメモリを確保します
        /// </summary>
        /// <param name="size">確保するサイズ</param>
        /// <param name="type">確保する理由となった確保タイプ</param>
        /// <returns>確保したメインメモリブロックを返します</returns>
        public override MemoryBlock<SrValue> AllocateValue(int size, AllocationType type)
        {
            // メインメモリアロケータからメモリを確保して返す
            return valueAllocator.Allocate(size, type);
        }


        /// <summary>
        /// オブジェクトメモリを解放します
        /// </summary>
        /// <param name="memoryBlock">解放するオブジェクトメモリブロック</param>
        public override void DeallocateObject(MemoryBlock<SrObject> memoryBlock)
        {
            // オブジェクトメモリアロケータにメモリを返す
            objectAllocator.Deallocate(memoryBlock);
        }


        /// <summary>
        /// メインメモリを解放します
        /// </summary>
        /// <param name="memoryBlock">解放するメインメモリブロック</param>
        public override void DeallocateValue(MemoryBlock<SrValue> memoryBlock)
        {
            // メインメモリにメモリを返す
            valueAllocator.Deallocate(memoryBlock);
        }
    }



    /// <summary>
    /// Unity向け雪兎仮想マシンのストレージクラスです
    /// </summary>
    public class SrvmUnityStorage : SrvmStorage
    {
        /// <summary>
        /// 指定されたパスからストリームを開きます
        /// </summary>
        /// <param name="path">ストリームを開く対象のパス</param>
        /// <returns>開かれたストリームを返します</returns>
        protected override Stream Open(string path)
        {
            // 指定されたパスからResourcesロードをしてメモリストリームとして返す
            var textAsset = Resources.Load<TextAsset>(path);
            return new MemoryStream(textAsset.bytes);
        }


        /// <summary>
        /// 指定されたストリームを閉じます
        /// </summary>
        /// <param name="stream">閉じるストリーム</param>
        protected override void Close(Stream stream)
        {
            // 素直に破棄する
            stream.Dispose();
        }
    }
}