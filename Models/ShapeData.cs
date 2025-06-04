using System.Collections.Generic;

namespace PaintBox.Models
{
    /// <summary>
    /// DTO‐класс для сериализации любой фигуры в JSON.
    /// Мы храним:
    ///  • TypeName: имя класса (Line, Rectangle, Ellipse, Polygon, Polyline или плагин)
    ///  • StrokeColor, FillColor: в виде строк (например, "#FF0000FF")
    ///  • StrokeThickness: толщина контура
    ///  • BoundsX, BoundsY, BoundsWidth, BoundsHeight: геометрия для Line/Rect/Ellipse
    ///  • Points: список вершин (для Polyline/Polygon)
    /// </summary>
    public class ShapeData
    {
        public string TypeName { get; set; } = string.Empty;

        // Цвета в формате ARGB (#AARRGGBB)
        public string StrokeColor { get; set; } = "#FF000000";
        public string FillColor { get; set; } = "#00FFFFFF";

        public double StrokeThickness { get; set; }

        // Bounds: X=левый, Y=верхний, Width, Height
        public double BoundsX { get; set; }
        public double BoundsY { get; set; }
        public double BoundsWidth { get; set; }
        public double BoundsHeight { get; set; }

        // Список вершин, если это Polyline или Polygon (в формате [ [x1,y1], [x2,y2], ... ])
        public List<PointData>? Points { get; set; }
    }

    /// <summary>
    /// Вспомогательный класс для хранения одной точки (X, Y) в JSON.
    /// </summary>
    public class PointData
    {
        public double X { get; set; }
        public double Y { get; set; }

        public PointData() { }

        public PointData(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
