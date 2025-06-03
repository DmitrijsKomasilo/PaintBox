using PaintBox.Models;

namespace PaintBox.Services
{
    /// <summary>
    /// Интерфейс, который должна реализовать любая сборка (DLL),
    /// чтобы стать «плагином фигур» для нашего PaintBox.
    /// </summary>
    public interface IShapePlugin
    {
        /// <summary>
        /// Уникальное имя типа фигуры (например, "Star", "Hexagon", и т.д.).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Фабричный метод: создаёт новый экземпляр IShape того типа, 
        /// который плагин поддерживает.
        /// </summary>
        IShape CreateShapeInstance();
    }
}
