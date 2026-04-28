using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;
using Aria2Manager.Core.Services;
using Aria2Manager.Core.Services.Interfaces;
using Aria2NET;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace Aria2Manager.Core.ViewModels
{
    //下载项信息
    public class TaskViewModel : ObservableObject
    {
        private DownloadStatusResult _model;
        private static Dictionary<string, string> _statusKeys = new()
        {
            ["active"] = "Status_Active",
            ["waiting"] = "Status_Waiting",
            ["paused"] = "Status_Paused",
            ["error"] = "Status_Error",
            ["complete"] = "Status_Complete",
            ["removed"] = "Status_Removed"
        };
        public TaskViewModel(DownloadStatusResult model) => _model = model;
        public void Update(DownloadStatusResult newModel)
        {
            _model = newModel;
            OnPropertyChanged(string.Empty); //通知所有属性更新
        }
        public string Gid => _model.Gid;
        public string Name
        {
            get
            {
                if ((_model.Bittorrent == null) || (_model.Bittorrent.Info == null))
                {
                    return Path.GetFileName(_model.Files[0].Path);
                }
                else
                {
                    //种子
                    return _model.Bittorrent.Info.Name;
                }
            }
        }
        public string Size => FormatterHelper.BytesToString(_model.TotalLength);
        public double Progress
        {
            get
            {
                if (_model.TotalLength == 0)
                {
                    return 0;
                }
                else
                {
                    return Math.Round((double)_model.CompletedLength * 100 / _model.TotalLength, 2);
                }
            }
        }
        public string ETA
        {
            get
            {
                if (_model.DownloadSpeed == 0)
                {
                    return "--";
                }
                else
                {
                    return FormatterHelper.SecondsToString(
                        Convert.ToInt32((double)(_model.TotalLength - _model.CompletedLength) / _model.DownloadSpeed)
                    );
                }
            }
        }
        public string Status => LanguageHelper.GetString(_statusKeys[_model.Status]); //下载状态，本地化语言
        public string UploadSpeed => FormatterHelper.BytesToString(_model.UploadSpeed) + "/s";
        public string DownloadSpeed => FormatterHelper.BytesToString(_model.DownloadSpeed) + "/s";
        public string Uploaded => FormatterHelper.BytesToString(_model.UploadLength);
        public string Downloaded => FormatterHelper.BytesToString(_model.CompletedLength);
        public double Ratio => _model.CompletedLength == 0 ? 0 : Math.Round((double)_model.UploadLength / _model.CompletedLength, 2);
        public long Connections => _model.Connections;
        public long? NumSeeders => _model.NumSeeders;
    }
    public partial class MainViewModel : ObservableObject
    {
        private readonly IUIService _uiService;
        private CancellationTokenSource? _refreshCts;
        private Aria2ServerService Server => GlobalContext.Instance.Aria2Server; //Aria2服务器服务实例
        public ObservableCollection<string> ServerNames => GlobalContext.Instance.Aria2ServerNames; //Aria2服务器名称列表
        public Aria2ServerInfo ServerStatus => GlobalContext.Instance.Aria2Server.ServerInfo; //Aria2服务器状态信息
        public string? SelectedTaskGid { get; set; } //当前选中任务的GID
        public Aria2TaskStatus SelectedStatus { get; set; } = Aria2TaskStatus.all; //当前选中任务的GID
        //Aria2任务列表
        public ObservableCollection<TaskViewModel> Aria2TaskCollection { get; private set; } = new ObservableCollection<TaskViewModel>();
        public MainViewModel(IUIService uiService)
        {
            _uiService = uiService;
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
                    await RefreshTaskLoop(SelectedStatus);
                }
            }
            catch { }
        }
        //更新任务列表
        private async Task RefreshTaskLoop(Aria2TaskStatus status)
        {
            await Server.GetAria2Status();
            var latestTasks = await Server.GetAria2Tasks(status);
            var toRemoveList = Aria2TaskCollection.Where(vm => !latestTasks.Any(m => m.Gid == vm.Gid)).ToList();
            foreach (var item in toRemoveList)
            {
                Aria2TaskCollection.Remove(item); //移除不存在的任务
            }
            foreach (var model in latestTasks)
            {
                var existingTask = Aria2TaskCollection.FirstOrDefault(vm => vm.Gid == model.Gid);
                if (existingTask != null)
                {
                    //更新现有项
                    existingTask.Update(model);
                }
                else
                {
                    //添加到新任务
                    Aria2TaskCollection.Add(new TaskViewModel(model));
                }
            }
        }
        [RelayCommand]
        private void ChangeAria2Server(string? selectedName)
        {
            if (string.IsNullOrWhiteSpace(selectedName)) return;
            GlobalContext.Instance.ServerSettings.Current = selectedName;
            GlobalContext.Instance.SaveServers();
        }
        [RelayCommand]
        private void OpenAria2Servers()
        {
            _uiService.ShowDialogAsync(WindowType.Aria2ServersWindow, new Aria2ServersViewModel(_uiService));
        }
        [RelayCommand]
        private void OpenAria2NewTask()
        {
            _uiService.ShowWindow(WindowType.Aria2NewTaskWindow, new Aria2NewTaskViewModel(_uiService));
        }
        [RelayCommand]
        private void OpenAria2TaskInfo()
        {
            if (string.IsNullOrWhiteSpace(SelectedTaskGid)) { return; }
            _uiService.ShowDialogAsync(WindowType.Aria2TaskInfoWindow, new Aria2TaskInfoViewModel(_uiService, SelectedTaskGid));
        }
        [RelayCommand]
        private void OpenAria2Options()
        {
            _uiService.ShowWindow(WindowType.Aria2OptionsWindow, new Aria2OptionsViewModel(_uiService));
        }
        [RelayCommand]
        private void OpenSettings()
        {
            _uiService.ShowDialogAsync(WindowType.SettingsWindow, new SettingsViewModel(_uiService));
        }
        [RelayCommand]
        private void OpenAria2Status()
        {
            _uiService.ShowWindow(WindowType.Aria2StatusWindow, new Aria2StatusViewModel());
        }
        [RelayCommand]
        private void OpenAbout()
        {
            _uiService.ShowWindow(WindowType.AboutWindow, new AboutViewModel(_uiService));
        }
        [RelayCommand]
        private async Task OpenAria2Website()
        {
            const string url = "https://aria2.github.io";
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Win32Exception)
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("No_Browser_Found"), "Error", MsgBoxLevel.Error);
            }
            catch (Exception ex)
            {
                await _uiService.ShowMessageBoxAsync(ex.Message, "Error", MsgBoxLevel.Error);
            }
        }
        [RelayCommand]
        private void Exit()
        {
            _uiService.Exit();
        }
        private void DeleteLocalFileorFolder(string path, bool isFolder = false)
        {
            try
            {
                if (isFolder)
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                }
                else
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
            catch { }
        }
        private async Task DeleteLocalFiles(DownloadStatusResult task)
        {
            if (await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Remove_Local_Files"), "Question", MsgBoxLevel.Question) != true)
            {
                return;
            }
            if (task.Bittorrent == null) //非种子
            {
                DeleteLocalFileorFolder(task.Files[0].Path);
                DeleteLocalFileorFolder(task.Files[0].Path + ".aria2");
            }
            else if (task.Bittorrent.Info != null) //种子
            {
                DeleteLocalFileorFolder(Path.Combine(task.Dir, task.Bittorrent.Info.Name), true);
                DeleteLocalFileorFolder(Path.Combine(task.Dir, task.Bittorrent.Info.Name + ".aria2"));
            }
            else //种子METADATA
            {
                DeleteLocalFileorFolder(Path.Combine(task.Dir, task.InfoHash + ".torrent"));
            }
            
        }
        [RelayCommand]
        private async Task RemoveTask()
        {
            if (string.IsNullOrWhiteSpace(SelectedTaskGid)) { return; }
            try
            {
                var task = await Server.RemoveTask(SelectedTaskGid);
                if (ServerStatus.IsLocal)
                {
                    if (task.Status != "active") //active则不删除文件
                    {
                        await DeleteLocalFiles(task); //删除文件
                    }
                }
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Remove_Task_Failed"), "Error", MsgBoxLevel.Error);
            }
        }
        [RelayCommand]
        private async Task UnpauseTask()
        {
            if (string.IsNullOrWhiteSpace(SelectedTaskGid)) { return; }
            try
            {
                await Server.UnpauseTask(SelectedTaskGid);
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Unpause_Task_Failed"), "Error", MsgBoxLevel.Error);
            }
        }
        [RelayCommand]
        private async Task UnpauseAllTask()
        {
            try
            {
                await Server.UnpauseTask();
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Unpause_Task_Failed"), "Error", MsgBoxLevel.Error);
            }
        }
        [RelayCommand]
        private async Task PauseTask()
        {
            if (string.IsNullOrWhiteSpace(SelectedTaskGid)) { return; }
            try
            {
                await Server.PauseTask(SelectedTaskGid);
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Pause_Task_Failed"), "Error", MsgBoxLevel.Error);
            }
        }
        [RelayCommand]
        private async Task PauseAllTask()
        {
            try
            {
                await Server.PauseTask();
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Pause_Task_Failed"), "Error", MsgBoxLevel.Error);
            }
        }
        [RelayCommand]
        private async Task PurgeAllTask()
        {
            try
            {
                await Server.PurgeAllTask();
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Purge_Task_Failed"), "Error", MsgBoxLevel.Error);
            }
        }
    }
}
