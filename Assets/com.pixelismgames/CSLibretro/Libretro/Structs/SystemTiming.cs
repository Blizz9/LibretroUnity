using System.Runtime.InteropServices;

namespace com.PixelismGames.CSLibretro.Libretro
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTiming
    {
        public double FPS;
        public double SampleRate;
    }
}
