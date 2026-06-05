using Aria2Manager.Core.ViewModels;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Aria2manager.Avalonia.Converters
{
    public class TaskGidConverter : IValueConverter
    {
        public static readonly TaskGidConverter Instance = new();
        // viewmodel->view
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        // view->viewmodel
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TaskViewModel task)
            {
                return task.Gid;
            }
            return null;
        }
    }
}