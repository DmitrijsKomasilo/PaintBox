using System.Windows;
using System.Windows.Shapes;

namespace PaintBox.Models
{
    /// <summary>
    /// Интерфейс «Drawable», позволяющий «рисовать» фигуру мышью:
    /// - StartDrawing – начало (положение первого клика),
    /// - UpdateDrawing – обновление при движении мыши,
    /// - CompleteDrawing – завершающий щелчок/отпускание кнопки.
    /// Метод CreateWpfShape() (из IShape) отдаёт «настоящее» Shape, а тут мы работаем с временным объектом PreviewShape.
    /// </summary>
    public interface IDrawableShape : IShape
    {
        /// <summary>
        /// Создаёт (или возвращает) временный WPF-Shape для «превью» рисования.
        /// </summary>
        /// <returns>WPF-элемент, который мы будем обновлять при MouseMove.</returns>
        Shape CreatePreviewShape();

        /// <summary>
        /// Вызвано при первом клике/нажатии левой кнопки мыши. 
        /// Здесь сохраняем стартовую точку (например, Bounds.X,Y или Points).
        /// </summary>
        void StartDrawing(Point startPoint);

        /// <summary>
        /// Вызвано при движении мышью (MouseMove) с зажатой ЛКМ (или между кликами для полигона/ломаной).
        /// Нужна для обновления PreviewShape: меняем Bounds/Points, а потом это отражается в WPF-объекте.
        /// </summary>
        void UpdateDrawing(Point currentPoint);

        /// <summary>
        /// Вызвано при завершении рисования (MouseLeftButtonUp или правый клик для многоугольников).
        /// Возвращает true, если рисование действительно завершено (и фигуру можно «закрыть» и добавить на Canvas).
        /// Для обычных фигур (line/rect/ellipse) всегда true. Для Polyline/Polygon – true только при двойном клике или правом клике.
        /// </summary>
        bool CompleteDrawing(Point endPoint);
    }
}
