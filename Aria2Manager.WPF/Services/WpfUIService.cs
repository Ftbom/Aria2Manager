using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Models;
using Aria2Manager.Core.Services;
using Aria2Manager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ControlzEx.Theming;
using Hardcodet.Wpf.TaskbarNotification;

namespace Aria2Manager.WPF.Services
{
    public class WpfUIService : UIServiceBase
    {
        private readonly Dictionary<string, Window> _openWindows = new();
        public TaskbarIcon? TaskBar { get; set; }
        public override string DefaultTheme => "Light.Green";
        public override List<string> ThemeList { get; } = new List<string> {
                "Light.Green", "Dark.Green",
                "Light.Blue", "Dark.Blue",
                "Light.Red", "Dark.Red",
                "Light.Purple", "Dark.Purple",
                "Light.Orange", "Dark.Orange",
                "Light.Lime", "Dark.Lime",
                "Light.Emerald", "Dark.Emerald",
                "Light.Teal", "Dark.Teal",
                "Light.Cyan", "Dark.Cyan",
                "Light.Cobalt", "Dark.Cobalt",
                "Light.Indigo", "Dark.Indigo",
                "Light.Violet", "Dark.Violet",
                "Light.Pink", "Dark.Pink",
                "Light.Magenta", "Dark.Magenta",
                "Light.Crimson", "Dark.Crimson",
                "Light.Amber", "Dark.Amber",
                "Light.Yellow", "Dark.Yellow",
                "Light.Brown", "Dark.Brown",
                "Light.Olive", "Dark.Olive",
                "Light.Steel", "Dark.Steel",
                "Light.Mauve", "Dark.Mauve",
                "Light.Taupe", "Dark.Taupe",
                "Light.Sienna", "Dark.Sienna"
        };
        protected override void ShowPhysicalWindow(string windowId, WindowType windowType, object? dataContext, string? ownerWindowId = null)
        {
            var window = CreateWindow(windowType, dataContext);
            if (!string.IsNullOrWhiteSpace(ownerWindowId) && _openWindows.TryGetValue(ownerWindowId, out Window? ownerWindow))
            {
                window.Owner = ownerWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else if (Application.Current.MainWindow != null && Application.Current.MainWindow != window)
            {
                window.Owner = Application.Current.MainWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            window.Closed += (s, e) => _openWindows.Remove(windowId);
            _openWindows[windowId] = window;
            window.Show();
        }
        protected override Task<bool?> ShowPhysicalDialogAsync(string windowId, WindowType windowType, object? dataContext)
        {
            var window = CreateWindow(windowType, dataContext);
            window.Closed += (s, e) => _openWindows.Remove(windowId);
            _openWindows[windowId] = window;
            //如果已经有主窗口，将弹窗的所有者设为主窗口，使其居中显示
            if (Application.Current.MainWindow != null && Application.Current.MainWindow != window)
            {
                window.Owner = Application.Current.MainWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            bool? result = window.ShowDialog();
            return Task.FromResult(result);
        }
        public override void CloseWindow(string windowId)
        {
            if (_openWindows.TryGetValue(windowId, out Window? window))
            {
                window.Close();
            }
        }
        protected override void ExitApplication()
        {
            Application.Current.Shutdown();
        }
        public override Task<bool?> ShowMessageBoxAsync(string message, string title, MsgBoxLevel icon = MsgBoxLevel.Information)
        {
            MessageBoxImage wpfIcon = icon switch
            {
                MsgBoxLevel.Information => MessageBoxImage.Information,
                MsgBoxLevel.Warning => MessageBoxImage.Warning,
                MsgBoxLevel.Error => MessageBoxImage.Error,
                MsgBoxLevel.Question => MessageBoxImage.Question,
                _ => MessageBoxImage.None
            };
            MessageBoxButton wpfButton = icon == MsgBoxLevel.Question ? MessageBoxButton.YesNo : MessageBoxButton.OK;
            var result = MessageBox.Show(message, title, wpfButton, wpfIcon);
            if (icon == MsgBoxLevel.Question)
            {
                return Task.FromResult<bool?>(result == MessageBoxResult.Yes);
            }
            return Task.FromResult<bool?>(true);
        }
        public override Task<string?> OpenFileDialogAsync(IEnumerable<FileDialogFilter>? filters = null)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            if (filters != null && filters.Any())
            {
                var filterStrings = filters.Select(f =>
                {
                    string exts = string.Join(";", f.Extensions.Select(e => $"*.{e.Trim('.')}"));
                    return $"{f.Name} ({exts})|{exts}";
                });
                openFileDialog.Filter = string.Join("|", filterStrings);
            }
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                return Task.FromResult<string?>(openFileDialog.FileName);
            }
            return Task.FromResult<string?>(null);
        }
        public override Task<string?> OpenFolderDialogAsync()
        {
            var folderDialog = new Microsoft.Win32.OpenFolderDialog();
            bool? result = folderDialog.ShowDialog();
            if (result == true)
            {
                return Task.FromResult<string?>(folderDialog.FolderName);
            }
            return Task.FromResult<string?>(null);
        }
        public override void ShowTrayNotification(string message, string title = "Aria2Manager", MsgBoxLevel icon = MsgBoxLevel.Information)
        {
            BalloonIcon taskBarIcon = icon switch
            {
                MsgBoxLevel.Information => BalloonIcon.Info,
                MsgBoxLevel.Warning => BalloonIcon.Warning,
                MsgBoxLevel.Error => BalloonIcon.Error,
                MsgBoxLevel.Question => BalloonIcon.Info,
                _ => BalloonIcon.None
            };
            TaskBar?.ShowBalloonTip(title, message, taskBarIcon);
        }

        public override Task<bool> ChangeThemeAsync(string theme)
        {
            if (ThemeList.Contains(theme))
            {
                ThemeManager.Current.ChangeTheme(Application.Current, theme);
                return Task.FromResult(false);
            }
            return Task.FromResult(false);
        }
        private Window CreateWindow(WindowType windowType, object? dataContext)
        {
            Window window = windowType switch
            {
                WindowType.MainWindow => new MainWindow(),
                WindowType.SettingsWindow => new SettingsWindow(),
                WindowType.AboutWindow => new AboutWindow(),
                WindowType.Aria2ServersWindow => new Aria2ServersWindow(),
                WindowType.Aria2NewTaskWindow => new Aria2NewTaskWindow(),
                WindowType.Aria2OptionsWindow => new Aria2OptionsWindow(),
                WindowType.Aria2TaskInfoWindow => new Aria2TaskInfoWindow(),
                WindowType.Aria2StatusWindow => new Aria2StatusWindow(),
                _ => throw new ArgumentOutOfRangeException(nameof(windowType), $"未配置该窗口类型的实例化逻辑: {windowType}")
            };
            if (dataContext != null)
            {
                window.DataContext = dataContext;
            }
            return window;
        }
    }
}
