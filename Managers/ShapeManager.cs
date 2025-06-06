using PaintBox.Commands;
using PaintBox.Interfaces;
using System.Collections.Generic;
using System.Windows.Controls;

namespace PaintBox.Managers
{
    public class ShapeManager
    {
        private readonly Canvas _canvas;
        private readonly List<IShape> _shapes = new();
        private readonly CommandManager _commandManager = new();

        public ShapeManager(Canvas canvas)
        {
            _canvas = canvas;
        }

        public bool CanUndo => _commandManager.CanUndo;
        public bool CanRedo => _commandManager.CanRedo;
        public void AddShape(IShape shape)
        {
            var cmd = new AddShapeCommand(shape, _canvas);
            _commandManager.Execute(cmd);
            _shapes.Add(shape);
        }

        public void Undo() => _commandManager.Undo();
        public void Redo() => _commandManager.Redo();

        public void LoadShapes(IEnumerable<IShape> shapes)
        {
            _canvas.Children.Clear();
            _commandManager.ClearAll();
            _shapes.Clear();

            foreach (var shape in shapes)
            {
                var wpf = shape.CreateWpfShape();
                _canvas.Children.Add(wpf);
                _shapes.Add(shape);
            }
        }

        public IEnumerable<IShape> GetAllShapes() => _shapes.AsReadOnly();
    }
}
