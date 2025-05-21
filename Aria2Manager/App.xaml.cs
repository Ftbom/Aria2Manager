using Aria2Manager.Models;
using Aria2Manager.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Xml;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Websocket.Client;
using Newtonsoft.Json.Linq;

namespace Aria2Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private bool CloseToExit;
        private int PID = 0;
        private Aria2ClientModel? Aria2Client;
        private TaskbarIcon? TaskBar;

        //需要在加载App.xaml后调用才可生效
        public void SetLanguageDictionary()
        {
            //根据系统语言环境设置语言文件
            SetLanguageDictionary(Thread.CurrentThread.CurrentCulture.ToString());
        }

        public void SetLanguageDictionary(String Language)
        {
            try
            {
                Resources.MergedDictionaries[0].Source = new Uri($"..\\Languages\\Strings.{Language}.xaml", UriKind.Relative);
            }
            catch
            {
                Resources.MergedDictionaries[0].Source = new Uri($"..\\Languages\\Strings.xaml", UriKind.Relative);
            }
        }

        private async void Tray_ToolTipOpen(object sender, RoutedEventArgs e)
        {
            if (sender is TaskbarIcon icon && icon.TrayToolTip is DependencyObject tooltip)
            {
                var textBlock = TaskBar?.TrayToolTip.FindChild<TextBlock>("ToolTipTextBlock");
                if ((textBlock != null) && (Aria2Client != null))
                {
                    var Aria2State = await Aria2Client.Aria2Client.GetGlobalStatAsync();
                    textBlock.Text = "Aria2Manager\n" + Resources["TrayInfo"].ToString()?
                        .Replace("\\n", "\n")
                        .Replace("{0}", Tools.BytesToString((long)Aria2State.DownloadSpeed) + "/s")
                        .Replace("{1}", Tools.BytesToString((long)Aria2State.UploadSpeed) + "/s")
                        .Replace("{2}", Aria2State.NumActive.ToString())
                        .Replace("{3}", Aria2State.NumWaiting.ToString())
                        .Replace("{4}", Aria2State.NumStopped.ToString());
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (PID != 0)
            {
                Process.GetProcessById(PID).Kill();
            }
            Current.Shutdown(); //退出程序
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            if (Current.Windows.Count > 0)
            {
                return;
            }
            MainWindow main_window = new MainWindow(CloseToExit, PID);
            main_window.Show();
        }

        private void ListenAria2Event()
        {
            Aria2ServerInfoModel? aria2_server;
            try
            {
                aria2_server = new Aria2ServerInfoModel(true);
                if (Aria2Client == null)
                {
                    Aria2Client = new Aria2ClientModel(aria2_server);
                }
            }
            catch
            {
                TaskBar?.ShowBalloonTip((string)Resources["NoServer"],
                    (string)Resources["NoServersAvaliable"], BalloonIcon.Error);
                return;
            }
            var aria2_url = new Uri(aria2_server.IsHttps ? "wss" : "ws" + "://" +
                aria2_server.ServerAddress + ":" + aria2_server.ServerPort + "/jsonrpc");
            var wsClient = new WebsocketClient(aria2_url);
            wsClient.MessageReceived
                .Subscribe(msg =>
                {
                    BalloonIcon infoLevel = BalloonIcon.Info;
                    string msgKey = "";
                    string? gid;
                    if (msg.Text == null)
                    {
                        return;
                    }
                    var json = JObject.Parse(msg.Text);
                    var method = json["method"]?.ToString();
                    gid = json["params"]?[0]?["gid"]?.ToString();
                    if (gid == null)
                    {
                        return;
                    }
                    Task.Run(async () =>
                    {
                        string name = "";
                        var item = await Aria2Client.Aria2Client.TellStatusAsync(gid);
                        if ((item.Bittorrent == null) || (item.Bittorrent.Info == null))
                        {
                            name = Path.GetFileName(item.Files[0].Path);
                        }
                        else
                        {
                            name = item.Bittorrent.Info.Name;
                        }
                        switch (method)
                        {
                            case "aria2.onDownloadStart":
                                infoLevel = BalloonIcon.Info;
                                msgKey = "DownloadStart";
                                name = gid;
                                break;
                            case "aria2.onDownloadPause":
                                infoLevel = BalloonIcon.None;
                                msgKey = "DownloadPause";
                                break;
                            case "aria2.onDownloadStop":
                                infoLevel = BalloonIcon.None;
                                msgKey = "DownloadStop";
                                break;
                            case "aria2.onDownloadComplete":
                                infoLevel = BalloonIcon.Info;
                                msgKey = "DownloadComplete";
                                break;
                            case "aria2.onDownloadError":
                                infoLevel = BalloonIcon.Error;
                                msgKey = "DownloadError";
                                break;
                            case "aria2.onBtDownloadComplete":
                                infoLevel = BalloonIcon.Info;
                                msgKey = "BtDownloadComplete";
                                break;
                        }
                        TaskBar?.ShowBalloonTip((string)Resources[msgKey],
                            name, infoLevel);
                    });
                }
            );
            _ = wsClient.Start();
        }

        private async void InfoProgramUpdate()
        {
            if (await Tools.CheckProgramUpdate())
            {
                TaskBar?.ShowBalloonTip((string)Resources["UpdateInfo"],
                        (string)Resources["ProgramHasUpdate"], BalloonIcon.Info);
            }
        }

        //检查Aria2更新
        private async void CheckAira2Update()
        {
            if (Aria2Client == null)
            {
                try
                {
                    Aria2ServerInfoModel aria2_server = new Aria2ServerInfoModel(true);
                    Aria2Client = new Aria2ClientModel(aria2_server);
                }
                catch
                {
                    TaskBar?.ShowBalloonTip((string)Resources["NoServer"],
                        (string)Resources["NoServersAvaliable"], BalloonIcon.Error);
                    return;
                }
            }
            var Aria2Version = await Aria2Client.Aria2Client.GetVersionAsync();
            var Version = Aria2Version.Version;
            string LatestVersion = await Tools.GetlatestReleaseTag("https://api.github.com/repos/aria2/aria2/releases"); //获取Aria2最新版本号
            LatestVersion = LatestVersion.Replace("release-", "");
            if (LatestVersion == "")
            {
                return;
            }
            if (Version != LatestVersion)
            {
                //提示更新
                TaskBar?.ShowBalloonTip((string)Resources["UpdateInfo"],
                        (string)Resources["Aria2HasUpdate"], BalloonIcon.Info);
            }
        }

        private async void CheckTrackersUpdate(int LastUpdate, int UpdateInterval, string TrackersSource)
        {
            if (TrackersSource == "")
            {
                return;
            }
            //当前时间
            int NowMinute = (int)(DateTime.Now - new DateTime(2001, 1, 1)).TotalMinutes;
            List<string>? trackers = null;
            bool NeedUpdate = false;
            if ((NowMinute - LastUpdate) >= UpdateInterval)
            {
                NeedUpdate = true;
            }
            else
            {
                if (!File.Exists("trackers.txt"))
                {
                    NeedUpdate = true;
                }
            }
            if (NeedUpdate)
            {
                try
                {
                    //获取Trackers
                    TrackersModel trackers_model = new TrackersModel();
                    trackers = await trackers_model.GetTrackers(TrackersSource);
                    XmlDocument _doc = new XmlDocument();
                    _doc.Load("Configurations\\Settings.xml");
                    var Node = _doc.SelectSingleNode("/Settings/UpdateTrackers/LastUpdate");
                    if (Node != null)
                    {
                        Node.InnerText = NowMinute.ToString();
                    }
                    _doc.Save("Configurations\\Settings.xml");
                    File.WriteAllLines("trackers.txt", trackers);
                }
                catch
                { }
            }
            trackers = File.ReadAllLines("trackers.txt").ToList();
            //每次启动设置Trackers
            if (trackers != null)
            {
                try
                {
                    if (Aria2Client == null)
                    {
                        Aria2ServerInfoModel aria2_server = new Aria2ServerInfoModel(true);
                        Aria2Client = new Aria2ClientModel(aria2_server);
                    }
                    try
                    {
                        await Aria2Client.Aria2Client.ChangeGlobalOptionAsync(
                            new Dictionary<string, string>
                            {
                                { "bt-tracker", string.Join(",", trackers.ToArray())}
                            }
                        );
                    }
                    catch { }
                }
                catch
                {
                    TaskBar?.ShowBalloonTip((string)Resources["NoServer"],
                        (string)Resources["NoServersAvaliable"], BalloonIcon.Error);
                }
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            new ConfigFile().Init();
            TaskBar = (TaskbarIcon)FindResource("AMNotifyIcon");
            //读取设置信息
            bool StartMin = false;
            bool StartAria2 = false;
            bool KillAria2 = false;
            bool CheckUpdate = false;
            bool UpdateTrackers = false;
            bool EnableAria2Notification = false;
            int UpdateInterval = 0;
            int LastUpdate = 0;
            string TrackersSource = "";
            XmlDocument doc = new XmlDocument();
            doc.Load("Configurations\\Settings.xml");
            var settings = doc.SelectSingleNode($"/Settings");
            if (settings == null)
            {
                return;
            }
            foreach (XmlNode node in settings.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Language":
                        SetLanguageDictionary(node.InnerText);
                        break;
                    case "Theme":
                        Resources.MergedDictionaries[3].Source = new Uri($"pack://application:,,,/MahApps.Metro;component/Styles/Themes/{node.InnerText}.xaml", UriKind.Absolute);
                        break;
                    case "StartMin":
                        StartMin = Convert.ToBoolean(node.InnerText);
                        break;
                    case "CloseToExit":
                        CloseToExit = Convert.ToBoolean(node.InnerText);
                        break;
                    case "StartAria2":
                        StartAria2 = Convert.ToBoolean(node.InnerText);
                        break;
                    case "KillAria2":
                        KillAria2 = Convert.ToBoolean(node.InnerText);
                        break;
                    case "CheckAria2Update":
                        CheckUpdate = Convert.ToBoolean(node.InnerText);
                        break;
                    case "CheckUpdate":
                        if (Convert.ToBoolean(node.InnerText))
                        {
                            InfoProgramUpdate();
                        }
                        break;
                    case "EnableAria2Notification":
                        EnableAria2Notification = Convert.ToBoolean(node.InnerText);
                        break;
                    case "UpdateTrackers":
                        foreach (XmlNode node2 in node.ChildNodes)
                        {
                            switch (node2.Name)
                            {
                                case "Enable":
                                    UpdateTrackers = Convert.ToBoolean(node2.InnerText);
                                    break;
                                case "UpdateInterval":
                                    UpdateInterval = Convert.ToInt32(node2.InnerText) * 24 * 60;
                                    break;
                                case "TrackersSource":
                                    TrackersSource = node2.InnerText;
                                    break;
                                case "LastUpdate":
                                    LastUpdate = Convert.ToInt32(node2.InnerText);
                                    break;
                            }
                        }
                        break;
                }
            }
            //启动Aria2
            if (StartAria2)
            {
                Process[] aria2Processes = Process.GetProcessesByName("aria2c");
                if (aria2Processes.Length > 0)
                {
                    if (KillAria2)
                    {
                        PID = aria2Processes[0].Id;
                    }
                }
                else
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory; // 或者其它确定的根目录
                    string exePath = Path.Combine(baseDir, "Aria2", "aria2c.exe");
                    string workDir = Path.Combine(baseDir, "Aria2");
                    Process process = new Process();
                    process.StartInfo.FileName = exePath;
                    process.StartInfo.Arguments = "--conf-path=aria2.conf";
                    process.StartInfo.WorkingDirectory = workDir;
                    //不显示Console窗口
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.CreateNoWindow = true;
                    try
                    {
                        process.Start();
                    }
                    catch
                    {
                        MessageBox.Show(Current.FindResource("StartAria2Fail").ToString(),
                            Current.FindResource("NoAria2File").ToString());
                        Current.Shutdown(); //退出程序
                        return;
                    }
                    if (KillAria2)
                    {
                        PID = process.Id;
                    }
                }
            }
            else
            {
                KillAria2 = false;
            }
            //启用Aria2通知
            if (EnableAria2Notification)
            {
                Task.Run(() => ListenAria2Event());
            }
            //检查Aria2更新
            if (CheckUpdate)
            {
                CheckAira2Update();
            }
            //定时更新Trackers
            if (UpdateTrackers)
            {
                CheckTrackersUpdate(LastUpdate, UpdateInterval, TrackersSource);
            }
            if (!StartMin) //是否打开主窗口
            {
                MainWindow main_window = new MainWindow(CloseToExit, PID);
                main_window.Show();
            }
        }
    }
}
