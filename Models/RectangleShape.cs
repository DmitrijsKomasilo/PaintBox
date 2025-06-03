using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace PaintBox.Models
{
    public class RectangleShape : ShapeBase
    {
        public override string TypeName => "Rectangle";

        public override Shape CreateWpfShape()
        {
            var rect = new Rectangle
            {
                Width = Bounds.Width,
                Height = Bounds.Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = new SolidColorBrush(FillColor)
            };
            // Задаём позицию через Canvas.SetLeft/Top:
            Canvas.SetLeft(rect, Bounds.X);
            Canvas.SetTop(rect, Bounds.Y);
            return rect;
        }

        public override void UpdateFromWpfShape(Shape shape)
        {
            base.UpdateFromWpfShape(shape);

            if (shape is Rectangle r)
            {
                // Получаем координаты, предполагая, что Canvas.SetLeft/Top заданы:
                double x = Canvas.GetLeft(r);
                double y = Canvas.GetTop(r);
                Bounds = new Rect(x, y, r.Width, r.Height);
            }
        }
    }
}
