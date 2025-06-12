using PaintBox.DTO;
using System.IO;
using PaintBox.Interfaces;
using PaintBox.Managers;
using PaintBox.Models;
using PaintBox.Services;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox
{
    public partial class MainWindow : Window
    {
        private readonly ShapeManager _shapeManager;
        private readonly IShapeSerializer _serializer;
        private readonly Dictionary<string, Func<IShape>> _shapeFactories = new();

        private IDrawableShape _currentDrawable;
        private Shape _previewWpfShape;

        public MainWindow()
        {
            InitializeComponent();

            _shapeManager = new ShapeManager(DrawingCanvas);

            RegisterBuiltInShapes();
            RegisterBasicColors();

            _serializer = new JsonShapeSerializer(_shapeFactories);

            UpdateUndoRedoButtons();
        }

        private void RegisterBuiltInShapes()
        {
            _shapeFactories.Add("Line", () => new LineShape());
            _shapeFactories.Add("Rectangle", () => new RectangleShape());
            _shapeFactories.Add("Ellipse", () => new EllipseShape());
            _shapeFactories.Add("Polygon", () => new PolygonShape());
            _shapeFactories.Add("Polyline", () => new PolylineShape());

            ComboShapes.ItemsSource = _shapeFactories.Keys.ToList();
            ComboShapes.SelectedIndex = 0;
        }

        private void RegisterBasicColors()
        {
            var allColors = typeof(Colors)
                .GetProperties()
                .Select(p => p.Name)
                .OrderBy(n => n)
                .ToList();

            ComboStrokeColor.ItemsSource = allColors;
            ComboStrokeColor.SelectedItem = "Black";

            ComboFillColor.ItemsSource = allColors;
            ComboFillColor.SelectedItem = "Transparent";
        }

        private void UpdateUndoRedoButtons()
        {
            BtnUndo.IsEnabled = _shapeManager.CanUndo;
            BtnRedo.IsEnabled = _shapeManager.CanRedo;
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            _shapeManager.Undo();
            UpdateUndoRedoButtons();
        }

        private void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            _shapeManager.Redo();
            UpdateUndoRedoButtons();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON-файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = ".json"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var shapes = _shapeManager.GetAllShapes();
                    _serializer.Save(dlg.FileName, shapes);
                    MessageBox.Show("Сохранение прошло успешно.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON-файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = ".json"
            };
            if (dlg.ShowDialog() != true)
                return;

            try
            {
                var jsonText = File.ReadAllText(dlg.FileName);
                var rawList = JsonSerializer.Deserialize<List<ShapeData>>(jsonText,
                    new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
                int expectedCount = rawList?.Count ?? 0;

                var shapes = _serializer.Load(dlg.FileName).ToList();
                int loadedCount = shapes.Count;

                if (loadedCount < expectedCount)
                {
                    MessageBox.Show(
                        $"Предупреждение: из {expectedCount} записей десериализовано только {loadedCount} фигур.\n" +
                        "Нераспознанные фигуры будут пропущены, остальные отрисованы.",
                        "Предупреждение при загрузке",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                _shapeManager.LoadShapes(shapes);
                UpdateUndoRedoButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnLoadPlugin_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "DLL-файлы (*.dll)|*.dll|Все файлы (*.*)|*.*",
                DefaultExt = ".dll"
            };
            if (dlg.ShowDialog() == true)
            {
                var plugins = PluginLoader.LoadPlugins(dlg.FileName);
                int count = 0;

                foreach (var plugin in plugins)
                {
                    if (string.IsNullOrEmpty(plugin.Name) || _shapeFactories.ContainsKey(plugin.Name))
                        continue;

                    _shapeFactories.Add(plugin.Name, () => plugin.CreateShapeInstance());
                    count++;
                }

                if (count > 0)
                {
                    ComboShapes.ItemsSource = _shapeFactories.Keys.ToList();
                    ComboShapes.SelectedIndex = 0;
                    MessageBox.Show($"Загружено {count} новых фигур из плагина.", "Плагин", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Плагинов не найдено или имена дублируются.", "Плагин", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private Point ClampToCanvas(Point raw)
        {
            double x = Math.Max(0, Math.Min(raw.X, DrawingCanvas.ActualWidth));
            double y = Math.Max(0, Math.Min(raw.Y, DrawingCanvas.ActualHeight));
            return new Point(x, y);
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point rawPt = e.GetPosition(DrawingCanvas);
            Point pt = ClampToCanvas(rawPt);

            if (_currentDrawable != null && _currentDrawable.IsMultiStep)
            {
                _currentDrawable.CompleteDrawing(pt);
                return;
            }

            if (ComboShapes.SelectedItem == null)
                return;

            string shapeType = ComboShapes.SelectedItem.ToString();
            if (!_shapeFactories.ContainsKey(shapeType))
                return;

            _currentDrawable = _shapeFactories[shapeType].Invoke() as IDrawableShape;
            if (_currentDrawable == null)
                return;

            _currentDrawable.StrokeThickness = SliderThickness.Value;
            _currentDrawable.StrokeColor = (Color)ColorConverter.ConvertFromString(ComboStrokeColor.SelectedItem.ToString());
            _currentDrawable.FillColor = (Color)ColorConverter.ConvertFromString(ComboFillColor.SelectedItem.ToString());

            _previewWpfShape = _currentDrawable.CreatePreviewShape();
            DrawingCanvas.Children.Add(_previewWpfShape);

            _currentDrawable.StartDrawing(pt);
            DrawingCanvas.CaptureMouse();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_currentDrawable == null)
                return;

            Point rawPt = e.GetPosition(DrawingCanvas);
            Point currentPt = ClampToCanvas(rawPt);
            _currentDrawable.UpdateDrawing(currentPt);
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_currentDrawable == null)
                return;

            if (!_currentDrawable.IsMultiStep)
            {
                Point rawPt = e.GetPosition(DrawingCanvas);
                Point endPt = ClampToCanvas(rawPt);

                bool finished = _currentDrawable.CompleteDrawing(endPt);
                if (finished)
                {
                    DrawingCanvas.Children.Remove(_previewWpfShape);
                    _shapeManager.AddShape(_currentDrawable);

                    _currentDrawable = null;
                    _previewWpfShape = null;
                    DrawingCanvas.ReleaseMouseCapture();
                    UpdateUndoRedoButtons();
                }
            }
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentDrawable == null)
                return;

            if (_currentDrawable.IsMultiStep)
            {
                Point rawPt = e.GetPosition(DrawingCanvas);
                Point pt = ClampToCanvas(rawPt);

                _currentDrawable.CompleteDrawing(pt);

                bool finished = _currentDrawable.FinishOnRightClick();
                if (finished)
                     {
                    DrawingCanvas.Children.Remove(_previewWpfShape);
                    _shapeManager.AddShape(_currentDrawable);

                    _currentDrawable = null;
                    _previewWpfShape = null;
                    DrawingCanvas.ReleaseMouseCapture();
                    UpdateUndoRedoButtons();
                }
            }
        }
    }
}
