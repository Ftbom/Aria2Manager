using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Services;
using Aria2Manager.Core.Services.Interfaces;
using Aria2NET;
using CommunityToolkit.Mvvm.ComponentModel;
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
        private TaskViewModel? _task = null;
        public ObservableCollection<FileViewModel> Files { get; set; } = new ObservableCollection<FileViewModel>();
        [ObservableProperty]
        private bool _fileListCheckable = true;
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
        public void Update(DownloadStatusResult result, Func<bool> onFileSelectionChanged)
        {
            _task = new TaskViewModel(result);
            DownloadPath = result.Dir;
            Hash = result.InfoHash ?? "--";
            foreach (var file in result.Files)
            {
                Files.Add(new FileViewModel(file, onFileSelectionChanged));
            }
            FileListCheckable = Files.Count > 1; //如果文件数大于1，则允许选择
            OnPropertyChanged(string.Empty); //通知所有属性更新
        }
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
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Change_Task_File_Fail"), "Error", MsgBoxLevel.Error);
            }
            TaskInfo.FileListCheckable = TaskInfo.Files.Count > 1; //根据文件数决定是否允许更改
        }
    }
}
