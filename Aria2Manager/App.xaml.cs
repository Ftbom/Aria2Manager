using Aria2Manager.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Xml;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace Aria2Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private bool CloseToExit;
        private bool KillAria2;
        private int PID;
        private bool UpdateTrackers;
        private int UpdateInterval;
        private int LastUpdate;
        private string? TrackersSource;

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

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (KillAria2)
            {
                Process.GetProcessById(PID).Kill();
            }
            Application.Current.Shutdown(); //退出程序
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.Count > 0)
            {
                return;
            }
            MainWindow main_window = new MainWindow(CloseToExit, PID);
            main_window.Show();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //读取设置信息
            bool StartMin = false;
            bool StartAria2 = false;
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
                        Resources.MergedDictionaries[3].Source = new Uri($"pack://application:,,,/MahApps.Metro;component/Styles/Themes/{node.InnerText}.Green.xaml", UriKind.Absolute);
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
                    PID = aria2Processes[0].Id;
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
                    process.Start();
                    PID = process.Id;
                }
            }
            //定时更新Trackers
            int NowMinute = (int)(DateTime.Now - new DateTime(2001, 1, 1)).TotalMinutes;
            List<string>? trackers = null;
            if (UpdateTrackers && ((NowMinute - LastUpdate) >= UpdateInterval))
            {
                //获取Trackers
                TrackersModel trackers_model = new TrackersModel();
                trackers = trackers_model.GetTrackers(TrackersSource);
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
            else
            {
                if (File.Exists("trackers.txt"))
                {
                    trackers = File.ReadAllLines("trackers.txt").ToList();
                }
            }
            //每次启动设置Trackers
            if (trackers != null)
            {
                try
                {
                    Aria2ServerInfoModel aria2_server = new Aria2ServerInfoModel(true);
                    try
                    {
                        Aria2ClientModel client = new Aria2ClientModel(aria2_server);
                        client.Aria2Client.ChangeGlobalOptionAsync(
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
                    MessageBox.Show(Application.Current.FindResource("NoServersAvaliable").ToString(),
                        "NoServersAvaliable", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            if (!StartMin) //是否打开主窗口
            {
                MainWindow main_window = new MainWindow(CloseToExit, PID);
                main_window.Show();
            }
        }
    }
}
