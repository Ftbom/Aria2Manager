using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using System;

namespace Aria2Manager.Avalonia.Controls
{
    public class PieceMapControl : Control
    {
        public static readonly StyledProperty<bool[]> PiecesProperty =
            AvaloniaProperty.Register<PieceMapControl, bool[]>(nameof(Pieces), defaultBindingMode: BindingMode.OneWay);
        public static readonly StyledProperty<IBrush> CompletedBrushProperty =
            AvaloniaProperty.Register<PieceMapControl, IBrush>(nameof(CompletedBrush), defaultBindingMode: BindingMode.OneWay,
                defaultValue: Brushes.LimeGreen);
        public static readonly StyledProperty<IBrush> PendingBrushProperty =
            AvaloniaProperty.Register<PieceMapControl, IBrush>(nameof(PendingBrush), defaultBindingMode: BindingMode.OneWay,
                defaultValue: Brushes.LightGray);
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
            //绘制底色
            var pieces = Pieces;
            if (pieces == null || pieces.Length == 0) { return; }
            int totalPieces = pieces.Length;
            //最小方块尺寸
            const double MIN_BLOCK_SIZE = 10;
            //计算列数和行数
            int cols = (int)Math.Min(Math.Ceiling(Math.Sqrt(totalPieces * (width / height))), totalPieces);
            int rows = (int)Math.Ceiling((double)totalPieces / cols);
            //检查算方块尺寸
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
            //每个方块代表的Piece数
            double piecesPerCell = (double)totalPieces / totalGridCells;
            for (int i = 0; i < totalGridCells; i++)
            {
                //计算起止索引
                int startIndex = (int)Math.Floor(i * piecesPerCell);
                int endIndex = Math.Min(totalPieces, (int)Math.Floor((i + 1) * piecesPerCell));
                if (startIndex >= endIndex) { continue; }
                //如果包含已完成的块，按比例绘制
                int completedCount = 0;
                for (int p = startIndex; p < endIndex; p++)
                {
                    if (pieces[p])
                    {
                        completedCount++;
                    }
                }
                int rangeLength = endIndex - startIndex;
                var rect = new Rect(
                    (i % cols) * blockWidth,
                    (i / cols) * blockHeight,
                    Math.Max(0.1, blockWidth - 0.2), //留出微小缝隙
                    Math.Max(0.1, blockHeight - 0.2)
                );
                var dynamicBrush = PendingBrush;
                if (completedCount > 0)
                {
                    double completionRatio = (double)completedCount / rangeLength;
                    //如果只完成一部分，半透明绘制
                    Color baseColor = (CompletedBrush is ISolidColorBrush scb) ? scb.Color : Colors.LimeGreen;
                    var alphaColor = Color.FromArgb((byte)(completionRatio * 255), baseColor.R, baseColor.G, baseColor.B);
                    dynamicBrush = new SolidColorBrush(alphaColor);
                }
                context.DrawRectangle(dynamicBrush, null, rect);
            }
        }
    }
}