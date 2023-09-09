using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using Aria2Manager.Models;
using Aria2Manager.Utils;
using Aria2NET;
using System.Windows.Input;
using System.Linq;
using System.Windows.Controls;

namespace Aria2Manager.ViewModels
{
    public class ItemInfoViewModel : INotifyPropertyChanged
    {
        private string? GID;
        private Aria2ServerInfoModel? _server;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand SelectFileCommand { get; private set; }
        //基本信息
        public string? Name { get; set; }
        public string? Size { get; set; }
        public string? Progress { get; set; }
        public string? Status { get; set; }
        public string? Speed { get; set; }
        public string? Ratio { get; set; }
        public string? Connections { get; set; }
        public string? InfoHash { get; set; }
        public string? DownloadPath { get; set; }
        public bool CanSelectFile { get; set; }
        //文件列表
        public List<ItemFileModel>? Files { get; set; }

        public ItemInfoViewModel(string? gid = null, Aria2ServerInfoModel? server = null)
        {
            SelectFileCommand = new RelayCommand(SelectFile);
            if (String.IsNullOrEmpty(gid))
            {
                return;
            }
            if (server == null)
            {
                return;
            }
            GID = gid;
            _server = server;
            Files = new List<ItemFileModel>();
            GetInfo(gid);
        }

        //获取信息
        async private void GetInfo(string gid)
        {
            var client = new Aria2ClientModel(_server);
            DownloadStatusResult Info = await client.Aria2Client.TellStatusAsync(gid);
            if ((Info.Bittorrent == null) || (Info.Bittorrent.Info == null))
            {
                Name = System.IO.Path.GetFileName(Info.Files[0].Path);
                CanSelectFile = false;
            }
            else
            {
                //种子文件
                Name = Info.Bittorrent.Info.Name;
                CanSelectFile = true;
            }
            Size = Tools.BytesToString(Info.TotalLength);
            Progress = Application.Current.FindResource("Downloaded").ToString() + ":"
                + Tools.BytesToString(Info.CompletedLength) + ","
                + Application.Current.FindResource("Uploaded").ToString() + ":"
                + Tools.BytesToString(Info.UploadLength);
            Status = Info.Status;
            Speed = Application.Current.FindResource("DownloadSpeed").ToString() + ":"
                + Tools.BytesToString(Info.DownloadSpeed) + "/s,"
                + Application.Current.FindResource("UploadSpeed").ToString() + ":"
                + Tools.BytesToString(Info.UploadSpeed) + "/s";
            if (Info.CompletedLength == 0)
            {
                Ratio = "--";
            }
            else
            {
                Ratio = ((double)(Info.UploadLength * 100 / Info.CompletedLength) / 100).ToString();
            }
            Connections = Application.Current.FindResource("Seeders").ToString() + ":"
                + Info.NumSeeders.ToString() + ","
                + Application.Current.FindResource("Connections").ToString() + ":"
                + Info.Connections.ToString();
            if (Info.InfoHash == null)
            {
                InfoHash = "--";
            }
            else
            {
                InfoHash = Info.InfoHash;
            }
            DownloadPath = Info.Dir;
            //仅有一个文件，不可设置文件
            if (Info.Files.Count == 1)
            {
                CanSelectFile = false;
            }
            foreach (var file in Info.Files)
            {
                Files.Add(new ItemFileModel { Name = System.IO.Path.GetFileName(file.Path), Selected = file.Selected, Index = file.Index.ToString()});
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(null)); //更新所有
            }
        }

        //选中或取消选中文件，则更改设置
        private void SelectFile(object? parameter)
        {
            var client = new Aria2ClientModel(_server);
            List<string> IndexList = new List<string>();
            foreach (var file in Files)
            {
                if (file.Selected)
                {
                    IndexList.Add(file.Index);
                }
            }
            if (IndexList.Count == 0)
            {
                ((CheckBox)parameter).IsChecked = true;
                return;
            }
            IDictionary<string, string> options = new Dictionary<string, string>();
            options["select-file"] = String.Join(',', IndexList.ToArray());
            client.Aria2Client.ChangeOptionAsync(GID, options);
        }
    }
}
