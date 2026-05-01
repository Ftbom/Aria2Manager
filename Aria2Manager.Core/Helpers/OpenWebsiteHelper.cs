using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Services.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Aria2Manager.Core.Helpers
{
    public static class OpenWebsiteHelper
    {
        public static async Task Open(string url, IUIService uiService)
        {
            if (string.IsNullOrWhiteSpace(url)) { return; }
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start(new ProcessStartInfo("xdg-open", url) { RedirectStandardOutput = true, RedirectStandardError = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start(new ProcessStartInfo("open", url) { RedirectStandardOutput = true, RedirectStandardError = true });
                }
            }
            catch
            {
                await uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Open_Website_Failed"), "Error", MsgBoxLevel.Error);
            }
        }
    }
}
