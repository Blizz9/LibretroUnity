using System.Runtime.InteropServices;

namespace com.PixelismGames.CSLibretro.Libretro
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemAVInfo
    {
        public GameGeometry Geometry;
        public SystemTiming Timing;
    }
}
