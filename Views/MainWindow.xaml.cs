using System;
using System.Reflection;
using System.Windows;
using Aria2Manager.Models;
using Aria2Manager.Views;

namespace Aria2Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Aria2ServerModel Aria2Server { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            var app = (App)Application.Current;
            app.SetLanguageDictionary(); //设置界面语言

            //从文件读取当前服务器信息
            try
            {
                Aria2Server = new Aria2ServerModel(true);
            }
            catch
            {
                Aria2Server = new Aria2ServerModel();
                //未发现配置，提示创建服务器信息
                MessageBox.Show(Application.Current.FindResource("NoServersAvaliable").ToString(), 
                    "NoServersAvaliable", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ManageAria2ServersMenu_Click(object sender, RoutedEventArgs e)
        {
            //新建Aria2服务器管理窗口
            ManageServersWindow newWin = new ManageServersWindow(Aria2Server);
            newWin.Owner = this;
            newWin.Show();
        }

        private void AboutMenu_Click(object sender, RoutedEventArgs e)
        {
            //显示关于窗口
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var copyright = "Copyright © Ftbom";
            var email = "lz490070@gmail.com";
            MessageBox.Show(
                name + "\nVersion " + version + "\n\n" + copyright + "\n" + email,
                "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
