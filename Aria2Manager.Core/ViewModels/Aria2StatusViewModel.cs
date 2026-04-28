using CommunityToolkit.Mvvm.ComponentModel;

namespace Aria2Manager.Core.ViewModels
{
    public partial class Aria2StatusViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _aria2Version = string.Empty;
        [ObservableProperty]
        private List<string> _enabledFeatures = new List<string>();
        public Aria2StatusViewModel()
        {
            LoadAria2Status();
        }
        private async void LoadAria2Status()
        {
            var status = await GlobalContext.Instance.Aria2Server.GetAria2Version();
            Aria2Version = status.Version;
            EnabledFeatures = status.EnabledFeatures;
        }
    }
}