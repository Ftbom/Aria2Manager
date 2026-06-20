using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using System;

namespace Aria2Manager.Avalonia.Controls
{
    public enum PieceMapMode
    {
        Grid, //网格模式
        ProgressBar //进度条模式
    }
    public class PieceMapControl : Control
    {
        public static readonly StyledProperty<PieceMapMode> DisplayModeProperty =
            AvaloniaProperty.Register<PieceMapControl, PieceMapMode>(nameof(DisplayMode), defaultValue: PieceMapMode.Grid,
                defaultBindingMode: BindingMode.OneWay);
        public static readonly StyledProperty<bool[]> PiecesProperty =
            AvaloniaProperty.Register<PieceMapControl, bool[]>(nameof(Pieces), defaultBindingMode: BindingMode.OneWay);
        public static readonly StyledProperty<IBrush> CompletedBrushProperty =
            AvaloniaProperty.Register<PieceMapControl, IBrush>(nameof(CompletedBrush), defaultBindingMode: BindingMode.OneWay,
                defaultValue: Brushes.LimeGreen);
        public static readonly StyledProperty<IBrush> PendingBrushProperty =
            AvaloniaProperty.Register<PieceMapControl, IBrush>(nameof(PendingBrush), defaultBindingMode: BindingMode.OneWay,
                defaultValue: Brushes.LightGray);
        public PieceMapMode DisplayMode
        {
            get => GetValue(DisplayModeProperty);
            set => SetValue(DisplayModeProperty, value);
        }
        public bool[] Pieces
        {
            get => GetValue(PiecesProperty);
            set => SetValue(PiecesProperty, value);
        }
        public IBrush CompletedBrush
        {
            get => GetValue(CompletedBrushProperty);
            set => SetValue(CompletedBrushProperty, value);
        }
        public IBrush PendingBrush
        {
            get => GetValue(PendingBrushProperty);
            set => SetValue(PendingBrushProperty, value);
        }
        static PieceMapControl()
        {
            //当Pieces、CompletedBrush或PendingBrush发生变化时，触发重绘
            AffectsRender<PieceMapControl>(PiecesProperty, CompletedBrushProperty, PendingBrushProperty);
        }
        public override void Render(DrawingContext context)
        {
            double width = Bounds.Width;
            double height = Bounds.Height;
            if (width <= 0 || height <= 0) { return; }
            var pieces = Pieces;
            if (pieces == null || pieces.Length == 0) { return; }
            if (DisplayMode == PieceMapMode.Grid)
            {
                RenderGridMode(context, width, height, pieces);
            }
            else
            {
                RenderProgressBarMode(context, width, height, pieces);
            }
        }
        private void RenderGridMode(DrawingContext context, double width, double height, bool[] pieces)
        {
            int totalPieces = pieces.Length;
            //最小方块尺寸
            const double MIN_BLOCK_SIZE = 10;
            //计算列数和行数
            int cols = (int)Math.Min(Math.Ceiling(Math.Sqrt(totalPieces * (width / height))), totalPieces);
            int rows = (int)Math.Ceiling((double)totalPieces / cols);
            if (width / cols < MIN_BLOCK_SIZE || height / rows < MIN_BLOCK_SIZE)
            {
                cols = (int)Math.Floor(width / MIN_BLOCK_SIZE);
                rows = (int)Math.Floor(height / MIN_BLOCK_SIZE);
                if (cols <= 0) { cols = 1; }
                if (rows <= 0) { rows = 1; }
            }
            //方块大小及总数
            double blockWidth = width / cols;
            double blockHeight = height / rows;
            int totalGridCells = Math.Min(cols * rows, totalPieces);
            for (int i = 0; i < totalGridCells; i++)
            {
                var rect = new Rect(
                    (i % cols) * blockWidth,
                    (i / cols) * blockHeight,
                    Math.Max(0.1, blockWidth - 0.2), //留出微小缝隙
                    Math.Max(0.1, blockHeight - 0.2)
                );
                DrawPiece(context, rect, i, totalGridCells, pieces);
            }
        }
        private void RenderProgressBarMode(DrawingContext context, double width, double height, bool[] pieces)
        {
            int totalPieces = pieces.Length;
            //最小块宽度
            const double MIN_BLOCK_WIDTH = 2;
            //计算列数
            int totalCells = (int)Math.Min(1000, totalPieces);
            //计算块尺寸
            if (width / totalCells < MIN_BLOCK_WIDTH)
            {
                totalCells = (int)Math.Floor(width / MIN_BLOCK_WIDTH);
                if (totalCells <= 0) { totalCells = 1; }
            }
            //块大小
            double blockWidth = width / totalCells;
            for (int i = 0; i < totalCells; i++)
            {
                var rect = new Rect(i * blockWidth, 0, Math.Max(0.1, blockWidth - 0.2), Height);
                DrawPiece(context, rect, i, totalCells, pieces);
            }
        }
        private void DrawPiece(DrawingContext context, Rect rect, int cellIndex, int totalCells, bool[] pieces)
        {
            int totalPieces = pieces.Length;
            //每个块代表的Piece数
            double piecesPerCell = (double)totalPieces / totalCells;
            //计算起止索引
            int startIndex = (int)Math.Floor(cellIndex * piecesPerCell);
            int endIndex = Math.Min(totalPieces, (int)Math.Floor((cellIndex + 1) * piecesPerCell));
            if (startIndex >= endIndex) { return; }
            //如果包含已完成的块，按比例绘制
            int completedCount = 0;
            for (int p = startIndex; p < endIndex; p++)
            {
                if (pieces[p])
                {
                    completedCount++;
                }
            }
            context.DrawRectangle(PendingBrush, null, rect); //绘制底色
            if (completedCount > 0)
            {
                double completionRatio = (double)completedCount / (endIndex - startIndex);
                //如果只完成一部分，半透明绘制
                Color baseColor = (CompletedBrush is ISolidColorBrush scb) ? scb.Color : Colors.LimeGreen;
                var alphaColor = Color.FromArgb((byte)(completionRatio * 255), baseColor.R, baseColor.G, baseColor.B);
                var dynamicBrush = new SolidColorBrush(alphaColor);
                context.DrawRectangle(dynamicBrush, null, rect);
            }
        }
    }
}