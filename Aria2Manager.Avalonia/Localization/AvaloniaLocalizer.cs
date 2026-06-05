using Aria2Manager.Core.Helpers;
using System.ComponentModel;
using System.Globalization;

namespace Aria2Manager.Avalonia.Localization
{
    public class AvaloniaLocalizer : INotifyPropertyChanged
    {
        public static AvaloniaLocalizer Instance { get; } = new AvaloniaLocalizer();
        public event PropertyChangedEventHandler? PropertyChanged;
        public CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture;
        public string GetString(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            string? uiTranslated = AvaloniaStrings.ResourceManager.GetString(key, CurrentCulture);
            if (!string.IsNullOrWhiteSpace(uiTranslated))
            {
                return uiTranslated;
            }
            return LanguageHelper.GetString(key);
        }
        public string this[string key] => GetString(key);
        public void ChangeCulture(CultureInfo newCulture)
        {
            CurrentCulture = newCulture;
            AvaloniaStrings.Culture = newCulture;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}