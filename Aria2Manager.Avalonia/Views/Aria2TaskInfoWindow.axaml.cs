using Aria2Manager.Core.ViewModels;
using Avalonia;
using Avalonia.Controls;

namespace Aria2Manager.Avalonia.Views
{
    public partial class Aria2TaskInfoWindow : Window
    {
        public Aria2TaskInfoWindow()
        {
            InitializeComponent();
            this.Closed += (s, e) => (this.DataContext as Aria2TaskInfoViewModel)?.StopRefreshLoop();
        }
        protected override void IsVisibleChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.IsVisibleChanged(e);
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