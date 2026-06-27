using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;
using Aria2Manager.Core.Services;
using Aria2Manager.Core.Services.Interfaces;
using Aria2NET;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Aria2Manager.Core.ViewModels
{
    public partial class BTPeerViewModel : ObservableObject
    {
        private long? _numPieces;
        private PeerResult _model;
        public string PeerId => BTPeerIdParser.Parse(_model.PeerId)?.ToString() ?? string.Empty;
        public string Address => $"{_model.Ip}:{_model.Port}";
        public bool[] Pieces => BitfieldParser.Parse(_model.Bitfield, _numPieces);
        public string DownloadSpeed => FormatterHelper.BytesToString(Convert.ToInt64(_model.DownloadSpeed)) + "/s";
        public string UploadSpeed => FormatterHelper.BytesToString(Convert.ToInt64(_model.UploadSpeed)) + "/s";
        public bool IsSeeder => _model.Seeder;
        public BTPeerViewModel(PeerResult model, long? numPieces)
        {
            _model = model;
            _numPieces = numPieces;
        }
        public void Update(PeerResult newModel, long? numPieces)
        {
            _model = newModel;
            _numPieces = numPieces;
            OnPropertyChanged(string.Empty); //通知所有属性更新
        }
    }
    //下载项的文件
    public partial class FileViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _selected; //文件是否选中
        [ObservableProperty]
        private bool _cancelAble = true; //文件是否可取消选中
        private string _baseDir; //下载任务的基础目录
        private DownloadStatusFile _model;
        private readonly Action _onSelectionChanged; //状态改变时的回调函数
        public FileViewModel(DownloadStatusFile model, string baseDir, Action onSelectionChanged)
        {
            _model = model;
            _baseDir = baseDir;
            _selected = model.Selected;
            _onSelectionChanged = onSelectionChanged;
        }
        public void UpdateCancelAble(bool oneFile)
        {
            CancelAble = (Selected && oneFile) ? false : true;
        }
        public int Index => _model.Index; //序号
        public string Name => Path.GetFileName(_model.Path); //名称
        public string Directory
        {
            get
            {
                string? dirName = Path.GetDirectoryName(_model.Path);
                if (string.IsNullOrWhiteSpace(dirName))
                {
                    return string.Empty;
                }
                string rePath = Path.GetRelativePath(_baseDir, dirName);
                if (rePath == ".")
                {
                    return string.Empty;
                }
                return rePath;
            }
        }
        partial void OnSelectedChanged(bool value)
        {
            _onSelectionChanged();
        }
    }
    public partial class TaskInfoViewModel : ObservableObject
    {
        private long? _numPieces = null; //总块数，用于解析下载进度
        private readonly Action _onFileSelectionChanged; //文件改变时的回调函数
        internal bool IsChangingFileSelection { get; set; } = false; //正在更改文件选择，避免重复触发
        public ObservableCollection<BTPeerViewModel> BTPeers { get; private set; } = new ObservableCollection<BTPeerViewModel>();
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
        private string _uploadSpeed = "--";
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
        private bool _isPeersVisible = false;
        [ObservableProperty]
        private bool _canChangeOptions = false;
        [ObservableProperty]
        private bool _canPause = true;
        [ObservableProperty]
        private bool _canResume = false;
        public TaskInfoViewModel(Action onFileChanged)
        {
            _onFileSelectionChanged = onFileChanged;
        }
        public void Update(DownloadStatusResult result)
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
            _numPieces = result.NumPieces;
            Pieces = BitfieldParser.Parse(result.Bitfield, _numPieces);
            CanChangeOptions = result.Status is "paused" or "waiting";
            IsBittorrent = CanChangeOptions && result.Bittorrent != null;
            IsPeersVisible = result.Status == "active" && result.Bittorrent != null && result.Bittorrent.Info != null;
            IsHttp = CanChangeOptions && result.Bittorrent == null;
            CanPause = result.Status == "active" || result.Status == "waiting";
            CanResume = result.Status == "paused";
            //文件列表初始化
            if (Files.Count == 0 && result.Files != null)
            {
                foreach (var file in result.Files)
                {
                    Files.Add(new FileViewModel(file, result.Dir, _onFileSelectionChanged));
                }
            }
            bool justOneSelecedFile = result.Files?.Count(file => file.Selected) == 1; //是否仅有一个文件被选中
            foreach (var file in Files)
            {
                file.UpdateCancelAble(justOneSelecedFile); //刷新文件可选状态
            }
            FileListCheckable = !IsChangingFileSelection && CanChangeOptions;
        }
        public void UpdatePeers(IList<PeerResult> peers)
        {
            var existingPeersDict = BTPeers.ToDictionary(vm => vm.Address);
            var PeersToAdd = new List<BTPeerViewModel>();
            foreach (var model in peers)
            {
                var address = $"{model.Ip}:{model.Port}";
                if (existingPeersDict.TryGetValue(address, out var existingPeer))
                {
                    existingPeer.Update(model, _numPieces);
                    existingPeersDict.Remove(address);
                }
                else
                {
                    PeersToAdd.Add(new BTPeerViewModel(model, _numPieces));
                }
            }
            foreach (var peerToRemove in existingPeersDict.Values)
            {
                BTPeers.Remove(peerToRemove);
            }
            foreach (var peerToAdd in PeersToAdd)
            {
                BTPeers.Add(peerToAdd);
            }
        }
    }
    public partial class Aria2TaskInfoViewModel
    {
        private CancellationTokenSource? _refreshCts;
        private readonly IUIService _uiService;
        private readonly string _gid;
        private Aria2ServerService Server => GlobalContext.Instance.Aria2Server; //Aria2服务器服务实例
        public ObservableCollection<NetSpeedData> NetSpeedLineDatas { get; private set; } = new ObservableCollection<NetSpeedData>();
        public TaskInfoViewModel TaskInfo { get; private set; }
        public bool IsRefreshBTPeers { get; set; } = false; //是否刷新BT连接Peer列表
        public Aria2TaskInfoViewModel(IUIService uiService, string gid)
        {
            _uiService = uiService;
            _gid = gid;
            TaskInfo = new TaskInfoViewModel(OnSelectTaskFiles);
            InitTaskOptions();
        }
        private async void InitTaskOptions()
        {
            try
            {
                var optionsData = await Server.GetAria2Options(["seed-ratio", "seed-time", "http-user",
                    "http-passwd", "all-proxy-passwd", "all-proxy-user", "all-proxy"], _gid);
                var parsedOptions = Aria2OptionsHelper.ParseAria2Options(optionsData);
                TaskInfo.SeedRatio = parsedOptions["seed-ratio"];
                TaskInfo.SeedTime = parsedOptions["seed-time"];
                TaskInfo.HttpUser = parsedOptions["http-user"];
                TaskInfo.HttpPasswd = parsedOptions["http-passwd"];
                TaskInfo.ProxyAddress = parsedOptions["proxy-address"];
                TaskInfo.ProxyPort = parsedOptions["proxy-port"];
                TaskInfo.ProxyUser = parsedOptions["all-proxy-user"];
                TaskInfo.ProxyPasswd = parsedOptions["all-proxy-passwd"];
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Load_Options_Failed"),
                    LanguageHelper.GetString("Error"), MsgBoxLevel.Error);
            }
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
            NetSpeedLineDatas.Clear();
        }
        private async void ExecuteRefreshLoopAsync(CancellationToken token)
        {
            //每隔1秒刷新一次
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            try
            {
                await RefreshStatus();
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
                if (TaskInfo.IsPeersVisible && IsRefreshBTPeers)
                {
                    var peers = await Server.GetTaskBTPeers(_gid);
                    TaskInfo.UpdatePeers(peers); //更新BT连接Peer列表
                }
                var task = await Server.GetTaskStatus(_gid);
                TaskInfo.Update(task); //更新任务信息
                if (NetSpeedLineDatas.Count > 120)
                {
                    NetSpeedLineDatas.RemoveAt(0);
                }
                NetSpeedLineDatas.Add(new NetSpeedData(task.DownloadSpeed, task.UploadSpeed));
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Load_Task_Status_Failed"),
                    LanguageHelper.GetString("Error"), MsgBoxLevel.Error);
                StopRefreshLoop();
            }
        }
        //文件选择改变时的回调函数
        private void OnSelectTaskFiles()
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
                return;
            }
            ChangeFileSelection(fileIndexs);
        }
        //更改任务文件设置
        private async void ChangeFileSelection(List<string> fileIndexs)
        {
            TaskInfo.IsChangingFileSelection = true; //更改文件选择时暂时禁止更改，避免重复触发
            try
            {
                await Server.ChangeAria2Options(new Dictionary<string, string>()
                {
                    ["select-file"] = String.Join(',', fileIndexs.ToArray())
                }, _gid);
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Change_Task_File_Failed"),
                    LanguageHelper.GetString("Error"), MsgBoxLevel.Error);
            }
            TaskInfo.IsChangingFileSelection = false;
        }
        [RelayCommand]
        private async Task PauseTask()
        {
            if (string.IsNullOrWhiteSpace(_gid)) { return; }
            try
            {
                await Server.PauseTask(_gid);
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Pause_Task_Failed"),
                    LanguageHelper.GetString("Error"), MsgBoxLevel.Error);
            }
        }
        [RelayCommand]
        private async Task UnPauseTask()
        {
            if (string.IsNullOrWhiteSpace(_gid)) { return; }
            try
            {
                await Server.UnpauseTask(_gid);
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Unpause_Task_Failed"),
                    LanguageHelper.GetString("Error"), MsgBoxLevel.Error);
            }
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
                var options = new Dictionary<string, string?>();
                //按种类分别设置
                if (TaskInfo.IsBittorrent)
                {
                    options["seed-time"] = TaskInfo.SeedTime;
                    options["seed-ratio"] = TaskInfo.SeedRatio;
                }
                else if (TaskInfo.IsHttp)
                {
                    options["http-user"] = TaskInfo.HttpUser;
                    options["http-passwd"] = TaskInfo.HttpPasswd;
                    options["all-proxy-passwd"] = TaskInfo.ProxyPasswd;
                    options["all-proxy-user"] = TaskInfo.ProxyUser;
                    options["proxy-port"] = TaskInfo.ProxyPort;
                    options["proxy-address"] = TaskInfo.ProxyAddress;
                }
                await Server.ChangeAria2Options(Aria2OptionsHelper.MergeAria2Options(options)
                    .Where(kvp => kvp.Value is string)
                    .ToDictionary(kvp => kvp.Key, kvp => (string)kvp.Value),
                    _gid);
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Change_Options_Failed"),
                    LanguageHelper.GetString("Error"), MsgBoxLevel.Error);
            }
            await Task.Delay(500);
        }
    }
}
