using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    /// <summary>
    /// Прямоугольник, реализующий IDrawableShape.
    /// </summary>
    public class RectangleShape : ShapeBase, IDrawableShape
    {
        public override string TypeName => "Rectangle";

        private Rectangle _previewRect;
        private Point _startPoint;

        public override Shape CreateWpfShape()
        {
            // Создаём финальную фигуру (сплошной контур + заливка)
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
            base.UpdateFromWpfShape(shape);
            if (shape is Rectangle r)
            {
                double x = Canvas.GetLeft(r);
                double y = Canvas.GetTop(r);
                Bounds = new Rect(x, y, r.Width, r.Height);
            }
        }

        #region IDrawableShape

        public Shape CreatePreviewShape()
        {
            if (_previewRect == null)
            {
                _previewRect = new Rectangle
                {
                    Stroke = new SolidColorBrush(StrokeColor),
                    StrokeThickness = StrokeThickness,
                    StrokeDashArray = new DoubleCollection { 4, 2 },
                    Fill = Brushes.Transparent
                };
            }
            return _previewRect;
        }

        public void StartDrawing(Point startPoint)
        {
            _startPoint = startPoint;
            Bounds = new Rect(startPoint.X, startPoint.Y, 0, 0);

            if (_previewRect != null)
            {
                _previewRect.Width = 0;
                _previewRect.Height = 0;
                Canvas.SetLeft(_previewRect, startPoint.X);
                Canvas.SetTop(_previewRect, startPoint.Y);
            }
        }

        public void UpdateDrawing(Point currentPoint)
        {
            // Вычисляем «левый верхний угол» и размеры по модулю
            double x = Math.Min(_startPoint.X, currentPoint.X);
            double y = Math.Min(_startPoint.Y, currentPoint.Y);
            double width = Math.Abs(currentPoint.X - _startPoint.X);
            double height = Math.Abs(currentPoint.Y - _startPoint.Y);

            Bounds = new Rect(x, y, width, height);

            if (_previewRect != null)
            {
                Canvas.SetLeft(_previewRect, x);
                Canvas.SetTop(_previewRect, y);
                _previewRect.Width = width;
                _previewRect.Height = height;
            }
        }

        public bool CompleteDrawing(Point endPoint)
        {

            UpdateDrawing(endPoint);
            return true;
        }

        #endregion
    }
}
