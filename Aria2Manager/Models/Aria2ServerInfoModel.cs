using System;
using System.Net;
using System.Xml;

namespace Aria2Manager.Models
{
    public class Aria2ServerInfoModel
    {
        public string? ServerName { get; set; } //服务器名
        public string? ServerAddress { get; set; } //服务器地址
        public string? ServerPort { get; set; } //端口
        public string? ServerSecret { get; set; } //密钥
        public bool IsHttps { get; set; } //是否使用https
        public bool UseProxy { get; set; } //是否使用代理

        //更新服务器信息
        public void UpdateServerInfo()
        {
            if (ServerName == null)
            {
                return;
            }
            ReadFromFileByName(ServerName);
        }

        //通过服务器名称从文件读取服务器配置
        private void ReadFromFileByName(string server_name)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("Configurations\\Aria2Servers.xml");
            var server = doc.SelectSingleNode($"/Servers/ServerConfigs/{server_name}");
            if (server == null)
            {
                throw new Exception($"Config of [{server_name}] don't find");
            }
            ServerName = server_name;
            foreach (XmlNode node in server.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Address":
                        ServerAddress = node.InnerText;
                        break;
                    case "Port":
                        ServerPort = node.InnerText;
                        break;
                    case "Secret":
                        ServerSecret = node.InnerText;
                        break;
                    case "IsHttps":
                        IsHttps = Convert.ToBoolean(node.InnerText);
                        break;
                    case "UseProxy":
                        UseProxy = Convert.ToBoolean(node.InnerText);
                        break;
                    default:
                        break;
                }
            }
            if ((ServerAddress == null) || (ServerPort == null))
            {
                throw new Exception($"Fail to read config of [{server_name}]");
            }
        }

        //初始化
        public Aria2ServerInfoModel(bool read_file = false)
        {
            if (!read_file)
            {
                ServerName = "New";
                ServerAddress = "127.0.0.1";
                ServerPort = "6800";
                ServerSecret = "";
                IsHttps = false;
                UseProxy = false;
            }
            else
            {
                //从配置文件读取当前服务器
                XmlDocument doc = new XmlDocument();
                doc.Load("Configurations\\Aria2Servers.xml");
                var current = doc.SelectSingleNode($"/Servers/Current");
                if (current == null)
                {
                    throw new Exception("Fail to read config file");
                }
                if (current.InnerText == "")
                {
                    throw new Exception("No Avaliable Server");
                }
                ReadFromFileByName(current.InnerText);
            }
        }

        //通过参数初始化
        public Aria2ServerInfoModel(string server_name, string server_address, string server_port,
            string server_secret, bool is_https, bool use_proxy)
        {
            ServerName = server_name;
            ServerAddress = server_address;
            ServerPort = server_port;
            ServerSecret = server_secret;
            IsHttps = is_https;
            UseProxy = use_proxy;
        }

        //通过服务器名初始化
        public Aria2ServerInfoModel(string server_name)
        {
            ReadFromFileByName(server_name);
        }

        static public WebProxy? GetProxies()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("Configurations\\Aria2Servers.xml");
                var proxy = doc.SelectSingleNode("/Servers/Proxy");
                string ProxyAddress = "";
                string ProxyPort = "";
                string ProxyType = "";
                string ProxyUser = "";
                string ProxyPasswd = "";
                foreach (XmlNode node in proxy.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Address":
                            ProxyAddress = node.InnerText;
                            break;
                        case "Port":
                            ProxyPort = node.InnerText;
                            break;
                        case "Type":
                            ProxyType = node.InnerText;
                            break;
                        case "User":
                            ProxyUser = node.InnerText;
                            break;
                        case "Passwd":
                            ProxyPasswd = node.InnerText;
                            break;
                        default:
                            break;
                    }
                }
                var ProxySetting = new WebProxy
                {
                    Address = new Uri($"{ProxyType}://{ProxyAddress}:{ProxyPort}"),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        userName: ProxyUser,
                        password: ProxyPasswd)
                };
                return ProxySetting;
            }
            catch
            {
                return null;
            }
        }
    }
}
