using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace Aria2Manager.Core.Helpers
{
    public static class AutoStartHelper
    {
        private const string WindowsRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private static string? GetCurrentAppPath()
        {
            using var process = Process.GetCurrentProcess();
            return process.MainModule?.FileName;
        }
        private static bool IsAutoStartEnabled(string appName)
        {
            if (OperatingSystem.IsWindows())
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(WindowsRegistryPath, false);
                return key?.GetValue(appName) != null;
            }
            if (OperatingSystem.IsLinux())
            {
                string desktopFilePath = GetLinuxDesktopFilePath(appName);
                return File.Exists(desktopFilePath);
            }
            return false;
        }
        public static void SetAutoStart(string appName, bool enable)
        {
            string? appPath = GetCurrentAppPath();
            if (appPath == null) { return; }
            if (IsAutoStartEnabled(appName) == enable) { return; }
            if (OperatingSystem.IsWindows())
            {
                SetWindowsAutoStart(appName, appPath, enable);
            }
            else if (OperatingSystem.IsLinux())
            {
                SetLinuxAutoStart(appName, appPath, enable);
            }
        }
        [SupportedOSPlatform("windows")]
        private static void SetWindowsAutoStart(string appName, string appPath, bool enable)
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(WindowsRegistryPath, true)
                                    ?? throw new InvalidOperationException("Failed to get registry values");
            if (enable)
            {
                key.SetValue(appName, $"\"{appPath}\"");
            }   
            else
            {
                key.DeleteValue(appName, false);
            }   
        }
        [SupportedOSPlatform("linux")]
        private static void SetLinuxAutoStart(string appName, string appPath, bool enable)
        {
            //检查是否为图形化桌面环境
            bool isGui = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP")) ||
                         !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")) ||
                         !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));
            if (!isGui) { return; }
            string desktopFilePath = GetLinuxDesktopFilePath(appName);
            if (enable)
            {
                string configDir = Path.GetDirectoryName(desktopFilePath)!;
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                string content = $"""
                             [Desktop Entry]
                             Type=Application
                             Exec="{appPath}"
                             Hidden=false
                             NoDisplay=false
                             X-GNOME-Autostart-enabled=true
                             Name={appName}
                             Comment=Auto start {appName}
                             """;
                File.WriteAllText(desktopFilePath, content);
                File.SetUnixFileMode(desktopFilePath,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                    UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                    UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
            }
            else
            {
                if (File.Exists(desktopFilePath))
                {
                    File.Delete(desktopFilePath);
                }
            }
        }
        private static string GetLinuxDesktopFilePath(string appName)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config", "autostart", $"{appName}.desktop");
        }
    }
}
