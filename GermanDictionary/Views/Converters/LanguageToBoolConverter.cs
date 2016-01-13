using GermanDictionary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GermanDictionary.Views.Converters
{
    public class LanguageToBoolConverter : IValueConverter
    {
        // If the CurrentLanguage property, as string, is the same as the parameter, returns true
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string currentLanguage = ((LanguageViewModel.Language)value).ToString();
            string languageParameter = parameter as string; // "English" or "Serbian"
            if (languageParameter != null && languageParameter == currentLanguage)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isSelected = (bool)value;
            string languageParameter = parameter as string; // "English" or "Serbian"
            LanguageViewModel.Language language = LanguageViewModel.Language.English;
            
            if (languageParameter == null)
            {
                throw new ArgumentNullException("parameter", "Language parameter not set.");
            }

            if (isSelected)
            {
                if (languageParameter == "Serbian")
                {
                    language = LanguageViewModel.Language.Serbian;
                }
                return language;
            }
            else
            {
                if (languageParameter == "English")
                {
                    language = LanguageViewModel.Language.Serbian;
                }
                return language;
            }            
        }
    }
}
