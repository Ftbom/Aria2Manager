using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aria2NET;

namespace Aria2Manager.Models
{
    internal class Aria2ClientModel
    {
        private Aria2NetClient aria2_client;

        public Aria2NetClient Aria2Client
        {
            get => aria2_client;
            private set { }
        }

        //使用Aria2ServerModel初始化类
        private void InitWithServerModel(Aria2ServerModel server_info)
        {
            string scheme;
            if (server_info.IsHttps)
            {
                scheme = "https";
            }
            else
            {
                scheme = "http";
            }
            string jsonrpc_url = $"{scheme}://{server_info.ServerAddress}:{server_info.ServerPort}/jsonrpc";
            HttpClient client;
            if (server_info.UseProxy)
            {
                var Proxy = Aria2ServerModel.GetProxies();
                if (Proxy == null)
                {
                    client = new HttpClient();
                }
                else
                {
                    var httpClientHandler = new HttpClientHandler
                    {
                        Proxy = Proxy,
                    };
                    client = new HttpClient(httpClientHandler, true);
                }
            }
            else
            {
                client = new HttpClient();
            }
            string? Aria2Secret;
            if (server_info.ServerSecret == "")
            {
                Aria2Secret = null;
            }
            else
            {
                Aria2Secret = server_info.ServerSecret;
            }
            aria2_client = new Aria2NetClient(aria2Url: jsonrpc_url, secret: Aria2Secret, httpClient: client);
        }

        public Aria2ClientModel(Aria2ServerModel server_info)
        {
            InitWithServerModel(server_info);
        }

        public Aria2ClientModel()
        {
            InitWithServerModel(new Aria2ServerModel());
        }
    }
}
