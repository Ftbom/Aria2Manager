using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace Aria2Manager.Views
{
    /// <summary>
    /// ProgramInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProgramInfoWindow : MetroWindow
    {
        public ProgramInfoWindow()
        {
            InitializeComponent();
            DataContext = new
            {
                AppName = Assembly.GetExecutingAssembly().GetName().Name,
                Copyright = "Copyright © Ftbom",
            };
        }

        private void GoWebsite(string web_url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(web_url) { UseShellExecute = true });
            }
            catch (Win32Exception noBrowser)
            {
                //未发现浏览器
                if (noBrowser.ErrorCode == -2147467259)
                {
                    MessageBox.Show(noBrowser.Message);
                }
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void AppIcon_Click(object sender, MouseButtonEventArgs e)
        {
            GoWebsite("https://github.com/Ftbom/Aria2Manager");
        }

        private void Github_Click(object sender, MouseButtonEventArgs e)
        {
            GoWebsite("https://github.com/Ftbom");
        }
    }
}
