using PaintBox.Models;

namespace PaintBox.Services
{
    public class RemoveShapeCommand : ICommand
    {
        private readonly ShapeManager _manager;
        private readonly IShape _shape;

        public RemoveShapeCommand(ShapeManager manager, IShape shape)
        {
            _manager = manager;
            _shape = shape;
        }

        public void Execute()
        {
            _manager.RemoveShape(_shape, recordCommand: false);
        }

        public void Unexecute()
        {
            _manager.AddShape(_shape, recordCommand: false);
        }
    }
}
