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
        // Его временный WPF-элемент (пунктир)
        private Shape _previewWpfShape;

        public MainWindow()
        {
            InitializeComponent();

            _shapeManager = new ShapeManager(DrawingCanvas);

            RegisterBuiltInShapes();
            RegisterBasicColors();
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
            ComboShapes.SelectedIndex = 0;
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
                Filter = "JSON‐файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = ".json"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _serializer.Save(dlg.FileName, _shapeManager.GetAllShapes());
                    MessageBox.Show("Сохранено успешно");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message);
                }
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "JSON‐файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = ".json"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var shapes = _serializer.Load(dlg.FileName);
                    _shapeManager.LoadShapes(shapes);
                    UpdateUndoRedoButtons();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке: " + ex.Message);
                }
            }
        }

        private void BtnLoadPlugin_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "DLL‐файлы (*.dll)|*.dll|Все файлы (*.*)|*.*",
                DefaultExt = ".dll"
            };
            if (dlg.ShowDialog() == true)
            {
                var plugins = PluginLoader.LoadPlugins(dlg.FileName);
                int count = 0;
                foreach (var plugin in plugins)
                {
                    if (plugin.Name == null)
                        continue;
                    if (!_shapeFactories.ContainsKey(plugin.Name))
                    {
                        _shapeFactories.Add(plugin.Name, () => plugin.CreateShapeInstance());
                        count++;
                    }
                }
                if (count > 0)
                {
                    ComboShapes.ItemsSource = _shapeFactories.Keys;
                    MessageBox.Show($"Загружено {count} типов фигур из плагина.");
                }
                else
                {
                    MessageBox.Show("В выбранной DLL не найдено ни одного IShapePlugin.");
                }
            }
        }

        #endregion

        #region Обработчики мыши для Canvas

        /// <summary>
        /// Приводит координату точки к области Canvas: [0, Canvas.ActualWidth] × [0, Canvas.ActualHeight].
        /// </summary>
        private Point ClampToCanvas(Point raw)
        {
            double x = Math.Max(0, Math.Min(raw.X, DrawingCanvas.ActualWidth));
            double y = Math.Max(0, Math.Min(raw.Y, DrawingCanvas.ActualHeight));
            return new Point(x, y);
        }

        /// <summary>
        /// Срабатывает при нажатии ЛКМ на Canvas.
        /// — Если уже идёт рисование Polygon/Polyline, добавляет новую вершину.
        /// — Иначе начинает новую фигуру.
        /// </summary>
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Берём «сырые» координаты мыши относительно Canvas
            Point rawPt = e.GetPosition(DrawingCanvas);
            // Клэмпим их внутрь, чтобы не было отрицательных
            Point pt = ClampToCanvas(rawPt);

            // 1) Если уже рисуем Polygon, фиксируем новую вершину
            if (_currentDrawable is PolygonShape polygonShape)
            {
                polygonShape.CompleteDrawing(pt);
                return;
            }

            // 2) Если уже рисуем Polyline, фиксируем новую вершину
            if (_currentDrawable is PolylineShape polylineShape)
            {
                polylineShape.CompleteDrawing(pt);
                return;
            }

            // 3) Иначе — начинаем новую фигуру
            if (ComboShapes.SelectedItem == null)
                return;

            string shapeType = ComboShapes.SelectedItem.ToString();
            if (!_shapeFactories.ContainsKey(shapeType))
                return;

            _currentDrawable = _shapeFactories[shapeType].Invoke() as IDrawableShape;
            if (_currentDrawable == null)
                return;

            // Задаём параметры из UI
            _currentDrawable.StrokeThickness = SliderThickness.Value;
            var strokeName = ComboStrokeColor.SelectedItem?.ToString() ?? "Black";
            _currentDrawable.StrokeColor = (Color)ColorConverter.ConvertFromString(strokeName);
            var fillName = ComboFillColor.SelectedItem?.ToString() ?? "Transparent";
            _currentDrawable.FillColor = (Color)ColorConverter.ConvertFromString(fillName);

            // Создаём превью‐элемент и добавляем его на Canvas
            _previewWpfShape = _currentDrawable.CreatePreviewShape();
            DrawingCanvas.Children.Add(_previewWpfShape);

            // Передаём первую точку (аккуратно с клэмпом)
            _currentDrawable.StartDrawing(pt);

            // Захватываем мышь, чтобы получать MouseMove и MouseRightButtonDown
            DrawingCanvas.CaptureMouse();
        }

        /// <summary>
        /// Срабатывает при движении мыши над Canvas. Всегда обновляем превью, 
        /// даже если ЛКМ не зажата (последняя вершина «следует» за курсором).
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
        /// — Для Line/Rectangle/Ellipse считаем, что рисование завершилось: 
        ///   удаляем превью, добавляем «реальную» фигуру и сбрасываем состояние.
        /// — Для Polygon/Polyline ничего не делаем: они ждут правого клика для завершения.
        /// </summary>
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_currentDrawable == null)
                return;

            // Для простых фигур (Line, Rectangle, Ellipse) завершаем на MouseLeftButtonUp
            if (_currentDrawable is LineShape ||
                _currentDrawable is RectangleShape ||
                _currentDrawable is EllipseShape)
            {
                Point rawPt = e.GetPosition(DrawingCanvas);
                Point endPt = ClampToCanvas(rawPt);

                _currentDrawable.CompleteDrawing(endPt);

                // Удаляем превью‐элемент
                DrawingCanvas.Children.Remove(_previewWpfShape);

                // Добавляем финальную фигуру через ShapeManager
                _shapeManager.AddShape(_currentDrawable);

                // Сбрасываем и освобождаем мышь
                _currentDrawable = null;
                _previewWpfShape = null;
                DrawingCanvas.ReleaseMouseCapture();
                UpdateUndoRedoButtons();
            }
            // Для Polygon/Polyline ничего не делаем: они ждут ПКМ
        }

        /// <summary>
        /// Срабатывает при нажатии ПКМ на Canvas.
        /// — Если текущая фигура Polygon/Polyline, добавляем последнюю вершину, 
        ///   а затем завершаем рисование (FinishOnRightClick).
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
                // Затем «закрываем» полигон
                finished = polygon.FinishOnRightClick();
            }
            else if (_currentDrawable is PolylineShape polyline)
            {
                // Сначала добавляем последнюю вершину
                polyline.CompleteDrawing(pt);
                // Затем «заканчиваем» ломаную
                finished = polyline.FinishOnRightClick();
            }

            if (finished)
            {
                // Удаляем превью‐элемент
                DrawingCanvas.Children.Remove(_previewWpfShape);

                // Добавляем финальную фигуру через ShapeManager
                _shapeManager.AddShape(_currentDrawable);

                // Сбрасываем и освобождаем мышь
                _currentDrawable = null;
                _previewWpfShape = null;
                DrawingCanvas.ReleaseMouseCapture();
                UpdateUndoRedoButtons();
            }
        }

        #endregion
    }
}
