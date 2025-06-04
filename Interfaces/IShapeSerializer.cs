namespace PaintBox.Interfaces
{
    using System.Collections.Generic;
    using PaintBox.DTO;
    using PaintBox.Interfaces;

    /// <summary>
    /// Интерфейс для сериализации/десериализации списка IShape через DTO (например, ShapeData → JSON).
    /// </summary>
    public interface IShapeSerializer
    {
        /// <summary>
        /// Сохраняет коллекцию фигур в файл. </summary>
        void Save(string filePath, IEnumerable<IShape> shapes);

        /// <summary>
        /// Загружает фигуры из файла и возвращает список восстановленных IShape. </summary>
        IEnumerable<IShape> Load(string filePath);
    }
}
