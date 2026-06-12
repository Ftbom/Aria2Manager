using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aria2Manager.Core.ViewModels
{
    public partial class Aria2ConfigFileViewModel : ObservableObject
    {
        private IUIService _uiService;
        private string? _configFilePath;
        private string _oldConfigFileContent = string.Empty;
        [ObservableProperty]
        private bool _isSavingFile = false;
        [ObservableProperty]
        private string _configFileContent = string.Empty;
        public Aria2ConfigFileViewModel(IUIService uiService)
        {
            _uiService = uiService;
            OpenConfigFile();
        }
        public async void OpenConfigFile()
        {
            try
            {
                _configFilePath = await GlobalContext.Instance.Aria2Server.GetConfigFilePath();
                if (_configFilePath != null)
                {
                    _oldConfigFileContent = await File.ReadAllTextAsync(_configFilePath);
                    ConfigFileContent = _oldConfigFileContent;
                }
            }
            catch
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Load_Config_File_Failed"),
                    LanguageHelper.GetString("Error"), MsgBoxLevel.Error);
            }
        }
        [RelayCommand]
        public async Task SaveConfigFile()
        {
            if (_configFilePath != null && _oldConfigFileContent != ConfigFileContent &&
                !string.IsNullOrWhiteSpace(ConfigFileContent))
            {
                try
                {
                    if (GlobalContext.Instance.AppSettings.StartAria2)
                    {
                        IsSavingFile = true;
                        //先暂停，再写入；写入后，再重启
                        await Aria2ProcessHelper.KillAria2Process();
                        await File.WriteAllTextAsync(_configFilePath, ConfigFileContent);
                        await Task.Delay(5000);
                        Aria2ProcessHelper.StartAria2Process();
                        await Task.Delay(3000);
                        IsSavingFile = false;
                    }
                    else
                    {
                        await File.WriteAllTextAsync(_configFilePath, ConfigFileContent);
                        await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Aria2_Restart_Needed"),
                            LanguageHelper.GetString("Warning"), MsgBoxLevel.Warning);
                    }
                    _oldConfigFileContent = ConfigFileContent;
                }
                catch
                {
                    await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Save_Config_File_Failed"),
                        LanguageHelper.GetString("Error"), MsgBoxLevel.Error);
                    IsSavingFile = false;
                }
            }
        }
    }
}