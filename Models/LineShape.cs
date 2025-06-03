using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    public class LineShape : ShapeBase
    {
        public override string TypeName => "Line";

        /// <summary>
        /// Для Line мы храним Bounds так: Bounds.X = X1, Bounds.Y = Y1, Bounds.Width = X2, Bounds.Height = Y2.
        /// </summary>
        public override Shape CreateWpfShape()
        {
            var line = new Line
            {
                X1 = Bounds.X,
                Y1 = Bounds.Y,
                X2 = Bounds.Width,
                Y2 = Bounds.Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness
            };
            return line;
        }

        public override void UpdateFromWpfShape(Shape shape)
        {
            base.UpdateFromWpfShape(shape);

            if (shape is Line line)
            {
                Bounds = new Rect(line.X1, line.Y1, line.X2, line.Y2);
            }
        }
    }
}
