using Aria2Manager.Core.Enums;
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
        public static string AppVersion = "1.0.8"; //应用版本
        // 保存的配置信息
        private readonly ConfigurationService<AppSettings> _settingsConfigService;
        private readonly ConfigurationService<ServerSettings> _serversConfigService;
        public CancellationToken GlobalCancelToken => _globalCts.Token;
        public readonly ObservableCollection<string> Aria2ServerNames = new ObservableCollection<string>();
        // 暴露数据对象
        public AppSettings AppSettings { get; set; }
        public ServerSettings ServerSettings { get; set; }
        public Aria2ServerService Aria2Server { get; private set; }
        private Aria2NotificationService? _aria2Notification = null;
        public Aria2Server GetCurrentAria2Server() => ServerSettings.ServerConfigs.FirstOrDefault(s => s.Name == ServerSettings.Current)
                                    ?? ServerSettings.ServerConfigs.First(); //如果找不到指定配置，默认选择第一个
        private GlobalContext()
        {
            _settingsConfigService = new ConfigurationService<AppSettings>("Settings.xml");
            _serversConfigService = new ConfigurationService<ServerSettings>("Servers.xml");
            //初始化时加载设置文件
            AppSettings = _settingsConfigService.Load();
            ServerSettings = _serversConfigService.Load();
            Aria2Server = new Aria2ServerService(new Aria2ServerInfo(GetCurrentAria2Server())); //初始化Aria2服务器服务
        }
        public async Task InitializeAsync(UIServiceBase uiService)
        {
            //根据设置更换语言
            LanguageHelper.ChangeLanguage(AppSettings.Language);
            //根据设置应用主题
            if (!string.IsNullOrWhiteSpace(AppSettings.Theme))
            {
                await uiService.ChangeThemeAsync(AppSettings.Theme);
            }
            //启动Aria2进程
            if (AppSettings.StartAria2)
            {
                Aria2ProcessHelper.StartAria2Process();
            }
            //初始化Aria2通知服务
            if (AppSettings.EnableAria2Notification)
            {
                _aria2Notification = new Aria2NotificationService(new Aria2ServerInfo(GetCurrentAria2Server()),
                    uiService, Aria2Server.GetTaskStatus);
            }
            try
            {
                //检查程序更新
                if (await UpdateCheckerHelper.CheckProgramUpdate() == true)
                {
                    uiService.ShowTrayNotification(LanguageHelper.GetString("Program_Update_Available"));
                }
                //检查Aria2更新
                var aria2Version = await Aria2Server.GetAria2Version();
                if (await UpdateCheckerHelper.CheckAria2Update(aria2Version.Version) == true)
                {
                    uiService.ShowTrayNotification(LanguageHelper.GetString("Aria2_Update_Available"));
                }
            }
            catch { }
            //更新Tracker
            try
            {
                BtTrackerService trackerService = new BtTrackerService(new BtTrackers(AppSettings.Trackers));
                var trackers = await trackerService.CheckTrackersUpdate();
                if (trackers != null)
                {
                    await Aria2Server.ChangeAria2Options(new Dictionary<string, string>
                    {
                        { "bt-tracker", string.Join(",", trackers.ToArray())}
                    });
                }
            }
            catch
            {
                await uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Update_Trackers_Failed"), "Error", MsgBoxLevel.Error);
            }
            if (AppSettings.EnableAria2Notification)
            {

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
        //取消Aria2异步任务
        public void CancelAllTasks()
        {
            _globalCts.Cancel();
            _globalCts.Dispose();
            _globalCts = new();
        }
        public void SaveAll()
        {
            SaveServers();
            SaveSettings();
        }
        public void SaveSettings() => _settingsConfigService.Save(AppSettings);
        public void SaveServers()
        {
            CancelAllTasks();
            Aria2ServerInfo newServer = new Aria2ServerInfo(GetCurrentAria2Server());
            Aria2Server.UpdateAria2Server(newServer);
            _aria2Notification?.ChangeWebsocketClient(newServer);
            SyncAria2ServerNames();
            _serversConfigService.Save(ServerSettings);
        }
    }
}
