namespace PaintBox.Interfaces
{
    using PaintBox.Interfaces;
    public interface IShapePlugin
    {
        string Name { get; }

        IShape CreateShapeInstance();
    }
}
