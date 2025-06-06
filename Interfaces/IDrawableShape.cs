using System.Windows;
using System.Windows.Shapes;

namespace PaintBox.Interfaces
{
    public interface IDrawableShape : IShape
    {
        Shape CreatePreviewShape();

        void StartDrawing(Point startPoint);

        void UpdateDrawing(Point currentPoint);

        bool CompleteDrawing(Point endPoint);

        bool FinishOnRightClick();

        bool IsMultiStep { get; }
    }
}
