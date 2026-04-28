using CommunityToolkit.Mvvm.ComponentModel;

namespace Aria2Manager.Core.Models
{
    //Aria2服务器信息
    public partial class Aria2ServerInfo : ObservableObject
    {
        [ObservableProperty]
        private bool _isConnected = true; //是否连接到服务器
        [ObservableProperty]
        private long _downloadSpeed = 0; //当前下载速度
        [ObservableProperty]
        private long _uploadSpeed = 0; //当前上传速度
        [ObservableProperty]
        private int _numActive = 0; //当前活动任务数
        [ObservableProperty]
        private int _numStopped = 0; //当前停止任务数
        [ObservableProperty]
        private int _numWaiting = 0; //当前等待任务数
        public string ServerName { get; set; } = "New"; //服务器名称
        public string ServerAddress { get; set; } = "127.0.0.1"; //服务器地址
        public int ServerPort { get; set; } = 6800; //服务器端口
        public string ServerSecret { get; set; } = ""; //服务器密钥
        public bool IsLocal { get; set; } = true; //是否本地服务器
        public bool IsHttps { get; set; } = false; //是否使用https
        public bool UseProxy { get; set; } = false; //是否使用代理进行连接
        public Aria2ServerInfo() { }
        public Aria2ServerInfo(Aria2Server config)
        {
            ServerName = config.Name;
            ServerAddress = config.Address;
            ServerPort = config.Port;
            ServerSecret = config.Secret;
            IsLocal = config.IsLocalServer;
            IsHttps = config.IsHttps;
            UseProxy = config.UseProxy;
        }
        public void SyncFrom(Aria2ServerInfo info)
        {
            ServerName = info.ServerName;
            ServerAddress = info.ServerAddress;
            ServerPort = info.ServerPort;
            ServerSecret = info.ServerSecret;
            IsLocal = info.IsLocal;
            IsHttps = info.IsHttps;
            UseProxy = info.UseProxy;
            IsConnected = info.IsConnected;
            DownloadSpeed = info.DownloadSpeed;
            UploadSpeed = info.UploadSpeed;
            NumActive = info.NumActive;
            NumWaiting = info.NumWaiting;
            NumStopped = info.NumStopped;
        }
    }
}
