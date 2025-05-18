using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Xml;
using Aria2Manager.Models;
using Aria2Manager.ViewModels;
using Aria2Manager.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Aria2Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Aria2ServerInfoModel Aria2Server { get; set; }
        private bool close_to_exit;
        private MainWindowViewModel model;

        public MainWindow(bool CloseToExit = false, int Aria2PID = 0)
        {
            InitializeComponent();
            //从文件读取当前服务器信息
            try
            {
                Aria2Server = new Aria2ServerInfoModel(true);
            }
            catch
            {
                Aria2Server = new Aria2ServerInfoModel();
                //未发现配置，提示创建服务器信息
                MessageBox.Show(Application.Current.FindResource("NoServersAvaliable").ToString(),
                    Application.Current.FindResource("NoServer").ToString(),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            model = new MainWindowViewModel(Aria2Server, Aria2PID, DialogCoordinator.Instance);
            DataContext = model;
            close_to_exit = CloseToExit;
        }

        private void ManageAria2ServersMenu_Click(object sender, RoutedEventArgs e)
        {
            //新建Aria2服务器管理窗口
            ManageServersWindow newWin = new ManageServersWindow(Aria2Server);
            newWin.Owner = this;
            newWin.ShowDialog();
            model.ServerNames = new ObservableCollection<string>();
            XmlDocument doc = new XmlDocument();
            doc.Load("Configurations\\Aria2Servers.xml");
            var current = doc.SelectSingleNode($"/Servers/Avaliable");
            foreach (string name in current.InnerText.Split(','))
            {
                model.ServerNames.Add(name);
            }
            model.CurrentServerName = doc.SelectSingleNode("/Servers/Current").InnerText;
        }

        private void AboutMenu_Click(object sender, RoutedEventArgs e)
        {
            //显示关于窗口
            var infoWindow = new ProgramInfoWindow();
            infoWindow.Owner = this;
            infoWindow.ShowDialog();
        }

        //异步
        private async void Aria2Info_Click(object sender, RoutedEventArgs e)
        {
            string? aria2VersionStr = Application.Current.FindResource("Aria2Version").ToString();
            string? aria2FeaturesStr = Application.Current.FindResource("Aria2Features").ToString();
            var client = new Aria2ClientModel(Aria2Server);
            var aria2Version = await client.Aria2Client.GetVersionAsync();
            var infoWin = new Aria2InfoWindow(
                aria2Version.Version,
                aria2Version.EnabledFeatures,
                aria2VersionStr??"",
                aria2FeaturesStr??""
            );
            infoWin.Owner = this;
            infoWin.ShowDialog();
        }

        private void AddNewItem(object sender, RoutedEventArgs e)
        {
            //新建添加下载窗口
            AddNewItemWindow newWin = new AddNewItemWindow(Aria2Server);
            newWin.Owner = this;
            newWin.ShowDialog();
        }

        private void ProgramSettingsMenu_Checked(object sender, RoutedEventArgs e)
        {
            //新建程序设置窗口
            SettingsWindow newWin = new SettingsWindow();
            newWin.Owner = this;
            newWin.ShowDialog();
        }

        private void mainwindow_Closed(object sender, EventArgs e)
        {
            if (close_to_exit)
            {
                Application.Current.Shutdown(); //退出程序
            }
        }

        private void Aria2SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            GlobalOptionsWindow newWin = new GlobalOptionsWindow(Aria2Server);
            newWin.Owner = this;
            newWin.ShowDialog();
        }
    }
}
