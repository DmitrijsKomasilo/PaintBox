﻿using PaintBox.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PaintBox.Services
{
    public static class PluginLoader
    {
        public static List<IShapePlugin> LoadPlugins(string dllPath)
        {
            var result = new List<IShapePlugin>();
            if (!File.Exists(dllPath)) return result;

            try
            {
                var asm = Assembly.LoadFrom(dllPath);
                var pluginTypes = asm.GetTypes()
                    .Where(t => !t.IsAbstract
                             && !t.IsInterface
                             && typeof(IShapePlugin).IsAssignableFrom(t));

                foreach (var type in pluginTypes)
                {
                    try
                    {
                        var pluginInstance = (IShapePlugin)Activator.CreateInstance(type)!;
                        if (!string.IsNullOrEmpty(pluginInstance.Name))
                            result.Add(pluginInstance);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch
            {
            }

            return result;
        }
    }
}
