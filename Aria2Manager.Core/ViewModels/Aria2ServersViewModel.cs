using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;
using Aria2Manager.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Aria2Manager.Core.ViewModels
{
    public partial class Aria2ServersViewModel : ObservableObject
    {
        private readonly IUIService _uiService;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentEditServer))]
        private int _currentEditServerIndex = 0; //当前编辑的服务器的索引号
        public Aria2Server? CurrentEditServer
        {
            get
            {
                //删除服务器时数组越界，进行保护
                if (CurrentEditServerIndex >= 0 && CurrentEditServerIndex < AvailableServers.Count)
                {
                    return AvailableServers[CurrentEditServerIndex];
                }
                return null;
            }
        }
        public ObservableCollection<Aria2Server> AvailableServers { get; set; }
        public ProxyConfig Proxy { get; set; } = GlobalContext.Instance.ServerSettings.Proxy.DeepClone();
        public List<ProxyType> ProxyTypes { get; set; } = Enum.GetValues<ProxyType>().ToList();
        public Aria2ServersViewModel(IUIService uiService)
        {
            _uiService = uiService;
            AvailableServers = new ObservableCollection<Aria2Server>(GlobalContext.Instance.ServerSettings.ServerConfigs.Select(s => s.DeepClone()));
        }
        private static string GetRandomString(int length = 4)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        [RelayCommand]
        private void AddNewServer()
        {
            Aria2Server NewServer = new Aria2Server() { Name = $"New_{GetRandomString()}" };
            AvailableServers.Add(NewServer);
            CurrentEditServerIndex = AvailableServers.Count - 1; //添加新服务器后进行编辑
        }
        [RelayCommand]
        private void DeleteServer() //删除当前编辑的服务器
        {
            if (CurrentEditServerIndex >= 0 && CurrentEditServerIndex < AvailableServers.Count)
            {
                AvailableServers.RemoveAt(CurrentEditServerIndex);
            }
            if (AvailableServers.Count == 0)
            {
                AddNewServer();
            }
            else
            {
                CurrentEditServerIndex = 0;
            }
        }
        [RelayCommand]
        private async Task SaveServers()
        {
            if (AvailableServers.Select(s => s.Name).Distinct().Count() != AvailableServers.Count)
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Duplicate_Server_Name"), "Warn", MsgBoxLevel.Warning);
            }
            string oldServerName = GlobalContext.Instance.ServerSettings.Current;
            GlobalContext.Instance.ServerSettings.ServerConfigs.Clear();
            bool needResetCurrent = true;
            foreach (var server in AvailableServers)
            {
                if (server.Name == oldServerName)
                {
                    needResetCurrent = false;
                }
                GlobalContext.Instance.ServerSettings.ServerConfigs.Add(server.DeepClone());
            }
            if (needResetCurrent)
            {
                GlobalContext.Instance.ServerSettings.Current = AvailableServers[0].Name; //重置当前服务器为第一个
            }
            GlobalContext.Instance.SaveServers();
            await Task.Delay(500); //点击按钮后等待一段时间再启用，防止用户连续点击
        }
        [RelayCommand]
        private async Task SaveProxy()
        {
            GlobalContext.Instance.ServerSettings.Proxy = Proxy.DeepClone();
            GlobalContext.Instance.SaveServers();
            await Task.Delay(500); //点击按钮后等待一段时间再启用，防止用户连续点击
        }
    }
}