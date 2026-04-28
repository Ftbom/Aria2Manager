using Aria2Manager.Core.Models;
using Aria2Manager.Models;
using Aria2Manager.Utils;
using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Websocket.Client;

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
