using System.Windows;
using Aria2Manager.ViewModels;

namespace Aria2Manager.Views
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            var model = new SettingsViewModel();
            DataContext = model;
        }
    }
}
