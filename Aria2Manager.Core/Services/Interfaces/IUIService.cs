using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Models;

namespace Aria2Manager.Core.Services.Interfaces
{
    
    //窗口调度服务接口
    public interface IUIService
    {
        List<string> ThemeList { get; }
        //打开一个非模态窗口，返回id
        string ShowWindow(WindowType windowType, object? dataContext = null);
        //打开一个模态窗口，非阻塞
        Task ShowDialogAsync(WindowType windowType, object? dataContext = null);
        //关闭指定id的窗口
        void CloseWindow(string windowId);
        //显示一个消息框，非阻塞
        Task<bool?> ShowMessageBoxAsync(string message, string title, MsgBoxLevel icon = MsgBoxLevel.Information);
        //显示一个托盘通知
        void ShowTrayNotification(string message, string title, MsgBoxLevel icon = MsgBoxLevel.Information);
        //显示文件选择窗口，非阻塞
        Task<string?> OpenFileDialogAsync(IEnumerable<FileDialogFilter>? filters = null);
        //显示文件夹选择窗口，非阻塞
        Task<string?> OpenFolderDialogAsync();
        //更改窗口主题，返回bool是否即时生效
        Task<bool> ChangeThemeAsync(string theme);
        //退出
        void Exit();
    }
}