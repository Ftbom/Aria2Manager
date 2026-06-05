using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Services;
using Aria2Manager.Core.Services.Interfaces;
using Aria2NET;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Aria2Manager.Core.ViewModels
{
    //下载项的文件
    public class FileViewModel : ObservableObject
    {
        private DownloadStatusFile _model;
        private readonly Func<bool> _onSelectionChanged; //状态改变时的回调函数
        public FileViewModel(DownloadStatusFile model, Func<bool> onSelectionChanged)
        {
            _model = model;
            _onSelectionChanged = onSelectionChanged;
        }
        public int Index => _model.Index; //序号
        public bool Selected
        {
            get => _model.Selected;
            set
            {
                _model.Selected = value;
                if (!_onSelectionChanged())
                {
                    _model.Selected = !value;
                    OnPropertyChanged(nameof(Selected));
                }
            }
        }
        public string Name => Path.GetFileName(_model.Path); //名称
    }
    public partial class TaskInfoViewModel : ObservableObject
    {
        public ObservableCollection<FileViewModel> Files { get; } = new ObservableCollection<FileViewModel>();
        [ObservableProperty]
        private bool _fileListCheckable = true;
        [ObservableProperty]
        private string? _seedRatio = string.Empty;
        [ObservableProperty]
        private string? _seedTime = string.Empty;
        [ObservableProperty]
        private string? _httpUser = string.Empty;
        [ObservableProperty]
        private string? _httpPasswd = string.Empty;
        [ObservableProperty]
        private string? _proxyAddress = string.Empty;
        [ObservableProperty]
        private string? _proxyPort = string.Empty;
        [ObservableProperty]
        private string? _proxyUser = string.Empty;
        [ObservableProperty]
        private string? _proxyPasswd = string.Empty;
        [ObservableProperty]
        private string _name = "--";
        [ObservableProperty]
        private string _size = "--";
        [ObservableProperty]
        private string _status = "--";
        [ObservableProperty]
        private string _downloaded = "--";
        [ObservableProperty]
        private string _uploaded = "--";
        [ObservableProperty]
        private string _downloadPath = "--";
        [ObservableProperty]
        private string _downloadSpeed = "--";
        [ObservableProperty]
        private string _UploadSpeed = "--";
        [ObservableProperty]
        private string _hash = "--";
        [ObservableProperty]
        private double _ratio = 0;
        [ObservableProperty]
        private string _seeders = "--";
        [ObservableProperty]
        private string _connections = "--";
        [ObservableProperty]
        private bool[] _pieces = new bool[0]; //下载进度
        [ObservableProperty]
        private bool _isBittorrent = false;
        [ObservableProperty]
        private bool _isHttp = true;
        [ObservableProperty]
        private bool _canChangeOptions = false;
        public void Update(DownloadStatusResult result, Func<bool> onFileSelectionChanged)
        {
            TaskViewModel task = new TaskViewModel(result);
            Name = task.Name;
            Size = task.Size;
            Status = task.Status;
            Downloaded = task.Downloaded;
            DownloadSpeed = task.DownloadSpeed;
            Uploaded = task.Uploaded;
            UploadSpeed = task.UploadSpeed;
            DownloadPath = result.Dir;
            Hash = result.InfoHash ?? "--";
            Ratio = task.Ratio;
            Seeders = task.NumSeeders?.ToString() ?? "--";
            Connections = task.Connections.ToString();
            if (Files.Count == 0 && result.Files != null)
            {
                foreach (var file in result.Files)
                {
                    Files.Add(new FileViewModel(file, onFileSelectionChanged));
                }
                FileListCheckable = Files.Count > 1; //如果文件数大于1，则允许选择
            }
            Pieces = ParseBitfield(result.Bitfield, result.NumPieces);
            CanChangeOptions = result.Status is "paused" or "waiting";
            IsBittorrent = CanChangeOptions && result.Bittorrent != null;
            IsHttp = CanChangeOptions && result.Bittorrent == null;
        }
        private bool[] ParseBitfield(string bitfieldHex, long? numPieces)
        {
            if (string.IsNullOrWhiteSpace(bitfieldHex) || !numPieces.HasValue)
            {
                return new bool[0];
            }
            bool[] piecesStatus = new bool[numPieces.Value];
            int pieceIndex = 0;
            foreach (char hexChar in bitfieldHex)
            {
                //将十六进制字符转换为4位整数
                int val = Convert.ToInt32(hexChar.ToString(), 16);
                //依次读取4个bit
                for (int i = 3; i >= 0; i--)
                {
                    if (pieceIndex >= numPieces)
                    {
                        break; //忽略填充的多余位
                    }
                    //位运算判断该bit是否为1
                    piecesStatus[pieceIndex] = (val & (1 << i)) != 0;
                    pieceIndex++;
                }
            }
            return piecesStatus;
        }
    }
    public partial class Aria2TaskInfoViewModel
    {
        private CancellationTokenSource? _refreshCts;
        private readonly IUIService _uiService;
        private readonly string _gid;
        private Aria2ServerService Server => GlobalContext.Instance.Aria2Server; //Aria2服务器服务实例
        public TaskInfoViewModel TaskInfo { get; private set; } = new TaskInfoViewModel();
        public Aria2TaskInfoViewModel(IUIService uiService, string gid)
        {
            _uiService = uiService;
            _gid = gid;
            InitTaskOptions();
        }
        private async void InitTaskOptions()
        {
            var optionsData = await Aria2OptionsHelper.LoadAria2Options(Server, _uiService, _gid);
            TaskInfo.SeedRatio = optionsData.SeedRatio;
            TaskInfo.SeedTime = optionsData.SeedTime;
            TaskInfo.HttpUser = optionsData.HttpUser;
            TaskInfo.HttpPasswd = optionsData.HttpPasswd;
            TaskInfo.ProxyAddress = optionsData.ProxyAddress;
            TaskInfo.ProxyPort = optionsData.ProxyPort;
            TaskInfo.ProxyUser = optionsData.ProxyUser;
            TaskInfo.ProxyPasswd = optionsData.ProxyPasswd;
        }
        //启动循环
        public void StartRefreshLoop()
        {
            //防止重复启动
            if (_refreshCts != null && !_refreshCts.IsCancellationRequested)
            {
                return;
            }
            _refreshCts = new CancellationTokenSource();
            //启动异步任务
            ExecuteRefreshLoopAsync(_refreshCts.Token);
        }
        //停止循环
        public void StopRefreshLoop()
        {
            if (_refreshCts != null)
            {
                _refreshCts.Cancel();
                _refreshCts.Dispose();
                _refreshCts = null;
            }
        }
        private async void ExecuteRefreshLoopAsync(CancellationToken token)
        {
            //每隔1秒刷新一次
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            try
            {
                while (await timer.WaitForNextTickAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    await RefreshStatus();
                }
            }
            catch { }
        }
        private async Task RefreshStatus()
        {
            try
            {
                var task = await Server.GetTaskStatus(_gid);
                TaskInfo.Update(task, OnSelectTaskFiles);
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Load_Task_Status_Failed"), "Error", MsgBoxLevel.Error);
            }
        }
        private bool OnSelectTaskFiles()
        {
            List<string> fileIndexs = new List<string>();
            foreach (var file in TaskInfo.Files)
            {
                if (file.Selected)
                {
                    fileIndexs.Add(file.Index.ToString());
                }
            }
            if (fileIndexs.Count == 0)
            {
                return false;
            }
            ChangeFileSelection(fileIndexs);
            return true;
        }
        private async void ChangeFileSelection(List<string> fileIndexs)
        {
            TaskInfo.FileListCheckable = false; //更改文件选择时暂时禁止更改，避免重复触发
            try
            {
                await Server.ChangeAria2Options(new Dictionary<string, string>()
                {
                    ["select-file"] = String.Join(',', fileIndexs.ToArray())
                }, _gid);
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Change_Task_File_Failed"), "Error", MsgBoxLevel.Error);
            }
            TaskInfo.FileListCheckable = TaskInfo.Files.Count > 1; //根据文件数决定是否允许更改
        }
        [RelayCommand]
        private async Task ChangeOptions()
        {
            if (!Server.ServerInfo.IsConnected) //服务器连接出错，直接退出
            {
                return;
            }
            try
            {
                var options = new Dictionary<string, string>();
                //按种类分别设置
                if (TaskInfo.IsBittorrent)
                {
                    Aria2OptionsHelper.AddOptions(options, "seed-time", TaskInfo.SeedTime);
                    Aria2OptionsHelper.AddOptions(options, "seed-ratio", TaskInfo.SeedRatio);
                }
                else if (TaskInfo.IsHttp)
                {
                    Aria2OptionsHelper.AddOptions(options, "http-user", TaskInfo.HttpUser);
                    Aria2OptionsHelper.AddOptions(options, "http-passwd", TaskInfo.HttpPasswd);
                    Aria2OptionsHelper.AddOptions(options, "all-proxy-passwd", TaskInfo.ProxyPasswd);
                    Aria2OptionsHelper.AddOptions(options, "all-proxy-user", TaskInfo.ProxyUser);
                    if ((!String.IsNullOrWhiteSpace(TaskInfo.ProxyAddress)) && (!String.IsNullOrWhiteSpace(TaskInfo.ProxyPort)))
                    {
                        options["all-proxy"] = TaskInfo.ProxyAddress + ":" + TaskInfo.ProxyPort;
                    }
                }
                await Server.ChangeAria2Options(options, _gid);
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Change_Options_Failed"), "Error", MsgBoxLevel.Error);
            }
        }
    }
}
