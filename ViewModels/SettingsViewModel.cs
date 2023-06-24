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

namespace Aria2Manager.ViewModels
{
    public class SettingsViewModel
    {
        public ICommand SaveSettingsCommand { get; private set; }
        public List<string> Languages { get; set; }
        public bool StartMin { get; set; }
        public bool CloseToExit { set; get; }
        public string SelectedLanguage { get; set; }

        public SettingsViewModel()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("Configurations\\Settings.xml");
            var settings = doc.SelectSingleNode($"/Settings");
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
                }
            }
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
                    Languages.Add("Default(en)");
                }
                else
                {
                    Languages.Add(language_name);
                }
            }
            SaveSettingsCommand = new RelayCommand(SaveSettings);
        }

        private void SaveSettings(object? parameter)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("Configurations\\Settings.xml");
            var Node = doc.SelectSingleNode("/Settings/Language");
            Node.InnerText = SelectedLanguage;
            Node = doc.SelectSingleNode("/Settings/StartMin");
            Node.InnerText = StartMin.ToString();
            Node = doc.SelectSingleNode("/Settings/CloseToExit");
            Node.InnerText = CloseToExit.ToString();
            doc.Save("Configurations\\Settings.xml");
            //保存成功提示
            Button button = (Button)parameter;
            button.Content = Application.Current.FindResource("SavedSuccessfully").ToString();
            button.Foreground = new SolidColorBrush(Colors.Green);
            Button2Default(button); //恢复按钮样式
        }

        async private void Button2Default(Button button)
        {
            await Task.Delay(1000);
            button.Content = Application.Current.FindResource("Save").ToString();
            button.Foreground = new SolidColorBrush(Colors.Black);
        }
    }
}
