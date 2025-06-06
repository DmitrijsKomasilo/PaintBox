using PaintBox.Interfaces;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PaintBox.Commands
{
    public class RemoveShapeCommand : ICommand
    {
        private readonly IShape _shape;
        private readonly Canvas _canvas;
        private Shape? _wpfShape;

        public RemoveShapeCommand(IShape shape, Canvas canvas)
        {
            _shape = shape;
            _canvas = canvas;
        }

        public void Do()
        {
            foreach (var child in _canvas.Children.OfType<Shape>())
            {

            }
        }

        public void Undo()
        {
            if (_wpfShape != null)
            {
                _canvas.Children.Add(_wpfShape);
            }
        }
    }
}
