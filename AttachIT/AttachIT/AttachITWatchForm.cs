using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Diagnostics;

namespace asinbow.AttachIT
{
    public enum AttachError
    {
        NoError = 0,
        NotWatching,
        DebuggerNotReady,
        AlreadyDebugged,
        ParentProcessNotDebugged,
        ProcessNotFound,
    };

    class AttachITWatchForm : Form
    {
        public const int WM_USER = 1024;
        public const int WM_ATTACHIT = WM_USER  + 0;
        public const string ATTACHIT_WND_TEXT = "asinbow.AttachIT.WatchForm";

        public delegate AttachError ActionDelegate(int pid, int ppid);

        private ActionDelegate _Action;

        public AttachITWatchForm(ActionDelegate action)
        {
            _Action = action;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            Text = ATTACHIT_WND_TEXT;
        }

        protected override void DefWndProc(ref Message msg)
        {
            if (msg.Msg == WM_ATTACHIT)
            {
                int pid = (int)msg.WParam;
                int ppid = (int)msg.LParam;
                AttachError error = _Action(pid, ppid);
                msg.Result = (IntPtr)error;
                return;
            }
            base.DefWndProc(ref msg);
        }

    }
}
