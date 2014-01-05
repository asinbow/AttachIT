using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Reflection;

using System.Threading;
using System.Runtime.InteropServices; 

namespace TestDebugger
{
    class Program
    {
        protected delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        protected static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        protected static void Notify(int pid, int ppid)
        {
            EnumWindows((hWnd, lParam) =>
                {
                    int len = GetWindowTextLength(hWnd);
                    StringBuilder sb = new StringBuilder(len + 1);
                    GetWindowText(hWnd, sb, len + 1);
                    string text = sb.ToString();
                    if (text.Length > 0)
                    {
                        //Console.WriteLine(text);
                    }
                    if (text == "asinbow.AttachIT.WatchForm")
                    {
                        SendMessage(hWnd, 1024, (IntPtr)pid, (IntPtr)ppid);
                    }
                    return true;
                }, (IntPtr)0);
        }

        static void Main(string[] args)
        {
            string workerArg = "-worker";
            string path = Assembly.GetExecutingAssembly().Location;
            Console.WriteLine("{0}", args.Length);
            Thread.Sleep(2000);

            if (args.Length < 1 || args[0] != workerArg)
            {
                Debugger.Log(1, "DEBUGEE", "master started\n");

                Debugger.Log(1, "DEBUGEE", "master starting worker\n");
                Process proc = Process.Start(path, workerArg);
                Debugger.Log(1, "DEBUGEE", "master started worker\n");
                Notify(proc.Id, Process.GetCurrentProcess().Id);
            }
            else
            {
                Debugger.Log(1, "DEBUGEE", "worker started\n");
            }

            Console.ReadLine();
        }
    }
}
