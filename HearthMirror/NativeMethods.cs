using System;
using System.Runtime.InteropServices;

namespace HearthMirror
{
  internal static class NativeMethods
  {
    [DllImport("kernel32", SetLastError = true)]
    internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, IntPtr nSize, out IntPtr lpNumberOfBytesRead);
  }
}
