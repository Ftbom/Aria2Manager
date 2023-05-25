using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Aria2Manager.Models;
using Aria2Manager.ViewModels;

namespace Aria2Manager.Views
{
    /// <summary>
    /// ManageServersWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ManageServersWindow : Window
    {
        public ManageServersWindow(Aria2ServerModel? Aria2Server = null)
        {
            InitializeComponent();
            ManageServersViewModel Model = new ManageServersViewModel(Aria2Server);
            DataContext = Model;
        }
    }
}