using PaintBox.Interfaces;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    public class LineShape : ShapeBase, IDrawableShape
    {
        private Line _previewLine = new Line();
        private Point _startPoint;
        private Point _endPoint;

        public override string TypeName => "Line";

        public bool IsMultiStep => false;

        #region IDrawableShape

        public Shape CreatePreviewShape()
        {
            _previewLine = new Line
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                StrokeDashArray = new DoubleCollection { 4, 2 }
            };
            return _previewLine;
        }

        public void StartDrawing(Point startPoint)
        {
            _startPoint = startPoint;
            _previewLine.X1 = startPoint.X;
            _previewLine.Y1 = startPoint.Y;
            _previewLine.X2 = startPoint.X;
            _previewLine.Y2 = startPoint.Y;
        }

        public void UpdateDrawing(Point currentPoint)
        {
            _previewLine.X2 = currentPoint.X;
            _previewLine.Y2 = currentPoint.Y;
        }

        public bool CompleteDrawing(Point endPoint)
        {
            _endPoint = endPoint;

            _previewLine.X2 = endPoint.X;
            _previewLine.Y2 = endPoint.Y;

            var x1 = _startPoint.X;
            var y1 = _startPoint.Y;
            var x2 = _endPoint.X;
            var y2 = _endPoint.Y;
            Bounds = new Rect(
                Math.Min(x1, x2),
                Math.Min(y1, y2),
                Math.Abs(x2 - x1),
                Math.Abs(y2 - y1)
            );

            return true;
        }

        public bool FinishOnRightClick() => false;

        #endregion

        public override Shape CreateWpfShape()
        {
            var line = new Line
            {
                X1 = _startPoint.X,
                Y1 = _startPoint.Y,
                X2 = _endPoint.X,
                Y2 = _endPoint.Y,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness
            };
            return line;
        }

        public override void UpdateFromWpfShape(Shape shape)
        {
            var l = (Line)shape;

            _startPoint = new Point(l.X1, l.Y1);
            _endPoint = new Point(l.X2, l.Y2);

            Bounds = new Rect(
                Math.Min(_startPoint.X, _endPoint.X),
                Math.Min(_startPoint.Y, _endPoint.Y),
                Math.Abs(_endPoint.X - _startPoint.X),
                Math.Abs(_endPoint.Y - _startPoint.Y)
            );
        }
    }
}
