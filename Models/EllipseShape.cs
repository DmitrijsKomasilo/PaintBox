using PaintBox.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    /// <summary>
    /// Эллипс. Реализует IDrawableShape.
    /// </summary>
    public class EllipseShape : ShapeBase, IDrawableShape
    {
        private Ellipse _previewEllipse = new Ellipse();
        private Point _startPoint;

        public override string TypeName => "Ellipse";

        #region IDrawableShape

        public Shape CreatePreviewShape()
        {
            _previewEllipse = new Ellipse
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                Fill = Brushes.Transparent
            };
            return _previewEllipse;
        }

        public void StartDrawing(Point startPoint)
        {
            _startPoint = startPoint;
            _previewEllipse.Width = 0;
            _previewEllipse.Height = 0;
            Canvas.SetLeft(_previewEllipse, startPoint.X);
            Canvas.SetTop(_previewEllipse, startPoint.Y);
        }

        public void UpdateDrawing(Point currentPoint)
        {
            double x = System.Math.Min(currentPoint.X, _startPoint.X);
            double y = System.Math.Min(currentPoint.Y, _startPoint.Y);
            double w = System.Math.Abs(currentPoint.X - _startPoint.X);
            double h = System.Math.Abs(currentPoint.Y - _startPoint.Y);

            Canvas.SetLeft(_previewEllipse, x);
            Canvas.SetTop(_previewEllipse, y);
            _previewEllipse.Width = w;
            _previewEllipse.Height = h;
        }

        public bool CompleteDrawing(Point endPoint)
        {
            UpdateDrawing(endPoint);
            Bounds = new Rect(
                Canvas.GetLeft(_previewEllipse),
                Canvas.GetTop(_previewEllipse),
                _previewEllipse.Width,
                _previewEllipse.Height
            );
            return true;
        }

        public bool FinishOnRightClick() => false;

        #endregion

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

            var e = (Ellipse)shape;
            Bounds = new Rect(
                Canvas.GetLeft(e),
                Canvas.GetTop(e),
                e.Width,
                e.Height
            );
        }
    }
}
