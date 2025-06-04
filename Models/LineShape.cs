using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    public class LineShape : ShapeBase, IDrawableShape
    {
        public override string TypeName => "Line";

        // Этот объект используется в качестве «превью». Создаётся один раз в StartDrawing, 
        // а затем при каждом UpdateDrawing мы меняем X2, Y2.
        private Line _previewLine;

        public override Shape CreateWpfShape()
        {
            // При финальном добавлении (через ShapeManager) мы создаём «полноценный» Line 
            var line = new Line
            {
                X1 = Bounds.X,
                Y1 = Bounds.Y,
                X2 = Bounds.Width,
                Y2 = Bounds.Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness
            };
            return line;
        }

        public override void UpdateFromWpfShape(Shape shape)
        {
            base.UpdateFromWpfShape(shape);
            if (shape is Line line)
            {
                Bounds = new Rect(line.X1, line.Y1, line.X2, line.Y2);
            }
        }

        #region IDrawableShape

        public Shape CreatePreviewShape()
        {
            // Если ещё не создавали preview, делаем его
            if (_previewLine == null)
            {
                _previewLine = new Line
                {
                    Stroke = new SolidColorBrush(StrokeColor),
                    StrokeThickness = StrokeThickness,
                    StrokeDashArray = new DoubleCollection { 4, 2 } // пунктир для preview
                };
            }
            return _previewLine;
        }

        public void StartDrawing(Point startPoint)
        {
            // Задаём начальную точку в Bounds: Bounds.X,Y = X1,Y1
            Bounds = new Rect(startPoint.X, startPoint.Y, startPoint.X, startPoint.Y);
            // Инициализируем previewLine: X1=Y1=то, что в Bounds.X,Y
            if (_previewLine != null)
            {
                _previewLine.X1 = startPoint.X;
                _previewLine.Y1 = startPoint.Y;
                _previewLine.X2 = startPoint.X;
                _previewLine.Y2 = startPoint.Y;
            }
        }

        public void UpdateDrawing(Point currentPoint)
        {
            // Обновляем конечную точку: Bounds.Width, Bounds.Height хранят X2, Y2
            Bounds = new Rect(Bounds.X, Bounds.Y, currentPoint.X, currentPoint.Y);
            if (_previewLine != null)
            {
                _previewLine.X2 = currentPoint.X;
                _previewLine.Y2 = currentPoint.Y;
            }
        }

        public bool CompleteDrawing(Point endPoint)
        {
            // В обычной линии рисование завершается при MouseLeftButtonUp
            Bounds = new Rect(Bounds.X, Bounds.Y, endPoint.X, endPoint.Y);
            // Preview окончательно не нужен – при завершении добавим «реальный» WPF Shape из CreateWpfShape()
            return true;
        }

        #endregion
    }
}
