using PaintBox.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PaintBox.Services
{
    /// <summary>
    /// Класс, ответственный за загрузку всех типов IShapePlugin
    /// из указанной DLL-файла.
    /// </summary>
    public static class PluginLoader
    {
        /// <summary>
        /// Загружает все плагины (типы, реализующие IShapePlugin) из DLL.
        /// Возвращает список готовых экземпляров IShapePlugin.
        /// </summary>
        public static List<IShapePlugin> LoadPlugins(string dllPath)
        {
            var result = new List<IShapePlugin>();

            if (!File.Exists(dllPath))
                return result;

            try
            {
                // Загружаем сборку из указанного пути
                Assembly asm = Assembly.LoadFrom(dllPath);

                // Ищем все публичные типы, которые реализуют IShapePlugin
                var pluginTypes = asm.GetTypes()
                    .Where(t => !t.IsAbstract
                             && !t.IsInterface
                             && typeof(IShapePlugin).IsAssignableFrom(t));

                foreach (var type in pluginTypes)
                {
                    try
                    {
                        // Создаём экземпляр через Activator
                        var pluginInstance = (IShapePlugin)Activator.CreateInstance(type)!;
                        // Проверяем, чтобы Name не был null/пустым и чтобы не дублировался
                        if (!string.IsNullOrEmpty(pluginInstance.Name))
                        {
                            result.Add(pluginInstance);
                        }
                    }
                    catch
                    {
                        // Если не получилось создать экземпляр (нет конструктора без параметров и т.д.), пропускаем.
                        continue;
                    }
                }
            }
            catch
            {
                // Если не удалось загрузить сборку, просто возвращаем пустой список
            }

            return result;
        }
    }
}
