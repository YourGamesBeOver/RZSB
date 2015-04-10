using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

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

        public static Font FindFont(System.Drawing.Graphics g, string longString, Size Room, Font PreferedFont) {
            if (String.IsNullOrWhiteSpace(longString)) return PreferedFont;
            //you should perform some scale functions!!!
            SizeF RealSize = g.MeasureString(longString, PreferedFont);
            float HeightScaleRatio = Room.Height / RealSize.Height;
            float WidthScaleRatio = Room.Width / RealSize.Width;
            float ScaleRatio = (HeightScaleRatio < WidthScaleRatio) ? ScaleRatio = HeightScaleRatio : ScaleRatio = WidthScaleRatio;
            float ScaleFontSize = PreferedFont.Size * ScaleRatio;
            return new Font(PreferedFont.FontFamily, ScaleFontSize, PreferedFont.Style);
        }

        internal class NativeMethods {
            [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
            internal static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);
        }
    }
}
