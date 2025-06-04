using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    /// <summary>
    /// Замкнутый многоугольник (Polygon), реализующий IDrawableShape.
    /// </summary>
    public class PolygonShape : ShapeBase, IDrawableShape
    {
        public override string TypeName => "Polygon";

        private Polygon _previewPolygon;
        private bool _isDrawing = false;

        public override Shape CreateWpfShape()
        {
            // Создаём финальный замкнутый многоугольник с заливкой
            var polygon = new Polygon
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = new SolidColorBrush(FillColor),
                Points = new PointCollection(Points)
            };
            return polygon;
        }

        public override void UpdateFromWpfShape(Shape shape)
        {
            base.UpdateFromWpfShape(shape);
            if (shape is Polygon p)
            {
                Points = new PointCollection(p.Points);
            }
        }

        #region IDrawableShape

        public Shape CreatePreviewShape()
        {
            if (_previewPolygon == null)
            {
                _previewPolygon = new Polygon
                {
                    Stroke = new SolidColorBrush(StrokeColor),
                    StrokeThickness = StrokeThickness,
                    StrokeDashArray = new DoubleCollection { 4, 2 },
                    Fill = Brushes.Transparent,
                    Points = new PointCollection()
                };
            }
            return _previewPolygon;
        }

        public void StartDrawing(Point startPoint)
        {
            // Начинаем новую коллекцию вершин
            Points = new PointCollection { startPoint };
            _isDrawing = true;

            if (_previewPolygon != null)
            {
                _previewPolygon.Points = new PointCollection(Points);
            }
        }

        public void UpdateDrawing(Point currentPoint)
        {
            if (!_isDrawing) return;

            // Копируем уже зафиксированные вершины и добавляем текущую как «временную»
            var tempPoints = new List<Point>(Points);
            tempPoints.Add(currentPoint);

            if (_previewPolygon != null)
            {
                _previewPolygon.Points = new PointCollection(tempPoints);
            }
        }

        public bool CompleteDrawing(Point endPoint)
        {
            if (!_isDrawing) return false;

            // Левый клик: добавляем новую вершину в Points
            Points.Add(endPoint);

            return false;
        }

        public bool FinishOnRightClick()
        {
            _isDrawing = false;
            return true;
        }

        #endregion
    }
}
