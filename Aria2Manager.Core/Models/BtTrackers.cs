namespace Aria2Manager.Core.Models
{
    //BT trackers配置
    public class BtTrackers
    {
        public static Dictionary<string, string> Sources { get; private set; } = new Dictionary<string, string> //Trackers来源
        {
            {"trackerslist", "https://cdn.jsdelivr.net/gh/ngosang/trackerslist@master/trackers_all_ip.txt"},
            {"animeTrackerList", "https://cdn.jsdelivr.net/gh/DeSireFire/animeTrackerList/ATline_all_ip.txt"}
        };
        public string SelectedSource { get; set; } = "trackerslist";
        public bool EnableUpdate { get; set; } = false;
        public double LastUpdateDay { get; set; } = double.MinValue;
        public int UpdateInterval { get; set; } = int.MaxValue; //更新间隔，天
        public BtTrackers(TrackerConfig config)
        {
            SelectedSource = config.TrackersSource;
            EnableUpdate = config.EnableUpdate;
            LastUpdateDay = config.LastUpdate;
            UpdateInterval = config.UpdateInterval;
        }
    }
}
