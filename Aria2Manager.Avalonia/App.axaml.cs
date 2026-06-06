using Aria2Manager.Avalonia.Localization;
using Aria2Manager.Avalonia.Services;
using Aria2Manager.Core;
using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace Aria2Manager.Avalonia
{
    public partial class App : Application
    {
        private AvaloniaUIService _uiService;
        public App()
        {
            InitLogger();
            _uiService = new AvaloniaUIService();
            LanguageHelper.OnLanguageChanged += culture =>
            {
                AvaloniaLocalizer.Instance.ChangeCulture(culture);
            };
        }
        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Log.Information("=== Application Exiting ===");
            Log.CloseAndFlush();
        }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public override void OnFrameworkInitializationCompleted()
        {
            GlobalContext.Instance.InitializeAsync(_uiService);
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                //桌面环境
                desktop.Exit += OnExit;
                if (!GlobalContext.Instance.AppSettings.StartMin) //是否打开主窗口
                {
                    _uiService.ShowWindow(WindowType.MainWindow, new MainViewModel(_uiService));
                }
            }
            base.OnFrameworkInitializationCompleted();
        }
        private void InitLogger()
        {
            // 设定日志存储路径：程序根目录/Logs/log.txt
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "log.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // 允许记录 Debug 及以上级别的日志
                .WriteTo.File(logPath,
                    rollingInterval: RollingInterval.Day, // 每天创建一个新文件
                    retainedFileCountLimit: 7,            // 只保留最近7天的日志
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            Log.Information("=== Application Starting ===");
        }
        private async void Exit_Click(object? sender, EventArgs e)
        {
            await _uiService.Exit();
        }
        private void Show_Click(object? sender, EventArgs e)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var existingMainWindow = desktop.Windows.FirstOrDefault(w => w is Views.MainWindow);
                if (existingMainWindow != null)
                {
                    //窗口已存在
                    existingMainWindow.Show();
                    if (existingMainWindow.WindowState == WindowState.Minimized)
                    {
                        existingMainWindow.WindowState = WindowState.Normal;
                    }
                    existingMainWindow.Activate();
                    existingMainWindow.Topmost = true;
                    existingMainWindow.Topmost = false;
                }
                else
                {
                    //窗口不存在
                    _uiService.ShowWindow(WindowType.MainWindow, new MainViewModel(_uiService));
                }
            }
        }
    }
}