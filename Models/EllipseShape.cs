using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace PaintBox.Models
{
    public class EllipseShape : ShapeBase
    {
        public override string TypeName => "Ellipse";

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
    }
}
