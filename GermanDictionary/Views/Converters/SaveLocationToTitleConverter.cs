using GermanDictionary.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GermanDictionary.Views.Converters
{
    public class SaveLocationToTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string saveLocation = values[0] as string;
            LanguageViewModel.Language language =(LanguageViewModel.Language) values[1];
            string title = "German dictionary";
            if (language == LanguageViewModel.Language.Serbian)
            {
                title = "Recnik nemackog jezika";
            }

            if (saveLocation != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(saveLocation);
                title += " - " + fileName;                                
            }
            return title;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] unsetValues = new object[] { DependencyProperty.UnsetValue, DependencyProperty.UnsetValue };
            return unsetValues;
        }
    }
}
