using PaintBox.Interfaces;
using PaintBox.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TrapezoidPlugin.Models
{
    public class TrapezoidShape : ShapeBase, IDrawableShape
    {
        private Polygon _preview;
        private Point _start;
        private Point _end;

        public override string TypeName => "Trapezoid";
        public bool IsMultiStep => false;

        #region IDrawableShape

        public Shape CreatePreviewShape()
        {
            if (_preview == null)
            {
                _preview = new Polygon
                {
                    Stroke = new SolidColorBrush(StrokeColor),
                    StrokeThickness = StrokeThickness,
                    StrokeDashArray = new DoubleCollection { 4, 2 },
                    Fill = Brushes.Transparent,
                    Points = new PointCollection()
                };
            }
            return _preview;
        }

        public void StartDrawing(Point startPoint)
        {
            _start = startPoint;
            _preview.Points = new PointCollection { startPoint };
        }

        public void UpdateDrawing(Point currentPoint)
        {
            _end = currentPoint;
            double dx = Math.Abs(_end.X - _start.X);
            double dy = Math.Abs(_start.Y - _end.Y);
            double halfB = dx;
            double halfT = dx / 2;

            var pts = new List<Point>
            {
                new Point(_start.X - halfB, _start.Y),
                new Point(_start.X + halfB, _start.Y),
                new Point(_start.X + halfT, _start.Y - dy),
                new Point(_start.X - halfT, _start.Y - dy)
            };

            _preview.Points = new PointCollection(pts);
        }

        public bool CompleteDrawing(Point endPoint)
        {
            UpdateDrawing(endPoint);

            Bounds = CalculateBounds(_preview.Points);
            return true;
        }

        public bool FinishOnRightClick() => false;

        #endregion

        public override Shape CreateWpfShape()
        {
            return new Polygon
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = new SolidColorBrush(FillColor),
                Points = new PointCollection(_preview.Points)
            };
        }

        public override void UpdateFromWpfShape(Shape shape)
        {
            var p = (Polygon)shape;
            Points = new PointCollection(p.Points);
            Bounds = CalculateBounds(Points);
        }

        private static Rect CalculateBounds(PointCollection pts)
        {
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            foreach (var pt in pts)
            {
                if (pt.X < minX) minX = pt.X;
                if (pt.Y < minY) minY = pt.Y;
                if (pt.X > maxX) maxX = pt.X;
                if (pt.Y > maxY) maxY = pt.Y;
            }
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
