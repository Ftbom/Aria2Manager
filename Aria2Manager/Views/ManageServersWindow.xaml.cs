using System.Windows;
using Aria2Manager.Models;
using Aria2Manager.ViewModels;

namespace Aria2Manager.Views
{
    /// <summary>
    /// ManageServersWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ManageServersWindow : Window
    {
        public ManageServersWindow(Aria2ServerInfoModel? Aria2Server = null)
        {
            InitializeComponent();
            ManageServersViewModel Model = new ManageServersViewModel(Aria2Server);
            DataContext = Model;
        }
    }
}