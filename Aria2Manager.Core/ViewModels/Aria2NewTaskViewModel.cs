using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;
using Aria2Manager.Core.Services;
using Aria2Manager.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aria2Manager.Core.ViewModels
{
    public class Aria2OptionsHelper
    {
        public record Aria2OptionsData(
            string? DownloadPath = null,
            string? SeedRatio = null,
            string? SeedTime = null,
            string? HttpUser = null,
            string? HttpPasswd = null,
            string? ProxyAddress = null,
            string? ProxyPort = null,
            string? ProxyUser = null,
            string? ProxyPasswd = null);
        public static async Task<Aria2OptionsData> LoadAria2Options(Aria2ServerService server, IUIService uiService, string? gid = null)
        {
            try
            {
                var Options = await server.GetAria2Options(["dir", "seed-ratio", "seed-time", "http-user",
                                    "http-passwd", "all-proxy-passwd", "all-proxy-user", "all-proxy"], gid);
                var ProxyAddress = string.Empty;
                var ProxyPort = string.Empty;
                var rawProxyOption = Options["all-proxy"];
                if (!String.IsNullOrWhiteSpace(rawProxyOption))
                {
                    try
                    {
                        if (!rawProxyOption.Contains("://"))
                        {
                            rawProxyOption = "http://" + rawProxyOption;
                        }
                        if (Uri.TryCreate(rawProxyOption, UriKind.Absolute, out var uri))
                        {
                            ProxyAddress = $"{uri.Scheme}://{uri.Host}";
                            ProxyPort = uri.Port.ToString();
                        }
                        else
                        {
                            LogHelper.Warning($"Failed to parse all-proxy option: {rawProxyOption}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Warning("Failed to parse all-proxy option", ex);
                        ProxyAddress = null;
                        ProxyPort = null;
                    }
                }
                return new Aria2OptionsData(
                    DownloadPath: Options["dir"],
                    SeedRatio: Options["seed-ratio"],
                    SeedTime: Options["seed-time"],
                    HttpUser: Options["http-user"],
                    HttpPasswd: Options["http-passwd"],
                    ProxyAddress: ProxyAddress,
                    ProxyPort: ProxyPort,
                    ProxyUser: Options["all-proxy-user"],
                    ProxyPasswd: Options["all-proxy-passwd"]
                );
            }
            catch
            {
                await uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Load_Options_Failed"), "Error", MsgBoxLevel.Error);
            }
            return new Aria2OptionsData();
        }
        public static void AddOptions(Dictionary<string, object> options, string key, string? value)
        {
            if (!String.IsNullOrWhiteSpace(value))
            {
                options[key] = value;
            }
        }
        public static void AddOptions(Dictionary<string, string> options, string key, string? value)
        {
            if (!String.IsNullOrWhiteSpace(value))
            {
                options[key] = value;
            }
        }
    }
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
            var optionsData = await Aria2OptionsHelper.LoadAria2Options(Server, _uiService);
            DownloadPath = optionsData.DownloadPath;
            SeedRatio = optionsData.SeedRatio;
            SeedTime = optionsData.SeedTime;
            HttpUser = optionsData.HttpUser;
            HttpPasswd = optionsData.HttpPasswd;
            ProxyAddress = optionsData.ProxyAddress;
            ProxyPort = optionsData.ProxyPort;
            ProxyUser = optionsData.ProxyUser;
            ProxyPasswd = optionsData.ProxyPasswd;
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
                var options = new Dictionary<string, object>();
                Aria2OptionsHelper.AddOptions(options, "dir", DownloadPath);
                //按种类分别设置
                if (IsMetalinkEnabled && !String.IsNullOrWhiteSpace(MetaLinkPath))
                {
                    await Server.AddMetalinkTask(File.ReadAllBytes(MetaLinkPath), options);
                }
                else if (IsTorrentEnabled && !String.IsNullOrWhiteSpace(TorrentPath))
                {
                    Aria2OptionsHelper.AddOptions(options, "seed-time", SeedTime);
                    Aria2OptionsHelper.AddOptions(options, "seed-ratio", SeedRatio);
                    await Server.AddTorrentTask(File.ReadAllBytes(TorrentPath), options);
                }
                else if (!String.IsNullOrWhiteSpace(Urls))
                {
                    Aria2OptionsHelper.AddOptions(options, "out", FileName);
                    Aria2OptionsHelper.AddOptions(options, "http-user", HttpUser);
                    Aria2OptionsHelper.AddOptions(options, "http-passwd", HttpPasswd);
                    Aria2OptionsHelper.AddOptions(options, "all-proxy-passwd", ProxyPasswd);
                    Aria2OptionsHelper.AddOptions(options, "all-proxy-user", ProxyUser);
                    if ((!String.IsNullOrWhiteSpace(ProxyAddress)) && (!String.IsNullOrWhiteSpace(ProxyPort)))
                    {
                        options["all-proxy"] = ProxyAddress + ":" + ProxyPort;
                    }
                    if (!String.IsNullOrWhiteSpace(HeaderString))
                    {
                        var headerList = new List<string>();
                        var headerStrs = HeaderString.Split(
                            new[] { '\r', '\n' },
                            StringSplitOptions.RemoveEmptyEntries
                        );
                        foreach (var header in headerStrs)
                        {
                            headerList.Add(header);
                        }
                        options["header"] = headerList.ToArray();
                    }
                    await Server.AddUrlsTask(Urls.Split(
                        new[] { '\r', '\n' },
                        StringSplitOptions.RemoveEmptyEntries
                    ).ToList<string>(), options);
                }
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("New_Task_Failed"), "Error", MsgBoxLevel.Error);
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
