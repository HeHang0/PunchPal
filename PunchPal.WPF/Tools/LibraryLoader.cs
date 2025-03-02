using PunchPal.Tools;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PunchPal.WPF.Tools
{
    public class LibraryLoader
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllName);

        public static void Inject()
        {
            if (OSVersionTools.IsWindowsNT)
            {
                if (!File.Exists(PathTools.SkiaSharpPath))
                {
                    File.WriteAllBytes(PathTools.SkiaSharpPath, Properties.Resources.SkiaSharp);
                }
                if (!File.Exists(PathTools.HarfBuzzSharpPath))
                {
                    File.WriteAllBytes(PathTools.HarfBuzzSharpPath, Properties.Resources.HarfBuzzSharp);
                }
            }
            TryLoadLibrary(PathTools.SkiaSharpPath);
            TryLoadLibrary(PathTools.HarfBuzzSharpPath);
        }

        private static void TryLoadLibrary(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }
            try
            {
                LoadLibrary(path);
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
