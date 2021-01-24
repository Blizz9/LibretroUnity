using System.Runtime.InteropServices;

namespace com.PixelismGames.CSLibretro.Libretro
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LogCallback
    {
        public LogHandler Log;
    }
}
