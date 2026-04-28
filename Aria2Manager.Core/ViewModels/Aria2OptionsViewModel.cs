using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Services;
using Aria2Manager.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Aria2Manager.Core.ViewModels
{
    //Aria2配置项信息
    public class OptionViewModel : ObservableObject
    {
        private string _id;
        private string _value;
        public OptionViewModel(string id, bool readOnly = false)
        {
            _id = id;
            _value = String.Empty;
            ReadOnly = readOnly;
            Name = LanguageHelper.GetString($"Aria2Option_{id}");
            Description = LanguageHelper.TryGetString($"Aria2Option_{id}_Tip");
        }
        public void Update(string id, string value)
        {
            if (id != _id) { return; }
            _value = value;
            OnPropertyChanged(nameof(Value));
        }
        public string Id => _id; //配置项id
        public string Value => _value; //配置项值
        public bool ReadOnly { get; private set; } //配置项是否只读
        public string Name { get; private set; } //配置项名称
        public string? Description { get; private set; } //配置项描述
    }
    public partial class Aria2OptionsViewModel : ObservableObject
    {
        private readonly IUIService _uiService;
        private readonly static List<string> _basicOptions = new List<string> {
            "dir", "log", "max-concurrent-downloads", "check-integrity", "continue"
        };
        private readonly static List<string> _httpFtpSftpOptions = new List<string> {
            "all-proxy", "all-proxy-user", "all-proxy-passwd", "connect-timeout", "dry-run", "lowest-speed-limit",
            "max-connection-per-server", "max-file-not-found", "max-tries", "min-split-size", "netrc-path", "no-netrc",
            "no-proxy", "proxy-method", "remote-time", "reuse-uri", "retry-wait", "server-stat-of",
            "server-stat-timeout", "split", "stream-piece-selector", "timeout", "uri-selector"
        };
        private readonly static List<string> _httpOptions = new List<string> {
            "check-certificate", "http-accept-gzip", "http-auth-challenge", "http-no-cache", "http-user",
            "http-passwd", "http-proxy", "http-proxy-user", "http-proxy-passwd", "https-proxy", "https-proxy-user",
            "https-proxy-passwd", "referer", "enable-http-keep-alive", "enable-http-pipelining", "header",
            "save-cookies", "use-head", "user-agent"
        };
        private readonly static List<string> _ftpSftpOptions = new List<string> {
            "ftp-user", "ftp-passwd", "ftp-pasv", "ftp-proxy", "ftp-proxy-user", "ftp-proxy-passwd",
            "ftp-type", "ftp-reuse-connection", "ssh-host-key-md"
        };
        private readonly static List<string> _btOptions = new List<string> {
            "bt-detach-seed-only", "bt-enable-hook-after-hash-check", "bt-enable-lpd", "bt-exclude-tracker",
            "bt-external-ip", "bt-force-encryption", "bt-hash-check-seed", "bt-load-saved-metadata", "bt-max-open-files", "bt-max-peers",
            "bt-metadata-only", "bt-min-crypto-level", "bt-prioritize-piece", "bt-remove-unselected-file",
            "bt-require-crypto", "bt-request-peer-speed-limit", "bt-save-metadata", "bt-seed-unverified",
            "bt-stop-timeout", "bt-tracker", "bt-tracker-connect-timeout", "bt-tracker-interval", "bt-tracker-timeout",
            "dht-file-path", "dht-file-path6", "dht-listen-port", "dht-message-timeout", "enable-dht", "enable-dht6",
            "enable-peer-exchange", "follow-torrent", "listen-port", "max-overall-upload-limit", "max-upload-limit",
            "peer-id-prefix", "peer-agent", "seed-ratio", "seed-time"
        };
        private readonly static List<string> _metalinkOptions = new List<string> {
            "follow-metalink", "metalink-base-uri", "metalink-language", "metalink-location", "metalink-os",
            "metalink-version", "metalink-preferred-protocol", "metalink-enable-unique-protocol"
        };
        private readonly static List<string> _rpcOptions = new List<string> {
            "enable-rpc", "pause-metadata", "rpc-allow-origin-all", "rpc-listen-all", "rpc-listen-port",
            "rpc-max-request-size", "rpc-save-upload-metadata", "rpc-secure"
        };
        private readonly static List<string> _advancedOptions = new List<string> {
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
        private readonly static List<string> _readonlyOptions = new List<string> {
            "netrc-path", "server-stat-timeout", "check-certificate", "bt-detach-seed-only", "dht-file-path", "dht-file-path6",
            "dht-listen-port", "dht-message-timeout", "enable-dht", "enable-dht6", "listen-port", "peer-id-prefix", "peer-agent",
            "enable-rpc", "rpc-allow-origin-all", "rpc-listen-all", "rpc-listen-port", "rpc-max-request-size", "rpc-secure",
            "auto-save-interval", "conf-path", "console-log-level", "daemon", "deferred-input", "disable-ipv6", "disk-cache",
            "dscp", "rlimit-nofile", "enable-color", "event-poll", "human-readable", "min-tls-version", "show-console-readout",
            "summary-interval", "no-conf", "quiet", "save-session-interval", "socket-recv-buffer-size", "stop",
            "truncate-console-readout"
        };
        private Aria2ServerService Server => GlobalContext.Instance.Aria2Server;
        public ObservableCollection<OptionViewModel> BasicOptions { get; set; } //基本设置
        public ObservableCollection<OptionViewModel> HttpFtpSftpOptions { get; set; } //HTTP/FTP/SFTP设置
        public ObservableCollection<OptionViewModel> HttpOptions { get; set; } //HTTP设置
        public ObservableCollection<OptionViewModel> FtpSftpOptions { get; set; } //FTP/SFTP设置
        public ObservableCollection<OptionViewModel> BTOptions { get; set; } //BT设置
        public ObservableCollection<OptionViewModel> MetalinkOptions { get; set; } //Metalink设置
        public ObservableCollection<OptionViewModel> RPCOptions { get; set; } //RPC设置
        public ObservableCollection<OptionViewModel> AdvancedOptions { get; set; } //高级设置
        public Aria2OptionsViewModel(IUIService uiService)
        {
            _uiService = uiService;
            BasicOptions = InitAria2Options(_basicOptions);
            HttpFtpSftpOptions = InitAria2Options(_httpFtpSftpOptions);
            HttpOptions = InitAria2Options(_httpOptions);
            FtpSftpOptions = InitAria2Options(_ftpSftpOptions);
            BTOptions = InitAria2Options(_btOptions);
            MetalinkOptions = InitAria2Options(_metalinkOptions);
            RPCOptions = InitAria2Options(_rpcOptions);
            AdvancedOptions = InitAria2Options(_advancedOptions);
            LoadAria2Options();
        }
        private ObservableCollection<OptionViewModel> InitAria2Options(List<string> keys)
        {
            var options = new ObservableCollection<OptionViewModel>();
            foreach (var key in keys)
            {
                options.Add(new OptionViewModel(key, _readonlyOptions.Contains(key)));
            }
            return options;
        }
        private async void LoadOptions(List<string> keys, ObservableCollection<OptionViewModel> optionVMs, IDictionary<string, string?> options)
        {
            foreach (var key in keys)
            {
                var option = options[key];
                if (option != null)
                {
                    var optionVM = optionVMs.FirstOrDefault(vm => vm.Id == key);
                    optionVM?.Update(key, option);
                }
            }
        }
        private async void LoadAria2Options()
        {
            try
            {
                var optionKeys = new List<string>();
                optionKeys.AddRange(_basicOptions);
                optionKeys.AddRange(_httpFtpSftpOptions);
                optionKeys.AddRange(_httpOptions);
                optionKeys.AddRange(_ftpSftpOptions);
                optionKeys.AddRange(_btOptions);
                optionKeys.AddRange(_metalinkOptions);
                optionKeys.AddRange(_rpcOptions);
                optionKeys.AddRange(_advancedOptions);
                var options = await Server.GetAria2Options(optionKeys);
                LoadOptions(_basicOptions, BasicOptions, options);
                LoadOptions(_httpFtpSftpOptions, HttpFtpSftpOptions, options);
                LoadOptions(_httpOptions, HttpOptions, options);
                LoadOptions(_ftpSftpOptions, FtpSftpOptions, options);
                LoadOptions(_btOptions, BTOptions, options);
                LoadOptions(_metalinkOptions, MetalinkOptions, options);
                LoadOptions(_rpcOptions, RPCOptions, options);
                LoadOptions(_advancedOptions, AdvancedOptions, options);
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Load_Options_Fail"), "Error", MsgBoxLevel.Error);
            }
        }
        private async void ReadOptions(Dictionary<string, string> options, ObservableCollection<OptionViewModel> optionVMs)
        {
            foreach (var option in optionVMs)
            {
                options.Add(option.Id, option.Value);
            }
        }
        [RelayCommand]
        private async Task ChangeAria2Options()
        {
            try
            {
                var options = new Dictionary<string, string>();
                ReadOptions(options, BasicOptions);
                ReadOptions(options, HttpFtpSftpOptions);
                ReadOptions(options, HttpOptions);
                ReadOptions(options, FtpSftpOptions);
                ReadOptions(options, BTOptions);
                ReadOptions(options, MetalinkOptions);
                ReadOptions(options, RPCOptions);
                ReadOptions(options, AdvancedOptions);
                await Server.ChangeAria2Options(options);
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Change_Options_Fail"), "Error", MsgBoxLevel.Error);
            }
        }
    }
}
