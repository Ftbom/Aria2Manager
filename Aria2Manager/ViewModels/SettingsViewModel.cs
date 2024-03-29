﻿using System;
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
        public bool? StartMin { get; set; } //启动时最小化
        public bool? CloseToExit { set; get; } //关闭主窗口时退出
        public bool? StartAria2 { get; set; } //启动时启动Aria2
        public bool? KillAria2 { get; set; } //关闭时停止Aria2
        public string? SelectedLanguage { get; set; } //当前选中的语言
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
            foreach (XmlNode node in settings.ChildNodes)
            {
                switch(node.Name)
                {
                    case "Language":
                        SelectedLanguage = node.InnerText;
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
            //保存到文件
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("Configurations\\Settings.xml");
                var Node = doc.SelectSingleNode("/Settings/Language");
                Node.InnerText = SelectedLanguage;
                Node = doc.SelectSingleNode("/Settings/StartMin");
                Node.InnerText = StartMin.ToString();
                Node = doc.SelectSingleNode("/Settings/CloseToExit");
                Node.InnerText = CloseToExit.ToString();
                Node = doc.SelectSingleNode("/Settings/StartAria2");
                Node.InnerText = StartAria2.ToString();
                Node = doc.SelectSingleNode("/Settings/KillAria2");
                Node.InnerText = KillAria2.ToString();
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
                button.Foreground = new SolidColorBrush(Colors.Green);
                Button2Default(button); //恢复按钮样式
            }
        }

        async private void Button2Default(Button button)
        {
            await Task.Delay(1000);
            button.Content = Application.Current.FindResource("Save").ToString();
            button.Foreground = new SolidColorBrush(Colors.Black);
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
