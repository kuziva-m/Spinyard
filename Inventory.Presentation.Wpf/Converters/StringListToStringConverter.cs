// In Inventory.Presentation.Wpf/Converters/StringListToStringConverter.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Inventory.Presentation.Wpf.Converters
{
    public class StringListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<string> list && list.Any())
            {
                return string.Join(", ", list);
            }
            return "Base Product"; // Fallback text
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}