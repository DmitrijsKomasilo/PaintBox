using PaintBox.Interfaces;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    public abstract class ShapeBase : IShape
    {
        public abstract string TypeName { get; }

        public Color StrokeColor { get; set; }
        public Color FillColor { get; set; }
        public double StrokeThickness { get; set; }
        public Rect Bounds { get; set; } = new Rect(0, 0, 0, 0);
        public PointCollection Points { get; set; } = new PointCollection();

        public abstract Shape CreateWpfShape();

        public virtual void UpdateFromWpfShape(Shape shape)
        {

        }
    }
}
