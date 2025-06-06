namespace PaintBox.Interfaces
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Shapes;

    public interface IShape
    {
        string TypeName { get; }

        Color StrokeColor { get; set; }

        Color FillColor { get; set; }

        double StrokeThickness { get; set; }

        Rect Bounds { get; set; }

        PointCollection Points { get; set; }

        Shape CreateWpfShape();

        void UpdateFromWpfShape(Shape shape);
    }
}
