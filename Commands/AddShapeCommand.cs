using PaintBox.Interfaces;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PaintBox.Commands
{
    public class AddShapeCommand : ICommand
    {
        private readonly IShape _shape;
        private readonly Canvas _canvas;
        private Shape? _wpfShape;

        public AddShapeCommand(IShape shape, Canvas canvas)
        {
            _shape = shape;
            _canvas = canvas;
        }

        public void Do()
        {
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
