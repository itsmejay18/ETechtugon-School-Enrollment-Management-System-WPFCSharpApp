using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace School_Management_System.Wpf.Services
{
    public static class WindowFullscreenHelper
    {
        private static readonly IntPtr HwndTop = IntPtr.Zero;
        private static readonly IntPtr HwndTopMost = new IntPtr(-1);

        private const uint MonitorDefaultToNearest = 2;
        private const uint SwpFrameChanged = 0x0020;
        private const uint SwpNoOwnerZOrder = 0x0200;
        private const uint SwpShowWindow = 0x0040;

        public static void ApplyMonitorBounds(Window window, bool topmost)
        {
            if (window == null)
            {
                return;
            }

            var helper = new WindowInteropHelper(window);
            if (helper.Handle == IntPtr.Zero)
            {
                return;
            }

            var monitor = MonitorFromWindow(helper.Handle, MonitorDefaultToNearest);
            var info = new MonitorInfo();
            info.cbSize = Marshal.SizeOf(typeof(MonitorInfo));

            if (monitor == IntPtr.Zero || !GetMonitorInfo(monitor, ref info))
            {
                return;
            }

            var bounds = info.rcMonitor;

            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.WindowState = WindowState.Normal;
            window.Left = bounds.Left;
            window.Top = bounds.Top;
            window.Width = Math.Max(0, bounds.Right - bounds.Left);
            window.Height = Math.Max(0, bounds.Bottom - bounds.Top);
            window.Topmost = topmost;

            SetWindowPos(
                helper.Handle,
                topmost ? HwndTopMost : HwndTop,
                bounds.Left,
                bounds.Top,
                Math.Max(0, bounds.Right - bounds.Left),
                Math.Max(0, bounds.Bottom - bounds.Top),
                SwpShowWindow | SwpFrameChanged | SwpNoOwnerZOrder);

            window.Activate();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MonitorInfo
        {
            public int cbSize;
            public Rect rcMonitor;
            public Rect rcWork;
            public uint dwFlags;
        }
    }
}
