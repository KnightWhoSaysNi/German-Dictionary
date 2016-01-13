using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GermanDictionary.Views.Converters
{
    // Limits the height of the object by multiplying the value with the given parameter
    public class FontSizeToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double fontSize = (double)value;
            double ratio = (double)parameter;
            double height = fontSize * ratio;
            return height;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
