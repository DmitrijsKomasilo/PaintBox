using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    /// <summary>
    /// Любая фигура, которую можно отрисовать на Canvas и сериализовать.
    /// </summary>
    public interface IShape
    {
        /// <summary>
        /// Уникальный идентификатор типа фигуры (например, "Line", "Rectangle", "Ellipse", "MyPluginStar" и т.д.).
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Цвет обводки (Stroke).
        /// </summary>
        Color StrokeColor { get; set; }

        /// <summary>
        /// Толщина линии (StrokeThickness).
        /// </summary>
        double StrokeThickness { get; set; }

        /// <summary>
        /// Цвет заливки (Fill). Может быть Transparent, если без заливки.
        /// </summary>
        Color FillColor { get; set; }

        /// <summary>
        /// Задает координаты и размер фигуры (можно интерпретировать по-разному).
        /// Например, для Line это будут X1, Y1, X2, Y2; для Rectangle – X, Y, Width, Height.
        /// </summary>
        /// <remarks>
        /// В простейшем виде можно сделать объект Rect, описывающий bounding box.
        /// </remarks>
        Rect Bounds { get; set; }

        /// <summary>
        /// Список точек (для Polygon, Polyline). Если не используется, то может быть null или пустым.
        /// </summary>
        PointCollection Points { get; set; }

        /// <summary>
        /// Создать WPF-элемент (System.Windows.Shapes.Shape), который соответствует этой фигуре,
        /// с применением StrokeColor, FillColor, StrokeThickness и координат.
        /// </summary>
        /// <returns>Объект типа Shape (Line, Rectangle, Ellipse, Polygon, Polyline).</returns>
        Shape CreateWpfShape();

        /// <summary>
        /// Обновить внутренние параметры фигуры по данным из WPF-элемента (например, если была редактирование).
        /// </summary>
        void UpdateFromWpfShape(Shape shape);
    }
}
