namespace PaintBox.DTO
{
    /// <summary>
    /// DTO-класс для сериализации (JSON) любой фигуры.
    /// Один класс — один файл.
    /// </summary>
    public class ShapeData
    {
        public string TypeName { get; set; } = string.Empty;
        public string StrokeColor { get; set; } = "#FF000000";  // ARGB
        public string FillColor { get; set; } = "#00FFFFFF";  // ARGB
        public double StrokeThickness { get; set; }
        public double BoundsX { get; set; }
        public double BoundsY { get; set; }
        public double BoundsWidth { get; set; }
        public double BoundsHeight { get; set; }

        /// <summary>Список точек (вершин) для Polyline/Polygon и подобных.</summary>
        public List<PointData>? Points { get; set; }
    }
}
