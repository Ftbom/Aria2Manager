using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;
using Aria2Manager.Models;

namespace Aria2Manager.ViewModels
{
    public class GlobalOptionsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private List<string> basic_options = new List<string> {
            "dir", "log", "max-concurrent-downloads", "check-integrity", "continue"
        };
        private List<string> http_ftp_sftp_options = new List<string> {
            "all-proxy", "all-proxy-user", "all-proxy-passwd", "connect-timeout", "dry-run", "lowest-speed-limit",
            "max-connection-per-server", "max-file-not-found", "max-tries", "min-split-size", "netrc-path", "no-netrc",
            "no-proxy", "proxy-method", "remote-time", "reuse-uri", "retry-wait", "server-stat-of",
            "server-stat-timeout", "split", "stream-piece-selector", "timeout", "uri-selector"
        };
        private List<string> http_options = new List<string> {
            "check-certificate", "http-accept-gzip", "http-auth-challenge", "http-no-cache", "http-user",
            "http-passwd", "http-proxy", "http-proxy-user", "http-proxy-passwd", "https-proxy", "https-proxy-user",
            "https-proxy-passwd", "referer", "enable-http-keep-alive", "enable-http-pipelining", "header",
            "save-cookies", "use-head", "user-agent"
        };
        private List<string> ftp_sftp_options = new List<string> {
            "ftp-user", "ftp-passwd", "ftp-pasv", "ftp-proxy", "ftp-proxy-user", "ftp-proxy-passwd",
            "ftp-type", "ftp-reuse-connection", "ssh-host-key-md"
        };
        private List<string> bt_options = new List<string> {
            "bt-detach-seed-only", "bt-enable-hook-after-hash-check", "bt-enable-lpd", "bt-exclude-tracker",
            "bt-external-ip", "bt-force-encryption", "bt-hash-check-seed", "bt-load-saved-metadata", "bt-max-open-files", "bt-max-peers",
            "bt-metadata-only", "bt-min-crypto-level", "bt-prioritize-piece", "bt-remove-unselected-file",
            "bt-require-crypto", "bt-request-peer-speed-limit", "bt-save-metadata", "bt-seed-unverified",
            "bt-stop-timeout", "bt-tracker", "bt-tracker-connect-timeout", "bt-tracker-interval", "bt-tracker-timeout",
            "dht-file-path", "dht-file-path6", "dht-listen-port", "dht-message-timeout", "enable-dht", "enable-dht6",
            "enable-peer-exchange", "follow-torrent", "listen-port", "max-overall-upload-limit", "max-upload-limit",
            "peer-id-prefix", "peer-agent", "seed-ratio", "seed-time"
        };
        private List<string> metalink_options = new List<string> {
            "follow-metalink", "metalink-base-uri", "metalink-language", "metalink-location", "metalink-os",
            "metalink-version", "metalink-preferred-protocol", "metalink-enable-unique-protocol"
        };
        private List<string> rpc_options = new List<string> {
            "enable-rpc", "pause-metadata", "rpc-allow-origin-all", "rpc-listen-all", "rpc-listen-port",
            "rpc-max-request-size", "rpc-save-upload-metadata", "rpc-secure"
        };
        private List<string> advanced_options = new List<string> {
            "allow-overwrite", "allow-piece-length-change", "always-resume", "async-dns", "auto-file-renaming",
            "auto-save-interval", "conditional-get", "conf-path", "console-log-level", "content-disposition-default-utf8", "daemon",
            "deferred-input", "disable-ipv6", "disk-cache", "download-result", "dscp", "rlimit-nofile", "enable-color", "enable-mmap",
            "event-poll", "file-allocation", "force-save", "save-not-found", "hash-check-only", "human-readable",
            "keep-unfinished-download-result", "max-download-result", "max-mmap-limit", "max-resume-failure-tries",
            "min-tls-version", "log-level", "optimize-concurrent-downloads", "piece-length", "show-console-readout",
            "summary-interval", "max-overall-download-limit", "max-download-limit", "no-conf",
            "no-file-allocation-limit", "parameterized-uri", "quiet", "realtime-chunk-checksum", "remove-control-file",
            "save-session", "save-session-interval", "socket-recv-buffer-size", "stop", "truncate-console-readout"
        };
        private List<string> readonly_options = new List<string> {
            "netrc-path", "server-stat-timeout", "check-certificate", "bt-detach-seed-only", "dht-file-path", "dht-file-path6",
            "dht-listen-port", "dht-message-timeout", "enable-dht", "enable-dht6", "listen-port", "peer-id-prefix", "peer-agent",
            "enable-rpc", "rpc-allow-origin-all", "rpc-listen-all", "rpc-listen-port", "rpc-max-request-size", "rpc-secure",
            "auto-save-interval", "conf-path", "console-log-level", "daemon", "deferred-input", "disable-ipv6", "disk-cache",
            "dscp", "rlimit-nofile", "enable-color", "event-poll", "human-readable", "min-tls-version", "show-console-readout",
            "summary-interval", "no-conf", "quiet", "save-session-interval", "socket-recv-buffer-size", "stop",
            "truncate-console-readout"

        };
        private Aria2ServerInfoModel aria2_server; //服务器信息
        private bool is_connect = true; //是否连接成功
        private IDictionary<string, string>? global_options_value;

        public ObservableCollection<OptionModel> BasicOptions { get; set; }
        public ObservableCollection<OptionModel> HttpFtpSftpOptions { get; set; }
        public ObservableCollection<OptionModel> HttpOptions { get; set; }
        public ObservableCollection<OptionModel> FtpSftpOptions { get; set; }
        public ObservableCollection<OptionModel> BTOptions { get; set; }
        public ObservableCollection<OptionModel> MetalinkOptions { get; set; }
        public ObservableCollection<OptionModel> RPCOptions { get; set; }
        public ObservableCollection<OptionModel> AdvancedOptions { get; set; }
        public bool EnableTab { get; set; }

        public GlobalOptionsViewModel(Aria2ServerInfoModel? Server = null)
        {
            if (Server == null)
            {
                aria2_server = new Aria2ServerInfoModel();
            }
            else
            {
                aria2_server = Server;
            }
            BasicOptions = InitOptions(basic_options);
            HttpFtpSftpOptions = InitOptions(http_ftp_sftp_options);
            HttpOptions = InitOptions(http_options);
            FtpSftpOptions = InitOptions(ftp_sftp_options);
            BTOptions = InitOptions(bt_options);
            MetalinkOptions = InitOptions(metalink_options);
            RPCOptions = InitOptions(rpc_options);
            AdvancedOptions = InitOptions(advanced_options);
            EnableTab = false;
            GetOptions();
        }

        //获取全局设置
        async private void GetOptions()
        {
            Aria2ClientModel client = new Aria2ClientModel(aria2_server);
            try
            {
                global_options_value = await client.Aria2Client.GetGlobalOptionAsync();
                is_connect = true;
            }
            catch
            {
                is_connect = false;
                MessageBox.Show(Application.Current.FindResource("ConnectionError").ToString());
            }
            //加载配置
            LoadOptions(BasicOptions);
            LoadOptions(HttpFtpSftpOptions);
            LoadOptions(HttpOptions);
            LoadOptions(FtpSftpOptions);
            LoadOptions(BTOptions);
            LoadOptions(MetalinkOptions);
            LoadOptions(RPCOptions);
            LoadOptions(AdvancedOptions);
            EnableTab = true;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(null));
            }
        }

        public void LoadOptions(ObservableCollection<OptionModel> Options)
        {
            if (!is_connect)
            {
                return;
            }
            foreach (OptionModel option in Options)
            {
                Options[Options.IndexOf(option)].value = GetOptionValueByKey(global_options_value, option.id);
            }
        }

        private ObservableCollection<OptionModel> InitOptions(List<string> OptionsId)
        {
            ObservableCollection<OptionModel> Options = new ObservableCollection<OptionModel>();
            foreach (string _id in OptionsId)
            {
                if (readonly_options.Contains(_id))
                {
                    Options.Add(new OptionModel { is_enabled = false, value = "" , id = _id});
                }
                else
                {
                    Options.Add(new OptionModel { is_enabled = true, value = "", id = _id });
                }
            }
            return Options;
        }

        //从字典读取值
        private string GetOptionValueByKey(IDictionary<string, string>? options, string key)
        {
            if (options == null)
            {
                return "";
            }
            try
            {
                if (options[key] == "")
                {
                    return "";
                }
                return options[key];
            }
            catch
            {
                return "";
            }
        }
    }
}
