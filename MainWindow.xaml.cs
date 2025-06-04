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
        private readonly IShapeSerializer _serializer;
        private readonly Dictionary<string, Func<IShape>> _shapeFactories = new();

        // Текущий «рисуемый» объект (Line, Rectangle, Ellipse, Polyline, Polygon или плагиновая фигура)
        private IDrawableShape _currentDrawable;
        // Пунктирное превью фигуры во время рисования
        private Shape _previewWpfShape;

        public MainWindow()
        {
            InitializeComponent();

            _shapeManager = new ShapeManager(DrawingCanvas);

            RegisterBuiltInShapes();
            RegisterBasicColors();

            // Теперь передаём в JsonShapeSerializer словарь
            _serializer = new JsonShapeSerializer(_shapeFactories);

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

            ComboShapes.ItemsSource = _shapeFactories.Keys.ToList();
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
                    // Загрузка фигур из JSON
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
                    if (string.IsNullOrEmpty(plugin.Name) || _shapeFactories.ContainsKey(plugin.Name))
                        continue;

                    _shapeFactories.Add(plugin.Name, () => plugin.CreateShapeInstance());
                    count++;
                }

                if (count > 0)
                {
                    // Обновляем ComboShapes (копируем в новый список, чтобы ItemsSource «увидел» изменение)
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

        #endregion

        #region Обработчики мыши для Canvas

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

            // Если рисуем Polygon, добавляем новую вершину
            if (_currentDrawable is PolygonShape polygonShape)
            {
                polygonShape.CompleteDrawing(pt);
                return;
            }

            // Если рисуем Polyline, добавляем новую вершину
            if (_currentDrawable is PolylineShape polylineShape)
            {
                polylineShape.CompleteDrawing(pt);
                return;
            }

            // Иначе начинаем новую фигуру (или плагиновую)
            if (ComboShapes.SelectedItem == null)
                return;

            string shapeType = ComboShapes.SelectedItem.ToString();
            if (!_shapeFactories.ContainsKey(shapeType))
                return;

            _currentDrawable = _shapeFactories[shapeType].Invoke() as IDrawableShape;
            if (_currentDrawable == null)
                return;

            // Передаём параметры рисования
            _currentDrawable.StrokeThickness = SliderThickness.Value;
            var strokeName = ComboStrokeColor.SelectedItem?.ToString() ?? "Black";
            _currentDrawable.StrokeColor = (Color)ColorConverter.ConvertFromString(strokeName);
            var fillName = ComboFillColor.SelectedItem?.ToString() ?? "Transparent";
            _currentDrawable.FillColor = (Color)ColorConverter.ConvertFromString(fillName);

            // Создаём пунктирное превью и кладём в Canvas
            _previewWpfShape = _currentDrawable.CreatePreviewShape();
            DrawingCanvas.Children.Add(_previewWpfShape);

            // Инициализируем первую точку
            _currentDrawable.StartDrawing(pt);

            // Захватываем мышь
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

            // Универсальный «одношаговый» вариант: 
            // любой IDrawableShape, кроме PolygonShape и PolylineShape, 
            // завершаем при отпускании ЛКМ
            if (!(_currentDrawable is PolygonShape) && !(_currentDrawable is PolylineShape))
            {
                Point rawPt = e.GetPosition(DrawingCanvas);
                Point endPt = ClampToCanvas(rawPt);

                // Завершаем рисование (CompleteDrawing → true)
                _currentDrawable.CompleteDrawing(endPt);

                // Удаляем превью, добавляем «реальную» фигуру
                DrawingCanvas.Children.Remove(_previewWpfShape);
                _shapeManager.AddShape(_currentDrawable);

                _currentDrawable = null;
                _previewWpfShape = null;
                DrawingCanvas.ReleaseMouseCapture();
                UpdateUndoRedoButtons();
            }
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentDrawable == null)
                return;

            Point rawPt = e.GetPosition(DrawingCanvas);
            Point pt = ClampToCanvas(rawPt);

            bool finished = false;

            if (_currentDrawable is PolygonShape polygon)
            {
                polygon.CompleteDrawing(pt);
                finished = polygon.FinishOnRightClick();
            }
            else if (_currentDrawable is PolylineShape polyline)
            {
                polyline.CompleteDrawing(pt);
                finished = polyline.FinishOnRightClick();
            }

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

        #endregion
    }
}
