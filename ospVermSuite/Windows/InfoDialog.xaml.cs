using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ospVermSuite.Windows
{
    /// <summary>
    /// Interaktionslogik für Info.xaml
    /// </summary>
    public partial class InfoDialog : Window
    {
        public InfoDialog()
        {
            InitializeComponent();
            VersionText.Text = Assembly.LoadFrom("ospVermSuite.dll").GetName().Version.ToString();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessStartInfo info = new ProcessStartInfo(e.Uri.ToString());
            info.UseShellExecute = true;
            System.Diagnostics.Process.Start(info);
        }
    }
}
