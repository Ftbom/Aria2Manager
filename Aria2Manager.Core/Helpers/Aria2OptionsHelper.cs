namespace Aria2Manager.Core.Helpers
{
    public class Aria2OptionsHelper
    {
        public static Dictionary<string, string?> ParseAria2Options(IDictionary<string, string?> options)
        {
            Dictionary<string, string?> parsedOptions = new Dictionary<string, string?>();
            foreach (var key in options.Keys)
            {
                if (key == "all-proxy")
                {
                    string? proxyAddress = null;
                    string? proxyPort = null;
                    string? rawProxyOption = options[key];
                    if (!String.IsNullOrWhiteSpace(rawProxyOption))
                    {
                        try
                        {
                            if (!rawProxyOption.Contains("://"))
                            {
                                rawProxyOption = "http://" + rawProxyOption;
                            }
                            if (Uri.TryCreate(rawProxyOption, UriKind.Absolute, out var uri))
                            {
                                proxyAddress = $"{uri.Scheme}://{uri.Host}";
                                proxyPort = uri.Port.ToString();
                            }
                            else
                            {
                                LogHelper.Warning($"Failed to parse all-proxy option: {rawProxyOption}");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Warning("Failed to parse all-proxy option", ex);
                            proxyAddress = null;
                            proxyPort = null;
                        }
                    }
                    parsedOptions["proxy-address"] = proxyAddress;
                    parsedOptions["proxy-port"] = proxyPort;
                }
                else
                {
                    parsedOptions[key] = options[key];
                }
            }
            return parsedOptions;
        }
        public static IDictionary<string, object> MergeAria2Options(Dictionary<string, string?> options)
        {
            IDictionary<string, object> mergedOptions = new Dictionary<string, object>();
            foreach (var key in options.Keys)
            {
                if (key == "proxy-address" && options.ContainsKey("proxy-port"))
                {
                    var proxyAddress = options["proxy-address"];
                    var proxyPort = options["proxy-port"];
                    if (String.IsNullOrWhiteSpace(proxyAddress))
                    {
                        mergedOptions["all-proxy"] = String.Empty;
                    }
                    else
                    {
                        mergedOptions["all-proxy"] = String.IsNullOrWhiteSpace(proxyPort) ? proxyAddress : proxyAddress + ":" + proxyPort;
                    }
                }
                else if (key == "proxy-port")
                {
                    continue;
                }
                else if (key == "header")
                {
                    var headerValue = options[key];
                    if (!String.IsNullOrWhiteSpace(headerValue))
                    {
                        var headerList = new List<string>();
                        var headerStrs = headerValue.Split(
                            new[] { '\r', '\n' },
                            StringSplitOptions.RemoveEmptyEntries
                        );
                        foreach (var header in headerStrs)
                        {
                            headerList.Add(header);
                        }
                        mergedOptions["header"] = headerList.ToArray();
                    }
                }
                else
                {
                    var value = options[key];
                    if (value != null)
                    {
                        mergedOptions[key] = value;
                    } 
                }
            }
            return mergedOptions;
        }
    }
}
