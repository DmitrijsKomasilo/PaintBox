using System.Collections.Generic;

namespace PaintBox.Services
{
    /// <summary>
    /// Управляет двумя стеками: undoStack и redoStack.
    /// </summary>
    public class CommandManager
    {
        private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// Выполнить команду и положить её в undoStack (очищая redoStack).
        /// </summary>
        public void Do(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        /// <summary>
        /// Отменить последнюю команду.
        /// </summary>
        public void Undo()
        {
            if (CanUndo)
            {
                var cmd = _undoStack.Pop();
                cmd.Unexecute();
                _redoStack.Push(cmd);
            }
        }

        /// <summary>
        /// Повторить (Redo) последнюю отменённую команду.
        /// </summary>
        public void Redo()
        {
            if (CanRedo)
            {
                var cmd = _redoStack.Pop();
                cmd.Execute();
                _undoStack.Push(cmd);
            }
        }
    }
}

