using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using Aria2Manager.Utils;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
using Aria2Manager.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aria2Manager.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand SaveSettingsCommand { get; private set; }
        public List<string>? Languages { get; set; } //语言列表
        public List<string>? Themes { get; private set; } //主题列表
        public bool? StartMin { get; set; } //启动时最小化
        public bool? CloseToExit { set; get; } //关闭主窗口时退出
        public bool? StartAria2 { get; set; } //启动时启动Aria2
        public bool? KillAria2 { get; set; } //关闭时停止Aria2
        public bool? CheckAria2Update { get; set; } //检查Aria2更新
        public bool? CheckUpdate { get; set; } //检查更新
        public bool? EnableAria2Notification { get; set; } //启用Aria2通知
        public string? SelectedLanguage { get; set; } //当前选中的语言
        public string? SelectedTheme { get; set; } //当前选中的主题
        public bool UpdateTrackers //是否更新Trackers
        {
            get
            {
                return _update_trackers;
            }
            set
            {
                _update_trackers = value;
                OnPropertyChanged(nameof(UpdateTrackers));
            }
        }
        public string? SelectedSource { get; set; } //选中的Trackers来源
        public int? UpdateInterval { get; set; } //Trackers更新间隔
        public Dictionary<string, string>? TrackersSources { get; set; } //Trackers来源列表

        private TrackersModel _trackers = new TrackersModel();
        private bool _update_trackers;

        public SettingsViewModel()
        {
            SaveSettingsCommand = new RelayCommand(SaveSettings);
            //从文件读取设置项
            XmlDocument doc = new XmlDocument();
            doc.Load("Configurations\\Settings.xml");
            var settings = doc.SelectSingleNode($"/Settings");
            if (settings == null)
            {
                return;
            }
            Themes = new List<string> {
                "Light.Green", "Dark.Green",
                "Light.Blue", "Dark.Blue",
                "Light.Red", "Dark.Red",
                "Light.Purple", "Dark.Purple",
                "Light.Orange", "Dark.Orange",
                "Light.Lime", "Dark.Lime",
                "Light.Emerald", "Dark.Emerald",
                "Light.Teal", "Dark.Teal",
                "Light.Cyan", "Dark.Cyan",
                "Light.Cobalt", "Dark.Cobalt",
                "Light.Indigo", "Dark.Indigo",
                "Light.Violet", "Dark.Violet",
                "Light.Pink", "Dark.Pink",
                "Light.Magenta", "Dark.Magenta",
                "Light.Crimson", "Dark.Crimson",
                "Light.Amber", "Dark.Amber",
                "Light.Yellow", "Dark.Yellow",
                "Light.Brown", "Dark.Brown",
                "Light.Olive", "Dark.Olive",
                "Light.Steel", "Dark.Steel",
                "Light.Mauve", "Dark.Mauve",
                "Light.Taupe", "Dark.Taupe",
                "Light.Sienna", "Dark.Sienna"
            };
            foreach (XmlNode node in settings.ChildNodes)
            {
                switch(node.Name)
                {
                    case "Language":
                        SelectedLanguage = node.InnerText;
                        break;
                    case "Theme":
                        SelectedTheme = node.InnerText;
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
                        CheckAria2Update = Convert.ToBoolean(node.InnerText);
                        break;
                    case "CheckUpdate":
                        CheckUpdate = Convert.ToBoolean(node.InnerText);
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
                                    UpdateInterval = Convert.ToInt32(node2.InnerText);
                                    break;
                                case "TrackersSource":
                                    SelectedSource = node2.InnerText;
                                    break;
                            }
                        }
                        break;
                }
            }
            TrackersSources = _trackers.TrackersSources;
            Languages = new List<string>();
            DirectoryInfo dir = new DirectoryInfo("Languages");
            FileInfo[] files = dir.GetFiles("*.xaml");
            foreach (var file in files)
            {
                StringBuilder builder = new StringBuilder(file.Name);
                builder.Replace("Strings.", "");
                builder.Replace("xaml", "");
                builder.Replace(".", "");
                var language_name = builder.ToString();
                if (language_name == "")
                {
                    Languages.Add("en-US");
                }
                else
                {
                    Languages.Add(language_name);
                }
            }
        }

        private void SaveSettings(object? parameter)
        {
            if (StartAria2 == true)
            {
                //检查Aria2是否存在
                if (!File.Exists("Aria2/aria2c.exe"))
                {
                    StartAria2 = false;
                    KillAria2 = false;
                    MessageBox.Show(Application.Current.FindResource("Aria2FileNotFound").ToString());
                    return;
                }
            }
            //保存到文件
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("Configurations\\Settings.xml");
                var Node = doc.SelectSingleNode("/Settings/Language");
                Node.InnerText = SelectedLanguage;
                if (SelectedLanguage == "en-US")
                {
                    Application.Current.Resources.MergedDictionaries[0].Source = new Uri($"..\\Languages\\Strings.xaml", UriKind.Relative);
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries[0].Source = new Uri($"..\\Languages\\Strings.{SelectedLanguage}.xaml", UriKind.Relative);
                }
                Node = doc.SelectSingleNode("/Settings/Theme");
                Node.InnerText = SelectedTheme;
                Application.Current.Resources.MergedDictionaries[3].Source = new Uri($"pack://application:,,,/MahApps.Metro;component/Styles/Themes/{SelectedTheme}.xaml", UriKind.Absolute); 
                Node = doc.SelectSingleNode("/Settings/StartMin");
                Node.InnerText = StartMin.ToString();
                Node = doc.SelectSingleNode("/Settings/CloseToExit");
                Node.InnerText = CloseToExit.ToString();
                Node = doc.SelectSingleNode("/Settings/StartAria2");
                Node.InnerText = StartAria2.ToString();
                Node = doc.SelectSingleNode("/Settings/KillAria2");
                Node.InnerText = KillAria2.ToString();
                Node = doc.SelectSingleNode("/Settings/CheckAria2Update");
                Node.InnerText = CheckAria2Update.ToString();
                Node = doc.SelectSingleNode("/Settings/CheckUpdate");
                Node.InnerText = CheckUpdate.ToString();
                Node = doc.SelectSingleNode("/Settings/EnableAria2Notification");
                Node.InnerText = EnableAria2Notification.ToString();
                Node = doc.SelectSingleNode("/Settings/UpdateTrackers/Enable");
                Node.InnerText = UpdateTrackers.ToString();
                Node = doc.SelectSingleNode("/Settings/UpdateTrackers/UpdateInterval");
                Node.InnerText = UpdateInterval.ToString();
                Node = doc.SelectSingleNode("/Settings/UpdateTrackers/TrackersSource");
                Node.InnerText = SelectedSource.ToString();
                doc.Save("Configurations\\Settings.xml");
            }
            catch
            {
                return;
            }
            //保存成功提示
            if (parameter != null)
            {
                Button button = (Button)parameter;
                button.Content = Application.Current.FindResource("SavedSuccessfully").ToString();
                button.Foreground = (SolidColorBrush)Application.Current.Resources["MahApps.Brushes.Accent"];
                Button2Default(button); //恢复按钮样式
            }
        }

        async private void Button2Default(Button button)
        {
            await Task.Delay(1000);
            button.Content = Application.Current.FindResource("Save").ToString();
            button.Foreground = (Brush)Application.Current.Resources["MahApps.Brushes.ThemeForeground"]; ;
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
