using System;
using System.Runtime.InteropServices;
using System.Text;

namespace com.PixelismGames.CSLibretro
{
    public class WindowsAPI
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllPath);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr dll, string methodName);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int _snprintf(StringBuilder buffer, uint count, string format, IntPtr arg0, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9);
    }
}
