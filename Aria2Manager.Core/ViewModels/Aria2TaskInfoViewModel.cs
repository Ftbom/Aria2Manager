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
    public class FileViewModel
    {
        private DownloadStatusFile _model;
        private readonly Action _onSelectionChanged; //回调委托
        public FileViewModel(DownloadStatusFile model, Action onSelectionChanged)
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
                _onSelectionChanged?.Invoke();
            }
        }
        public string Name => Path.GetFileName(_model.Path); //名称
    }
    public class TaskInfoViewModel : ObservableObject
    {
        private TaskViewModel? _task = null;
        public ObservableCollection<FileViewModel> Files { get; set; } = new ObservableCollection<FileViewModel>();
        public bool FileCheckable => Files.Count > 1;
        public string Name => _task?.Name ?? "--";
        public string Size => _task?.Size ?? "--";
        public string Status => _task?.Status ?? "--";
        public string Progress => _task == null ? "--" : $"{LanguageHelper.GetString("Downloaded")}:{_task.Downloaded}, {LanguageHelper.GetString("Uploaded")}:{_task.Uploaded}";
        public string DownloadPath { get; private set; } = "--";
        public string Speed => _task == null ? "--" : $"{LanguageHelper.GetString("Download_Speed")}:{_task.DownloadSpeed}, {LanguageHelper.GetString("Upload_Speed")}:{_task.UploadSpeed}";
        public string Hash { get; private set; } = "--";
        public string Ratio => _task?.Ratio.ToString() ?? "--";
        public string Connections
        {
            get
            {
                if (_task == null)
                {
                    return "--";
                }
                else
                {
                    if (_task.NumSeeders == null)
                    {

                        return $"{LanguageHelper.GetString("Connections")}:{_task.Connections}";
                    }
                    else
                    {
                        return $"{LanguageHelper.GetString("Seeds")}:{_task.NumSeeders}, {LanguageHelper.GetString("Connections")}:{_task.Connections}";
                    }
                }
            }
        }
        public void Update(DownloadStatusResult result, Action onFileSelectionChanged)
        {
            _task = new TaskViewModel(result);
            DownloadPath = result.Dir;
            Hash = result.InfoHash ?? "--";
            foreach (var file in result.Files)
            {
                Files.Add(new FileViewModel(file, onFileSelectionChanged));
            }
            OnPropertyChanged(string.Empty); //通知所有属性更新
        }
        public void RefreshFileCheckable() => OnPropertyChanged(nameof(FileCheckable));
    }
    public partial class Aria2TaskInfoViewModel
    {
        private readonly IUIService _uiService;
        private readonly string _gid;
        private Aria2ServerService Server => GlobalContext.Instance.Aria2Server; //Aria2服务器服务实例
        public TaskInfoViewModel TaskInfo { get; private set; } = new TaskInfoViewModel();
        public Aria2TaskInfoViewModel(IUIService uiService, string gid)
        {
            _uiService = uiService;
            _gid = gid;
            RefreshStatus();
        }
        private async void RefreshStatus()
        {
            try
            {
                var task = await Server.GetTaskStatus(_gid);
                TaskInfo.Update(task, async () => await OnSelectTaskFiles());
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Load_Task_Status_Failed"), "Error", MsgBoxLevel.Error);
            }
        }
        private async Task OnSelectTaskFiles()
        {
            TaskInfo.RefreshFileCheckable();
            try
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
                await Server.ChangeAria2Options(new Dictionary<string, string>()
                {
                    ["select-file"] = String.Join(',', fileIndexs.ToArray())
                }, _gid);
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Change_Task_File_Fail"), "Error", MsgBoxLevel.Error);
            }
        }
    }
}
