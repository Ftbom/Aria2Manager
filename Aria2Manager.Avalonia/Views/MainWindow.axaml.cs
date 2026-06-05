using Aria2Manager.Core.ViewModels;
using Avalonia;
using Avalonia.Controls;

namespace Aria2Manager.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //确保窗口彻底关闭时停止轮询
            this.Closed += (s, e) => (this.DataContext as MainViewModel)?.StopRefreshLoop();
        }
        protected override void IsVisibleChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.IsVisibleChanged(e);
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