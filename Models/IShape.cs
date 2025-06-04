using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    /// Любая фигура, которую можно отрисовать на Canvas и сериализовать.
    public interface IShape
    {
        /// Уникальный идентификатор типа фигуры (например, "Line", "Rectangle", "Ellipse", "MyPluginStar" и т.д.).
        string TypeName { get; }

        /// Цвет обводки (Stroke).
        Color StrokeColor { get; set; }

        /// Толщина линии (StrokeThickness).
        double StrokeThickness { get; set; }

        /// Цвет заливки (Fill).
        Color FillColor { get; set; }

        /// Задает координаты и размер фигуры (можно интерпретировать по-разному).
        /// Например, для Line это будут X1, Y1, X2, Y2; для Rectangle – X, Y, Width, Height.
        Rect Bounds { get; set; }

        /// Список точек (для Polygon, Polyline). Если не используется, то может быть null или пустым.
        PointCollection Points { get; set; }

        /// Создать WPF-элемент (System.Windows.Shapes.Shape), который соответствует этой фигуре,
        /// с применением StrokeColor, FillColor, StrokeThickness и координат.
        /// <returns>Объект типа Shape (Line, Rectangle, Ellipse, Polygon, Polyline).</returns>
        Shape CreateWpfShape();

        /// <summary>
        /// Обновить внутренние параметры фигуры по данным из WPF-элемента (например, если была редактирование).
        /// </summary>
        void UpdateFromWpfShape(Shape shape);
    }
}
