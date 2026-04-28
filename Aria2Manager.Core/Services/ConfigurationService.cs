using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Aria2Manager.Core.Services
{
    public class ConfigurationService<T> where T : class, new()
    {
        private readonly string _filePath;
        private readonly XmlSerializer _serializer;
        public ConfigurationService(string fileName)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            _filePath = Path.Combine(dir, fileName);
            _serializer = new XmlSerializer(typeof(T));
        }
        // 加载配置
        public T Load()
        {
            if (!File.Exists(_filePath))
            {
                T newConfig = new T();
                Save(newConfig); // 不存在则创建默认并保存
                return newConfig;
            }
            try
            {
                var migratedConfig = ConfigMigrator<T>.LoadAndMigrate(_filePath);
                if (migratedConfig != null)
                {
                    Save(migratedConfig); // 把旧格式覆盖成新格式
                    return migratedConfig;
                }
                //migratedConfig为null说明无需迁移，正常读取
                using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
                {
                    return (_serializer.Deserialize(stream) as T)!;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Failed to load configuration of {typeof(T)}", ex, false);
                return new T(); // 加载配置失败返回默认值
            }
        }
        // 保存配置
        public void Save(T config)
        {
            try
            {
                using var writer = new StreamWriter(_filePath);
                _serializer.Serialize(writer, config);
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Failed to save configuration of {typeof(T)}", ex, false);
                System.Diagnostics.Debug.WriteLine($"保存配置失败: {ex.Message}");
            }
        }
    }
    public static class ConfigMigrator<T> where T : class, new()
    {
        public static T? LoadAndMigrate(string filePath)
        {
            if (!File.Exists(filePath)) { return new T(); }
            return typeof(T).Name switch
            {
                nameof(ServerSettings) => MigrateServerConfig(filePath) as T,
                _ => null // 其他类型无需迁移
            };
        }
        private static ServerSettings? MigrateServerConfig(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);
            var root = doc.Element("Servers");
            // 获取XML中的版本号
            int fileVersion = (int?)root?.Attribute("version") ?? 1;
            if (fileVersion == 1)
            {
                return MigrateServerConfigV1ToV2(doc);
            }
            else if (fileVersion == ServerSettings.Version)
            {
                return null;
            }
            else
            {
                return new ServerSettings();
            }
        }
        private static ServerSettings MigrateServerConfigV1ToV2(XDocument doc)
        {
            var settings = new ServerSettings();
            var root = doc.Element("Servers");
            //  迁移
            settings.Current = root?.Element("Current")?.Value ?? "Local";
            var proxyEl = root?.Element("Proxy");
            if (proxyEl != null)
            {
                string? proxyAddress = proxyEl.Element("Address")?.Value;
                string? proxyPort = proxyEl.Element("Port")?.Value;
                if (string.IsNullOrWhiteSpace(proxyPort) || string.IsNullOrWhiteSpace(proxyAddress))
                {
                    settings.Proxy.Type = ProxyType.None;
                }
                else
                {
                    switch (proxyEl.Element("Type")?.Value)
                    {
                        case "http":
                            settings.Proxy.Type = ProxyType.Http;
                            break;
                        case "socks4":
                            settings.Proxy.Type = ProxyType.Socks4;
                            break;
                        case "socks5":
                            settings.Proxy.Type = ProxyType.Socks5;
                            break;
                        default:
                            settings.Proxy.Type = ProxyType.None;
                            break;
                    }
                }
                settings.Proxy.Address = proxyAddress ?? "127.0.0.1";
                settings.Proxy.Port = int.TryParse(proxyPort, out int p) ? p : 10809;
                settings.Proxy.User = proxyEl.Element("User")?.Value ?? string.Empty;
                settings.Proxy.Passwd = proxyEl.Element("Passwd")?.Value ?? string.Empty;
            }
            var configsEl = root?.Element("ServerConfigs");
            if (configsEl != null)
            {
                settings.ServerConfigs.Clear(); // 清除默认项
                foreach (var serverNode in configsEl.Elements())
                {
                    var server = new Aria2Server
                    {
                        Name = serverNode.Name.LocalName,
                        Address = serverNode.Element("Address")?.Value ?? "127.0.0.1",
                        Port = int.TryParse(serverNode.Element("Port")?.Value, out int p) ? p : 6800,
                        Secret = serverNode.Element("Secret")?.Value ?? string.Empty,
                        IsHttps = (serverNode.Element("IsHttps")?.Value?.ToLower() == "true"),
                        IsLocalServer = (serverNode.Element("IsLocal")?.Value?.ToLower() == "true"),
                        UseProxy = (serverNode.Element("UseProxy")?.Value?.ToLower() == "true")
                    };
                    settings.ServerConfigs.Add(server);
                }
            }
            return settings;
        }
    }
}
