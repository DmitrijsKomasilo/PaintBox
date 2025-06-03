using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    public class PolygonShape : ShapeBase
    {
        public override string TypeName => "Polygon";

        public override Shape CreateWpfShape()
        {
            var polygon = new Polygon
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = new SolidColorBrush(FillColor),
                Points = new PointCollection(Points) // копируем список точек
            };
            return polygon;
        }

        public override void UpdateFromWpfShape(Shape shape)
        {
            base.UpdateFromWpfShape(shape);

            if (shape is Polygon p)
            {
                Points = new PointCollection(p.Points);
                // Для Bounds можно вычислить минимальный bounding box, 
                // но в ЛР 2 это не обязательно.
            }
        }
    }
}
