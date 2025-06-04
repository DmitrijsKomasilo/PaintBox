namespace PaintBox.Interfaces
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Shapes;

    /// <summary>
    /// Базовый интерфейс для любой фигуры.
    /// </summary>
    public interface IShape
    {
        /// <summary>Уникальное имя типа (напр. "Rectangle", "Ellipse", "Trapezoid").</summary>
        string TypeName { get; }

        /// <summary>Цвет обводки (Stroke).</summary>
        Color StrokeColor { get; set; }

        /// <summary>Цвет заливки (Fill).</summary>
        Color FillColor { get; set; }

        /// <summary>Толщина линии (StrokeThickness).</summary>
        double StrokeThickness { get; set; }

        /// <summary>Границы фигуры (Bounds: X, Y, Width, Height).</summary>
        Rect Bounds { get; set; }

        /// <summary>Для многоугольников и ломаных — коллекция вершин.</summary>
        PointCollection Points { get; set; }

        /// <summary>Создаёт финальный WPF-элемент (Rectangle, Ellipse, Polygon…) для добавления на Canvas.</summary>
        Shape CreateWpfShape();

        /// <summary>Восстанавливает данные (Bounds, Points и т.д.) из уже существующего WPF-Shape.</summary>
        void UpdateFromWpfShape(Shape shape);
    }
}
