using Aria2Manager.Utils;
using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

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
                Version = "V" + Assembly.GetExecutingAssembly().GetName().Version?.ToString()[..^2],
                Copyright = "Copyright © Ftbom"
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

        private async void InfoCheckUpdate(object sender, RoutedEventArgs e)
        {
            if (await Tools.CheckProgramUpdate())
            {
                MessageBox.Show(Application.Current.FindResource("ProgramHasUpdate").ToString());
            }
            else
            {
                MessageBox.Show(Application.Current.FindResource("NoUpdate").ToString());
            }
        }
    }
}
