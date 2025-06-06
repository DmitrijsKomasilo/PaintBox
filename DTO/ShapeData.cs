namespace PaintBox.DTO
{
    public class ShapeData
    {
        public string TypeName { get; set; } = string.Empty;
        public string StrokeColor { get; set; } = "#FF000000"; 
        public string FillColor { get; set; } = "#00FFFFFF";
        public double StrokeThickness { get; set; }
        public double BoundsX { get; set; }
        public double BoundsY { get; set; }
        public double BoundsWidth { get; set; }
        public double BoundsHeight { get; set; }

        public List<PointData>? Points { get; set; }
    }
}
