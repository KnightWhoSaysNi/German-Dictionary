using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GermanDictionary.ViewModels;

namespace GermanDictionary.Views.Converters
{
    class FilterToComboBoxSelectedItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// From ComboBox SelectedItem to Filter.
        /// </summary>        
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {            
            ComboBoxItem selectedItem = value as ComboBoxItem;
            MainViewModel.WordType wordType = MainViewModel.WordType.All;
            if (selectedItem != null)
            {
                string name = selectedItem.Name;
                switch (name)
                {
                    case "nouns": 
                        wordType = MainViewModel.WordType.Nouns; 
                        break;
                    case "verbs": 
                        wordType = MainViewModel.WordType.Verbs; 
                        break;
                    case "allButNounsAndVerbs":                          
                        wordType = MainViewModel.WordType.AllButNounsAndVerbs;
                        break;
                    default:
                        wordType = MainViewModel.WordType.All;
                        break;
                }
            }            
            return wordType;
        }
    }
}
