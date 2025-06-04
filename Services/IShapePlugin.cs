// PaintBox\Services\IShapePlugin.cs

using PaintBox.Models;

namespace PaintBox.Services
{
    /// <summary>
    /// Интерфейс, который должен реализовать каждый плагин-расширение.
    /// Плагин выдаёт:
    ///  • Name — уникальное имя фигуры (то же, что будет в ComboShapes),
    ///  • CreateShapeInstance() — фабрика, возвращающая новый объект IShape.
    /// </summary>
    public interface IShapePlugin
    {
        /// <summary>
        /// Уникальное имя фигуры (используется как ключ в ComboShapes и в фабрике).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Создаёт и возвращает новый экземпляр класса, реализующего IShape (и IDrawableShape).
        /// </summary>
        IShape CreateShapeInstance();
    }
}
