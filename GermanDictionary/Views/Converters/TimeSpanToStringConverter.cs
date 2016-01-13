using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GermanDictionary.Views.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {              
        /// <summary>
        /// From TimesSpan.Minutes to string.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return "0";

            TimeSpan timeSpan = (TimeSpan)value;
            return timeSpan.Minutes;            
        }

        /// <summary>
        /// From string to TimeSpan. Value represents minutes.
        /// </summary>        
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string text = value as string;
            if (text == null)
            {
                return new TimeSpan(0, 0, 0);
            }
            int number;
            bool isParsed = int.TryParse(text, out number);
            if (number > 0)
            {
                return new TimeSpan(0, number, 0);
            }

            return new TimeSpan(0, 0, 0);
        }
    }
}
