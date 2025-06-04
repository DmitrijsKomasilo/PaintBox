using PaintBox.Interfaces;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PaintBox.Commands
{
    /// <summary>
    /// Команда для добавления фигуры на Canvas (Undo/Redo).
    /// </summary>
    public class AddShapeCommand : ICommand
    {
        private readonly IShape _shape;       // модель фигуры (e.g. RectangleShape)
        private readonly Canvas _canvas;      // Canvas, на который рисуем
        private Shape? _wpfShape;             // экземпляр WPF-элемента (Rectangle, Ellipse, Polygon...)

        public AddShapeCommand(IShape shape, Canvas canvas)
        {
            _shape = shape;
            _canvas = canvas;
        }

        public void Do()
        {
            // Создаём WPF-элемент у модели
            _wpfShape = _shape.CreateWpfShape();
            _canvas.Children.Add(_wpfShape!);
        }

        public void Undo()
        {
            if (_wpfShape != null)
            {
                _canvas.Children.Remove(_wpfShape);
            }
        }
    }
}
