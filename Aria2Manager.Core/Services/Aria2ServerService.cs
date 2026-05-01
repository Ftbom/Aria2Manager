using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;
using Aria2NET;
using System.Collections.Concurrent;
using System.Net;

namespace Aria2Manager.Core.Services
{
    public class Aria2ServerService
    {
        private static readonly ConcurrentDictionary<string, HttpClient> _clientCache = new(); //缓存HttpClient，避免耗尽系统端口
        private Aria2NetClient _aria2Client = null!;
        public Aria2ServerInfo ServerInfo { get; set; } = new Aria2ServerInfo(); //Aria2服务器信息
        public Aria2ServerService(Aria2Server server)
        {
            Update(server);
        }
        //更新Aria2客户端实例或代理配置
        public void Update(Aria2Server server)
        {
            ServerInfo.SyncFrom(server);
            string scheme = server.IsHttps ? "https" : "http";
            string jsonrpcUrl = $"{scheme}://{server.Address}:{server.Port}/jsonrpc";
            string? Aria2Secret = (server.Secret.Length == 0) ? null : server.Secret;
            _aria2Client = new Aria2NetClient(aria2Url: jsonrpcUrl, secret: Aria2Secret, httpClient: GetHttpClient(server.UseProxy));
        }
        private HttpClient GetHttpClient(bool useProxy)
        {
            ProxyConfig proxyConfig = GlobalContext.Instance.ServerSettings.Proxy.DeepClone();
            string proxyString = $"{proxyConfig.Type.ToString().ToLower()}://{proxyConfig.Address}:{proxyConfig.Port.ToString()}";
            return _clientCache.GetOrAdd(useProxy ? proxyString : "no_proxy", _ =>
            {
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
                return new HttpClient(new HttpClientHandler { Proxy = proxy, UseProxy = proxy != null }, true);
            });
        }
        public async Task<IList<DownloadStatusResult>> GetAria2Tasks(Aria2TaskStatus status)
        {
            try
            {
                IList<DownloadStatusResult> results;
                if (status == Aria2TaskStatus.all)
                {
                    results = await _aria2Client.TellAllAsync(GlobalContext.Instance.GlobalCancelToken);
                }
                else if (status == Aria2TaskStatus.active)
                {
                    results = await _aria2Client.TellActiveAsync(GlobalContext.Instance.GlobalCancelToken);
                }
                else if (status == Aria2TaskStatus.waiting)
                {
                    results = await _aria2Client.TellWaitingAsync(0, int.MaxValue, GlobalContext.Instance.GlobalCancelToken);
                }
                else
                {
                    results = await _aria2Client.TellStoppedAsync(0, int.MaxValue, GlobalContext.Instance.GlobalCancelToken);
                }
                ServerInfo.IsConnected = true;
                return results;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to get current tasks list", ex, false);
                ServerInfo.IsConnected = false;
                return new List<DownloadStatusResult>();
            }
        }
        public async Task GetAria2Status()
        {
            try
            {
                GlobalStatResult status = await _aria2Client.GetGlobalStatAsync(GlobalContext.Instance.GlobalCancelToken);
                ServerInfo.DownloadSpeed = (long)status.DownloadSpeed;
                ServerInfo.UploadSpeed = (long)status.UploadSpeed;
                ServerInfo.NumActive = status.NumActive;
                ServerInfo.NumWaiting = status.NumWaiting;
                ServerInfo.NumStopped = status.NumStopped;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to get aria2 status", ex, false);
            }
        }
        public async Task<IDictionary<string, string?>> GetAria2Options(IEnumerable<string> keys, string? gid = null)
        {
            try
            {
                IDictionary<string, string> allOptions;
                if (!String.IsNullOrWhiteSpace(gid))
                {
                    allOptions = await _aria2Client.GetOptionAsync(gid, GlobalContext.Instance.GlobalCancelToken);
                }
                else
                {
                    allOptions = await _aria2Client.GetGlobalOptionAsync(GlobalContext.Instance.GlobalCancelToken);
                }
                return keys.ToDictionary(
                    key => key,
                    key => allOptions.TryGetValue(key, out var value) ? value : null
                );
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to get aria2 options", ex);
                return new Dictionary<string, string?>();
            }
        }
        public async Task<List<string>> AddMetalinkTask(byte[] metalink, IDictionary<string, object> options)
        {
            try
            {
                return await _aria2Client.AddMetalinkAsync(metalink, options, cancellationToken: GlobalContext.Instance.GlobalCancelToken);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to add Metalink task", ex);
                return new List<string>();
            }
        }
        public async Task<string> AddTorrentTask(byte[] torrent, IDictionary<string, object> options)
        {
            try
            {
                return await _aria2Client.AddTorrentAsync(torrent, options: options, cancellationToken: GlobalContext.Instance.GlobalCancelToken);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to add Torrent task", ex);
                return String.Empty;
            }
        }
        public async Task<List<string>> AddUrlsTask(List<string> urls, IDictionary<string, object> options)
        {
            try
            {
                List<string> gidList = new List<string>();
                foreach (string url in urls)
                {
                    gidList.Add(await _aria2Client.AddUriAsync([url], options: options, cancellationToken: GlobalContext.Instance.GlobalCancelToken));
                }
                return gidList;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to add URL task", ex);
                return new List<string>();
            }
        }
        public async Task<DownloadStatusResult> GetTaskStatus(string gid)
        {
            try
            {
                return await _aria2Client.TellStatusAsync(gid, GlobalContext.Instance.GlobalCancelToken);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to get task status", ex);
                return new DownloadStatusResult { Gid = gid };
            }
        }
        public async Task ChangeAria2Options(IDictionary<string, string> options, string? gid = null)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(gid))
                {
                    await _aria2Client.ChangeOptionAsync(gid, options, GlobalContext.Instance.GlobalCancelToken);
                }
                else
                {
                    await _aria2Client.ChangeGlobalOptionAsync(options, GlobalContext.Instance.GlobalCancelToken);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to change aria2 options", ex);
            }
        }
        public async Task<VersionResult> GetAria2Version()
        {
            try
            {
                return await _aria2Client.GetVersionAsync(GlobalContext.Instance.GlobalCancelToken);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to get aria2 version", ex);
                return new VersionResult();
            }
        }
        public async Task<DownloadStatusResult> RemoveTask(string gid)
        {
            try
            {
                var task = await _aria2Client.TellStatusAsync(gid, GlobalContext.Instance.GlobalCancelToken);
                if ((task.Status == "complete") || (task.Status == "error") || (task.Status == "removed"))
                {
                    await _aria2Client.RemoveDownloadResultAsync(gid, GlobalContext.Instance.GlobalCancelToken);
                }
                else
                {
                    await _aria2Client.ForceRemoveAsync(gid, GlobalContext.Instance.GlobalCancelToken);
                }
                return task;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to remove aria2 task", ex);
                return new DownloadStatusResult { Gid = gid };
            }
        }
        public async Task UnpauseTask(string? gid = null)
        {
            try
            {
                if (gid == null)
                {
                    await _aria2Client.UnpauseAllAsync(GlobalContext.Instance.GlobalCancelToken);
                    return;
                }
                await _aria2Client.UnpauseAsync(gid, GlobalContext.Instance.GlobalCancelToken);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to unpause aria2 task", ex);
            }
        }
        public async Task PauseTask(string? gid = null)
        {
            try
            {
                if (gid == null)
                {
                    await _aria2Client.PauseAllAsync(GlobalContext.Instance.GlobalCancelToken);
                    return;
                }
                await _aria2Client.PauseAsync(gid, GlobalContext.Instance.GlobalCancelToken);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to pause aria2 task", ex);
            }
        }
        public async Task PurgeAllTask()
        {
            try
            {
                await _aria2Client.PurgeDownloadResultAsync(GlobalContext.Instance.GlobalCancelToken);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Failed to purge aria2 tasks", ex);
            }
        }
    }
}
