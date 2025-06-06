namespace PaintBox.Interfaces
{

    public interface ICommand
    {
        void Do();

        void Undo();
    }
}
