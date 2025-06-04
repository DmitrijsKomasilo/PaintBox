using PaintBox.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    /// <summary>
    /// Прямоугольник. Реализует IDrawableShape.
    /// </summary>
    public class RectangleShape : ShapeBase, IDrawableShape
    {
        private Rectangle _previewRect = new Rectangle();
        private Point _startPoint;

        public override string TypeName => "Rectangle";

        #region IDrawableShape

        public Shape CreatePreviewShape()
        {
            _previewRect = new Rectangle
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                Fill = Brushes.Transparent
            };
            return _previewRect;
        }

        public void StartDrawing(Point startPoint)
        {
            _startPoint = startPoint;
            _previewRect.Width = 0;
            _previewRect.Height = 0;
            Canvas.SetLeft(_previewRect, startPoint.X);
            Canvas.SetTop(_previewRect, startPoint.Y);
        }

        public void UpdateDrawing(Point currentPoint)
        {
            double x = Math.Min(currentPoint.X, _startPoint.X);
            double y = Math.Min(currentPoint.Y, _startPoint.Y);
            double w = Math.Abs(currentPoint.X - _startPoint.X);
            double h = Math.Abs(currentPoint.Y - _startPoint.Y);

            Canvas.SetLeft(_previewRect, x);
            Canvas.SetTop(_previewRect, y);
            _previewRect.Width = w;
            _previewRect.Height = h;
        }

        public bool CompleteDrawing(Point endPoint)
        {
            UpdateDrawing(endPoint);
            Bounds = new Rect(
                Canvas.GetLeft(_previewRect),
                Canvas.GetTop(_previewRect),
                _previewRect.Width,
                _previewRect.Height
            );
            return true;
        }

        public bool FinishOnRightClick() => false;

        #endregion

        public override Shape CreateWpfShape()
        {
            var rect = new Rectangle
            {
                Width = Bounds.Width,
                Height = Bounds.Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = new SolidColorBrush(FillColor)
            };
            Canvas.SetLeft(rect, Bounds.X);
            Canvas.SetTop(rect, Bounds.Y);
            return rect;
        }

        public override void UpdateFromWpfShape(Shape shape)
        {
            var r = (Rectangle)shape;
            Bounds = new Rect(
                Canvas.GetLeft(r),
                Canvas.GetTop(r),
                r.Width,
                r.Height
            );
        }
    }
}
