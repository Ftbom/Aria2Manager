using System.Diagnostics;

namespace Aria2Manager.Core.Helpers
{
    public static class Aria2ProcessHelper
    {
        private static int? pid = null;
        //启动Aria2进程并返回PID
        public static void StartAria2Process()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string aria2Dir = Path.Combine(baseDir, "Aria2");
            string exePath = Path.Combine(aria2Dir, "aria2c.exe");
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
                    pid = process.Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to start aria2c process", ex, false);
            }
        }
        public static void KillAria2Process()
        {
            if (pid != null)
            {
                try
                {
                    var process = Process.GetProcessById(pid.Value);
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Failed to kill aria2c process", ex, false);
                }
            }
        }
    }
}
