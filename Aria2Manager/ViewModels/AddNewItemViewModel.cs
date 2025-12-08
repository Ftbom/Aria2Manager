using Aria2Manager.Models;
using Aria2Manager.Views;
using Aria2Manager.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System;
using System.Windows;

namespace Aria2Manager.ViewModels
{
    public class AddNewItemViewModel : INotifyPropertyChanged
    {
        private const char HEADER_DELIMITER = '|';
        
        public event PropertyChangedEventHandler? PropertyChanged;

        //新下载项分类
        public bool IsUrl
        {
            get => _is_url;
            set
            {
                _is_url = value;
                OnPropertyChanged();
            }
        }
        public bool IsTorrent
        {
            get => _is_torrent;
            set
            {
                _is_torrent = value;
                OnPropertyChanged();
            }
        }
        public bool IsMetaLink
        {
            get => _is_metalink;
            set
            {
                _is_metalink = value;
                OnPropertyChanged();
            }
        }
        public bool IsLocal
        {
            get => _aria2_server.IsLocal;
            set { }
        }
        //基本设置项
        public string? Url { get; set; }
        public string? TorrentPath { get; set; }
        public string? MetaLinkPath { get; set; }
        public string? HeaderString { get; set; }
        public string? FileName { get; set; }
        public string? DownloadPath { get; set; }
        public string? HTTPUser { get; set; }
        public string? HTTPPasswd { get; set; }
        public string? SeedTime { get; set; }
        public string? SeedRatio { get; set; }
        public string? ProxyAddress { get; set; }
        public string? ProxyPort { get; set; }
        public string? ProxyUser { get; set; }
        public string? ProxyPasswd { get; set; }

        public ICommand CheckTorrentCommand { get; private set; }
        public ICommand CheckMetaLinkCommand { get; private set; }
        public ICommand BrowseFileCommand { get; private set; }
        public ICommand AddDownloadCommand { get; private set; }

        private Aria2ServerInfoModel _aria2_server;
        private bool _is_url;
        private bool _is_torrent;
        private bool _is_metalink;
        private bool is_connect; //是否已连接

        public AddNewItemViewModel(Aria2ServerInfoModel? Server = null)
        {
            //新下载项默认类别为URL
            IsUrl = true;
            IsTorrent = false;
            IsMetaLink = false;
            if (Server == null)
            {
                _aria2_server = new Aria2ServerInfoModel(); //使用默认服务器信息
            }
            else
            {
                _aria2_server = Server;
            }
            CheckTorrentCommand = new RelayCommand(CheckTorrent);
            CheckMetaLinkCommand = new RelayCommand(CheckMetaLink);
            BrowseFileCommand = new RelayCommand(BrowseFiles);
            AddDownloadCommand = new RelayCommand(AddDownload);
            GetOptions();
        }

        //获取全局设置，用作默认值
        async private void GetOptions()
        {
            Aria2ClientModel client = new Aria2ClientModel(_aria2_server);
            IDictionary<string, string> options;
            try
            {
                options = await client.Aria2Client.GetGlobalOptionAsync();
                is_connect = true;
            }
            catch
            {
                is_connect = false;
                MessageBox.Show(Application.Current.FindResource("ConnectionError").ToString());
                return;
            }
            DownloadPath = GetOptionValueByKey(options, "dir");
            SeedRatio = GetOptionValueByKey(options, "seed-ratio");
            SeedTime = GetOptionValueByKey(options, "seed-time");
            HTTPUser = GetOptionValueByKey(options, "http-user");
            HTTPPasswd = GetOptionValueByKey(options, "http-passwd");
            ProxyPasswd = GetOptionValueByKey(options, "all-proxy-passwd");
            ProxyUser = GetOptionValueByKey(options, "all-proxy-user");
            //代理信息处理
            //获取IP地址和端口
            string? ProxyString = GetOptionValueByKey(options, "all-proxy");
            try
            {
                if (ProxyString != null)
                {
                    ProxyString = ProxyString.Replace("http://", "");
                    ProxyString = ProxyString.Replace("https://", "");
                    ProxyString = ProxyString.Replace("/", "");
                    var ProxySplit = ProxyString.Split(':');
                    ProxyAddress = ProxySplit[0];
                    ProxyPort = ProxySplit[1];
                }
            }
            catch
            {
                ProxyAddress = null;
                ProxyPort = null;
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(null)); //更新所有
            }
            return;
        }

        //从字典读取值，null或非空字符串
        private static string? GetOptionValueByKey(IDictionary<string, string> options, string key)
        {
            return options.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value) ? value : null;
        }

        //切换Torrent模式
        private void CheckTorrent(object? parameter)
        {
            if (parameter is not CheckBox box) return;

            if (IsTorrent)
            {
                //切换回URL模式
                IsUrl = true;
                IsMetaLink = false;
                IsTorrent = false;
            }
            else
            {
                //切换为BitTorrent模式
                box.IsChecked = false;
                IsUrl = false;
                IsMetaLink = false;
                IsTorrent = true;
            }
        }

        //切换MetaLink模式
        private void CheckMetaLink(object? parameter)
        {
            if (parameter is not CheckBox box) return;

            if (IsMetaLink)
            {
                //切换回URL模式
                IsUrl = true;
                IsMetaLink = false;
                IsTorrent = false;
            }
            else
            {
                //切换为Metalink模式
                box.IsChecked = false;
                IsUrl = false;
                IsMetaLink = true;
                IsTorrent = false;
            }
        }

        //添加下载
        private void AddDownload(object? parameter)
        {
            if (parameter is not AddNewItemWindow window) return;

            if (!is_connect) //服务器连接出错，直接退出
            {
                window.Close();
                return;
            }

            var client = new Aria2ClientModel(_aria2_server);
            try
            {
                var options = new Dictionary<string, object>();
                SetOptions(options, "dir", DownloadPath);

                //按种类分别设置
                if (IsMetaLink && MetaLinkPath != null)
                {
                    client.Aria2Client.AddMetalinkAsync(torrent: File.ReadAllBytes(MetaLinkPath), options: options);
                }
                else if (IsTorrent && TorrentPath != null)
                {
                    SetOptions(options, "seed-time", SeedTime);
                    SetOptions(options, "seed-ratio", SeedRatio);
                    client.Aria2Client.AddTorrentAsync(torrent: File.ReadAllBytes(TorrentPath), options: options);
                }
                else if (Url != null)
                {
                    if (!string.IsNullOrEmpty(FileName))
                    {
                        SetOptions(options, "out", FileName);
                    }
                    SetOptions(options, "http-user", HTTPUser);
                    SetOptions(options, "http-passwd", HTTPPasswd);
                    SetOptions(options, "all-proxy-passwd", ProxyPasswd);
                    SetOptions(options, "all-proxy-user", ProxyUser);
                    
                    if (!string.IsNullOrEmpty(ProxyAddress) && !string.IsNullOrEmpty(ProxyPort))
                    {
                        options["all-proxy"] = $"http://{ProxyAddress}:{ProxyPort}";
                    }
                    
                    if (!string.IsNullOrEmpty(HeaderString))
                    {
                        //header 设置
                        var headerList = HeaderString.Split(HEADER_DELIMITER);
                        options["header"] = headerList;
                    }
                    
                    client.Aria2Client.AddUriAsync(uriList: new[] { Url }, options: options);
                }
            }
            catch
            {
            }
            window.Close();
        }

        //设置配置项
        private static void SetOptions(Dictionary<string, object> options, string key, object? value)
        {
            if (value != null)
            {
                options[key] = value;
            }
        }

        //浏览文件或文件夹
        private void BrowseFiles(object? parameter)
        {
            if (parameter is not string cmd) return;

            switch (cmd)
            {
                case "0": // Torrent文件
                    {
                        var openFileDialog = new Microsoft.Win32.OpenFileDialog
                        {
                            Filter = "Torrent files (*.torrent)|*.torrent"
                        };
                        if (openFileDialog.ShowDialog() == true)
                        {
                            TorrentPath = openFileDialog.FileName;
                            OnPropertyChanged(nameof(TorrentPath));
                        }
                        break;
                    }
                case "1": // MetaLink文件
                    {
                        var openFileDialog = new Microsoft.Win32.OpenFileDialog
                        {
                            Filter = "MetaLink files (*.metalink)|*.metalink"
                        };
                        if (openFileDialog.ShowDialog() == true)
                        {
                            MetaLinkPath = openFileDialog.FileName;
                            OnPropertyChanged(nameof(MetaLinkPath));
                        }
                        break;
                    }
                default: // 文件夹
                    {
                        var openFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
                        if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            DownloadPath = openFolderDialog.SelectedPath;
                            OnPropertyChanged(nameof(DownloadPath));
                        }
                        break;
                    }
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
