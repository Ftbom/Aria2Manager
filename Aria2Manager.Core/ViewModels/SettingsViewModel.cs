using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;
using Aria2Manager.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace Aria2Manager.Core.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        public AppSettings Settings { get; set; } = GlobalContext.Instance.AppSettings.DeepClone(); //应用设置
        public List<CultureInfo> LanguageList { get; set; } //语言列表
        public List<string> ThemeList { get; private set; } //主题列表
        public List<string> TrackersSources { get; set; } //Trackers来源列表
        private readonly IUIService _uiService;
        public SettingsViewModel(IUIService uiService)
        {
            _uiService = uiService;
            LanguageList = LanguageHelper.GetSupportedLanguages();
            TrackersSources = BtTrackers.Sources.Keys.ToList();
            ThemeList = uiService.ThemeList;
        }
        [RelayCommand]
        private async Task SaveSettings()
        {
            bool LanguageChanged = Settings.Language != GlobalContext.Instance.AppSettings.Language;
            bool ThemeNeedRestart = false;
            if (Settings.Theme != GlobalContext.Instance.AppSettings.Theme)
            {
                ThemeNeedRestart = await _uiService.ChangeThemeAsync(Settings.Theme);
            }
            if (LanguageChanged)
            {
                if (ThemeNeedRestart)
                {
                    await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Theme_Language_ReStart"), "Info", MsgBoxLevel.Information);
                }
                else
                {
                    await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Language_ReStart"), "Info", MsgBoxLevel.Information);
                }
            }
            else if (ThemeNeedRestart)
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Theme_ReStart"), "Info", MsgBoxLevel.Information);
            }
            GlobalContext.Instance.AppSettings = Settings.DeepClone();
            GlobalContext.Instance.SaveSettings();
        }
    }
}