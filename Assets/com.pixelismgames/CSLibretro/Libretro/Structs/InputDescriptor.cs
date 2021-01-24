using System.Runtime.InteropServices;

namespace com.PixelismGames.CSLibretro.Libretro
{
    [StructLayout(LayoutKind.Sequential)]
    public struct InputDescriptor
    {
        public uint Port;
        public uint Device;
        public uint Index;
        public uint ID;
        public string Description;
    }
}
