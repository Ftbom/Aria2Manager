using System.Net.Http;
using Aria2NET;

namespace Aria2Manager.Models
{
    //aria2 cient
    public class Aria2ClientModel
    {
        public Aria2NetClient Aria2Client { get; }

        public Aria2ClientModel(Aria2ServerInfoModel serverInfo)
        {
            Aria2Client = InitWithServerModel(serverInfo);
        }

        public Aria2ClientModel() : this(new Aria2ServerInfoModel())
        {
        }

        //使用Aria2ServerInfoModel初始化类
        private static Aria2NetClient InitWithServerModel(Aria2ServerInfoModel serverInfo)
        {
            string scheme = serverInfo.IsHttps ? "https" : "http";
            string jsonrpcUrl = $"{scheme}://{serverInfo.ServerAddress}:{serverInfo.ServerPort}/jsonrpc";
            
            //配置代理
            HttpClient client = CreateHttpClient(serverInfo.UseProxy);
            
            string? aria2Secret = string.IsNullOrEmpty(serverInfo.ServerSecret) ? null : serverInfo.ServerSecret;
            
            return new Aria2NetClient(aria2Url: jsonrpcUrl, secret: aria2Secret, httpClient: client);
        }

        private static HttpClient CreateHttpClient(bool useProxy)
        {
            if (!useProxy)
            {
                return new HttpClient();
            }

            var proxy = Aria2ServerInfoModel.GetProxies();
            if (proxy == null)
            {
                return new HttpClient();
            }

            var httpClientHandler = new HttpClientHandler { Proxy = proxy };
            return new HttpClient(httpClientHandler, true);
        }
    }
}
