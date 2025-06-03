namespace PaintBox.Services
{
    /// <summary>
    /// Интерфейс для команд, которые можно выполнять и отменять.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Выполнить команду (например, добавить фигуру).
        /// </summary>
        void Execute();

        /// <summary>
        /// Отменить эффект Execute (например, убрать фигуру с Canvas).
        /// </summary>
        void Unexecute();
    }
}
