using msheadtool.Clipboard;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace msheadtool
{
    public static class ClipboardUtil
    {
        public static void SetText(string text)
        {
            if (IsWindows()) WindowsClipboard.SetText(text);
            if (IsMacOS()) OsxClipboard.SetText(text);
            if (IsLinux()) LinuxClipboard.SetText(text);
        }

        public static bool IsWindows() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}
