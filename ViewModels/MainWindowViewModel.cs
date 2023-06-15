using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Aria2Manager.Models;
using Aria2Manager.Utils;

namespace Aria2Manager.ViewModels
{
    internal class MainWindowViewModel
    {
        public Aria2ServerModel Aria2Server { get; set; }
        public string Connected //服务器状态，颜色
        {
            get
            {
                if (server_connected)
                {
                    return "Green";
                }
                else
                {
                    return "Red";
                }
            }
            private set { }
        }
        public string? UploadSpeed { get; set; }
        public string? DownloadSpeed { get; set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand OpenAria2WebsiteCommand { get; private set; }
        public ObservableCollection<DownloadItemModel> DownloadItems { get; set; }

        private bool server_connected;

        public MainWindowViewModel(Aria2ServerModel? aria2_server = null)
        {
            if (aria2_server == null)
            {
                Aria2Server = new Aria2ServerModel();
            }
            else
            {
                Aria2Server = aria2_server;
            }
            ExitCommand = new RelayCommand(Exit);
            OpenAria2WebsiteCommand = new RelayCommand(OpenAria2Website);
            //TODO:判断服务器可连接状态
            server_connected = true;
            DownloadItems = new ObservableCollection<DownloadItemModel>();
        }

        private void Exit(object? parameter)
        {
            //退出程序
            Application.Current.Shutdown();
        }

        //打开Aria2网站
        private void OpenAria2Website(object? parameter)
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo("https://aria2.github.io"){ UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
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
    }
}