using Aria2Manager.Core.Helpers;
using System.ComponentModel;
using System.Globalization;

namespace Aria2Manager.WPF.Localization
{
    public class WpfLocalizer : INotifyPropertyChanged
    {
        public static WpfLocalizer Instance { get; } = new WpfLocalizer();
        public event PropertyChangedEventHandler? PropertyChanged;
        public CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture;
        public string GetString(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            //尝试从WPF资源库中获取
            string? uiTranslated = WpfStrings.ResourceManager.GetString(key, CurrentCulture);
            if (!string.IsNullOrWhiteSpace(uiTranslated))
            {
                return uiTranslated;
            }
            //从Core中查找
            return LanguageHelper.GetString(key);
        }
        public string this[string key] => GetString(key);
        //语言变更
        public void ChangeCulture(CultureInfo newCulture)
        {
            CurrentCulture = newCulture;
            WpfStrings.Culture = newCulture;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }
}