using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReadVolumeFile
{
    internal static class NativeMethods
    {
        internal enum DwFlags
        {
            /// <summary>
            /// Throws error if specified, in case allocation fails.
            /// If not specified & allocation failed, return would be null
            /// </summary>
            HEAP_GENERATE_EXCEPTIONS = 0x00000004,
            
            /// <summary>
            /// Removes support for serialize funactionality
            /// </summary>
            HEAP_NO_SERIALIZE = 0x00000001,
            
            /// <summary>
            /// Zeros memory while allocating
            /// </summary>
            HEAP_ZERO_MEMORY = 0x00000008
        }

        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcessHeap();
    }
}