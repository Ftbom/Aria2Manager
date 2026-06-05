using Aria2Manager.Core.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;

namespace Aria2Manager.WPF.Views
{
    /// <summary>
    /// Aria2TaskInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Aria2TaskInfoWindow : MetroWindow
    {
        public Aria2TaskInfoWindow()
        {
            InitializeComponent();
            //监听可见性变化
            this.IsVisibleChanged += TaskInfoWindow_IsVisibleChanged;
            //确保窗口彻底关闭时停止轮询
            this.Closed += (s, e) => (this.DataContext as Aria2TaskInfoViewModel)?.StopRefreshLoop();
        }
        private void TaskInfoWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is Aria2TaskInfoViewModel vm)
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
