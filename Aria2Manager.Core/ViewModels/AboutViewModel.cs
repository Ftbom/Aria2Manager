using Aria2Manager.Core.Enums;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace Aria2Manager.Core.ViewModels
{
    public partial class AboutViewModel
    {
        private IUIService _uiService;
        public string AppName { get; set; } = "Aria2Manager";
        public string AppVersion { get; set; } = $"V{GlobalContext.AppVersion}";
        public string AuthorName { get; set; } = "Ftbom";
        public string AuthorEmail { get; set; } = "lz490070@gmail.com";
        public string AuthorGithub { get; set; } = "https://github.com/Ftbom";
        public string AuthorGithubName { get; set; } = "Ftbom";
        public string AuthorWebsite { get; set; } = "https://ftbom.github.io";
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
    }
}