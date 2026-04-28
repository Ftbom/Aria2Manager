using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;

namespace Aria2Manager.Core.Services
{
    public class BtTrackerService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string _localCachePath = "trackers.txt";
        private BtTrackers _trackerConfig;
        public BtTrackerService(BtTrackers config)
        {
            _trackerConfig = config;
        }
        //获取Trackers
        private async Task<List<string>> GetTrackers(string sourceUrl)
        {
            List<string> result = new List<string>();
            string responce = await _httpClient.GetStringAsync(sourceUrl);
            result = responce.Split('\n').ToList();
            result.RemoveAll(s => string.IsNullOrWhiteSpace(s)); //去除空字符串
            return result;
        }
        public async Task<List<string>?> CheckTrackersUpdate()
        {
            string source = _trackerConfig.SelectedSource;
            if (string.IsNullOrWhiteSpace(source) || !BtTrackers.Sources.ContainsKey(source)) { return null; }
            //当前时间
            double nowDays = (DateTime.Now - new DateTime(2001, 1, 1)).TotalDays;
            if (!File.Exists(_localCachePath) || (nowDays - _trackerConfig.LastUpdateDay) >= _trackerConfig.UpdateInterval)
            {
                try
                {
                    List<string> trackers = await GetTrackers(BtTrackers.Sources[source]);
                    File.WriteAllLines(_localCachePath, trackers);
                    //更新配置文件
                    _trackerConfig.LastUpdateDay = nowDays;
                    GlobalContext.Instance.AppSettings.Trackers.LastUpdate = nowDays;
                    GlobalContext.Instance.SaveSettings();
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"Failed to update bt trackers of {source}", ex, false);
                }
            }
            if (!File.Exists(_localCachePath))
            {
                return null; //trackers缓存文件不存在
            }
            return File.ReadAllLines(_localCachePath).ToList();
        }
    }    
}
