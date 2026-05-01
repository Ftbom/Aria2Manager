using Aria2Manager.Core;
using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;
using Aria2Manager.Core.ViewModels;
using Aria2Manager.WPF.Services;
using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls;
using Serilog;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Aria2Manager.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private WpfUIService _uiService;
        private TaskbarIcon? _taskBar = null;
        private Aria2ServerInfo _aria2Status => GlobalContext.Instance.Aria2Server.ServerInfo;
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
        public App()
        {
            InitLogger();
            _uiService = new WpfUIService();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("=== Application Exiting ===");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
        private async void Tray_ToolTipOpen(object sender, RoutedEventArgs e)
        {
            if (sender is TaskbarIcon icon && icon.TrayToolTip is DependencyObject tooltip)
            {
                var textBlock = _taskBar?.TrayToolTip.FindChild<TextBlock>("ToolTipTextBlock");
                if (textBlock != null)
                {
                    await GlobalContext.Instance.Aria2Server.GetAria2Status();
                    textBlock.Text = $"{GlobalContext.AppName}\n" +
                        $"{LanguageHelper.GetString("Current_Server")}:{_aria2Status.ServerName}\n" +
                        $"{LanguageHelper.GetString("Download_Speed")}:{FormatterHelper.BytesToString(_aria2Status.DownloadSpeed)}/s\n" +
                        $"{LanguageHelper.GetString("Upload_Speed")}:{FormatterHelper.BytesToString(_aria2Status.UploadSpeed)}/s\n" +
                        $"{LanguageHelper.GetString("Status_Active")}:{_aria2Status.NumActive}\n" +
                        $"{LanguageHelper.GetString("Status_Waiting")}:{_aria2Status.NumWaiting}\n" +
                        $"{LanguageHelper.GetString("Status_Stopped")}:{_aria2Status.NumStopped}";
                }
            }
        }
        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            await _uiService.Exit();
        }
        private void Show_Click(object sender, RoutedEventArgs e)
        {
            if (Current.Windows.Count > 0)
            {
                return;
            }
            _uiService.ShowWindow(WindowType.MainWindow, new MainViewModel(_uiService));
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _taskBar = (TaskbarIcon)Current.FindResource("AMNotifyIcon");
            _uiService.TaskBar = _taskBar;
            GlobalContext.Instance.InitializeAsync(_uiService);
            if (!GlobalContext.Instance.AppSettings.StartMin) //是否打开主窗口
            {
                _uiService.ShowWindow(WindowType.MainWindow, new MainViewModel(_uiService));
            }
        }
    }
}
