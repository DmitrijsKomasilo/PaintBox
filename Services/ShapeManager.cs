using PaintBox.Models;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PaintBox.Services
{
    /// <summary>
    /// Класс, который хранит текущие фигуры (IShape),
    /// умеет добавлять/удалять их и выводить на WPF-Canvas,
    /// а также взаимодействует с CommandManager для undo/redo.
    /// </summary>
    public class ShapeManager
    {
        private readonly List<IShape> _shapes = new List<IShape>();
        private readonly CommandManager _commandManager = new CommandManager();
        private readonly Canvas _canvas;

        public ShapeManager(Canvas canvas)
        {
            _canvas = canvas;
        }

        /// <summary>
        /// Добавляет фигуру: создаёт WPF-Shape через CreateWpfShape и помещает на Canvas.
        /// Если recordCommand = true, то операция оборачивается в AddShapeCommand и отправляется в CommandManager.Do().
        /// </summary>
        public void AddShape(IShape shape, bool recordCommand = true)
        {
            if (recordCommand)
            {
                var addCmd = new AddShapeCommand(this, shape);
                _commandManager.Do(addCmd);
                return;
            }

            // Собственно добавление в список и на Canvas:
            _shapes.Add(shape);
            var wpfShape = shape.CreateWpfShape();
            _canvas.Children.Add(wpfShape);
        }

        /// <summary>
        /// Удаляет фигуру: убирает из Canvas и из списка. Если recordCommand = true, то упаковывается в RemoveShapeCommand.
        /// </summary>
        public void RemoveShape(IShape shape, bool recordCommand = true)
        {
            if (recordCommand)
            {
                var remCmd = new RemoveShapeCommand(this, shape);
                _commandManager.Do(remCmd);
                return;
            }

            Shape toRemove = null;
            foreach (var child in _canvas.Children)
            {
                if (child is Shape s && s.Uid == shape.TypeName)
                {
                }
            }


            if (_canvas.Children.Count > 0)
            {
                var last = _canvas.Children[_canvas.Children.Count - 1] as Shape;
                _canvas.Children.Remove(last);
            }

            _shapes.Remove(shape);
        }

        public bool CanUndo => _commandManager.CanUndo;
        public bool CanRedo => _commandManager.CanRedo;

        public void Undo() => _commandManager.Undo();
        public void Redo() => _commandManager.Redo();

        /// <summary>
        /// Возвращает текущее «набор» фигур.
        /// </summary>
        public IReadOnlyList<IShape> GetAllShapes() => _shapes.AsReadOnly();

        /// <summary>
        /// Полностью очистить Canvas и сбросить все списки/стэки команд.
        /// </summary>
        public void ClearAll()
        {
            _canvas.Children.Clear();
            _shapes.Clear();
        }

        /// <summary>
        /// Восстановление фигур на Canvas из коллекции IShape (после десериализации).
        /// </summary>
        public void LoadShapes(IEnumerable<IShape> shapes)
        {
            ClearAll();
            foreach (var shape in shapes)
            {

                AddShape(shape, recordCommand: false);
            }
        }
    }
}

