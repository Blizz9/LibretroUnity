using System;
using System.Runtime.InteropServices;

namespace com.PixelismGames.CSLibretro.Libretro
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GameInfo
    {
        public string Path;
        public IntPtr Data;
        public uint Size;
        public string Meta;
    }
}
