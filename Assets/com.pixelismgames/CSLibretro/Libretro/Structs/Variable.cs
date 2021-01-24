using System.Runtime.InteropServices;

namespace com.PixelismGames.CSLibretro.Libretro
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Variable
    {
        public string Key;
        public string Value;
    }
}
