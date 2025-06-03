using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PaintBox.Models;

namespace PaintBox.Services
{
    /// <summary>
    /// Класс, который умеет загружать плагины (DLL), находить в них все IShapePlugin 
    /// и возвращать список доступных типов фигур.
    /// </summary>
    public static class PluginLoader
    {
        /// <summary>
        /// Ищет в переданном файле (DLL) все типы, реализующие IShapePlugin,
        /// и возвращает список "Name" и фабрик для создания фигур.
        /// </summary>
        public static IEnumerable<IShapePlugin> LoadPlugins(string dllPath)
        {
            if (!File.Exists(dllPath))
                yield break;

            Assembly asm;
            try
            {
                asm = Assembly.LoadFrom(dllPath);
            }
            catch
            {
                yield break;
            }

            foreach (var type in asm.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract)
                    continue;

                if (typeof(IShapePlugin).IsAssignableFrom(type))
                {
                    IShapePlugin pluginInstance = null;
                    try
                    {
                        pluginInstance = (IShapePlugin)Activator.CreateInstance(type);
                    }
                    catch { continue; }

                    if (pluginInstance != null)
                        yield return pluginInstance;
                }
            }
        }
    }
}
