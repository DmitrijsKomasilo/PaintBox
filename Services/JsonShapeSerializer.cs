using PaintBox.DTO;
using PaintBox.Interfaces;
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
    /// JSON‐сериализатор, работающий с ShapeData DTO
    /// и принимающий словарь всех фабрик (_shapeFactories из MainWindow),
    /// включая плагины.
    /// </summary>
    public class JsonShapeSerializer : IShapeSerializer
    {
        private readonly IReadOnlyDictionary<string, Func<IShape>> _factories;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public JsonShapeSerializer(IReadOnlyDictionary<string, Func<IShape>> factories)
        {
            _factories = factories ?? throw new ArgumentNullException(nameof(factories));
        }

        public void Save(string filePath, IEnumerable<IShape> shapes)
        {
            var dataList = shapes.Select(ConvertToShapeData).ToList();
            string json = JsonSerializer.Serialize(dataList, _options);
            File.WriteAllText(filePath, json);
        }

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

        private ShapeData ConvertToShapeData(IShape shape)
        {
            var sd = new ShapeData
            {
                TypeName = shape.TypeName,
                StrokeColor = ColorToHex(shape.StrokeColor),
                FillColor = ColorToHex(shape.FillColor),
                StrokeThickness = shape.StrokeThickness,
                BoundsX = shape.Bounds.X,
                BoundsY = shape.Bounds.Y,
                BoundsWidth = shape.Bounds.Width,
                BoundsHeight = shape.Bounds.Height,
                Points = shape.Points?.Select(p => new PointData(p.X, p.Y)).ToList()
            };
            return sd;
        }

        private IShape? ConvertToIShape(ShapeData data)
        {
            IShape? shape = null;

            if (_factories.TryGetValue(data.TypeName, out var factory))
            {
                shape = factory.Invoke();
            }
            else
            {
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

            if (shape == null) return null;

            shape.StrokeThickness = data.StrokeThickness;
            shape.StrokeColor = HexToColor(data.StrokeColor);
            shape.FillColor = HexToColor(data.FillColor);
            shape.Bounds = new Rect(data.BoundsX, data.BoundsY, data.BoundsWidth, data.BoundsHeight);

            if (data.Points != null && data.Points.Count > 0)
            {
                shape.Points = new PointCollection(data.Points.Select(pd => new Point(pd.X, pd.Y)));
            }
            else
            {
                shape.Points = new PointCollection();
            }

            return shape;
        }

        private static string ColorToHex(Color color)
            => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

        private static Color HexToColor(string hex)
            => (Color)ColorConverter.ConvertFromString(hex);
    }
}
