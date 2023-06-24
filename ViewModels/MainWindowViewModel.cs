using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Aria2Manager.Models;
using Aria2Manager.Utils;
using Aria2Manager.Views;

namespace Aria2Manager.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Aria2ServerModel Aria2Server { get; set; }
        public string Connected //服务器状态，颜色
        {
            get
            {
                return _connected;
            }
            private set
            {
                if (_connected != value)
                {
                    _connected = value;
                    OnPropertyChanged();
                }
            }
        }
        public string UploadSpeed //上传速度
        {
            get
            {
                return _uploadspeed;
            }
            set
            {
                _uploadspeed = value;
                OnPropertyChanged();
            }
        }
        public string DownloadSpeed //下载速度
        {
            get
            {
                return _downloadspeed;
            }
            set
            {
                _downloadspeed = value;
                OnPropertyChanged();
            }
        }
        public DownloadItemModel? SelectedItem
        {
            get
            {
                return _selecteditem;
            }
            set
            {
                _selecteditem = value;
                OnPropertyChanged();
            }
        }
        public string? SelectedGid { get; set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand OpenAria2WebsiteCommand { get; private set; }
        public ICommand ChosenStatusChangedCommand { get; private set; }
        public ICommand RemoveItemCommand { get; private set; }
        public ICommand PauseItemCommand { get; private set; }
        public ICommand ResumeItemCommand { get; private set; }
        public ICommand ManageAllItemCommand { get; private set; }
        public ICommand OnCloseWindowCommand { get; private set; }
        public ICommand ItemInfoCommand { get; private set; }
        public List<DownloadItemModel> DownloadItems //下载项
        {
            get => _downloaditems;
            set
            {
                _downloaditems = value;
                OnPropertyChanged();
            }
        }
        public string CurrentChosenStatus { get; set; }
        public bool UpdateItems; //是否退出更新下载项

        private string _connected;
        private string _downloadspeed;
        private string _uploadspeed;
        private List<DownloadItemModel> _downloaditems;
        private DownloadItemModel? _selecteditem;

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
            ChosenStatusChangedCommand = new RelayCommand(ChosenStatusChanged);
            RemoveItemCommand = new RelayCommand(RemoveItem);
            PauseItemCommand = new RelayCommand(PauseItem);
            ResumeItemCommand = new RelayCommand(ResumeItem);
            ManageAllItemCommand = new RelayCommand(ManageAllItem);
            OnCloseWindowCommand = new RelayCommand(OnCloseWindow);
            ItemInfoCommand = new RelayCommand(ItemInfo);
            //TODO:判断服务器可连接状态
            _connected = "Green";
            _downloaditems = new List<DownloadItemModel>();
            CurrentChosenStatus = "all";
            _uploadspeed = "0KB/s";
            _downloadspeed = "0KB/s";
            UpdateItems = true;
            UpdataDownloadItems();
        }

        private void Exit(object? parameter)
        {
            //退出程序
            Application.Current.Shutdown();
        }

        private void ItemInfo(object? parameter)
        {
            //新建添加下载窗口
            ItemInfoWindow newWin = new ItemInfoWindow(SelectedGid, Aria2Server);
            newWin.Owner = (MainWindow)parameter;
            newWin.ShowDialog();
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

        private void OnCloseWindow(object? parameter)
        {
            UpdateItems = false;
        }

        //更新筛选条件
        private void ChosenStatusChanged(object? parameter)
        {
            TextBlock item = (TextBlock)parameter;
            CurrentChosenStatus = item.Name.ToLower();
        }

        //操作所有项
        private void ManageAllItem(object? parameter)
        {
            if (parameter == null)
            {
                return;
            }
            var client = new Aria2ClientModel(Aria2Server);
            string cmd_num = (string)parameter;
            if (cmd_num == "0")
            {
                client.Aria2Client.PauseAllAsync();
            }
            else if (cmd_num == "1")
            {
                client.Aria2Client.UnpauseAllAsync();
            }
            else if (cmd_num == "2")
            {
                client.Aria2Client.PurgeDownloadResultAsync();
            }
        }

        //删除项
        private void RemoveItem(object? parameter)
        {
            if (parameter == null)
            {
                return;
            }
            DownloadItemModel item = (DownloadItemModel)parameter;
            var client = new Aria2ClientModel(Aria2Server);
            if ((item.Status == "complete") || (item.Status == "error") || (item.Status == "removed"))
            {
                client.Aria2Client.RemoveDownloadResultAsync(item.Gid);
            }
            else
            {
                client.Aria2Client.ForceRemoveAsync(item.Gid);
            }
        }

        //暂停项
        private void PauseItem(object? parameter)
        {
            if (parameter == null)
            {
                return;
            }
            DownloadItemModel item = (DownloadItemModel)parameter;
            var client = new Aria2ClientModel(Aria2Server);
            client.Aria2Client.PauseAsync(item.Gid);
        }

        //继续项
        private void ResumeItem(object? parameter)
        {
            if (parameter == null)
            {
                return;
            }
            DownloadItemModel item = (DownloadItemModel)parameter;
            var client = new Aria2ClientModel(Aria2Server);
            client.Aria2Client.UnpauseAsync(item.Gid);
        }

        //更新下载项
        private async void UpdataDownloadItems()
        {
            while (UpdateItems)
            {
                List<DownloadItemModel> download_items = new List<DownloadItemModel>(); //新建列表
                try
                {
                    var client = new Aria2ClientModel(Aria2Server);
                    var Items = await client.Aria2Client.TellAllAsync(); //获取所有项
                    long total_download_speed = 0;
                    long total_upload_speed = 0;
                    foreach (var item in Items)
                    {
                        if ((CurrentChosenStatus != "all") && (item.Status != CurrentChosenStatus))
                        {
                            //筛选
                            continue;
                        }
                        DownloadItemModel download_item = new DownloadItemModel();
                        //更新信息
                        download_item.Gid = item.Gid;
                        if ((item.Bittorrent == null) || (item.Bittorrent.Info == null))
                        {
                            download_item.Name = Path.GetFileName(item.Files[0].Path);
                        }
                        else
                        {
                            download_item.Name = item.Bittorrent.Info.Name;
                        }
                        download_item.Size = Tools.BytesToString(item.TotalLength);
                        if (item.TotalLength == 0)
                        {
                            download_item.Progress = 0;
                        }
                        else
                        {
                            download_item.Progress = (double)(item.CompletedLength * 10000 / item.TotalLength) / 100;
                        }
                        download_item.Status = item.Status;
                        download_item.Downloaded = Tools.BytesToString(item.CompletedLength);
                        download_item.Uploaded = Tools.BytesToString(item.UploadLength);
                        download_item.DownloadSpeed = Tools.BytesToString(item.DownloadSpeed) + "/s";
                        download_item.UploadSpeed = Tools.BytesToString(item.UploadSpeed) + "/s";
                        total_download_speed += item.DownloadSpeed;
                        total_upload_speed += item.UploadSpeed;
                        if (item.DownloadSpeed == 0)
                        {
                            download_item.ETA = "--";
                        }
                        else
                        {
                            download_item.ETA = Tools.SecondsToString((int)((double)(item.TotalLength - item.CompletedLength) / item.DownloadSpeed));
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
                        download_items.Add(download_item);
                    }
                    //下载项列表更新，根据GID找出最新的选中项
                    if (SelectedGid != null)
                    {
                        try
                        {
                            SelectedItem = download_items.First(x => x.Gid == SelectedGid);
                        }
                        catch
                        {
                            SelectedItem = null;
                        }
                    }
                    //更新下载项列表
                    DownloadItems = download_items;
                    //更新显示速度
                    UploadSpeed = Tools.BytesToString(total_upload_speed) + "/s";
                    DownloadSpeed = Tools.BytesToString(total_download_speed) + "/s";
                    Connected = "Green";
                }
                catch
                {
                    Connected = "Red"; //无法连接
                }
                await Task.Delay(500);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}