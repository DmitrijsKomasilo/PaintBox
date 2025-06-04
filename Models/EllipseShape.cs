using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    /// <summary>
    /// Эллипс, реализующий IDrawableShape.
    /// Поддерживает рисование «в любом направлении» аналогично RectangleShape.
    /// </summary>
    public class EllipseShape : ShapeBase, IDrawableShape
    {
        public override string TypeName => "Ellipse";

        private Ellipse _previewEllipse;
        private Point _startPoint;

        public override Shape CreateWpfShape()
        {
            var ellipse = new Ellipse
            {
                Width = Bounds.Width,
                Height = Bounds.Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = new SolidColorBrush(FillColor)
            };
            Canvas.SetLeft(ellipse, Bounds.X);
            Canvas.SetTop(ellipse, Bounds.Y);
            return ellipse;
        }

        public override void UpdateFromWpfShape(Shape shape)
        {
            base.UpdateFromWpfShape(shape);
            if (shape is Ellipse e)
            {
                double x = Canvas.GetLeft(e);
                double y = Canvas.GetTop(e);
                Bounds = new Rect(x, y, e.Width, e.Height);
            }
        }

        #region IDrawableShape

        public Shape CreatePreviewShape()
        {
            if (_previewEllipse == null)
            {
                _previewEllipse = new Ellipse
                {
                    Stroke = new SolidColorBrush(StrokeColor),
                    StrokeThickness = StrokeThickness,
                    StrokeDashArray = new DoubleCollection { 4, 2 },
                    Fill = Brushes.Transparent
                };
            }
            return _previewEllipse;
        }

        public void StartDrawing(Point startPoint)
        {
            _startPoint = startPoint;
            Bounds = new Rect(startPoint.X, startPoint.Y, 0, 0);

            if (_previewEllipse != null)
            {
                _previewEllipse.Width = 0;
                _previewEllipse.Height = 0;
                Canvas.SetLeft(_previewEllipse, startPoint.X);
                Canvas.SetTop(_previewEllipse, startPoint.Y);
            }
        }

        public void UpdateDrawing(Point currentPoint)
        {
            double x = Math.Min(_startPoint.X, currentPoint.X);
            double y = Math.Min(_startPoint.Y, currentPoint.Y);
            double width = Math.Abs(currentPoint.X - _startPoint.X);
            double height = Math.Abs(currentPoint.Y - _startPoint.Y);

            Bounds = new Rect(x, y, width, height);

            if (_previewEllipse != null)
            {
                Canvas.SetLeft(_previewEllipse, x);
                Canvas.SetTop(_previewEllipse, y);
                _previewEllipse.Width = width;
                _previewEllipse.Height = height;
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
