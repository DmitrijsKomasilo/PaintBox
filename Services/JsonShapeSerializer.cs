using PaintBox.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace PaintBox.Services
{
    /// <summary>
    /// Класс для сериализации/десериализации фигур в JSON.
    /// Использует DTO ShapeData и PointData.
    /// </summary>
    public class JsonShapeSerializer : IShapeSerializer
    {
        // Опции для JsonSerializer (красивый вывод, без циклических ссылок)
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            // Чтобы свойство Points (List<PointData>) корректно
            // сериализовалось даже если null.
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Сохраняет список фигур в JSON-файл.
        /// </summary>
        public void Save(string filePath, IEnumerable<IShape> shapes)
        {
            // Конвертируем каждую IShape → ShapeData
            List<ShapeData> dataList = shapes
                .Select(shape => ConvertToShapeData(shape))
                .ToList();

            // Сериализуем в строку JSON
            string json = JsonSerializer.Serialize(dataList, _options);

            // Записываем в файл
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Загружает JSON‐файл, десериализует массив ShapeData и создает реальные IShape.
        /// </summary>
        public IEnumerable<IShape> Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");

            string json = File.ReadAllText(filePath);
            var dataList = JsonSerializer.Deserialize<List<ShapeData>>(json, _options);

            if (dataList == null)
                return Enumerable.Empty<IShape>();

            List<IShape> shapes = new List<IShape>();

            foreach (var data in dataList)
            {
                var shape = ConvertToIShape(data);
                if (shape != null)
                    shapes.Add(shape);
            }

            return shapes;
        }

        #region Преобразования: IShape ↔ ShapeData

        /// <summary>
        /// Создаёт DTO ShapeData из IShape.
        /// </summary>
        private ShapeData ConvertToShapeData(IShape shape)
        {
            var sd = new ShapeData
            {
                TypeName = shape.TypeName,

                // Цвета: преобразуем Color → строку "#AARRGGBB"
                StrokeColor = ColorToHex(shape.StrokeColor),
                FillColor = ColorToHex(shape.FillColor),

                StrokeThickness = shape.StrokeThickness
            };

            // Если есть Bounds (Line/Rect/Ellipse)
            var bounds = shape.Bounds;
            sd.BoundsX = bounds.X;
            sd.BoundsY = bounds.Y;
            sd.BoundsWidth = bounds.Width;
            sd.BoundsHeight = bounds.Height;

            // Если фигура содержит Points (например, Polygon или Polyline)
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
        /// Восстанавливает IShape на основе данных ShapeData.
        /// Для неизвестных TypeName возвращает null.
        /// </summary>
        private IShape? ConvertToIShape(ShapeData data)
        {
            // По имени типа создаём экземпляр нужного класса
            IShape? shape = data.TypeName switch
            {
                "Line" => new LineShape(),
                "Rectangle" => new RectangleShape(),
                "Ellipse" => new EllipseShape(),
                "Polygon" => new PolygonShape(),
                "Polyline" => new PolylineShape(),

                // Если у вас в будущем будут плагины, 
                // проверка на них может идти так (pseudo-код):
                // var pluginFactory = PluginLoader.GetFactoryByName(data.TypeName);
                // if (pluginFactory != null) shape = pluginFactory();
                _ => null
            };

            if (shape == null)
                return null;

            // Заполняем параметры цвета/толщины
            shape.StrokeThickness = data.StrokeThickness;
            shape.StrokeColor = HexToColor(data.StrokeColor);
            shape.FillColor = HexToColor(data.FillColor);

            // Устанавливаем Bounds
            shape.Bounds = new System.Windows.Rect(
                data.BoundsX, data.BoundsY,
                data.BoundsWidth, data.BoundsHeight
            );

            // Если есть вершины – восстанавливаем Points
            if (data.Points != null && data.Points.Count > 0)
            {
                shape.Points = new System.Windows.Media.PointCollection(
                    data.Points.Select(pd => new System.Windows.Point(pd.X, pd.Y))
                );
            }
            else
            {
                // Если это Polyline/Polygon, но Points отсутствуют, 
                // оставляем пустую коллекцию
                shape.Points = new System.Windows.Media.PointCollection();
            }

            return shape;
        }

        #endregion

        #region Утилиты для Color ↔ Hex

        /// <summary>
        /// Преобразует System.Windows.Media.Color в строку "#AARRGGBB".
        /// </summary>
        private static string ColorToHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// Преобразует строку "#AARRGGBB" в System.Windows.Media.Color.
        /// </summary>
        private static Color HexToColor(string hex)
        {
            // Предполагаем, что hex в виде "#AARRGGBB" или "#RRGGBB"
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        #endregion
    }
}
