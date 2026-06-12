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
        [ObservableProperty]
        private bool _enableTrackersUpdate;
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
            EnableTrackersUpdate = Settings.Trackers.EnableUpdate;
        }
        partial void OnEnableTrackersUpdateChanged(bool value)
        {
            Settings.Trackers.EnableUpdate = value;
        }
        [RelayCommand]
        private async Task SaveSettings()
        {
            LanguageHelper.ChangeLanguage(Settings.Language); //应用语言
            bool ThemeNeedRestart = false;
            if (Settings.Theme != GlobalContext.Instance.AppSettings.Theme)
            {
                ThemeNeedRestart = await _uiService.ChangeThemeAsync(Settings.Theme);
            }
            GlobalContext.Instance.AppSettings = Settings.DeepClone();
            _ = GlobalContext.Instance.SaveSettings();
            if (ThemeNeedRestart)
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Theme_ReStart_Needed"),
                    LanguageHelper.GetString("Info"), MsgBoxLevel.Information);
            }
            else
            {
                await Task.Delay(500); //点击按钮后等待一段时间再启用，防止用户连续点击
            }
        }
    }
}