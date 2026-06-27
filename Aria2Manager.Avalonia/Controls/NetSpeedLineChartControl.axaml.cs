using Aria2Manager.Avalonia.Localization;
using Aria2Manager.Core.Helpers;
using Aria2Manager.Core.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Skia;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Aria2Manager.Avalonia.Controls
{
    public partial class NetSpeedLineChartControl : UserControl
    {
        public static readonly StyledProperty<IReadOnlyCollection<NetSpeedData>> DatasProperty =
            AvaloniaProperty.Register<NetSpeedLineChartControl, IReadOnlyCollection<NetSpeedData>>(nameof(Datas), defaultBindingMode: BindingMode.OneWay);
        public static readonly StyledProperty<Color> DownloadColorProperty =
            AvaloniaProperty.Register<NetSpeedLineChartControl, Color>(nameof(DownloadColor), defaultValue: Colors.DodgerBlue,
                defaultBindingMode: BindingMode.OneWay);
        public static readonly StyledProperty<Color> UploadColorProperty =
            AvaloniaProperty.Register<NetSpeedLineChartControl, Color>(nameof(UploadColor), defaultValue: Colors.YellowGreen,
                defaultBindingMode: BindingMode.OneWay);
        public IReadOnlyCollection<NetSpeedData> Datas
        {
            get => GetValue(DatasProperty);
            set => SetValue(DatasProperty, value);
        }
        public Color DownloadColor
        {
            get => GetValue(DownloadColorProperty);
            set => SetValue(DownloadColorProperty, value);
        }
        public Color UploadColor
        {
            get => GetValue(UploadColorProperty);
            set => SetValue(UploadColorProperty, value);
        }
        private readonly LineSeries<NetSpeedData> _downloadSeries;
        private readonly LineSeries<NetSpeedData> _uploadSeries;
        public NetSpeedLineChartControl()
        {
            InitializeComponent();
            _downloadSeries = new LineSeries<NetSpeedData>
            {
                Name = AvaloniaLocalizer.Instance.GetString("Download_Speed"),
                Values = Datas,
                Mapping = (data, index) => new(data.Timestamp.Ticks, data.DownloadSpeed),
                Stroke = new SolidColorPaint(DownloadColor.ToSKColor()) { StrokeThickness = 2 },
                Fill = new SolidColorPaint(DownloadColor.ToSKColor().WithAlpha(40)),
                GeometryFill = null,
                GeometryStroke = null,
                GeometrySize = 0,
                LineSmoothness = 0.4
            };
            _uploadSeries = new LineSeries<NetSpeedData>
            {
                Name = AvaloniaLocalizer.Instance.GetString("Upload_Speed"),
                Values = Datas,
                Mapping = (data, index) => new(data.Timestamp.Ticks, data.UploadSpeed),
                Stroke = new SolidColorPaint(UploadColor.ToSKColor()) { StrokeThickness = 2 },
                Fill = new SolidColorPaint(UploadColor.ToSKColor().WithAlpha(40)),
                GeometryFill = null,
                GeometryStroke = null,
                GeometrySize = 0,
                LineSmoothness = 0.4
            };
            InnerChart.Series = new ISeries[] { _downloadSeries, _uploadSeries };
            InnerChart.XAxes = new Axis[]
            {
                new Axis
                {
                    Labeler = value => new DateTime((long)value).ToString("HH:mm:ss"),
                    LabelsRotation = 15,
                    CrosshairPaint = new SolidColorPaint(SKColors.Gray)
        {
            StrokeThickness = 1,
            PathEffect = new DashEffect(new float[] { 4, 4 }) // 4像素实线与4像素空白交替
        }
                }
            };

            InnerChart.YAxes = new Axis[]
            {
                new Axis { Labeler = BytesToString, MinLimit = 0 }
            };
        }
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == DatasProperty)
            {
                var newData = change.GetNewValue<IReadOnlyCollection<NetSpeedData>>();
                _downloadSeries.Values = newData;
                _uploadSeries.Values = newData;
            }
            else if (change.Property == DownloadColorProperty)
            {
                var skColor = change.GetNewValue<Color>().ToSKColor();
                if (_downloadSeries.Stroke is SolidColorPaint stroke)
                {
                    stroke.Color = skColor;
                }
                if (_downloadSeries.Fill is SolidColorPaint fill)
                {
                    fill.Color = skColor.WithAlpha(40);
                }
            }
            else if (change.Property == UploadColorProperty)
            {
                var skColor = change.GetNewValue<Color>().ToSKColor();
                if (_uploadSeries.Stroke is SolidColorPaint stroke)
                {
                    stroke.Color = skColor;
                }
                if (_uploadSeries.Fill is SolidColorPaint fill)
                {
                    fill.Color = skColor.WithAlpha(40);
                }
            }
        }
        private string BytesToString(double value)
        {
            return FormatterHelper.BytesToString((Int64)value);
        }
    }
}