using System;
using System.Windows.Controls;
using System.Xml;

namespace Aria2Manager.Models
{
    public class Aria2ServerModel
    {
        public string? ServerName { get; set; }
        public string? ServerAddress { get; set; }
        public string? ServerPort { get; set; }
        public string? ServerSecret { get; set; }
        public bool IsHttps { get; set; }
        public bool UseProxy { get; set; }

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

        public Aria2ServerModel(bool read_file = false)
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

        public Aria2ServerModel(string server_name, string server_address, string server_port,
            string server_secret, bool is_https, bool use_proxy)
        {
            ServerName = server_name;
            ServerAddress = server_address;
            ServerPort = server_port;
            ServerSecret = server_secret;
            IsHttps = is_https;
            UseProxy = use_proxy;
        }

        public Aria2ServerModel(string server_name)
        {
            ReadFromFileByName(server_name);
        }
    }
}
