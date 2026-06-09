using Aria2Manager.Core.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia;
using Aria2Manager.Core;

namespace Aria2Manager.Avalonia.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }
        private async void SaveSettings(object? sender, RoutedEventArgs e)
        {
            bool needRestart = false;
            if (this.DataContext is SettingsViewModel viewModel)
            {
                if (viewModel.Settings.Theme != GlobalContext.Instance.AppSettings.Theme)
                {
                    needRestart = true;
                }
                await viewModel.SaveSettingsCommand.ExecuteAsync(null);
            }
            if (needRestart)
            {
                if (Application.Current is App appInstance)
                {
                    await appInstance.OpenSettings();
                }
                this.Close();
            }
        }
    }
}