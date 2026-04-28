using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;
using Aria2Manager.Core.Services;
using Aria2Manager.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aria2Manager.Core.ViewModels
{
    public partial class Aria2NewTaskViewModel : ObservableObject, IWindowAware
    {
        private Aria2ServerService Server => GlobalContext.Instance.Aria2Server;
        private readonly IUIService _uiService;
        public string? WindowId { get; set; }
        [ObservableProperty]
        private string? _downloadPath = string.Empty;
        [ObservableProperty]
        private string? _seedRatio = string.Empty;
        [ObservableProperty]
        private string? _seedTime = string.Empty;
        [ObservableProperty]
        private string? _httpUser = string.Empty;
        [ObservableProperty]
        private string? _httpPasswd = string.Empty;
        [ObservableProperty]
        private string? _proxyAddress = string.Empty;
        [ObservableProperty]
        private string? _proxyPort = string.Empty;
        [ObservableProperty]
        private string? _proxyUser = string.Empty;
        [ObservableProperty]
        private string? _proxyPasswd = string.Empty;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUrlEnabled))] //当值变了，自动刷新IsUrlEnabled
        private bool _isTorrentEnabled;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUrlEnabled))]
        private bool _isMetalinkEnabled;
        public bool IsUrlEnabled => !IsTorrentEnabled && !IsMetalinkEnabled;
        public bool IsDownloadPathBrowseEnabled => Server.ServerInfo.IsLocal;
        public string? Urls { get; set; } = string.Empty;
        public string? TorrentPath { get; set; } = string.Empty;
        public string? MetaLinkPath { get; set; } = string.Empty;
        public string? HeaderString { get; set; } = string.Empty;
        public string? FileName { get; set; } = string.Empty;
        public Aria2NewTaskViewModel(IUIService uiService)
        {
            _uiService = uiService;
            LoadAria2Options();
        }
        private async void LoadAria2Options()
        {
            try
            {
                var Options = await Server.GetAria2Options(["dir", "seed-ratio", "seed-time", "http-user",
                                    "http-passwd", "all-proxy-passwd", "all-proxy-user", "all-proxy"]);
                DownloadPath = Options["dir"];
                SeedRatio = Options["seed-ratio"];
                SeedTime = Options["seed-time"];
                HttpUser = Options["http-user"];
                HttpPasswd = Options["http-passwd"];
                ProxyUser = Options["all-proxy-user"];
                ProxyPasswd = Options["all-proxy-passwd"];
                if (!String.IsNullOrWhiteSpace(Options["all-proxy"]))
                {
                    try
                    {
                        var ProxySplit = Options["all-proxy"]?.Replace("http://", "").Replace("https://", "").Replace("/", "").Split(':');
                        ProxyAddress = ProxySplit?[0];
                        ProxyPort = ProxySplit?[1];
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Warning("Failed to parse all-proxy option", ex);
                        ProxyAddress = null;
                        ProxyPort = null;
                    }
                }
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Load_Options_Fail"), "Error", MsgBoxLevel.Error);
            }
        }
        //当IsTorrentEnabled改变时，自动调用该Partial方法
        partial void OnIsTorrentEnabledChanged(bool value)
        {
            if (value)
            {
                IsMetalinkEnabled = false; // 互斥逻辑
            }
        }
        partial void OnIsMetalinkEnabledChanged(bool value)
        {
            if (value)
            {
                IsTorrentEnabled = false; // 互斥逻辑
            }
        }
        private void AddOptions(Dictionary<string, object> options, string key, string? value)
        {
            if (!String.IsNullOrWhiteSpace(value))
            {
                options[key] = value;
            }
        }
        [RelayCommand]
        private async Task AddNewTask()
        {
            if (!Server.ServerInfo.IsConnected) //服务器连接出错，直接退出
            {
                if (!string.IsNullOrEmpty(WindowId))
                {
                    _uiService.CloseWindow(WindowId);
                }
                return;
            }
            try
            {
                var options = new Dictionary<string, object>();
                AddOptions(options, "dir", DownloadPath);
                //按种类分别设置
                if (IsMetalinkEnabled && !String.IsNullOrWhiteSpace(MetaLinkPath))
                {
                    await Server.AddMetalinkTask(File.ReadAllBytes(MetaLinkPath), options);
                }
                else if (IsTorrentEnabled && !String.IsNullOrWhiteSpace(TorrentPath))
                {
                    AddOptions(options, "seed-time", SeedTime);
                    AddOptions(options, "seed-ratio", SeedRatio);
                    await Server.AddTorrentTask(File.ReadAllBytes(TorrentPath), options);
                }
                else if (!String.IsNullOrWhiteSpace(Urls))
                {
                    AddOptions(options, "out", FileName);
                    AddOptions(options, "http-user", HttpUser);
                    AddOptions(options, "http-passwd", HttpPasswd);
                    AddOptions(options, "all-proxy-passwd", ProxyPasswd);
                    AddOptions(options, "all-proxy-user", ProxyUser);
                    if ((!String.IsNullOrWhiteSpace(ProxyAddress)) && (!String.IsNullOrWhiteSpace(ProxyPort)))
                    {
                        options["all-proxy"] = "http://" + ProxyAddress + ":" + ProxyPort;
                    }
                    if (!String.IsNullOrWhiteSpace(HeaderString))
                    {
                        var HeaderList = new List<string>();
                        foreach (var header in HeaderString.Split('\n'))
                        {
                            HeaderList.Add(header);
                        }
                        options["header"] = HeaderList.ToArray();
                    }
                    await Server.AddUrlTask(Urls.Split('\n').ToList<string>(), options);
                }
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("New_Task_Fail"), "Error", MsgBoxLevel.Error);
            }
            if (!string.IsNullOrEmpty(WindowId))
            {
                _uiService.CloseWindow(WindowId);
            }
        }
        [RelayCommand]
        private async Task BrowseFiles(string? filter)
        {
            if (filter == "torrent")
            {
                var filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter
                    {
                        Name = "Torrent files",
                        Extensions = new List<string> { "torrent" }
                    }
                };
                var path = await _uiService.OpenFileDialogAsync(filters);
                if (!String.IsNullOrWhiteSpace(path))
                {
                    TorrentPath = path;
                }
            }
            else if (filter == "metalink")
            {
                var filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter
                    {
                        Name = "Metalink files",
                        Extensions = new List<string> { "metalink" }
                    }
                };
                var path = await _uiService.OpenFileDialogAsync(filters);
                if (!String.IsNullOrWhiteSpace(path))
                {
                    MetaLinkPath = path;
                }
            }
            else if (filter == "folder")
            {
                var path = await _uiService.OpenFolderDialogAsync();
                if (!String.IsNullOrWhiteSpace(path))
                {
                    DownloadPath = path;
                }
            }
        }
    }
}
