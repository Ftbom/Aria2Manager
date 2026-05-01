using Aria2NET;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Aria2Manager.Core.Helpers
{
    public static class Aria2ProcessHelper
    {
        private static bool aria2Started = false;
        //解析aria2.conf获取RPC端口和密钥
        private static (int Port, string? Secret) GetRpcConfig(string configPath)
        {
            int port = 6800; //默认端口
            string? secret = null; //默认无密钥
            if (!File.Exists(configPath))
            {
                return (port, secret);
            }
            try
            {
                var lines = File.ReadAllLines(configPath);
                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                    {
                        continue;
                    }
                    //找到第一个=的位置进行拆分，防止值内部也含有=
                    int equalsIndex = trimmedLine.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        string key = trimmedLine.Substring(0, equalsIndex).Trim();
                        string value = trimmedLine.Substring(equalsIndex + 1).Trim();
                        if (key.Equals("rpc-listen-port", StringComparison.OrdinalIgnoreCase))
                        {
                            if (int.TryParse(value, out int parsedPort))
                            {
                                port = parsedPort;
                            }
                        }
                        else if (key.Equals("rpc-secret", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!String.IsNullOrWhiteSpace(value))
                            {
                                secret = value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warning("Failed to parse aria2.conf", ex);
            }
            return (port, secret);
        }
        //启动Aria2进程并返回PID
        public static void StartAria2Process()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string aria2Dir = Path.Combine(baseDir, "Aria2");
            string exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "aria2c.exe" : "aria2c";
            string? exePath = FindInPath(exeName);
            if (exePath == null)
            {
                exePath = Path.Combine(aria2Dir, exeName);
                if (!File.Exists(exePath))
                {
                    LogHelper.Error($"Aria2 executable not found in system PATH or local directory: {exePath}", new FileNotFoundException(), false);
                    return;
                }
            }
            string configPath = Path.Combine(aria2Dir, "aria2.conf");
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"--conf-path=\"{configPath}\"",
                    WorkingDirectory = aria2Dir,
                    //彻底隐藏窗口
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                };
                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    aria2Started = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to start aria2c process", ex, false);
            }
        }
        public static async Task KillAria2Process()
        {
            if (aria2Started)
            {
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string configPath = Path.Combine(baseDir, "Aria2", "aria2.conf");
                    var (port, secret) = GetRpcConfig(configPath);
                    string rpcUrl = $"http://localhost:{port}/jsonrpc";
                    await new Aria2NetClient(rpcUrl, secret).ForceShutdownAsync();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Failed to kill aria2c process", ex, false);
                }
                aria2Started = false;
            }
        }
        //在系统环境变量查找指定文件
        private static string? FindInPath(string fileName)
        {
            string? pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrWhiteSpace(pathEnv)) { return null; }
            string[] paths = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (string path in paths)
            {
                string cleanPath = path.Trim('\"', '\'');
                if (string.IsNullOrWhiteSpace(cleanPath)) { continue; }
                try
                {
                    string fullPath = Path.Combine(cleanPath, fileName);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
                catch { continue; }
            }
            return null;
        }
    }
}
