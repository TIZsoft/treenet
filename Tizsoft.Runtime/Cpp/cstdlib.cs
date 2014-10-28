using System;
using System.Runtime.InteropServices;

namespace Tizsoft.Runtime.Cpp
{
    public static class cstdlib
    {
        [DllImport(DllConfig.CstdlibDllName, EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dst, IntPtr src, ulong count);
    }
}
