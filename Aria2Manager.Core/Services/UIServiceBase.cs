using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;
using Aria2Manager.Core.Services.Interfaces;

namespace Aria2Manager.Core.Services
{
    public abstract class UIServiceBase : IUIService
    {
        public string ShowWindow(WindowType windowType, object? dataContext = null, string? ownerWindowId = null)
        {
            string windowId = Guid.NewGuid().ToString();
            InjectWindowIdIfAware(windowId, dataContext);
            ShowPhysicalWindow(windowId, windowType, dataContext, ownerWindowId);
            return windowId;
        }
        public async Task ShowDialogAsync(WindowType windowType, object? dataContext = null)
        {
            string windowId = Guid.NewGuid().ToString();
            InjectWindowIdIfAware(windowId, dataContext);
            await ShowPhysicalDialogAsync(windowId, windowType, dataContext);
        }
        public async Task Exit()
        {
            //执行清理操作
            GlobalContext.Instance.CancelAllTasks();
            if (GlobalContext.Instance.AppSettings.KillAria2)
            {
                await Aria2ProcessHelper.KillAria2Process();
            }
            //退出应用
            ExitApplication();
        }
        //注入ID
        private void InjectWindowIdIfAware(string windowId, object? dataContext)
        {
            if (dataContext is IWindowAware windowAware)
            {
                windowAware.WindowId = windowId;
            }
        }
        //非模态窗口
        protected abstract void ShowPhysicalWindow(string windowId, WindowType windowType, object? dataContext, string? ownerWindowId = null);
        //模态对话框
        protected abstract Task<bool?> ShowPhysicalDialogAsync(string windowId, WindowType windowType, object? dataContext);
        protected abstract void ExitApplication();
        public abstract string DefaultTheme { get; }
        public abstract List<string> ThemeList { get; }
        public abstract void CloseWindow(string windowId);
        public abstract Task<bool?> ShowMessageBoxAsync(string message, string title, MsgBoxLevel icon = MsgBoxLevel.Information);
        public abstract void ShowTrayNotification(string message, string title = "Aria2Manager", MsgBoxLevel icon = MsgBoxLevel.Information);
        public abstract Task<string?> OpenFileDialogAsync(IEnumerable<FileDialogFilter>? filters = null);
        public abstract Task<string?> OpenFolderDialogAsync();
        public abstract Task<bool> ChangeThemeAsync(string theme);
    }
}