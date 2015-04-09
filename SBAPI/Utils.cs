using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace RZSB.Util {
    internal class Utils {
        public static void print(object o) {
#if TRACE
            System.Console.Write(o);
#endif
        }

        public static void println(object o) {
#if TRACE
            System.Console.WriteLine(o);
#endif
        }

        public static void printf(string format, params object[] data) {
#if TRACE
            System.Console.WriteLine(string.Format(format, data));
#endif
        }
        public static void print(object o, ConsoleColor foreground, ConsoleColor background) {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            print(0);
            Console.ResetColor();
        }
        public static void print(object o, ConsoleColor foreground) {
            print(o, foreground, Console.BackgroundColor);
        }

        public static void println(object o, ConsoleColor foreground, ConsoleColor background) {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            println(o);
            Console.ResetColor();
        }
        public static void println(object o, ConsoleColor foreground) {
            println(o, foreground, Console.BackgroundColor);
        }
        internal class NativeMethods {
            [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
            internal static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);
        }
    }
}
