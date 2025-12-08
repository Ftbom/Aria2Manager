using System;
using System.Net;
using System.Xml;

namespace Aria2Manager.Models
{
    //aria2服务器信息
    public class Aria2ServerInfoModel
    {
        private const string CONFIG_PATH = "Configurations\\Aria2Servers.xml";

        public string? ServerName { get; set; } //服务器名
        public string? ServerAddress { get; set; } //服务器地址
        public string? ServerPort { get; set; } //端口
        public string? ServerSecret { get; set; } //密钥
        public bool IsLocal { get; set; } //是否本地服务器
        public bool IsHttps { get; set; } //是否使用https
        public bool UseProxy { get; set; } //是否使用代理

        //更新服务器信息
        public void UpdateServerInfo()
        {
            if (ServerName != null)
            {
                ReadFromFileByName(ServerName);
            }
        }

        //通过服务器名称从文件读取服务器配置
        private void ReadFromFileByName(string serverName)
        {
            var doc = new XmlDocument();
            doc.Load(CONFIG_PATH);
            var server = doc.SelectSingleNode($"/Servers/ServerConfigs/{serverName}") 
                ?? throw new Exception($"Config of [{serverName}] don't find");

            ServerName = serverName;
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
                    case "IsLocal":
                        IsLocal = Convert.ToBoolean(node.InnerText);
                        break;
                }
            }
            
            if (string.IsNullOrEmpty(ServerAddress) || string.IsNullOrEmpty(ServerPort))
            {
                throw new Exception($"Fail to read config of [{serverName}]");
            }
        }

        //初始化
        public Aria2ServerInfoModel(bool readFile = false)
        {
            if (!readFile)
            {
                //默认配置
                ServerName = "New";
                ServerAddress = "127.0.0.1";
                ServerPort = "6800";
                ServerSecret = "";
                IsHttps = false;
                UseProxy = false;
                IsLocal = true;
            }
            else
            {
                //从配置文件读取当前服务器
                var doc = new XmlDocument();
                doc.Load(CONFIG_PATH);
                var current = doc.SelectSingleNode("/Servers/Current") 
                    ?? throw new Exception("Fail to read config file");
                
                if (string.IsNullOrEmpty(current.InnerText))
                {
                    throw new Exception("No Avaliable Server");
                }
                ReadFromFileByName(current.InnerText);
            }
        }

        //通过参数初始化
        public Aria2ServerInfoModel(string serverName, string serverAddress, string serverPort,
            string serverSecret, bool isHttps, bool useProxy, bool isLocal)
        {
            ServerName = serverName;
            ServerAddress = serverAddress;
            ServerPort = serverPort;
            ServerSecret = serverSecret;
            IsHttps = isHttps;
            UseProxy = useProxy;
            IsLocal = isLocal;
        }

        //通过服务器名初始化
        public Aria2ServerInfoModel(string serverName)
        {
            ReadFromFileByName(serverName);
        }

        public static WebProxy? GetProxies()
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(CONFIG_PATH);
                var proxy = doc.SelectSingleNode("/Servers/Proxy");
                if (proxy == null)
                {
                    return null;
                }

                string proxyAddress = "";
                string proxyPort = "";
                string proxyType = "";
                string proxyUser = "";
                string proxyPasswd = "";

                foreach (XmlNode node in proxy.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Address":
                            proxyAddress = node.InnerText;
                            break;
                        case "Port":
                            proxyPort = node.InnerText;
                            break;
                        case "Type":
                            proxyType = node.InnerText;
                            break;
                        case "User":
                            proxyUser = node.InnerText;
                            break;
                        case "Passwd":
                            proxyPasswd = node.InnerText;
                            break;
                    }
                }

                return new WebProxy
                {
                    Address = new Uri($"{proxyType}://{proxyAddress}:{proxyPort}"),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(userName: proxyUser, password: proxyPasswd)
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
