namespace PaintBox.Interfaces
{
    /// <summary>
    /// Интерфейс команды для Undo/Redo (Command Pattern).
    /// </summary>
    public interface ICommand
    {
        /// <summary>Выполнить команду (например, добавить фигуру).</summary>
        void Do();

        /// <summary>Отменить команду (например, удалить фигуру).</summary>
        void Undo();
    }
}
