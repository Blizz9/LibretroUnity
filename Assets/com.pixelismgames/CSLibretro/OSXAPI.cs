using System;
using System.Runtime.InteropServices;
using System.Text;

namespace com.PixelismGames.CSLibretro
{
    public class OSXAPI
    {
		[DllImport("/usr/lib/system/libsystem_c.dylib", CallingConvention = CallingConvention.Cdecl)]
		public static extern int snprintf(StringBuilder buffer, uint count, string format, IntPtr arg0, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9);

		[DllImport ("libdl.dylib", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern IntPtr dlopen(string dllPath, int flags);

		[DllImport ("libdl.dylib", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern IntPtr dlsym(IntPtr dll, string methodName);
    }
}
