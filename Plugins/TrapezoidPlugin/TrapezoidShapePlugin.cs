using PaintBox.Interfaces;
using TrapezoidPlugin.Models;

namespace TrapezoidPlugin.Plugins
{
    public class TrapezoidShapePlugin : IShapePlugin
    {
        public string Name => "Trapezoid";
        public IShape CreateShapeInstance() => new TrapezoidShape();
    }
}
