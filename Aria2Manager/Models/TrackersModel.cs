using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Aria2Manager.Models
{
    public class TrackersModel
    {
        public Dictionary<string, string> TrackersSources { get; set; }

        public TrackersModel()
        {
            //Trackers来源
            TrackersSources = new Dictionary<string, string>
            {
                {"trackerslist", "https://cdn.jsdelivr.net/gh/ngosang/trackerslist@master/trackers_all_ip.txt"},
                {"animeTrackerList", "https://cdn.jsdelivr.net/gh/DeSireFire/animeTrackerList/ATline_all_ip.txt"}
            };
        }

        //获取Trackers
        public List<string> GetTrackers(string? source)
        {
            List<string> result = new List<string>();
            if (source == null)
            {
                return result;
            }
            if (TrackersSources.ContainsKey(source))
            {
                HttpClient client = new HttpClient();
                string responce = client.GetStringAsync(TrackersSources[source]).Result;
                result = responce.Split('\n').ToList();
            }
            result.RemoveAll(s => string.IsNullOrWhiteSpace(s)); //去除空字符串
            return result;
        }
    }
}
