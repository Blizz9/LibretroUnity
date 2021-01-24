using System.Runtime.InteropServices;

namespace com.PixelismGames.CSLibretro.Libretro
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GameGeometry
    {
        public uint BaseWidth;
        public uint BaseHeight;
        public uint MaxWidth;
        public uint MaxHeight;
        public float AspectRatio;
    }
}
