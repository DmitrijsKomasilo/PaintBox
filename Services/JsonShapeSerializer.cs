// PaintBox\Services\JsonShapeSerializer.cs

using PaintBox.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace PaintBox.Services
{
    /// <summary>
    /// JSON‐сериализатор
    /// </summary>
    public class JsonShapeSerializer : IShapeSerializer
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Словарь: ключ = строковое имя фигуры (TypeName), значение = Func<IShape> (фабрика)
        private readonly IReadOnlyDictionary<string, Func<IShape>> _factories;

        /// <summary>
        /// Конструктор принимает словарь фабрик из MainWindow (включая плагины).
        /// </summary>
        public JsonShapeSerializer(IReadOnlyDictionary<string, Func<IShape>> factories)
        {
            _factories = factories ?? throw new ArgumentNullException(nameof(factories));
        }

        /// <summary>
        /// Сохраняет список IShape в JSON-файл.
        /// </summary>
        public void Save(string filePath, IEnumerable<IShape> shapes)
        {
            // 1) Преобразуем каждую IShape -> ShapeData
            var dataList = shapes.Select(ConvertToShapeData).ToList();

            // 2) Сериализуем в JSON
            string json = JsonSerializer.Serialize(dataList, _options);

            // 3) Записываем в файл
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Загружает JSON-файл, десериализует массив ShapeData → IShape (включая плагины).
        /// </summary>
        public IEnumerable<IShape> Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");

            string json = File.ReadAllText(filePath);
            var dataList = JsonSerializer.Deserialize<List<ShapeData>>(json, _options);

            if (dataList == null)
                return Enumerable.Empty<IShape>();

            var shapes = new List<IShape>();
            foreach (var data in dataList)
            {
                var shape = ConvertToIShape(data);
                if (shape != null)
                    shapes.Add(shape);
            }

            return shapes;
        }

        #region IShape ↔ ShapeData

        /// <summary>
        /// Преобразует IShape (RectangleShape, TrapezoidShape и т.д.) → DTO ShapeData.
        /// </summary>
        private ShapeData ConvertToShapeData(IShape shape)
        {
            var sd = new ShapeData
            {
                TypeName = shape.TypeName,
                StrokeColor = ColorToHex(shape.StrokeColor),
                FillColor = ColorToHex(shape.FillColor),
                StrokeThickness = shape.StrokeThickness
            };

            // Bounds: X, Y, Width, Height
            var bounds = shape.Bounds;
            sd.BoundsX = bounds.X;
            sd.BoundsY = bounds.Y;
            sd.BoundsWidth = bounds.Width;
            sd.BoundsHeight = bounds.Height;

            // Если есть вершины (Polygon/Polyline/Trapezoid и т.д.)
            if (shape.Points != null && shape.Points.Count > 0)
            {
                sd.Points = shape.Points
                    .Select(p => new PointData(p.X, p.Y))
                    .ToList();
            }
            else
            {
                sd.Points = null;
            }

            return sd;
        }

        /// <summary>
        /// Преобразует DTO ShapeData → IShape (включая поддержку плагинов через _factories).
        /// </summary>
        private IShape? ConvertToIShape(ShapeData data)
        {
            IShape? shape = null;

            // 1) Попробовать создать фигуру через словарь
            if (_factories.TryGetValue(data.TypeName, out var factory))
            {
                shape = factory.Invoke();
            }
            else
            {
                // 2) Если в словаре нет, пробуем встроенные через switch (на случай, если фабрика не зарегистрирована)
                shape = data.TypeName switch
                {
                    "Line" => new LineShape(),
                    "Rectangle" => new RectangleShape(),
                    "Ellipse" => new EllipseShape(),
                    "Polygon" => new PolygonShape(),
                    "Polyline" => new PolylineShape(),
                    _ => null
                };
            }

            if (shape == null)
                return null;

            // 3) Устанавливаем параметры цвета/толщины
            shape.StrokeThickness = data.StrokeThickness;
            shape.StrokeColor = HexToColor(data.StrokeColor);
            shape.FillColor = HexToColor(data.FillColor);

            // 4) Устанавливаем Bounds
            shape.Bounds = new Rect(
                data.BoundsX, data.BoundsY,
                data.BoundsWidth, data.BoundsHeight
            );

            // 5) Восстанавливаем Points, если есть
            if (data.Points != null && data.Points.Count > 0)
            {
                shape.Points = new PointCollection(
                    data.Points.Select(pd => new Point(pd.X, pd.Y))
                );
            }
            else
            {
                shape.Points = new PointCollection();
            }

            return shape;
        }

        #endregion

        #region Color ↔ Hex

        private static string ColorToHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private static Color HexToColor(string hex)
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        #endregion
    }
}
