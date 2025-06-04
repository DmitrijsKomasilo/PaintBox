using Microsoft.Win32;
using PaintBox.Models;
using PaintBox.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox
{
    public partial class MainWindow : Window
    {
        private readonly ShapeManager _shapeManager;
        private readonly IShapeSerializer _serializer = new JsonShapeSerializer();
        private readonly Dictionary<string, Func<IShape>> _shapeFactories = new();

        // Текущий «рисуемый» объект (Line, Rectangle, Ellipse, Polyline или Polygon)
        private IDrawableShape _currentDrawable;
        // Его временный WPF-элемент (пунктир), который мы рисуем как превью
        private Shape _previewWpfShape;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализируем ShapeManager, передаём ему Canvas
            _shapeManager = new ShapeManager(DrawingCanvas);

            // Регистрируем встроенные фигуры
            RegisterBuiltInShapes();
            // Заполняем списки цветов
            RegisterBasicColors();
            // Обновляем кнопки Undo/Redo
            UpdateUndoRedoButtons();
        }

        #region Регистрация фигур и цветов

        private void RegisterBuiltInShapes()
        {
            _shapeFactories.Add("Line", () => new LineShape());
            _shapeFactories.Add("Rectangle", () => new RectangleShape());
            _shapeFactories.Add("Ellipse", () => new EllipseShape());
            _shapeFactories.Add("Polygon", () => new PolygonShape());
            _shapeFactories.Add("Polyline", () => new PolylineShape());

            ComboShapes.ItemsSource = _shapeFactories.Keys;
            ComboShapes.SelectedIndex = 0; // по умолчанию первая фигура
        }

        private void RegisterBasicColors()
        {
            var allColors = typeof(Colors)
                .GetProperties()
                .Select(p => p.Name)
                .OrderBy(name => name)
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

        #endregion

        #region Обработчики кнопок (Undo/Redo, Save/Load, LoadPlugin)

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
            var dlg = new SaveFileDialog
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
                    MessageBox.Show("Сохранение завершено успешно.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "JSON-файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = ".json"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var shapes = _serializer.Load(dlg.FileName);
                    _shapeManager.LoadShapes(shapes);
                    UpdateUndoRedoButtons();
                    MessageBox.Show("Загрузка завершена успешно.", "Загрузка", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnLoadPlugin_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
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
                    if (plugin.Name == null) continue;
                    if (!_shapeFactories.ContainsKey(plugin.Name))
                    {
                        _shapeFactories.Add(plugin.Name, () => plugin.CreateShapeInstance());
                        count++;
                    }
                }

                if (count > 0)
                {
                    ComboShapes.ItemsSource = _shapeFactories.Keys;
                    MessageBox.Show($"Загружено {count} типов фигур из плагина.", "Плагин", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("В выбранной DLL не найдено ни одного IShapePlugin.", "Плагин", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        #endregion

        #region Обработчики мыши для Canvas

        /// <summary>
        /// Клэмпит "сырые" координаты мыши в области Canvas: [0, ActualWidth] × [0, ActualHeight].
        /// </summary>
        private Point ClampToCanvas(Point raw)
        {
            double x = Math.Max(0, Math.Min(raw.X, DrawingCanvas.ActualWidth));
            double y = Math.Max(0, Math.Min(raw.Y, DrawingCanvas.ActualHeight));
            return new Point(x, y);
        }

        /// <summary>
        /// Срабатывает при нажатии ЛКМ на Canvas.
        /// Если уже рисуется Polygon/Polyline, добавляет новую вершину.
        /// Иначе создаёт новый IDrawableShape и начинает рисование.
        /// </summary>
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point rawPt = e.GetPosition(DrawingCanvas);
            Point pt = ClampToCanvas(rawPt);

            // Если уже рисуем Polygon, фиксируем новую вершину
            if (_currentDrawable is PolygonShape polygonShape)
            {
                polygonShape.CompleteDrawing(pt);
                return;
            }

            // Если уже рисуем Polyline, фиксируем новую вершину
            if (_currentDrawable is PolylineShape polylineShape)
            {
                polylineShape.CompleteDrawing(pt);
                return;
            }

            // Иначе: начинаем новую фигуру
            if (ComboShapes.SelectedItem == null)
                return;

            string shapeType = ComboShapes.SelectedItem.ToString();
            if (!_shapeFactories.ContainsKey(shapeType))
                return;

            _currentDrawable = _shapeFactories[shapeType].Invoke() as IDrawableShape;
            if (_currentDrawable == null)
                return;

            // Устанавливаем параметры из UI: толщина, цвет контура, цвет заливки
            _currentDrawable.StrokeThickness = SliderThickness.Value;
            var strokeName = ComboStrokeColor.SelectedItem?.ToString() ?? "Black";
            _currentDrawable.StrokeColor = (Color)ColorConverter.ConvertFromString(strokeName);
            var fillName = ComboFillColor.SelectedItem?.ToString() ?? "Transparent";
            _currentDrawable.FillColor = (Color)ColorConverter.ConvertFromString(fillName);

            // Создаём превью-примитив и добавляем его на Canvas
            _previewWpfShape = _currentDrawable.CreatePreviewShape();
            DrawingCanvas.Children.Add(_previewWpfShape);

            // Передаём первую точку (начало рисования)
            _currentDrawable.StartDrawing(pt);

            // Захватываем мышь, чтобы получать MouseMove и MouseRightButtonDown
            DrawingCanvas.CaptureMouse();
        }

        /// <summary>
        /// Срабатывает при движении мыши над Canvas. Обновляет превью-фигуру.
        /// </summary>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_currentDrawable == null)
                return;

            Point rawPt = e.GetPosition(DrawingCanvas);
            Point currentPt = ClampToCanvas(rawPt);
            _currentDrawable.UpdateDrawing(currentPt);
        }

        /// <summary>
        /// Срабатывает при отпускании ЛКМ над Canvas.
        /// Для простых фигур (Line, Rectangle, Ellipse) завершает рисование.
        /// Для Polygon/Polyline ничего не делает (они ждут ПКМ).
        /// </summary>
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_currentDrawable == null)
                return;

            if (_currentDrawable is LineShape ||
                _currentDrawable is RectangleShape ||
                _currentDrawable is EllipseShape)
            {
                Point rawPt = e.GetPosition(DrawingCanvas);
                Point endPt = ClampToCanvas(rawPt);

                _currentDrawable.CompleteDrawing(endPt);

                // Удаляем превью и добавляем реальную фигуру
                DrawingCanvas.Children.Remove(_previewWpfShape);
                _shapeManager.AddShape(_currentDrawable);

                _currentDrawable = null;
                _previewWpfShape = null;
                DrawingCanvas.ReleaseMouseCapture();
                UpdateUndoRedoButtons();
            }
            // Для Polygon/Polyline: ничего не делаем ― ждем правый клик
        }

        /// <summary>
        /// Срабатывает при нажатии ПКМ на Canvas.
        /// Для Polygon/Polyline: добавляет последнюю вершину и завершает рисование.
        /// </summary>
        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentDrawable == null)
                return;

            Point rawPt = e.GetPosition(DrawingCanvas);
            Point pt = ClampToCanvas(rawPt);

            bool finished = false;

            if (_currentDrawable is PolygonShape polygon)
            {
                // Сначала добавляем последнюю вершину
                polygon.CompleteDrawing(pt);
                // Затем «закрываем» многоугольник
                finished = polygon.FinishOnRightClick();
            }
            else if (_currentDrawable is PolylineShape polyline)
            {
                // Сначала добавляем последнюю вершину
                polyline.CompleteDrawing(pt);
                // Затем завершаем полилинию
                finished = polyline.FinishOnRightClick();
            }

            if (finished)
            {
                // Удаляем превью и добавляем «реальную» фигуру
                DrawingCanvas.Children.Remove(_previewWpfShape);
                _shapeManager.AddShape(_currentDrawable);

                _currentDrawable = null;
                _previewWpfShape = null;
                DrawingCanvas.ReleaseMouseCapture();
                UpdateUndoRedoButtons();
            }
        }

        #endregion
    }
}
