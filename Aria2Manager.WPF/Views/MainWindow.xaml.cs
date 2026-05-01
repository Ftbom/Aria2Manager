using Aria2Manager.Core.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;

namespace Aria2Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            //监听可见性变化
            this.IsVisibleChanged += MainWindow_IsVisibleChanged;
            //确保窗口彻底关闭时停止轮询
            this.Closed += (s, e) => (this.DataContext as MainViewModel)?.StopRefreshLoop();
        }
        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                if (this.IsVisible)
                {
                    vm.StartRefreshLoop(); //窗口显示，开始刷新
                }
                else
                {
                    vm.StopRefreshLoop(); //窗口隐藏到托盘，停止刷新
                }
            }
        }
    }
}
