using PaintBox.Commands;
using PaintBox.Interfaces;
using System.Collections.Generic;
using System.Windows.Controls;

namespace PaintBox.Managers
{
    /// <summary>
    /// Управляет всеми фигурами: добавление, Undo/Redo, загрузка из сериализации.
    /// </summary>
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

        /// <summary>
        /// Добавляет фигуру: создаёт и выполняет AddShapeCommand.
        /// </summary>
        public void AddShape(IShape shape)
        {
            var cmd = new AddShapeCommand(shape, _canvas);
            _commandManager.Execute(cmd);
            _shapes.Add(shape);
        }

        public void Undo() => _commandManager.Undo();
        public void Redo() => _commandManager.Redo();

        /// <summary>
        /// Загрузка списка фигур (после Deserialize).
        /// Очищаем Canvas и историю, затем добавляем все новые фигуры «как есть».
        /// </summary>
        public void LoadShapes(IEnumerable<IShape> shapes)
        {
            // 1) Очистить Canvas
            _canvas.Children.Clear();
            // 2) Очистить историю команд
            _commandManager.ClearAll();
            _shapes.Clear();

            // 3) Добавить каждую фигуру (создать её WPF-элемент) без записи в команду
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
