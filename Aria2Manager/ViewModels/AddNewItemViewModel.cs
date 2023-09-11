﻿using Aria2Manager.Models;
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
        public event PropertyChangedEventHandler? PropertyChanged;

        //分类
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
            IsUrl = true;
            IsTorrent = false;
            IsMetaLink = false;
            if (Server == null)
            {
                _aria2_server = new Aria2ServerInfoModel();
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
            }
            catch
            {
                is_connect = false;
                MessageBox.Show(Application.Current.FindResource("ConnectionError").ToString());
                return;
            }
            DownloadPath = GetOptionValueByKey(options, "dir");
            //TODO:UPdate UI
            SeedRatio = GetOptionValueByKey(options, "seed-ratio");
            SeedTime = GetOptionValueByKey(options, "seed-time");
            HTTPUser = GetOptionValueByKey(options, "http-user");
            HTTPPasswd = GetOptionValueByKey(options, "http-passwd");
            ProxyPasswd = GetOptionValueByKey(options, "all-proxy-passwd");
            ProxyUser = GetOptionValueByKey(options, "all-proxy-user");
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
        private string? GetOptionValueByKey(IDictionary<string, string> options, string key)
        {
            try
            {
                if (options[key] == "")
                {
                    return null;
                }
                return options[key];
            }
            catch
            {
                return null;
            }
        }

        //切换Torrent模式
        private void CheckTorrent(object? parameter)
        {
            if (IsTorrent)
            {
                IsUrl = true;
                IsMetaLink = false;
                IsTorrent = false;
            }
            else
            {
                CheckBox box = (CheckBox)parameter;
                box.IsChecked = false;
                IsUrl = false;
                IsMetaLink = false;
                IsTorrent = true;
            }
        }

        //切换MetaLink模式
        private void CheckMetaLink(object? parameter)
        {
            if (IsMetaLink)
            {
                IsUrl = true;
                IsMetaLink = false;
                IsTorrent = false;
            }
            else
            {
                CheckBox box = (CheckBox)parameter;
                box.IsChecked = false;
                IsUrl = false;
                IsMetaLink = true;
                IsTorrent = false;
            }
        }

        //添加下载
        private void AddDownload(object? parameter)
        {
            if (!is_connect)
            {
                ((AddNewItemWindow)parameter).Close();
                return;
            }
            var Client = new Aria2ClientModel(_aria2_server);
            try
            {
                var Options = new Dictionary<String, object>();
                Options["dir"] = DownloadPath;
                //按种类分别设置
                if (IsMetaLink)
                {
                    Client.Aria2Client.AddMetalinkAsync(torrent: File.ReadAllBytes(MetaLinkPath), options: Options);
                }
                else if (IsTorrent)
                {
                    Options["seed-time"] = SeedTime;
                    Options["seed-ratio"] = SeedRatio;
                    Client.Aria2Client.AddTorrentAsync(torrent: File.ReadAllBytes(TorrentPath), options: Options);
                }
                else
                {
                    if (!String.IsNullOrEmpty(FileName))
                    {
                        Options["out"] = FileName;
                    }
                    Options["http-user"] = HTTPUser;
                    Options["http-passwd"] = HTTPPasswd;
                    Options["all-proxy-passwd"] = ProxyPasswd;
                    Options["all-proxy-user"] = ProxyUser;
                    if ((!String.IsNullOrEmpty(ProxyAddress)) && (!String.IsNullOrEmpty(ProxyPort)))
                    {
                        Options["all-proxy"] = "http://" + ProxyAddress + ":" + ProxyPort;
                    }
                    if (!String.IsNullOrEmpty(HeaderString))
                    {
                        var HeaderList = new List<string>();
                        foreach(var header in HeaderString.Split(';'))
                        {
                            HeaderList.Add(header);
                        }
                        Options["header"] = HeaderList.ToArray();
                    }
                    Client.Aria2Client.AddUriAsync(uriList: new string[] { Url }, options: Options);
                }
            }
            catch
            { }
            ((AddNewItemWindow)parameter).Close();
        }

        //浏览文件或文件夹
        private void BrowseFiles(object? parameter)
        {
            if (parameter == null)
            {
                return;
            }
            string cmd = (string)parameter;
            if (cmd == "0")
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "Torrent files (*.torrent)|*.torrent";
                if (openFileDialog.ShowDialog() == true)
                {
                    TorrentPath = openFileDialog.FileName;
                    OnPropertyChanged(nameof(TorrentPath));
                }
            }
            else if (cmd == "1")
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "MetaLink files (*.metalink)|*.metalink";
                if (openFileDialog.ShowDialog() == true)
                {
                    MetaLinkPath = openFileDialog.FileName;
                    OnPropertyChanged(nameof(MetaLinkPath));
                }
            }
            else
            {
                System.Windows.Forms.FolderBrowserDialog openFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
                if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DownloadPath = openFolderDialog.SelectedPath;
                    OnPropertyChanged(nameof(DownloadPath));
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