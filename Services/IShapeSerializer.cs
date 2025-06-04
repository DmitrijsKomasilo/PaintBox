using PaintBox.Models;
using System.Collections.Generic;

namespace PaintBox.Services
{
    /// <summary>
    /// Интерфейс, обеспечивающий сериализацию/десериализацию списка фигур.
    /// </summary>
    public interface IShapeSerializer
    {
        /// <summary>
        /// Сохраняет список фигур в файл (формат по выбору: JSON, XML и т.д.).
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="shapes">Коллекция IShape (ShapeBase и наследники).</param>
        void Save(string filePath, IEnumerable<IShape> shapes);

        /// <summary>
        /// Загружает список фигур из файла, восстанавливает каждый объект IShape.
        /// </summary>
        /// <param name="filePath">Путь к JSON‐файлу.</param>
        /// <returns>Список восстановленных IShape.</returns>
        IEnumerable<IShape> Load(string filePath);
    }
}
