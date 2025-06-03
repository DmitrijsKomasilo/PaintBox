using System.Collections.Generic;

namespace PaintBox.Services
{
    /// <summary>
    /// Сериализует и десериализует набор фигур (IShape) в файл и обратно.
    /// </summary>
    public interface IShapeSerializer
    {
        /// <summary>
        /// Сохранить список фигур в файл (json/xml/и т.д.).
        /// </summary>
        void Save(string path, IEnumerable<PaintBox.Models.IShape> shapes);

        /// <summary>
        /// Загрузить список фигур из файла.
        /// </summary>
        IEnumerable<PaintBox.Models.IShape> Load(string path);
    }
}
