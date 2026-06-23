using System.Text.Json;
using Aria2Manager.Core.Helpers;

namespace Aria2Manager.Core.Services
{
    public class UpdateCheckerService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly Dictionary<string, string> _githubTagCache = new Dictionary<string, string>();
        public UpdateCheckerService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Aria2Manager-Client");
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }
        //检查程序更新
        public async Task<bool?> CheckProgramUpdate(string appVersion, string? prefix)
        {
            string latestTag = await GetGithubLatestTag("https://api.github.com/repos/Ftbom/Aria2Manager/releases", prefix);
            latestTag = latestTag.Replace($"{prefix}-v", "");
            return CompareVersions(appVersion, latestTag);
        }
        //检查Aria2更新
        public async Task<bool?> CheckAria2Update(string aria2Version)
        {
            string latestTag = await GetGithubLatestTag("https://api.github.com/repos/aria2/aria2/releases", useCache: true);
            latestTag = latestTag.Replace("release-", "");
            return CompareVersions(aria2Version, latestTag);
        }
        private bool? CompareVersions(string current, string latest)
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
        private async Task<string> GetGithubLatestTag(string url, string? prefix = null, bool useCache = false)
        {
            if (useCache)
            {
                if (_githubTagCache.TryGetValue(url, out string? cachedTag) && cachedTag != null)
                {
                    return cachedTag;
                }
            }
            string tagName = string.Empty;
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);
                if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                {
                    foreach (var release in doc.RootElement.EnumerateArray())
                    {
                        tagName = release.GetProperty("tag_name").GetString() ?? "";
                        if (prefix == null)
                        {
                            //获取第一个Release的标签名
                            break;
                        }
                        else
                        {
                            //获取指定前缀的第一个Release的标签名
                            if (tagName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Failed to get GitHub latest tag of '{url}'", ex, false);
            }
            if (useCache)
            {
                _githubTagCache[url] = tagName;
            }
            return tagName;
        }
    }
}
