using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLine_Click(object sender, RoutedEventArgs e)
        {
            var line = new Line
            {
                X1 = 50,
                Y1 = 50,
                X2 = 200,
                Y2 = 50,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            DrawingCanvas.Children.Add(line);
        }

        private void BtnRectangle_Click(object sender, RoutedEventArgs e)
        {
            var rect = new Rectangle
            {
                Width = 150,
                Height = 100,
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                Fill = Brushes.LightBlue
            };
            Canvas.SetLeft(rect, 100);
            Canvas.SetTop(rect, 100);
            DrawingCanvas.Children.Add(rect);
        }

        private void BtnEllipse_Click(object sender, RoutedEventArgs e)
        {
            var ellipse = new Ellipse
            {
                Width = 120,
                Height = 80,
                Stroke = Brushes.Green,
                StrokeThickness = 2,
                Fill = Brushes.LightGreen
            };
            Canvas.SetLeft(ellipse, 300);
            Canvas.SetTop(ellipse, 150);
            DrawingCanvas.Children.Add(ellipse);
        }

        private void BtnPolygon_Click(object sender, RoutedEventArgs e)
        {
            var polygon = new Polygon
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                Fill = Brushes.Pink,
                Points = new PointCollection()
                {
                    new Point(500, 50),
                    new Point(600, 150),
                    new Point(450, 150)
                }
            };
            DrawingCanvas.Children.Add(polygon);
        }

        // ← Новая функция для Button Polyline
        private void BtnPolyline_Click(object sender, RoutedEventArgs e)
        {
            // Создаём Polyline (зигзаг/ломаная)
            var polyline = new Polyline
            {
                Stroke = Brushes.Orange,        // цвет линии
                StrokeThickness = 2,            // толщина контура
                // Точки: зигзаг из нескольких сегментов
                Points = new PointCollection()
                {
                    new Point(100, 250),
                    new Point(150, 300),
                    new Point(200, 250),
                    new Point(250, 300),
                    new Point(300, 250)
                }
            };

            DrawingCanvas.Children.Add(polyline);
        }
    }
}
