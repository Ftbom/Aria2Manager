using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
            UpdataDownloadItems();
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
                Process.Start(new ProcessStartInfo("https://aria2.github.io"){ UseShellExecute = true });
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

        private async void UpdataDownloadItems()
        {
            while (true)
            {
                try
                {
                    var client = new Aria2ClientModel(Aria2Server);
                    var Items = await client.Aria2Client.TellAllAsync();
                    long total_download_speed = 0;
                    long total_upload_speed = 0;
                    DownloadItems.Clear();
                    foreach (var item in Items)
                    {
                        DownloadItemModel download_item = new DownloadItemModel();
                        download_item.Gid = item.Gid;
                        if ((item.Bittorrent == null) || (item.Bittorrent.Info == null))
                        {
                            download_item.Name = Path.GetFileName(item.Files[0].Path);
                        }
                        else
                        {
                            download_item.Name = item.Bittorrent.Info.Name;
                        }
                        download_item.Size = BytesToString(item.TotalLength);
                        download_item.Progress = (double)(item.CompletedLength * 10000 / item.TotalLength) / 100;
                        download_item.Status = item.Status;
                        download_item.Downloaded = BytesToString(item.CompletedLength);
                        download_item.Uploaded = BytesToString(item.UploadLength);
                        download_item.DownloadSpeed = BytesToString(item.DownloadSpeed) + "/s";
                        download_item.UploadSpeed = BytesToString(item.UploadSpeed) + "/s";
                        total_download_speed += item.DownloadSpeed;
                        total_upload_speed += item.UploadSpeed;
                        if (item.DownloadSpeed == 0)
                        {
                            download_item.ETA = "--";
                        }
                        else
                        {
                            download_item.ETA = SecondsToString((int)((double)(item.TotalLength - item.CompletedLength) / item.DownloadSpeed));
                        }
                        if (item.CompletedLength == 0)
                        {
                            download_item.Ratio = null;
                        }
                        else
                        {
                            download_item.Ratio = (double)(item.UploadLength * 100 / item.CompletedLength) / 100;
                        }
                        download_item.Connections = item.Connections;
                        download_item.Seeds = item.NumSeeders;
                        DownloadItems.Add(download_item);
                    }
                    //UploadSpeed = BytesToString(total_upload_speed) + "/s";
                    //DownloadSpeed = BytesToString(total_download_speed) + "/s";
                }
                catch { }
                await Task.Delay(500);
            }
        }

        private string BytesToString(long byteCount)
        {
            long GBSize = 1024 * 1024 * 1024;
            long MBSize = 1024 * 1024;
            long KBSize = 1024;
            if (byteCount >= GBSize)
            {
                return ((double)(byteCount * 100 / GBSize) / 100).ToString() + "GB";
            }
            else if (byteCount >= MBSize)
            {
                return ((double)(byteCount * 100 / MBSize) / 100).ToString() + "MB";
            }
            else
            {
                return ((double)(byteCount * 100 / KBSize) / 100).ToString() + "KB";
            }
        }

        private string SecondsToString(long secCount)
        {
            string result = "";
            long HSize = 60 * 60;
            long MSize = 60;
            int h_num = (int)(secCount / HSize);
            result += h_num.ToString() + "h";
            secCount -= h_num * HSize;
            int m_num = (int)(secCount / MSize);
            result += m_num.ToString() + "m";
            secCount -= m_num * MSize;
            result += secCount.ToString() + "s";
            return result;
        }
    }
}