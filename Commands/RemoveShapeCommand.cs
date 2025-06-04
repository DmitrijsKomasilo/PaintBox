using PaintBox.Interfaces;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace PaintBox.Commands
{
    /// <summary>
    /// Если вдруг реализуется удаление фигуры по щелчку,
    /// можно создать отдельную команду удаления.
    /// </summary>
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
            // Здесь предполагаем, что модель умеет вернуть свой WPF-элемент
            // либо _wpfShape передавался извне.
            // Для простоты: найдём-удалим по совпадению по типу и координатам.
            foreach (var child in _canvas.Children.OfType<Shape>())
            {
                // Некоторая логика поиска, какой именно элемент удалять...
                // (например, сравнение геометрии, тегов или привязка «ссылки»)
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
