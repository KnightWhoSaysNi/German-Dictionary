using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GermanDictionary.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();   
        }

        /// <summary>
        /// Sets SaveRequired to true if any changes are made to the translation
        /// </summary>
        private void UpdateSaveRequired(object sender, TextChangedEventArgs e)
        {
            dynamic parentDataContext = DataContext;
            parentDataContext.SaveRequired = true;
        }
    }
}
