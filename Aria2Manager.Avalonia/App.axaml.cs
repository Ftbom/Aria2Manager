using Aria2Manager.Avalonia.Services;
using Aria2Manager.Avalonia.Views;
using Aria2Manager.Core;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.ViewModels;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Aria2Manager.Avalonia.Localization;

namespace Aria2Manager.Avalonia
{
    public partial class App : Application
    {
        private AvaloniaUIService _uiService;
        public App()
        {
            _uiService = new AvaloniaUIService();
            LanguageHelper.OnLanguageChanged += culture =>
            {
                AvaloniaLocalizer.Instance.ChangeCulture(culture);
            };
        }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            GlobalContext.Instance.InitializeAsync(_uiService);
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(_uiService),
                };
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}