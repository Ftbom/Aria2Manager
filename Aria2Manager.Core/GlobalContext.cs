using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;
using Aria2Manager.Core.Services;
using System.Collections.ObjectModel;

namespace Aria2Manager.Core
{
    public class GlobalContext
    {
        private static readonly Lazy<GlobalContext> _instance = new(() => new GlobalContext()); // 唯一性
        public static GlobalContext Instance => _instance.Value;
        private CancellationTokenSource _globalCts = new(); //用于管理Aria2异步取消信号
        public static string CoreVersion = "2.0.0"; //应用版本
        public static string AppName = "Aria2Manager"; //应用版本
        // 保存的配置信息
        private readonly ConfigurationService<AppSettings> _settingsConfigService;
        private readonly ConfigurationService<ServerSettings> _serversConfigService;
        public CancellationToken GlobalCancelToken => _globalCts.Token;
        public readonly ObservableCollection<string> Aria2ServerNames = new ObservableCollection<string>();
        // 暴露数据对象
        public AppSettings AppSettings { get; set; }
        public ServerSettings ServerSettings { get; set; }
        public Aria2ServerService Aria2Server { get; private set; } = null!;
        private Aria2NotificationService? _aria2Notification = null;
        public UpdateCheckerService UpdateCheckerService = new UpdateCheckerService();
        private Action<string>? _showTrayNotification;
        private BtTrackerService? _trackerService = null;
        public event Action? OnServersUpdated; //服务器列表更新事件
        public Aria2Server GetCurrentAria2Server() => ServerSettings.ServerConfigs.FirstOrDefault(s => s.Name == ServerSettings.Current)
                                    ?? ServerSettings.ServerConfigs.First(); //如果找不到指定配置，默认选择第一个
        private GlobalContext()
        {
            _settingsConfigService = new ConfigurationService<AppSettings>("Settings.xml",
                () => new AppSettings());
            _serversConfigService = new ConfigurationService<ServerSettings>("Aria2Servers.xml",
                () =>
                {
                    return new ServerSettings
                    {
                        ServerConfigs = new List<Aria2Server> { new Aria2Server() }
                    };
                });
            //初始化时加载设置文件
            AppSettings = _settingsConfigService.Load();
            ServerSettings = _serversConfigService.Load();
        }
        //初始化
        public async void InitializeAsync(UIServiceBase uiService)
        {
            SyncAria2ServerNames();
            //初始化Aria2服务器服务
            Aria2Server = new Aria2ServerService(GetCurrentAria2Server());
            //根据设置更换语言
            LanguageHelper.ChangeLanguage(AppSettings.Language);
            //根据设置应用主题
            if (!string.IsNullOrWhiteSpace(AppSettings.Theme))
            {
                await uiService.ChangeThemeAsync(AppSettings.Theme);
            }
            else
            {
                AppSettings.Theme = uiService.DefaultTheme;
                _ = SaveSettings();
                await uiService.ChangeThemeAsync(uiService.DefaultTheme);
            }
            //启动Aria2进程
            if (AppSettings.StartAria2)
            {
                Aria2EnvironmentHelper.InitializeAria2Environment();
                Aria2ProcessHelper.StartAria2Process();
            }
            //初始化Aria2通知服务
            _aria2Notification = new Aria2NotificationService(uiService, Aria2Server.GetTaskStatus);
            await ChangeNotification(GetCurrentAria2Server());
            try
            {
                //检查程序更新
                if (AppSettings.CheckUpdate)
                {
                    if (await UpdateCheckerService.CheckProgramUpdate(uiService.UIVersion, uiService.UIName.ToLower()) == true)
                    {
                        uiService.ShowTrayNotification(LanguageHelper.GetString("Program_Update_Available"));
                    }
                }
                _showTrayNotification = msg => uiService.ShowTrayNotification(msg);
                OperationsForAria2Change();
            }
            catch { }
            //初始化Tracker更新服务
            _trackerService = new BtTrackerService(new BtTrackers(AppSettings.Trackers));
        }
        private async void OperationsForAria2Change()
        {
            try
            {
                //检查Aria2更新
                if (AppSettings.CheckAria2Update)
                {
                    var aria2Version = await Aria2Server.GetAria2Version();
                    if (await UpdateCheckerService.CheckAria2Update(aria2Version.Version) == true)
                    {
                        _showTrayNotification?.Invoke($"[{Aria2Server.ServerInfo.ServerName}] {LanguageHelper.GetString("Aria2_Update_Available")}");
                    }
                }
                //更新Tracker列表
                if (_trackerService != null)
                {
                    var trackers = await _trackerService.CheckTrackersUpdate();
                    if (trackers != null)
                    {
                        await Aria2Server.ChangeAria2Options(new Dictionary<string, string>
                        {
                            { "bt-tracker", string.Join(",", trackers.ToArray())}
                        });
                    }
                } 
            }
            catch (Exception ex)
            {
                LogHelper.Warning("Failed to check updates for aria2.", ex);
            }
        }
        private void SyncAria2ServerNames()
        {
            var targetNames = ServerSettings.ServerConfigs.Select(s => s.Name).ToList();
            for (int i = (Aria2ServerNames.Count - 1); i >= 0; i--)
            {
                if (!targetNames.Contains(Aria2ServerNames[i]))
                {
                    Aria2ServerNames.RemoveAt(i);
                }
            }
            foreach (var name in targetNames)
            {
                if (!Aria2ServerNames.Contains(name))
                {
                    Aria2ServerNames.Add(name);
                }
            }
        }
        private async Task ChangeNotification(Aria2Server server)
        {
            if (_aria2Notification != null)
            {
                if (AppSettings.EnableAria2Notification)
                {
                    await _aria2Notification.StartListeningAsync(server);
                }
                else
                {
                    await _aria2Notification.StopListeningAsync();
                }
            }
        }
        //取消Aria2异步任务
        public void CancelAllTasks()
        {
            _globalCts.Cancel();
            _globalCts.Dispose();
            _globalCts = new();
        }
        public async Task SaveSettings()
        {
            await ChangeNotification(GetCurrentAria2Server());
            _trackerService?.UpdateConfig(new BtTrackers(AppSettings.Trackers));
            _settingsConfigService.Save(AppSettings);
        }
        public async Task SaveServers()
        {
            CancelAllTasks();
            Aria2Server newServer = GetCurrentAria2Server();
            Aria2Server.Update(newServer);
            await ChangeNotification(newServer);
            OperationsForAria2Change();
            SyncAria2ServerNames();
            _serversConfigService.Save(ServerSettings);
            OnServersUpdated?.Invoke();
        }
    }
}
