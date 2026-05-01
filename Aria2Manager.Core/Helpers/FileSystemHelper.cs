using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Aria2Manager.Core.Helpers
{
    public static class FileSystemHelper
    {
        //删除文件或文件夹
        public static bool Delete(string path, bool isFolder = false)
        {
            if (string.IsNullOrWhiteSpace(path)) { return false; }
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
                return true;
            }
            catch
            {
                return false;
            }
        }
        //获取系统默认下载目录
        public static string GetDefaultDownloadDirectory()
        {
            string downloadPath = string.Empty;
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    downloadPath = Path.Combine(userProfile, "Downloads");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string configPath = Path.Combine(userProfile, ".config", "user-dirs.dirs");
                    if (File.Exists(configPath))
                    {
                        string[] lines = File.ReadAllLines(configPath);
                        string? downloadLine = lines.FirstOrDefault(l => l.StartsWith("XDG_DOWNLOAD_DIR="));
                        if (!string.IsNullOrEmpty(downloadLine))
                        {
                            string dir = downloadLine.Split('=')[1].Trim('\"');
                            downloadPath = dir.Replace("$HOME", userProfile);
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(downloadPath))
                {
                    downloadPath = Path.Combine(userProfile, "Downloads");
                }
            }
            catch
            {
                downloadPath = Path.Combine(userProfile, "Downloads");
            }
            return downloadPath.Replace('\\', '/');
        }
        //在系统资源管理器中打开路径并定位
        public static void ShowInExplorer(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { return; }
            path = Path.GetFullPath(path);
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start("explorer.exe", $"/select,\"{path}\"");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", $"-R \"{path}\"");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string directoryToOpen = path;
                    if (File.Exists(path))
                    {
                        directoryToOpen = Path.GetDirectoryName(path) ?? path;
                    }
                    Process.Start(new ProcessStartInfo("xdg-open", $"\"{directoryToOpen}\"") { RedirectStandardOutput = true, RedirectStandardError = true });
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to open path in explorer", ex);
            }
        }
    }
}