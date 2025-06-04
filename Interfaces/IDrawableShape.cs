namespace PaintBox.Interfaces
{
    using System.Windows;
    using System.Windows.Shapes;

    /// <summary>
    /// Интерфейс для «рисуемых» фигур, которые интерактивно создаются мышью.
    /// </summary>
    public interface IDrawableShape : IShape
    {
        /// <summary>
        /// Создаёт WPF-элемент «превью» (обычно пунктир), который показывается при движении мыши.
        /// </summary>
        Shape CreatePreviewShape();

        /// <summary>
        /// Вызывается при первом клике ЛКМ: задаём начальную точку «рисования». </summary>
        void StartDrawing(Point startPoint);

        /// <summary>Вызывается на каждом MouseMove: обновляем превью-фигуру (проводим линию, меняем размер и т.п.).</summary>
        void UpdateDrawing(Point currentPoint);

        /// <summary>
        /// Вызывается, когда фигура «заканчивается» (обычно при отпускании ЛКМ или правом клике).
        /// Возвращает true, если рисование закончено и надо «утвердить» фигуру.
        /// </summary>
        bool CompleteDrawing(Point endPoint);

        /// <summary>
        /// Для Polygon/Polyline: вызывается при ПКМ, чтобы «закрыть» контур или закончить ломаную.
        /// </summary>
        bool FinishOnRightClick();
    }
}
