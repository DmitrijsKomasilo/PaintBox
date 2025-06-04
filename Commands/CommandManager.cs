using PaintBox.Interfaces;
using System.Collections.Generic;

namespace PaintBox.Commands
{
    /// <summary>
    /// Менеджер для любых команд (Undo/Redo).
    /// </summary>
    public class CommandManager
    {
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// Выполнить команду и сохранить в истории.
        /// </summary>
        public void Execute(ICommand command)
        {
            command.Do();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        /// <summary>Отменить последнюю команду.</summary>
        public void Undo()
        {
            if (CanUndo)
            {
                var cmd = _undoStack.Pop();
                cmd.Undo();
                _redoStack.Push(cmd);
            }
        }

        /// <summary>Повторить команду из Redo-стека.</summary>
        public void Redo()
        {
            if (CanRedo)
            {
                var cmd = _redoStack.Pop();
                cmd.Do();
                _undoStack.Push(cmd);
            }
        }

        /// <summary>Очистить всю историю (после LoadShapes или новой сессии).</summary>
        public void ClearAll()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
