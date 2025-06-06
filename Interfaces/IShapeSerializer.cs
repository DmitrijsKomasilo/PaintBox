namespace PaintBox.Interfaces
{
    using System.Collections.Generic;
    using PaintBox.DTO;
    using PaintBox.Interfaces;

    public interface IShapeSerializer
    {
        void Save(string filePath, IEnumerable<IShape> shapes);

        IEnumerable<IShape> Load(string filePath);
    }
}
