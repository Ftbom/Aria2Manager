using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Aria2Manager.Models;
using Aria2Manager.Utils;
using Aria2Manager.Views;

namespace Aria2Manager.ViewModels
{
    public class GlobalOptionsViewModel
    {
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
        private IDictionary<string, string>? global_options_value; //获取到的设置
        private List<string> LoadedOptionsName; //记录已加载的设置项，用于分批加载设置，减少加载时间

        public List<OptionModel> BasicOptions { get;set; } //基本设置
        public List<OptionModel> HttpFtpSftpOptions { get; set; } //HTTP/FTP/SFTP设置
        public List<OptionModel> HttpOptions { get; set; } //HTTP设置
        public List<OptionModel> FtpSftpOptions { get; set; } //FTP/SFTP设置
        public List<OptionModel> BTOptions { get; set; } //BT设置
        public List<OptionModel> MetalinkOptions { get; set; } //Metalink设置
        public List<OptionModel> RPCOptions { get; set; } //RPC设置
        public List<OptionModel> AdvancedOptions { get; set; } //高级设置
        public ICommand TabChangeCommand { get; private set; } //Tab切换事件
        public ICommand SetOptionsCommand { get; private set; } //保存设置

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
            //初始化配置项
            BasicOptions = InitOptions(basic_options, "BasicOptionNames");
            HttpFtpSftpOptions = InitOptions(http_ftp_sftp_options, "HTTPFTPSFTPOptionNames");
            HttpOptions = InitOptions(http_options, "HTTPOptionNames");
            FtpSftpOptions = InitOptions(ftp_sftp_options, "FTPSFTPOptionNames");
            BTOptions = InitOptions(bt_options, "BitTorrentOptionNames");
            MetalinkOptions = InitOptions(metalink_options, "MetalinkOptionNames");
            RPCOptions = InitOptions(rpc_options, "RPCOptionNames");
            AdvancedOptions = InitOptions(advanced_options, "AdvancedOptionNames");
            TabChangeCommand = new RelayCommand(ChangeLoadedOptions);
            SetOptionsCommand = new RelayCommand(SetOptions);
            LoadedOptionsName = new List<string>();
            GetOptions(); //获取配置项的值
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
            LoadOptions(BasicOptions, nameof(BasicOptions));
        }

        //根据选中的Tab动态加载配置
        private void ChangeLoadedOptions(object? parameter)
        {
            if (parameter == null)
            {
                return;
            }
            //根据切换的Tab加载设置项
            string tab_name = (string)parameter;
            switch (tab_name)
            {
                case "HttpFtpSftpOptionsTab":
                    LoadOptions(HttpFtpSftpOptions, nameof(HttpFtpSftpOptions));
                    break;
                case "HttpOptionsTab":
                    LoadOptions(HttpOptions, nameof(HttpOptions));
                    break;
                case "FtpSftpOptionsTab":
                    LoadOptions(FtpSftpOptions, nameof(FtpSftpOptions));
                    break;
                case "BitTorrentOptionsTab":
                    LoadOptions(BTOptions, nameof(BTOptions));
                    break;
                case "MetalinkOptionsTab":
                    LoadOptions(MetalinkOptions, nameof(MetalinkOptions));
                    break;
                case "RPCOptionsTab":
                    LoadOptions(RPCOptions, nameof(RPCOptions));
                    break;
                case "AdvancedOptionsTab":
                    LoadOptions(AdvancedOptions, nameof(AdvancedOptions));
                    break;
                default:
                    break;
            }
        }

        //加载配置
        public void LoadOptions(List<OptionModel> options, string options_name)
        {
            if (!is_connect)
            {
                return;
            }
            //设置已加载则跳过
            if (LoadedOptionsName.Contains(options_name))
            {
                return;
            }
            LoadedOptionsName.Add(options_name); //记录已加载的配置
            foreach (OptionModel option in options)
            {
                options[options.IndexOf(option)].value = GetOptionValueByKey(global_options_value, option.id);
            }
        }

        private List<OptionModel> InitOptions(List<string> options_id, string options_source_name)
        {
            List<OptionModel> options = new List<OptionModel>();
            string source_string;
            try
            {
                //获取资源文件中设置项的名称和描述
                source_string = Application.Current.FindResource(options_source_name).ToString();
                if (source_string != null)
                {
                    source_string = source_string.Replace("\n", "");
                }
            }
            catch
            {
                return options;
            }
            List<string> option_names = source_string.Split('|').ToList(); //分隔设置项
            for (int i = 0; i < options_id.Count; i++)
            {
                string[] name_string = option_names[i].Split('%'); //分隔名称和描述
                string _name = name_string[0];
                string? _description;
                if (name_string.Length == 2)
                {
                    _description = name_string[1]; //存在描述
                }
                else
                {
                    _description = null; //不存在描述
                }
                if (readonly_options.Contains(options_id[i]))
                {
                    options.Add(new OptionModel { is_enabled = false, value = "" , id = options_id[i], name = _name, description = _description });
                }
                else
                {
                    options.Add(new OptionModel { is_enabled = true, value = "", id = options_id[i], name = _name, description = _description });
                }
            }
            return options;
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

        //设置配置项（保存设置）
        async private void SetOptions(object? parameter)
        {
            if (is_connect)
            {
                Dictionary<string, string> options = new Dictionary<string, string>();
                //加载经界面修改后的设置
                GetChangedOptions(options, BasicOptions, nameof(BasicOptions));
                GetChangedOptions(options, HttpFtpSftpOptions, nameof(HttpFtpSftpOptions));
                GetChangedOptions(options, FtpSftpOptions, nameof(FtpSftpOptions));
                GetChangedOptions(options, BTOptions, nameof(BTOptions));
                GetChangedOptions(options, MetalinkOptions, nameof(MetalinkOptions));
                GetChangedOptions(options, RPCOptions, nameof(RPCOptions));
                GetChangedOptions(options, AdvancedOptions, nameof(AdvancedOptions));
                GetChangedOptions(options, HttpOptions, nameof(HttpOptions));
                //应用更改
                Aria2ClientModel client = new Aria2ClientModel(aria2_server);
                try
                {
                    await client.Aria2Client.ChangeGlobalOptionAsync(options);
                }
                catch { }
                if (parameter != null)
                {
                    ((GlobalOptionsWindow)parameter).Close();
                }
            }
        }

        //获取界面更改的设置
        private void GetChangedOptions(Dictionary<string, string> options, List<OptionModel> options_list, string options_name)
        {
            if (!LoadedOptionsName.Contains(options_name)) //跳过未在界面加载的设置
            {
                return;
            }
            foreach (OptionModel option in options_list)
            {
                if (option.is_enabled) //是否只读
                {
                    if (option.value != null)
                    {
                        options[option.id] = option.value;
                    }
                }
            }
        }
    }
}
