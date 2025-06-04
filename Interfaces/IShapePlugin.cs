namespace PaintBox.Interfaces
{
    using PaintBox.Interfaces;

    /// <summary>
    /// Интерфейс, который должен реализовать каждый плагин-фигура.
    /// </summary>
    public interface IShapePlugin
    {
        /// <summary>Имя фигуры (например, "Trapezoid").</summary>
        string Name { get; }

        /// <summary>
        /// Создаёт новый экземпляр нужной IShape (например, new TrapezoidShape()).
        /// </summary>
        IShape CreateShapeInstance();
    }
}
