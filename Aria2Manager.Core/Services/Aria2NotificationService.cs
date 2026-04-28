using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;
using Aria2Manager.Core.Services.Interfaces;
using Aria2NET;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.WebSockets;
using Websocket.Client;

namespace Aria2Manager.Core.Services
{
    public class Aria2NotificationService
    {
        private readonly IUIService _uiService;
        private WebsocketClient? _aria2WebsocketClient = null;
        private IDisposable? _eventSubscription = null;
        private Func<string, Task<DownloadStatusResult>> _statusFunc;
        public Aria2NotificationService(Aria2ServerInfo server, IUIService uiService, Func<string, Task<DownloadStatusResult>> statusFunc)
        {
            _uiService = uiService;
            _statusFunc = statusFunc;
            ChangeWebsocketClient(server);
        }
        private WebProxy? GetProxy(ProxyConfig proxyConfig, bool useProxy)
        {
            string proxyString = $"{proxyConfig.Type.ToString().ToLower()}://{proxyConfig.Address}:{proxyConfig.Port.ToString()}";
            WebProxy? proxy = null;
            if (useProxy && (proxyConfig.Type != ProxyType.None))
            {
                try
                {
                    proxy = new WebProxy(new Uri(proxyString))
                    {
                        BypassProxyOnLocal = false,
                        UseDefaultCredentials = false
                    };
                    if (!string.IsNullOrWhiteSpace(proxyConfig.User))
                    {
                        proxy.Credentials = new NetworkCredential(proxyConfig.User, proxyConfig.Passwd);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Warning("Failed to create aria2client proxy object", ex);
                    proxy = null;
                }
            }
            return proxy;
        }
        public async void ChangeWebsocketClient(Aria2ServerInfo server)
        {
            if (_aria2WebsocketClient != null)
            {
                await _aria2WebsocketClient!.Stop(WebSocketCloseStatus.NormalClosure, "Switching Aria2 Server");
                _aria2WebsocketClient?.Dispose();
                _eventSubscription?.Dispose();
            }
            string wsScheme = server.IsHttps ? "wss" : "ws";
            var wsUri = new Uri($"{wsScheme}://{server.ServerAddress}:{server.ServerPort}/jsonrpc");
            var factory = new Func<ClientWebSocket>(() =>
            {
                var client = new ClientWebSocket();
                if (server.UseProxy)
                {
                    client.Options.Proxy = GetProxy(GlobalContext.Instance.ServerSettings.Proxy.Clone(), server.UseProxy);
                }
                return client;
            });
            _aria2WebsocketClient = new WebsocketClient(wsUri, factory);
            // 重新启动并重新订阅事件
            _eventSubscription = _aria2WebsocketClient!.MessageReceived.Subscribe(HandleAria2Message);
            await _aria2WebsocketClient!.Start();
        }
        private async void HandleAria2Message(ResponseMessage msg)
        {
            if (string.IsNullOrWhiteSpace(msg.Text)) return;
            try
            {
                var json = JObject.Parse(msg.Text);
                var method = json["method"]?.ToString();
                var gid = json["params"]?[0]?["gid"]?.ToString();
                //过滤无关消息
                if (string.IsNullOrWhiteSpace(method) || string.IsNullOrWhiteSpace(gid)) return;
                await ProcessAria2NotificationAsync(method, gid);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Parse Aria2 WebSocket Message Error", ex, false);
            }
        }
        private async Task<string> GetTaskName(string gid)
        {
            try
            {
                await Task.Delay(500); //等待一段时间
                DownloadStatusResult status = await _statusFunc(gid);
                if ((status.Bittorrent == null) || (status.Bittorrent.Info == null))
                {
                    return Path.GetFileName(status.Files[0].Path);
                }
                else
                {
                    return status.Bittorrent.Info.Name;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to get task name for Aria2 notification", ex, false);
                return gid;
            }
        }
        private async Task ProcessAria2NotificationAsync(string method, string gid)
        {
            string taskName = await GetTaskName(gid);
            var result = method switch
            {
                "aria2.onDownloadStart" => new { Key = "Download_Start", Level = MsgBoxLevel.Information },
                "aria2.onDownloadPause" => new { Key = "Download_Pause", Level = MsgBoxLevel.None },
                "aria2.onDownloadStop" => new { Key = "Download_Stop", Level = MsgBoxLevel.None },
                "aria2.onDownloadComplete" => new { Key = "Download_Complete", Level = MsgBoxLevel.Information },
                "aria2.onDownloadError" => new { Key = "Download_Error", Level = MsgBoxLevel.Error },
                "aria2.onBtDownloadComplete" => new { Key = "BtDownload_Complete", Level = MsgBoxLevel.Information },
                _ => null
            };
            if (result != null)
            {
                _uiService.ShowTrayNotification(taskName,LanguageHelper.GetString(result.Key),result.Level);
            }
        }
    }
}
