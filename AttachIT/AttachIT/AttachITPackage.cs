using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace asinbow.AttachIT
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(AttachITToolWindow))]
    [Guid(GuidList.guidAttachITPkgString)]
    public sealed class AttachITPackage : Package
    {

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public AttachITPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(AttachITToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidAttachITCmdSet, (int)PkgCmdIDList.cmdidAttachIT);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID );
                mcs.AddCommand( menuItem );
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidAttachITCmdSet, (int)PkgCmdIDList.cmdidAttachITWindow);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );
            }

            _dte = (EnvDTE80.DTE2)GetService(typeof(EnvDTE.DTE));

            _Form = new AttachITWatchForm(OnWatched);
            _Form.ControlBox = false;
            _Form.Visible = false;
            _Form.Show();
            _Form.Hide();
        }

        private EnvDTE80.DTE2 _dte;
        private AttachITWatchForm _Form;
        private bool Watching = true;

        public EnvDTE80.DTE2 GetDTE()
        {
            return _dte;
        }

        private AttachError MatchProcess(int pid, int ppid, out EnvDTE.Process proc, out EnvDTE.Process pproc)
        {
            proc = null;
            pproc = null;
            if (!Watching)
            {
                return AttachError.NotWatching;
            }
            EnvDTE.Processes procs = _dte.Debugger.DebuggedProcesses;
            if (procs == null)
            {
                return AttachError.DebuggerNotReady;
            }

            bool ppidMatched = ppid == 0;
            foreach (EnvDTE.Process p in _dte.Debugger.DebuggedProcesses)
            {
                if (p.ProcessID == pid)
                {
                    return AttachError.AlreadyDebugged;
                }
                if (p.ProcessID == ppid)
                {
                    ppidMatched = true;
                    pproc = p;
                }
            }
            if (!ppidMatched)
            {
                return AttachError.ParentProcessNotDebugged;
            }
            foreach (EnvDTE.Process p in _dte.Debugger.LocalProcesses)
            {
                if (p.ProcessID == pid)
                {
                    proc = p;
                    return AttachError.NoError;
                }
            }

            return AttachError.ProcessNotFound;
        }

        private AttachError OnWatched(int pid, int ppid)
        {
            EnvDTE.Process proc;
            EnvDTE.Process pproc;
            AttachError error = MatchProcess(pid, ppid, out proc, out pproc);
            if (error == AttachError.NoError)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    EnvDTE.Process _proc;
                    EnvDTE.Process _pproc;
                    AttachError _error = MatchProcess(pid, ppid, out _proc, out _pproc);
                    if (error == AttachError.NoError)
                    {
                        try
                        {
                            _proc.Attach();
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Exception when attaching process");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Process not found when recheck.");
                    }
                });
            }
            else
            {
                Console.WriteLine("Process not found.");
            }
            return error;
        }

        public void ShowWatchForm(bool show)
        {
            if (show)
            {
                _Form.Show();
            }
            else
            {
                _Form.Hide();
            }
        }

        public void EnableWatch(bool enable)
        {
            Watching = enable;
        }

        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            ShowToolWindow(sender, e);
        }

    }
}
