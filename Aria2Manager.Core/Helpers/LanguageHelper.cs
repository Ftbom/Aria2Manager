using Aria2Manager.Core.Localization;
using System.Globalization;
using System.Reflection;

namespace Aria2Manager.Core.Helpers
{
    public static class LanguageHelper
    {
        //获取当前系统语言
        public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture;
        //获取翻译后的字符串，如果找不到对应的key，返回key本身
        public static string GetString(string key)
        {
            string? translated = Strings.ResourceManager.GetString(key, CurrentCulture);
            return string.IsNullOrWhiteSpace(translated) ? key : translated;
        }
        //获取翻译后的字符串，如果找不到对应的key，返回null
        public static string? TryGetString(string key)
        {
            return Strings.ResourceManager.GetString(key, CurrentCulture);
        }
        //语言切换
        public static void ChangeLanguage(string language)
        {
            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo(language);
                CurrentCulture = cultureInfo;
                Strings.Culture = cultureInfo;
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Failed to change language to '{language}'", ex, false);
            }
        }
        //获取支持的语言列表，默认包含 "en-US"
        public static List<CultureInfo> GetSupportedLanguages(string defaultCultureCode = "en-US")
        {
            var supportedLanguages = new List<CultureInfo>();
            try
            {
                supportedLanguages.Add(CultureInfo.GetCultureInfo(defaultCultureCode));
            }
            catch (CultureNotFoundException) { }
            string baseDirectory = AppContext.BaseDirectory;
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;
            string satelliteFileName = $"{assemblyName}.resources.dll";
            foreach (string directory in Directory.GetDirectories(baseDirectory))
            {
                string dirName = Path.GetFileName(directory);
                string satelliteFilePath = Path.Combine(directory, satelliteFileName);
                if (File.Exists(satelliteFilePath))
                {
                    try
                    {
                        var culture = CultureInfo.GetCultureInfo(dirName);
                        if (!supportedLanguages.Exists(c => c.Name.Equals(culture.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            supportedLanguages.Add(culture);
                        }
                    }
                    catch (CultureNotFoundException) { }
                }
            }
            return supportedLanguages;
        }
    }
}