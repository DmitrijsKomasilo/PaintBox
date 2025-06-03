using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PaintBox.Models;

namespace PaintBox.Services
{
    /// <summary>
    /// Простейшая JSON-сериализация: сохраняет в файл все объекты IShape с их свойствами.
    /// </summary>
    public class JsonShapeSerializer : IShapeSerializer
    {
        public void Save(string path, IEnumerable<IShape> shapes)
        {
            // Упрощённо: Serializaем в JSON, используя System.Text.Json.
            // Нужно, чтобы все ConcreteShape (LineShape, RectangleShape и т.д.) были [Serializable]

            // 1) сделать у ShapeBase и всех наследников [Serializable],
            // 2) в JsonSerializerOptions указать: IncludeFields = true, 
            //    или прописать атрибут [JsonInclude] над полями.
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true,
                // Для десериализации интерфейсов, потребуется конвертер,
                // который по TypeName умеет создать нужный конкретный класс.
            };
            using var file = File.Create(path);
            JsonSerializer.Serialize(file, shapes, options);
        }

        public IEnumerable<IShape> Load(string path)
        {
            // Сложность: JsonSerializer не умеет «сразу десериализовать»
            // в интерфейсы. Нужно писать кастомный JsonConverter<IShape>,
            // который по полю TypeName создаёт нужный объект (LineShape и т.д.).

            // 1) Загрузить сырые JSON-объекты,
            // 2) Прочитать у каждого поля TypeName,
            // 3) Создать вручную экземпляр нужного класса и заполнить поля.
            return new List<IShape>();
        }
    }
}
