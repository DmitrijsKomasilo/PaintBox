using Microsoft.Win32;
using PaintBox.Models;
using PaintBox.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintBox
{
    public partial class MainWindow : Window
    {
        private readonly ShapeManager _shapeManager;
        private readonly IShapeSerializer _serializer = new JsonShapeSerializer();
        private readonly Dictionary<string, Func<IShape>> _shapeFactories = new();

        public MainWindow()
        {
            InitializeComponent();

            // 1) Инициализация ShapeManager
            _shapeManager = new ShapeManager(DrawingCanvas);

            // 2) Заполнить ComboShapes базовыми фигурами
            RegisterBuiltInShapes();

            // 3) Заполнить ComboStrokeColor и ComboFillColor
            RegisterBasicColors();

            // 4) Подписаться на изменение CanUndo/CanRedo (в ЛР 2 это можно реализовать просто вручную после каждого действия)
            UpdateUndoRedoButtons();
        }

        /// <summary>
        /// Регистрируем встроенные ("built-in") фигуры: Line, Rectangle, Ellipse, Polygon, Polyline.
        /// Ключ — это TypeName (строка), значение — фабрика.
        /// </summary>
        private void RegisterBuiltInShapes()
        {
            _shapeFactories.Add("Line", () => new LineShape());
            _shapeFactories.Add("Rectangle", () => new RectangleShape());
            _shapeFactories.Add("Ellipse", () => new EllipseShape());
            _shapeFactories.Add("Polygon", () => new PolygonShape()
            {
                // Для ЛР 2 сделаем заглушку: треугольник (Points будут фиксированы)
                Points = new PointCollection
                {
                    new Point(50, 50),
                    new Point(100, 150),
                    new Point(0, 150)
                }
            });
            _shapeFactories.Add("Polyline", () => new PolylineShape()
            {
                // Зигзаг по умолчанию:
                Points = new PointCollection
                {
                    new Point(150, 200),
                    new Point(200, 250),
                    new Point(250, 200),
                    new Point(300, 250)
                }
            });

            // Теперь устанавливаем источник данных ComboShapes:
            ComboShapes.ItemsSource = _shapeFactories.Keys;
            ComboShapes.SelectedIndex = 0; // по умолчанию первая фигура
        }

        /// <summary>
        /// Заполняем списки цветов (через KnownColors).
        /// </summary>
        private void RegisterBasicColors()
        {
            var allColors = typeof(Colors).GetProperties()
                               .Select(p => p.Name)
                               .OrderBy(name => name)
                               .ToList();

            ComboStrokeColor.ItemsSource = allColors;
            ComboStrokeColor.SelectedItem = "Black";

            ComboFillColor.ItemsSource = allColors;
            ComboFillColor.SelectedItem = "Transparent";
        }

        /// <summary>
        /// Обновляет доступность кнопок Undo/Redo.
        /// </summary>
        private void UpdateUndoRedoButtons()
        {
            BtnUndo.IsEnabled = _shapeManager.CanUndo;
            BtnRedo.IsEnabled = _shapeManager.CanRedo;
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Нарисовать".
        /// </summary>
        private void BtnDrawShape_Click(object sender, RoutedEventArgs e)
        {
            if (ComboShapes.SelectedItem == null) return;
            string typeName = ComboShapes.SelectedItem.ToString();

            if (!_shapeFactories.ContainsKey(typeName))
            {
                MessageBox.Show($"Неизвестный тип: {typeName}");
                return;
            }

            // 1) Создаём пустой IShape через фабрику
            IShape shape = _shapeFactories[typeName].Invoke();

            // 2) Задаём параметры из GUI
            // Толщина
            shape.StrokeThickness = SliderThickness.Value;

            // Цвет обводки
            var strokeColorName = ComboStrokeColor.SelectedItem.ToString();
            var strokeColor = (Color)ColorConverter.ConvertFromString(strokeColorName);
            shape.StrokeColor = strokeColor;

            // Цвет заливки
            var fillColorName = ComboFillColor.SelectedItem.ToString();
            var fillColor = (Color)ColorConverter.ConvertFromString(fillColorName);
            shape.FillColor = fillColor;

            // Для простоты: Bounds у фигуры заданы в конструкторе (например, у RectangleShape Bounds уже всё содержит).
            // Если нужна возможность самому двигать/задавать координаты далее — сделаем в ЛР 3.

            // 3) Добавляем фигуру через команду (AddShapeCommand)
            var addCmd = new AddShapeCommand(_shapeManager, shape);
            _shapeManager.AddShape(shape);
            // (Во vst. _shapeManager.AddShape(shape) сразу упакует в команду, благодаря recordCommand = true по умолчанию)

            // 4) После добавления обновляем состояние кнопок Undo/Redo
            UpdateUndoRedoButtons();
        }

        /// <summary>
        /// Undo
        /// </summary>
        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            _shapeManager.Undo();
            UpdateUndoRedoButtons();
        }

        /// <summary>
        /// Redo
        /// </summary>
        private void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            _shapeManager.Redo();
            UpdateUndoRedoButtons();
        }

        /// <summary>
        /// Сохранить текущие фигуры в JSON.
        /// </summary>
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
                    _serializer.Save(dlg.FileName, _shapeManager.GetAllShapes());
                    MessageBox.Show("Сохранено успешно");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Загрузить JSON-файл и отобразить фигуры.
        /// </summary>
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Загрузить DLL-плагин с новыми фигурами.
        /// </summary>
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
                    // дополняем словарь: имя → фабрика
                    if (!_shapeFactories.ContainsKey(plugin.Name))
                    {
                        _shapeFactories.Add(plugin.Name, () => plugin.CreateShapeInstance());
                        count++;
                    }
                }

                if (count > 0)
                {
                    // Перезаписываем ItemsSource в ComboShapes, 
                    // чтобы появилось сразу после загрузки
                    ComboShapes.ItemsSource = _shapeFactories.Keys;
                    MessageBox.Show($"Загружено {count} типа(ов) фигур из плагина.");
                }
                else
                {
                    MessageBox.Show("Ни одного плагина IShapePlugin не найдено в этой DLL.");
                }
            }
        }
    }
}
