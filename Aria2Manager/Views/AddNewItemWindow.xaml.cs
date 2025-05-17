using Aria2Manager.ViewModels;
using Aria2Manager.Models;
using System.Windows;
using MahApps.Metro.Controls;

namespace Aria2Manager.Views
{
    /// <summary>
    /// AddNewItemWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddNewItemWindow : MetroWindow
    {
        public AddNewItemWindow(Aria2ServerInfoModel? Aria2Server = null)
        {
            InitializeComponent();
            AddNewItemViewModel Model = new AddNewItemViewModel(Aria2Server);
            DataContext = Model;
        }
    }
}