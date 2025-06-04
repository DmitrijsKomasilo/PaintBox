// Путь: PaintBox/ColorNameToBrushConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PaintBox
{
    /// <summary>
    /// Конвертирует строку-имя цвета ("Red", "Blue", "Transparent" и т.д.)
    /// в соответствующий SolidColorBrush.
    /// </summary>
    public class ColorNameToBrushConverter : IValueConverter
    {
        // При конвертации из строки (имени цвета) в Brush:
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorName)
            {
                try
                {
                    // Пробуем распарсить строку как Color
                    var color = (Color)ColorConverter.ConvertFromString(colorName);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    // Если не удалось распознать, вернём прозрачный
                    return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
