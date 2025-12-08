using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Aria2Manager.Utils
{
    public class ConfigFile
    {
        private int _Aria2Servers_version = 1;
        private string _Aria2Servers_path = "Configurations/Aria2Servers.xml";
        private string _Aria2Servers_config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Servers version=""1"">
	<Proxy>
		<Type>http</Type>
		<Address></Address>
		<Port></Port>
		<User></User>
		<Passwd></Passwd>
	</Proxy>
	<Current>Local</Current>
	<Avaliable>Local</Avaliable>
	<ServerConfigs>
		<Local>
			<Address>127.0.0.1</Address>
			<Port>6800</Port>
			<Secret></Secret>
			<IsHttps>False</IsHttps>
			<UseProxy>False</UseProxy>
			<IsLocal>True</IsLocal>
		</Local>
	</ServerConfigs>
</Servers>";

        private int _Settings_version = 1;
        private string _Settings_path = "Configurations/Settings.xml";
        private string _Settings_config = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Settings version=""1"">
	<Language>zh-CN</Language>
	<Theme>Light.Green</Theme>
	<StartMin>False</StartMin>
	<CloseToExit>False</CloseToExit>
	<CheckUpdate>True</CheckUpdate>
	<StartAria2>True</StartAria2>
	<KillAria2>True</KillAria2>
	<CheckAria2Update>False</CheckAria2Update>
	<EnableAria2Notification>True</EnableAria2Notification>
	<UpdateTrackers>
		<Enable>False</Enable>
		<UpdateInterval>7</UpdateInterval>
		<LastUpdate>0</LastUpdate>
		<TrackersSource>trackerslist</TrackersSource>
	</UpdateTrackers>
</Settings>";

        public void Init()
        {
            //初始化配置文件
            Directory.CreateDirectory(Path.GetDirectoryName(_Aria2Servers_path)??".");
            if (!File.Exists(_Aria2Servers_path))
            {
                File.WriteAllText(_Aria2Servers_path, _Aria2Servers_config);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(_Aria2Servers_path);
                int version = int.Parse(doc.DocumentElement.GetAttribute("version"));
                if (version < _Aria2Servers_version)
                {
                    //配置迁移
                }
            }
            if (!File.Exists(_Settings_path))
            {
                File.WriteAllText(_Settings_path, _Settings_config);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(_Settings_path);
                int version = int.Parse(doc.DocumentElement.GetAttribute("version"));
                if (version < _Settings_version)
                {
                    //配置迁移
                }
            }
        }
    }

    public class Tools
    {
        private const long GB_SIZE = 1024L * 1024 * 1024;
        private const long MB_SIZE = 1024L * 1024;
        private const long KB_SIZE = 1024L;
        private const int HOURS_IN_SECONDS = 3600;
        private const int MINUTES_IN_SECONDS = 60;

        //Byte转可读字符串
        public static string BytesToString(long byteCount)
        {
            if (byteCount >= GB_SIZE)
            {
                return $"{(double)byteCount / GB_SIZE:F2}GB";
            }
            if (byteCount >= MB_SIZE)
            {
                return $"{(double)byteCount / MB_SIZE:F2}MB";
            }
            return $"{(double)byteCount / KB_SIZE:F2}KB";
        }

        //秒（时间）转可读字符串
        public static string SecondsToString(long secCount)
        {
            int hours = (int)(secCount / HOURS_IN_SECONDS);
            secCount -= hours * HOURS_IN_SECONDS;
            int minutes = (int)(secCount / MINUTES_IN_SECONDS);
            int seconds = (int)(secCount - minutes * MINUTES_IN_SECONDS);
            return $"{hours}h{minutes}m{seconds}s";
        }

        public static async Task<bool> CheckProgramUpdate()
        {
            string latestVersion = await GetLatestReleaseTag("https://api.github.com/repos/Ftbom/Aria2Manager/releases");
            if (string.IsNullOrEmpty(latestVersion))
            {
                return false;
            }
            
            string? localVersion = "v" + Assembly.GetExecutingAssembly().GetName().Version?.ToString()[..^2];
            return localVersion != null && latestVersion != localVersion;
        }

        public static async Task<string> GetLatestReleaseTag(string releasesUrl)
        {
            using var client = new HttpClient();
            // GitHub API 要求 User-Agent，否则可能返回 403
            client.DefaultRequestHeaders.Add("User-Agent", "C# HttpClient");
            try
            {
                HttpResponseMessage response = await client.GetAsync(releasesUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return "";
                }

                string jsonContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonContent);
                JsonElement releases = doc.RootElement;
                return releases.GetArrayLength() > 0
                    ? releases[0].GetProperty("tag_name").GetString() ?? ""
                    : "";
            }
            catch
            {
                return "";
            }
        }
    }
}
