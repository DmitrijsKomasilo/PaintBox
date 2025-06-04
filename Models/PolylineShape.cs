using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    /// <summary>
    /// Ломаная (Polyline), реализующая IDrawableShape.
    /// ЛКМ добавляет вершину, а РКМ завершает отрисовку.
    /// </summary>
    public class PolylineShape : ShapeBase, IDrawableShape
    {
        public override string TypeName => "Polyline";

        private Polyline _previewPolyline;
        private bool _isDrawing = false;

        public override Shape CreateWpfShape()
        {
            // Создаём финальную ломанаю (Polyline)
            var polyline = new Polyline
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = Brushes.Transparent,
                Points = new PointCollection(Points)
            };
            return polyline;
        }

        public override void UpdateFromWpfShape(Shape shape)
        {
            base.UpdateFromWpfShape(shape);
            if (shape is Polyline p)
            {
                Points = new PointCollection(p.Points);
            }
        }

        #region IDrawableShape

        public Shape CreatePreviewShape()
        {
            if (_previewPolyline == null)
            {
                _previewPolyline = new Polyline
                {
                    Stroke = new SolidColorBrush(StrokeColor),
                    StrokeThickness = StrokeThickness,
                    StrokeDashArray = new DoubleCollection { 4, 2 },
                    Points = new PointCollection()
                };
            }
            return _previewPolyline;
        }

        public void StartDrawing(Point startPoint)
        {
            // Начинаем новый список вершин
            Points = new PointCollection { startPoint };
            _isDrawing = true;

            if (_previewPolyline != null)
            {
                _previewPolyline.Points = new PointCollection(Points);
            }
        }

        public void UpdateDrawing(Point currentPoint)
        {
            if (!_isDrawing) return;

            // Временные точки: уже зафиксированные + текущий
            var tempPoints = new List<Point>(Points);
            tempPoints.Add(currentPoint);

            if (_previewPolyline != null)
            {
                _previewPolyline.Points = new PointCollection(tempPoints);
            }
        }

        public bool CompleteDrawing(Point endPoint)
        {
            if (!_isDrawing) return false;

            // Левый клик: добавляем вершину
            Points.Add(endPoint);

            return false;
        }

        /// Вызывается из MainWindow при правом клике, чтобы «закончить» ломаную.
        public bool FinishOnRightClick()
        {
            _isDrawing = false;
            return true;
        }

        #endregion
    }
}
