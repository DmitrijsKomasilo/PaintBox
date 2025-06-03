using PaintBox.Models;
using System.Collections.Generic;

namespace PaintBox.Services
{
    /// <summary>
    /// Команда, которая добавляет фигуру в менеджер (ShapeManager).
    /// Unexecute() удаляет её обратно.
    /// </summary>
    public class AddShapeCommand : ICommand
    {
        private readonly ShapeManager _manager;
        private readonly IShape _shape;

        public AddShapeCommand(ShapeManager manager, IShape shape)
        {
            _manager = manager;
            _shape = shape;
        }

        public void Execute()
        {
            _manager.AddShape(_shape, recordCommand: false);
        }

        public void Unexecute()
        {
            _manager.RemoveShape(_shape, recordCommand: false);
        }
    }
}
