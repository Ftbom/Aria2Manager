using Aria2Manager.Avalonia.Localization;
using Aria2Manager.Avalonia.Themes;
using Aria2Manager.Avalonia.Views;
using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Models;
using Aria2Manager.Core.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aria2Manager.Avalonia.Services
{
    public class AvaloniaUIService : UIServiceBase
    {
        private readonly Dictionary<string, Window> _openWindows = new();
        public override string DefaultTheme => "Default";
        public override List<string> ThemeList { get; } = ThemePresetsData.GetDefaultPresets().Select(t => t.Name).ToList();
        protected override void ShowPhysicalWindow(string windowId, WindowType windowType, object? dataContext, string? ownerWindowId = null)
        {
            var window = CreateWindow(windowType, dataContext);
            var desktop = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            if (windowType == WindowType.MainWindow && desktop != null)
            {
                desktop.MainWindow = window;
            }
            window.Closed += (s, e) =>
            {
                _openWindows.Remove(windowId);
                if (desktop != null && desktop.MainWindow == window)
                {
                    desktop.MainWindow = null;
                }
            };
            _openWindows[windowId] = window;
            if (!string.IsNullOrWhiteSpace(ownerWindowId) && _openWindows.TryGetValue(ownerWindowId, out Window? ownerWindow))
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Show(ownerWindow);
            }
            else if (desktop?.MainWindow != null && desktop.MainWindow != window)
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Show(desktop.MainWindow);
            }
            else
            {
                window.Show();
            }
        }
        protected override async Task<bool?> ShowPhysicalDialogAsync(string windowId, WindowType windowType, object? dataContext)
        {
            var window = CreateWindow(windowType, dataContext);
            var desktop = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            window.Closed += (s, e) => _openWindows.Remove(windowId);
            _openWindows[windowId] = window;
            Window? owner = desktop?.MainWindow;
            if (owner != null && owner != window)
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                await window.ShowDialog(owner);
                return true;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.Show();
                return null;
            }
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
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.Shutdown();
            }
        }
        public override async Task<bool?> ShowMessageBoxAsync(string message, string title, MsgBoxLevel icon = MsgBoxLevel.Information)
        {
            var desktop = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var activeWindow = _openWindows.Values.FirstOrDefault(x => x.IsActive) ?? desktop?.MainWindow;
            bool dialogResult = false;
            var tcs = new TaskCompletionSource<bool?>();
            //动态构建原生的 Avalonia Window 作为对话框
            var dialogWindow = new Window
            {
                Title = title,
                Width = 400,
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                ShowInTaskbar = false,
                Padding = new Thickness(20)
            };
            var mainPanel = new StackPanel { Spacing = 20 };
            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap
            };
            mainPanel.Children.Add(textBlock);
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10
            };
            if (icon == MsgBoxLevel.Question)
            {
                var btnYes = new Button { Content = AvaloniaLocalizer.Instance.GetString("Yes"), Width = 80, HorizontalContentAlignment = HorizontalAlignment.Center };
                btnYes.Click += (s, e) => { dialogResult = true; dialogWindow.Close(); };
                var btnNo = new Button { Content = AvaloniaLocalizer.Instance.GetString("No"), Width = 80, HorizontalContentAlignment = HorizontalAlignment.Center };
                btnNo.Click += (s, e) => { dialogResult = false; dialogWindow.Close(); };
                buttonPanel.Children.Add(btnYes);
                buttonPanel.Children.Add(btnNo);
            }
            else
            {
                var btnOk = new Button { Content = AvaloniaLocalizer.Instance.GetString("OK"), Width = 80, HorizontalContentAlignment = HorizontalAlignment.Center };
                btnOk.Click += (s, e) => { dialogResult = true; dialogWindow.Close(); };
                buttonPanel.Children.Add(btnOk);
            }
            mainPanel.Children.Add(buttonPanel);
            dialogWindow.Content = mainPanel;
            dialogWindow.Closed += (s, e) => tcs.TrySetResult(dialogResult);
            if (activeWindow != null)
            {
                await dialogWindow.ShowDialog(activeWindow);
            }
            else
            {
                dialogWindow.Show();
            }
            return await tcs.Task;
        }
        public override async Task<string?> OpenFileDialogAsync(IEnumerable<FileDialogFilter>? filters = null)
        {
            var topLevel = GetTopLevel();
            if (topLevel == null) return null;
            var filePickerOptions = new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = filters?.Select(f => new FilePickerFileType(f.Name)
                {
                    Patterns = f.Extensions.Select(e => $"*.{e.Trim('.')}").ToList()
                }).ToList()
            };
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(filePickerOptions);
            return files?.FirstOrDefault()?.TryGetLocalPath();
        }
        public override async Task<string?> OpenFolderDialogAsync()
        {
            var topLevel = GetTopLevel();
            if (topLevel == null) return null;
            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                AllowMultiple = false
            });
            return folders?.FirstOrDefault()?.TryGetLocalPath();
        }
        public override void ShowTrayNotification(string message, string title = "Aria2Manager", MsgBoxLevel icon = MsgBoxLevel.Information)
        {
            //映射图标类型
            NotificationType notificationType = icon switch
            {
                MsgBoxLevel.Information => NotificationType.Information,
                MsgBoxLevel.Warning => NotificationType.Warning,
                MsgBoxLevel.Error => NotificationType.Error,
                _ => NotificationType.Information
            };
        }
        public override Task<bool> ChangeThemeAsync(string theme)
        {
            ThemeManager.ApplyPreset(theme);
            return Task.FromResult(false);
        }
        private Window CreateWindow(WindowType windowType, object? dataContext)
        {
            Window window = windowType switch
            {
                WindowType.MainWindow => new MainWindow(),
                //WindowType.SettingsWindow => new SettingsWindow(),
                //WindowType.AboutWindow => new AboutWindow(),
                //WindowType.Aria2ServersWindow => new Aria2ServersWindow(),
                //WindowType.Aria2NewTaskWindow => new Aria2NewTaskWindow(),
                //WindowType.Aria2OptionsWindow => new Aria2OptionsWindow(),
                //WindowType.Aria2TaskInfoWindow => new Aria2TaskInfoWindow(),
                //WindowType.Aria2StatusWindow => new Aria2StatusWindow(),
                _ => throw new ArgumentOutOfRangeException(nameof(windowType), $"未配置该窗口类型的实例化逻辑: {windowType}")
            };
            if (dataContext != null)
            {
                window.DataContext = dataContext;
            }
            return window;
        }
        private TopLevel? GetTopLevel()
        {
            var activeWindow = _openWindows.Values.FirstOrDefault(x => x.IsActive);
            var desktop = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var window = activeWindow ?? desktop?.MainWindow;

            return window != null ? TopLevel.GetTopLevel(window) : null;
        }
    }
}