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
        private string? _torrentPath = string.Empty;
        [ObservableProperty]
        private string? _metaLinkPath = string.Empty;
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
        public string? HeaderString { get; set; } = string.Empty;
        public string? FileName { get; set; } = string.Empty;
        public Aria2NewTaskViewModel(IUIService uiService)
        {
            _uiService = uiService;
            InitializeOptions();
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
        private async void InitializeOptions()
        {
            try
            {
                var optionsData = await Server.GetAria2Options(["dir", "seed-ratio", "seed-time", "http-user",
                    "http-passwd", "all-proxy-passwd", "all-proxy-user", "all-proxy"]);
                var parsedOptions = Aria2OptionsHelper.ParseAria2Options(optionsData);
                DownloadPath = parsedOptions["dir"];
                SeedRatio = parsedOptions["seed-ratio"];
                SeedTime = parsedOptions["seed-time"];
                HttpUser = parsedOptions["http-user"];
                HttpPasswd = parsedOptions["http-passwd"];
                ProxyAddress = parsedOptions["proxy-address"];
                ProxyPort = parsedOptions["proxy-port"];
                ProxyUser = parsedOptions["all-proxy-user"];
                ProxyPasswd = parsedOptions["all-proxy-passwd"];
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Load_Options_Failed"),
                    LanguageHelper.GetString("Error"), MsgBoxLevel.Error);
            }
        }
        [RelayCommand]
        private async Task AddNewTask()
        {
            if (!Server.ServerInfo.IsConnected) //服务器连接出错，直接退出
            {
                if (!string.IsNullOrWhiteSpace(WindowId))
                {
                    _uiService.CloseWindow(WindowId);
                }
                return;
            }
            try
            {
                var options = new Dictionary<string, string?>();
                options["dir"] = DownloadPath;
                //按种类分别设置
                if (IsMetalinkEnabled && !String.IsNullOrWhiteSpace(MetaLinkPath))
                {
                    await Server.AddMetalinkTask(File.ReadAllBytes(MetaLinkPath), Aria2OptionsHelper.MergeAria2Options(options));
                }
                else if (IsTorrentEnabled && !String.IsNullOrWhiteSpace(TorrentPath))
                {
                    options["seed-time"] = SeedTime;
                    options["seed-ratio"] = SeedRatio;
                    await Server.AddTorrentTask(File.ReadAllBytes(TorrentPath), Aria2OptionsHelper.MergeAria2Options(options));
                }
                else if (!String.IsNullOrWhiteSpace(Urls))
                {
                    options["out"] = FileName;
                    options["http-user"] = HttpUser;
                    options["http-passwd"] = HttpPasswd;
                    options["all-proxy-passwd"] = ProxyPasswd;
                    options["all-proxy-user"] = ProxyUser;
                    options["proxy-address"] = ProxyAddress;
                    options["proxy-port"] = ProxyPort;
                    options["header"] = HeaderString;
                    await Server.AddUrlsTask(Urls.Split(
                        new[] { '\r', '\n' },
                        StringSplitOptions.RemoveEmptyEntries
                    ).ToList<string>(), Aria2OptionsHelper.MergeAria2Options(options));
                }
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("New_Task_Failed"),
                    LanguageHelper.GetString("Error"), MsgBoxLevel.Error);
            }
            if (!string.IsNullOrWhiteSpace(WindowId))
            {
                _uiService.CloseWindow(WindowId);
            }
        }
        [RelayCommand]
        private async Task BrowseFileOrFolder(string? filter)
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
