using Aria2Manager.Core;
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
            this.Closing += MainWindow_Closing;
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
        private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                if (viewModel.CloseCommand.CanExecute(null))
                {
                    viewModel.CloseCommand.Execute(null);
                }
                if (!GlobalContext.Instance.AppSettings.CloseToExit)
                {
                    e.Cancel = true; //阻止销毁窗口资源
                    this.Hide(); //仅在视觉上隐藏窗口
                }
            }
        }
    }
}