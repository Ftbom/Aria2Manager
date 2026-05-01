using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace Aria2Manager.Core.ViewModels
{
    public partial class AboutViewModel
    {
        private IUIService _uiService;
        public string AppName => GlobalContext.AppName;
        public string AppVersion => $"V{GlobalContext.AppVersion}";
        public string AppWebsite => "https://github.com/Ftbom/Aria2Manager";
        public string AuthorName => "Ftbom";
        public string AuthorEmail => "lz490070@gmail.com";
        public string AuthorGithub => "https://github.com/Ftbom";
        public string AuthorGithubName => "Ftbom";
        public string AuthorWebsite => "https://ftbom.github.io";
        public AboutViewModel(IUIService uiService)
        {
            _uiService = uiService;
        }
        [RelayCommand]
        private async Task CheckProgramUpdate()
        {
            bool? result = await UpdateCheckerHelper.CheckProgramUpdate();
            if (result == null)
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Check_Program_Update_Failed"), "Error", MsgBoxLevel.Error);
            }
            else if (result == true)
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Program_Update_Available"), "Info", MsgBoxLevel.Information);
            }
            else
            {
                await _uiService.ShowMessageBoxAsync(LanguageHelper.GetString("Program_Up_To_Date"), "Info", MsgBoxLevel.Information);
            }
        }
        [RelayCommand]
        private async Task OpenWebsite(string url)
        {
            await OpenWebsiteHelper.Open(url, _uiService);
        }
    }
}