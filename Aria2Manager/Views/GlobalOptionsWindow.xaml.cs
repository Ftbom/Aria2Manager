using Aria2Manager.Models;
using Aria2Manager.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;

namespace Aria2Manager.Views
{
    /// <summary>
    /// GlobalOptionsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GlobalOptionsWindow : MetroWindow
    {
        public GlobalOptionsWindow(Aria2ServerInfoModel? Aria2Server = null)
        {
            InitializeComponent();
            GlobalOptionsViewModel Model = new GlobalOptionsViewModel(Aria2Server);
            DataContext = Model;
        }
    }
}
