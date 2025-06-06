using PaintBox.Interfaces;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    public class PolylineShape : ShapeBase, IDrawableShape
    {
        private Polyline _previewPolyline = new Polyline();
        private bool _isDrawing;

        public override string TypeName => "Polyline";

        public bool IsMultiStep => true;

        #region IDrawableShape

        public Shape CreatePreviewShape()
        {
            _previewPolyline = new Polyline
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                Points = new PointCollection()
            };
            return _previewPolyline;
        }

        public void StartDrawing(Point startPoint)
        {
            Points = new PointCollection { startPoint };
            _previewPolyline.Points = new PointCollection(Points);
            _isDrawing = true;
        }

        public void UpdateDrawing(Point currentPoint)
        {
            if (!_isDrawing || Points == null) return;

            var pts = new PointCollection(Points)
            {
                currentPoint
            };
            _previewPolyline.Points = pts;
        }

        public bool CompleteDrawing(Point endPoint)
        {
            if (!_isDrawing) return false;

            Points.Add(endPoint);
            _previewPolyline.Points = new PointCollection(Points);
            return false;
        }

        public bool FinishOnRightClick()
        {
            if (!_isDrawing) return false;

            _isDrawing = false;
            Bounds = CalculateBounds(Points);
            return true;
        }

        #endregion

        public override Shape CreateWpfShape()
        {
            var polyline = new Polyline
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Points = new PointCollection(Points)
            };
            return polyline;
        }

        public override void UpdateFromWpfShape(Shape shape)
        {
            var pl = (Polyline)shape;
            Points = new PointCollection(pl.Points);
            Bounds = CalculateBounds(Points);
        }

        private static Rect CalculateBounds(PointCollection pts)
        {
            if (pts == null || pts.Count == 0) return new Rect();

            double minX = pts[0].X, maxX = pts[0].X;
            double minY = pts[0].Y, maxY = pts[0].Y;

            foreach (var pt in pts)
            {
                if (pt.X < minX) minX = pt.X;
                if (pt.X > maxX) maxX = pt.X;
                if (pt.Y < minY) minY = pt.Y;
                if (pt.Y > maxY) maxY = pt.Y;
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
