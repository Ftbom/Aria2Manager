using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Aria2Manager.Core.Enums;

namespace Aria2manager.Avalonia.Converters
{
    public class TaskTagConverter : IValueConverter
    {
        public static readonly TaskTagConverter Instance = new();
        // viewmodel->view
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        // view->viewmodel
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ListBoxItem lbi)
            {
                return lbi.Tag?.ToString();
            }
            return null;
        }
    }
}