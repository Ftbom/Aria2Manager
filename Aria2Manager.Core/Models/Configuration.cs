using System.Xml.Serialization;

namespace Aria2Manager.Core.Models
{
    // 程序设置
    [XmlRoot("Settings")]
    public class AppSettings
    {
        [XmlAttribute("version")]
        static public int Version { get; set; } = 1;
        public string Language { get; set; } = "zh-CN";
        public string Theme { get; set; } = string.Empty;
        public bool StartMin { get; set; } = false;
        public bool CloseToExit { get; set; } = true;
        public bool CheckUpdate { get; set; } = true;
        public bool StartAria2 { get; set; } = true;
        public bool KillAria2 { get; set; } = true;
        public bool CheckAria2Update { get; set; } = false;
        public bool EnableAria2Notification { get; set; } = true;
        [XmlElement("UpdateTrackers")]
        public TrackerConfig Trackers { get; set; } = new();
        public AppSettings Clone() => (AppSettings)this.MemberwiseClone();
    }
    public class TrackerConfig
    {
        [XmlElement("Enable")]
        public bool EnableUpdate { get; set; } = false;
        public int UpdateInterval { get; set; } = int.MaxValue;
        public double LastUpdate { get; set; } = double.MinValue;
        public string TrackersSource { get; set; } = "trackerslist";
    }
    // Aria2服务器设置
    [XmlRoot("Servers")]
    public class ServerSettings
    {
        [XmlAttribute("version")]
        static public int Version { get; set; } = 2;
        public ProxyConfig Proxy { get; set; } = new();
        public string Current { get; set; } = "Local";
        [XmlArray("ServerConfigs")]
        [XmlArrayItem("Server")]
        public List<Aria2Server> ServerConfigs { get; set; } = new List<Aria2Server> { new Aria2Server() };
    }
    public class Aria2Server
    {
        [XmlAttribute("Name")]
        public string Name { get; set; } = "Local";
        public string Address { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 6800;
        public string Secret { get; set; } = string.Empty;
        public bool IsHttps { get; set; } = false;
        public bool UseProxy { get; set; } = false;
        [XmlElement("IsLocal")]
        public bool IsLocalServer { get; set; } = false;
        public Aria2Server Clone() => (Aria2Server)this.MemberwiseClone();
    }
    public class ProxyConfig
    {
        public ProxyType Type { get; set; } = ProxyType.None;
        public string Address { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 10809;
        public string User { get; set; } = string.Empty;
        public string Passwd { get; set; } = string.Empty;
        public ProxyConfig Clone() => (ProxyConfig)this.MemberwiseClone();
    }
    public enum ProxyType
    {
        [XmlEnum("http")]
        Http,
        [XmlEnum("socks4")]
        Socks4,
        [XmlEnum("socks5")]
        Socks5,
        [XmlEnum("")] // 空值不启用
        None
    }
}
