using Aria2Manager.Core;
using Aria2Manager.Core.ViewModels;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Input;
using System;

namespace Aria2Manager.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        private bool _isExiting = false;
        private bool _firstLoad = true;
        private DataGridCollectionView? _collectionView;
        public MainWindow()
        {
            InitializeComponent();
            //确保窗口彻底关闭时停止轮询
            this.Closed += (s, e) => (this.DataContext as MainViewModel)?.StopRefreshLoop();
            this.Closing += MainWindow_Closing;
            this.Loaded += MainWindow_Loaded;
            this.DataContextChanged += OnDataContextChanged;
        }
        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            if (_firstLoad)
            {
                _isExiting = false;
                if (GlobalContext.Instance.AppSettings.StartMin)
                {
                    this.Hide(); //启动时隐藏窗口
                }
            }
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
        private async void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            if (!GlobalContext.Instance.AppSettings.CloseToExit)
            {
                e.Cancel = true; //阻止销毁窗口资源
                this.Hide(); //仅在视觉上隐藏窗口
                return;
            }
            if (!_isExiting)
            {
                e.Cancel = true; //保证进程不会直接退出，等待命令执行完成后再关闭
                this.Hide();
                _isExiting = true; //保证Shutdown时能正常退出
                if (this.DataContext is MainViewModel viewModel)
                {
                    await viewModel.CloseCommand.ExecuteAsync(null);
                }
            }
        }
        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (this.DataContext is MainViewModel coreVm)
            {
                _collectionView = new DataGridCollectionView(coreVm.Aria2TaskCollection);
                //设置过滤逻辑
                _collectionView.Filter = taskObj =>
                {
                    var searchText = FilterTextBox.Text?.Trim();
                    if (string.IsNullOrWhiteSpace(searchText))
                        return true;
                    if (taskObj is TaskViewModel task && task.Name != null)
                    {
                        return task.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                    }
                    return false;
                };
                //绑定过滤后的视图到DataGrid
                Dispatcher.UIThread.Post(() =>
                {
                    TaskList.ItemsSource = _collectionView;
                });
            }
        }
        private void FilterText_Changed(object? sender, TextChangedEventArgs e)
        {
            //每次文本变化时刷新过滤结果
            _collectionView?.Refresh();
        }
        private void TaskList_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (this.DataContext is MainViewModel coreVm)
            {
                coreVm.OpenAria2TaskInfoCommand.Execute(null);
            }
        }
    }
}