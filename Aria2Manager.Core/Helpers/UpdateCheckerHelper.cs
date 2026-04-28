using System.Text.Json;

namespace Aria2Manager.Core.Helpers
{
    public static class UpdateCheckerHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        static UpdateCheckerHelper()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Aria2Manager-Client");
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }
        //检查程序更新
        public static async Task<bool?> CheckProgramUpdate()
        {
            string latestTag = await GetGithubLatestTag("https://api.github.com/repos/Ftbom/Aria2Manager/releases");
            latestTag = latestTag.Replace("v", "");
            return CompareVersions(GlobalContext.AppVersion, latestTag);
        }
        //检查Aria2更新
        public static async Task<bool?> CheckAria2Update(string aria2Version)
        {
            string latestTag = await GetGithubLatestTag("https://api.github.com/repos/aria2/aria2/releases");
            latestTag = latestTag.Replace("release-", "");
            return CompareVersions(aria2Version, latestTag);
        }
        private static bool? CompareVersions(string current, string latest)
        {
            if (string.IsNullOrWhiteSpace(latest) || string.IsNullOrWhiteSpace(current))
            {
                return null;
            }
            if (Version.TryParse(current, out Version? currentVersion) &&
                Version.TryParse(latest, out Version? latestVersion))
            {
                return latestVersion > currentVersion;
            }
            return current != latest;
        }
        private static async Task<string> GetGithubLatestTag(string url)
        {
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);
                if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                {
                    // 获取第一个Release的标签名
                    return doc.RootElement[0].GetProperty("tag_name").GetString() ?? "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Failed to get GitHub latest tag of '{url}'", ex, false);
            }
            return string.Empty;
        }
    }
}
