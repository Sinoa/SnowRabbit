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

namespace SnowRabbit.RuntimeEngine.VirtualMachine
{
    /// <summary>
    /// SnowRabbit が実装する仮想マシンプロセッサクラスです
    /// </summary>
    public class SrvmProcessor : SrvmMachineParts
    {
        // 定数定義
        public const byte RegisterAIndex = 0; // General Accumulator Register[A]
        public const byte RegisterBIndex = 1; // General Base Register[B]
        public const byte RegisterCIndex = 2; // General Counter Register[C]
        public const byte RegisterDIndex = 3; // General Data Register[D]
        public const byte RegisterSIIndex = 4; // General SourceIndex Register[SI]
        public const byte RegisterDIIndex = 5; // General DestinationIndex Register[DI]
        public const byte RegisterBPIndex = 6; // General BasePointer Register[BP]
        public const byte RegisterSPIndex = 7; // General StackPointer Register[SP]
        public const byte RegisterR8Index = 8; // FullGeneral Register[R8]
        public const byte RegisterR9Index = 9; // FullGeneral Register[R9]
        public const byte RegisterR10Index = 10; // FullGeneral Register[R10]
        public const byte RegisterR11Index = 11; // FullGeneral Register[R11]
        public const byte RegisterR12Index = 12; // FullGeneral Register[R12]
        public const byte RegisterR13Index = 13; // FullGeneral Register[R13]
        public const byte RegisterR14Index = 14; // FullGeneral Register[R14]
        public const byte RegisterR15Index = 15; // FullGeneral Register[R15]
        public const byte RegisterIPIndex = 30; // InstructionPointer Register[IP]
        public const byte RegisterZeroIndex = 31; // Zero Register[ZERO]
        public const byte RegisterInvalidIndex = 31; // Invalid Register[INVALID]
    }
}