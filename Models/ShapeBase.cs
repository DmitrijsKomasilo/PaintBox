using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    /// <summary>
    /// Базовый абстрактный класс для фигур, реализующих IShape.
    /// Содержит общие свойства: цвет обводки, заливки, толщина, Bounds, Points.
    /// </summary>
    public abstract class ShapeBase : IShape
    {
        public abstract string TypeName { get; }

        public Color StrokeColor { get; set; } = Colors.Black;
        public double StrokeThickness { get; set; } = 2.0;
        public Color FillColor { get; set; } = Colors.Transparent;
        public Rect Bounds { get; set; } = new Rect(0, 0, 0, 0);
        public PointCollection Points { get; set; } = new PointCollection();

        /// <summary>
        /// Конструирует соответствующий объект WPF-Shape.
        /// </summary>
        public abstract Shape CreateWpfShape();

        public virtual void UpdateFromWpfShape(Shape shape)
        {
            // По умолчанию: обновляем лишь цвет и толщину
            if (shape == null) return;
            shape.Stroke = new SolidColorBrush(StrokeColor);
            shape.StrokeThickness = StrokeThickness;
            if (shape is System.Windows.Shapes.Shape s)
            {
                s.Fill = new SolidColorBrush(FillColor);
            }
        }
    }
}
