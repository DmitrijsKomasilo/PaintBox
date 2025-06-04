using PaintBox.Interfaces;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    /// <summary>
    /// Линия. Реализует IDrawableShape.
    /// </summary>
    public class LineShape : ShapeBase, IDrawableShape
    {
        private Line _previewLine = new Line();

        public override string TypeName => "Line";

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
            UpdateDrawing(endPoint);
            Bounds = new Rect(
                Math.Min(_previewLine.X1, _previewLine.X2),
                Math.Min(_previewLine.Y1, _previewLine.Y2),
                Math.Abs(_previewLine.X2 - _previewLine.X1),
                Math.Abs(_previewLine.Y2 - _previewLine.Y1)
            );
            return true;
        }

        public bool FinishOnRightClick() => false;

        #endregion

        public override Shape CreateWpfShape()
        {

            var line = new Line
            {
                X1 = Bounds.X,
                Y1 = Bounds.Y,
                X2 = Bounds.X + Bounds.Width,
                Y2 = Bounds.Y + Bounds.Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness
            };
            return line;
        }

        public override void UpdateFromWpfShape(Shape shape)
        {

            var l = (Line)shape;
            Bounds = new Rect(
                Math.Min(l.X1, l.X2),
                Math.Min(l.Y1, l.Y2),
                Math.Abs(l.X2 - l.X1),
                Math.Abs(l.Y2 - l.Y1)
            );
        }
    }
}
