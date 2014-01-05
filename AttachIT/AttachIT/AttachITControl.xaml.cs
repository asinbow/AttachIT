using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace asinbow.AttachIT
{
    /// <summary>
    /// Interaction logic for AttachITControl.xaml
    /// </summary>
    public partial class AttachITControl : UserControl
    {
        public AttachITControl(AttachITToolWindow toolWindow)
        {
            _ToolWindow = toolWindow;
            InitializeComponent();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]

        private AttachITToolWindow _ToolWindow;

        protected AttachITPackage Package
        {
            get
            {
                return _ToolWindow.GetPackage();
            }
        }

        private void KeepWatching_Checked(object sender, RoutedEventArgs e)
        {
            if (Package != null)
            {
                Package.EnableWatch(true);
            }
        }
        private void KeepWatching_Unchecked(object sender, RoutedEventArgs e)
        {
            Package.EnableWatch(false);
        }

        private void ShowWatchForm_Checked(object sender, RoutedEventArgs e)
        {
            Package.ShowWatchForm(true);
        }
        private void ShowWatchForm_Unchecked(object sender, RoutedEventArgs e)
        {
            Package.ShowWatchForm(false);
        }
    }
}