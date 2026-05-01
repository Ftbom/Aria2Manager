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
        private readonly HashSet<string> _notifiedStartGids = new HashSet<string>(); //缓存gid，避免重复通知下载开始事件
        private readonly object _gidLock = new object(); //锁对象，保护gid缓存的线程安全
        public Aria2NotificationService(IUIService uiService, Func<string, Task<DownloadStatusResult>> statusFunc)
        {
            _uiService = uiService;
            _statusFunc = statusFunc;
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
        //停止监听
        public async Task StopListeningAsync()
        {
            if (_eventSubscription != null)
            {
                _eventSubscription.Dispose();
                _eventSubscription = null;
            }
            if (_aria2WebsocketClient != null)
            {
                try
                {
                    await _aria2WebsocketClient.Stop(WebSocketCloseStatus.NormalClosure, "Client stopped listening");
                }
                catch (Exception ex)
                {
                    LogHelper.Warning("Error occurred while stopping websocket client", ex);
                }
                finally
                {
                    _aria2WebsocketClient.Dispose();
                    _aria2WebsocketClient = null;
                }
            }
        }
        //启动或切换监听
        public async Task StartListeningAsync(Aria2Server server)
        {
            await StopListeningAsync();
            string wsScheme = server.IsHttps ? "wss" : "ws";
            var wsUri = new Uri($"{wsScheme}://{server.Address}:{server.Port}/jsonrpc");
            var factory = new Func<ClientWebSocket>(() =>
            {
                var client = new ClientWebSocket();
                if (server.UseProxy)
                {
                    client.Options.Proxy = GetProxy(GlobalContext.Instance.ServerSettings.Proxy.DeepClone(), server.UseProxy);
                }
                return client;
            });
            _aria2WebsocketClient = new WebsocketClient(wsUri, factory);
            // 重新启动并重新订阅事件
            _eventSubscription = _aria2WebsocketClient!.MessageReceived.Subscribe(HandleAria2Message);
            await _aria2WebsocketClient.Start();
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
            if (method == "aria2.onDownloadStart")
            {
                lock (_gidLock)
                {
                    if (!_notifiedStartGids.Add(gid))
                    {
                        //跳过gid通知
                        return;
                    }
                }
            }
            else if (method == "aria2.onDownloadStop" ||
                     method == "aria2.onDownloadComplete" ||
                     method == "aria2.onDownloadError" ||
                     method == "aria2.onDownloadPause")
            {
                lock (_gidLock)
                {
                    _notifiedStartGids.Remove(gid);
                }
            }
            string taskName = string.Empty;
            for (int i = 0; i < 15; i++)
            {
                await Task.Delay(200);
                taskName = await GetTaskName(gid);
                if (!string.IsNullOrWhiteSpace(taskName))
                {
                    break;
                }
            }
            if (string.IsNullOrWhiteSpace(taskName))
            {
                taskName = gid;
            }
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
                _uiService.ShowTrayNotification(taskName, LanguageHelper.GetString(result.Key), result.Level);
            }
        }
    }
}
