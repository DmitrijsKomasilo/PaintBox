using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    public class PolylineShape : ShapeBase
    {
        public override string TypeName => "Polyline";

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
            base.UpdateFromWpfShape(shape);

            if (shape is Polyline p)
            {
                Points = new PointCollection(p.Points);
            }
        }
    }
}
