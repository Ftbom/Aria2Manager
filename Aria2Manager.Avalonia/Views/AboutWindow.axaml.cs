using Aria2Manager.Core.ViewModels;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Input;
using Avalonia.Input.Platform;

namespace Aria2Manager.Avalonia.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }
        private void Website_Tapped(object? sender, TappedEventArgs e)
        {
            if (this.DataContext is AboutViewModel viewModel)
            {
                viewModel.OpenWebsiteCommand.Execute(viewModel.AppWebsite);
            }
        }
        private void Github_Tapped(object? sender, TappedEventArgs e)
        {
            if (this.DataContext is AboutViewModel viewModel)
            {
                viewModel.OpenWebsiteCommand.Execute(viewModel.AuthorGithub);
            }
        }
    }
}