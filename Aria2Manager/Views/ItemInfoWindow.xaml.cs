using Aria2Manager.Models;
using Aria2Manager.ViewModels;
using System.Windows;

namespace Aria2Manager.Views
{
    /// <summary>
    /// ItemInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ItemInfoWindow : Window
    {
        public ItemInfoWindow(string? gid = null, Aria2ServerInfoModel? server = null)
        {
            InitializeComponent();
            var model = new ItemInfoViewModel(gid, server);
            DataContext = model;
        }
    }
}
