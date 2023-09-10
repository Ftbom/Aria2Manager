using Aria2Manager.ViewModels;
using Aria2Manager.Models;
using System.Windows;

namespace Aria2Manager.Views
{
    /// <summary>
    /// AddNewItemWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddNewItemWindow : Window
    {
        public AddNewItemWindow(Aria2ServerInfoModel? Aria2Server = null)
        {
            InitializeComponent();
            AddNewItemViewModel Model = new AddNewItemViewModel(Aria2Server);
            DataContext = Model;
        }
    }
}