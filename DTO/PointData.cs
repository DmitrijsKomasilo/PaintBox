namespace PaintBox.DTO
{
    /// <summary>
    /// Простая структура для одной точки: X, Y.
    /// Используется внутри ShapeData.Points.
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
    