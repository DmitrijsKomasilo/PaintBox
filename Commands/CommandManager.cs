using PaintBox.Interfaces;
using System.Collections.Generic;

namespace PaintBox.Commands
{
    public class CommandManager
    {
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Execute(ICommand command)
        {
            command.Do();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public void Undo()
        {
            if (CanUndo)
            {
                var cmd = _undoStack.Pop();
                cmd.Undo();
                _redoStack.Push(cmd);
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                var cmd = _redoStack.Pop();
                cmd.Do();
                _undoStack.Push(cmd);
            }
        }

        public void ClearAll()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
