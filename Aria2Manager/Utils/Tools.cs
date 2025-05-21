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
        //Byte转可读字符串
        static public string BytesToString(long byteCount)
        {
            long GBSize = 1024 * 1024 * 1024;
            long MBSize = 1024 * 1024;
            long KBSize = 1024;
            if (byteCount >= GBSize)
            {
                return ((double)(byteCount * 100 / GBSize) / 100).ToString() + "GB";
            }
            else if (byteCount >= MBSize)
            {
                return ((double)(byteCount * 100 / MBSize) / 100).ToString() + "MB";
            }
            else
            {
                return ((double)(byteCount * 100 / KBSize) / 100).ToString() + "KB";
            }
        }

        //秒（时间）转可读字符串
        static public string SecondsToString(long secCount)
        {
            string result = "";
            long HSize = 60 * 60;
            long MSize = 60;
            int h_num = (int)(secCount / HSize);
            result += h_num.ToString() + "h";
            secCount -= h_num * HSize;
            int m_num = (int)(secCount / MSize);
            result += m_num.ToString() + "m";
            secCount -= m_num * MSize;
            result += secCount.ToString() + "s";
            return result;
        }

        static public async Task<bool> CheckProgramUpdate()
        {
            string LatestVersion = await GetlatestReleaseTag("https://api.github.com/repos/Ftbom/Aria2Manager/releases"); //获取Aria2最新版本号
            if (LatestVersion == "")
            {
                return false;
            }
            string? LocalVersion = "v" + Assembly.GetExecutingAssembly().GetName().Version?.ToString()[..^2];
            if ((LocalVersion != null) && (LatestVersion != LocalVersion))
            {
                return true;
            }
            return false;
        }

        static public async Task<string> GetlatestReleaseTag(string releasesUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                // GitHub API 要求 User-Agent，否则可能返回 403
                client.DefaultRequestHeaders.Add("User-Agent", "C# HttpClient");
                try
                {
                    HttpResponseMessage response = await client.GetAsync(releasesUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonContent = await response.Content.ReadAsStringAsync();
                        using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                        {
                            JsonElement releases = doc.RootElement;
                            if (releases.GetArrayLength() > 0)
                            {
                                return releases[0].GetProperty("tag_name").GetString()??"";
                            }
                            else
                            {
                                return "";
                            }
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
                catch
                {
                    return "";
                }
            }
        }
    }
}
