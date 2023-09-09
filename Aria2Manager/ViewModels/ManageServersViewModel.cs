using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Aria2Manager.Models;
using Aria2Manager.Utils;
using System.Windows.Controls;

namespace Aria2Manager.ViewModels
{
    public class ManageServersViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        //用于绑定的指令
        public ICommand AddNewServerCommand { get; set; }
        public ICommand DeleteServerCommand { get; set; }
        public ICommand SaveEditCommand { get; set; }
        public ICommand SaveSettingsCommand { get; set; }


        private ObservableCollection<Aria2ServerInfoModel> _servers; //服务器配置列表
        private Aria2ServerInfoModel _editserver; //正在编辑的服务器配置
        private Aria2ServerInfoModel _currentserver; //当前正使用的服务器
        private int _serverindex; //选中的服务器在列表中的位置

        //代理设置
        public string? ProxyType { get; set; }
        public string? ProxyAddress { get; set; }
        public string? ProxyPort { get; set; }
        public string? ProxyUser { get; set; }
        public string? ProxyPasswd { get; set; }
        public List<string> ProxyTypes { get; set; }
        public Aria2ServerInfoModel CurrentServer
        {
            get
            {
                //根据_currentserver返回服务器列表中对应元素
                foreach (Aria2ServerInfoModel server in Servers)
                {
                    if (server.ServerName == _currentserver.ServerName)
                    {
                        return server;
                    }
                }
                return _currentserver;
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                //根据选中的服务器，更新index
                for (int i = 0; i < Servers.Count; i++)
                {
                    if (Servers[i].ServerName == value.ServerName)
                    {
                        _serverindex = i;
                        break;
                    }
                }
            }
        }
        public Aria2ServerInfoModel EditServer
        {
            get => _editserver;
            set
            {
                if (value != _editserver)
                {
                    _editserver = value;
                    OnPropertyChanged(); //更改通知
                }
            }
        }
        public ObservableCollection<Aria2ServerInfoModel> Servers
        {
            get => _servers;
            set
            {
                if (value != _servers)
                {
                    _servers = value;
                    OnPropertyChanged();
                }
            }
        }

        public ManageServersViewModel(Aria2ServerInfoModel? Server = null)
        {
            if (Server == null)
            {
                _currentserver = new Aria2ServerInfoModel(); //储存当前服务器信息
            }
            else
            {
                _currentserver = Server;
            }
            //设置指令
            AddNewServerCommand = new RelayCommand(AddNewServer);
            SaveEditCommand = new RelayCommand(SaveServers);
            SaveSettingsCommand = new RelayCommand(SaveSettings);
            DeleteServerCommand = new RelayCommand(DeleteServer);
            //从配置文件中加载服务器信息
            _servers = new ObservableCollection<Aria2ServerInfoModel>();
            XmlDocument doc = new XmlDocument();
            doc.Load("Configurations\\Aria2Servers.xml");
            var current = doc.SelectSingleNode($"/Servers/Avaliable");
            try
            {
                if (current == null)
                {
                    throw new Exception("Fail to read config file");
                }
                foreach (string name in current.InnerText.Split(','))
                {
                    _servers.Add(new Aria2ServerInfoModel(name));
                }
                _editserver = _servers[0];
            }
            catch
            {
                //配置文件无信息则使用默认值
                _servers.Add(new Aria2ServerInfoModel());
                _editserver = _servers[0];
            }
            //从文件中读取代理信息
            try
            {
                var proxy = doc.SelectSingleNode("/Servers/Proxy");
                if (proxy == null)
                {
                    throw new Exception("Config of Proxy don't find");
                }
                foreach (XmlNode node in proxy.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Address":
                            ProxyAddress = node.InnerText;
                            break;
                        case "Port":
                            ProxyPort = node.InnerText;
                            break;
                        case "Type":
                            ProxyType = node.InnerText;
                            break;
                        case "User":
                            ProxyUser = node.InnerText;
                            break;
                        case "Passwd":
                            ProxyPasswd = node.InnerText;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch
            {
                //默认代理信息
                ProxyType = "http";
                ProxyAddress = "127.0.0.1";
                ProxyPort = "10809";
                ProxyUser = "";
                ProxyPasswd = "";
            }
            ProxyTypes = new List<string>();
            ProxyTypes.AddRange(new string[] { "http", "socks4", "socks5" }); //代理类型
        }

        //添加新服务
        private void AddNewServer(object? parameter)
        {
            Aria2ServerInfoModel NewServer = new Aria2ServerInfoModel();
            Servers.Add(NewServer);
            EditServer = NewServer;
        }

        //删除服务
        private void DeleteServer(object? parameter)
        {
            if (_currentserver.ServerName == EditServer.ServerName)
            {
                Button button = (Button)parameter;
                button.Content = Application.Current.FindResource("DeleteFail").ToString();
                button.Foreground = new SolidColorBrush(Colors.Red);
                Button2Default(button); //恢复按钮样式
            }
            else
            {
                Servers.Remove(EditServer);
                EditServer = Servers[0];
            }
        }

        //保存服务器编辑结果
        private void SaveServers(object? parameter)
        {
            //若当前仅有一个服务器配置，将其作为当前配置
            if (Servers.Count == 1)
            {
                _currentserver.ServerName = Servers[0].ServerName;
                _currentserver.ServerAddress = Servers[0].ServerAddress;
                _currentserver.ServerPort = Servers[0].ServerPort;
                _currentserver.ServerSecret = Servers[0].ServerSecret;
                _currentserver.IsHttps = Servers[0].IsHttps;
                _currentserver.UseProxy = Servers[0].UseProxy;
            }
            //保存到文件
            XmlDocument doc = new XmlDocument();
            doc.Load("Configurations\\Aria2Servers.xml");
            XmlNode Node = doc.SelectSingleNode("/Servers/Current");
            Node.InnerText = CurrentServer.ServerName;
            List<string> server_names = new List<string>();
            XmlNode ServerConfigsNode = doc.SelectSingleNode("/Servers/ServerConfigs");
            ServerConfigsNode.RemoveAll();
            foreach (Aria2ServerInfoModel server in Servers)
            {
                XmlNode ServerNode = doc.CreateElement(server.ServerName);
                XmlNode TempNode = doc.CreateElement("Address");
                TempNode.InnerText = server.ServerAddress;
                ServerNode.AppendChild(TempNode);
                TempNode = doc.CreateElement("Port");
                TempNode.InnerText = server.ServerPort;
                ServerNode.AppendChild(TempNode);
                TempNode = doc.CreateElement("Secret");
                TempNode.InnerText = server.ServerSecret;
                ServerNode.AppendChild(TempNode);
                TempNode = doc.CreateElement("IsHttps");
                TempNode.InnerText = server.IsHttps.ToString();
                ServerNode.AppendChild(TempNode);
                TempNode = doc.CreateElement("UseProxy");
                TempNode.InnerText = server.UseProxy.ToString();
                ServerNode.AppendChild(TempNode);
                ServerConfigsNode.AppendChild(ServerNode);
                server_names.Add(server.ServerName);
            }
            Node = doc.SelectSingleNode("/Servers/Avaliable");
            Node.InnerText = String.Join(',', server_names.ToArray());
            doc.Save("Configurations\\Aria2Servers.xml");
            _currentserver.UpdateServerInfo(); //更新服务器信息
            //保存成功提示
            Button button = (Button)parameter;
            button.Content = Application.Current.FindResource("SavedSuccessfully").ToString();
            button.Foreground = new SolidColorBrush(Colors.Green);
            Button2Default(button); //恢复按钮样式
        }

        //保存服务器设置
        private void SaveSettings(object? parameter)
        {
            //更新当前服务器信息
            _currentserver.ServerName = Servers[_serverindex].ServerName;
            _currentserver.ServerAddress = Servers[_serverindex].ServerAddress;
            _currentserver.ServerPort = Servers[_serverindex].ServerPort;
            _currentserver.ServerSecret = Servers[_serverindex].ServerSecret;
            _currentserver.IsHttps = Servers[_serverindex].IsHttps;
            _currentserver.UseProxy = Servers[_serverindex].UseProxy;
            //保存代理设置
            XmlDocument doc = new XmlDocument();
            doc.Load("Configurations\\Aria2Servers.xml");
            XmlNode Node = doc.SelectSingleNode("/Servers/Current");
            Node.InnerText = CurrentServer.ServerName;
            var proxy = doc.SelectSingleNode("/Servers/Proxy");
            foreach (XmlNode node in proxy.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Address":
                        node.InnerText = ProxyAddress;
                        break;
                    case "Port":
                        node.InnerText = ProxyPort;
                        break;
                    case "Type":
                        node.InnerText = ProxyType;
                        break;
                    case "User":
                        node.InnerText = ProxyUser;
                        break;
                    case "Passwd":
                        node.InnerText = ProxyPasswd;
                        break;
                    default:
                        break;
                }
            }
            doc.Save("Configurations\\Aria2Servers.xml");
            //成功提示
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

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
