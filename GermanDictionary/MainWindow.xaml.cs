using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using GermanDictionary.Models;
using GermanDictionary.Views;
using GermanDictionary.ViewModels;
using System.Collections.Specialized;


namespace GermanDictionary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {  
        private MainViewModel viewModel;
                
        public MainWindow()
        {
            viewModel = new MainViewModel();
            this.DataContext = viewModel;

            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }
        
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Since Dictionary reference points to null on deserialization, CollectionChanged 
            // is reset and is set again in NewDictionaryOpened's handler
            viewModel.Dictionary.CollectionChanged += CollectionChanged;    
            viewModel.NewDictionaryOpened += viewModel_NewDictionaryOpened; 
        }
        
        /// <summary>
        /// Clears insert text box and selected word and translation text boxes and attaches a handler 
        /// to the CollectionChanged event.
        /// </summary>
        void viewModel_NewDictionaryOpened(object sender, EventArgs e)
        {
            mainView.allWords.UnselectAll();
            mainView.insertBox.Clear();
            mainView.insertBox.Focus();

            viewModel.Dictionary.CollectionChanged += CollectionChanged;
        }       


        // TODO see if using this can be avoided somehow
        // High class coupling
        void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var action = e.Action;
            if (action == NotifyCollectionChangedAction.Add)
            {
                // Insert box cleared to get full list in list box
                mainView.insertBox.Clear();                

                // New item added. That item should get selected right away
                // in the list box, and translation box needs to get focus
                var newItem = e.NewItems[0];

                if (!mainView.allWords.Items.Contains(newItem))
                {
                    // List box is not showing the new item because of filtering
                    // so filter needs to be set to default (shows all words)
                    mainView.filter.SelectedItem = mainView.filter.Items[0];
                }
                mainView.allWords.SelectedItem = e.NewItems[0];
                mainView.translation.Focus();
                mainView.translation.SelectAll();
            }
        }
        
        /// <summary>
        /// If required asks the user if he/she wants to save the dictionary.
        /// </summary>
        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            string caption = "Dictionary closing";
            if (viewModel.Language.ToString() == "Serbian")
            {
                caption = "Zatvaranje recnika.";
            }

            MessageBoxResult result = viewModel.AskToSave(caption, true);

            if (result == MessageBoxResult.Cancel)
            {
                // The user changed his/her mind about closing
                e.Cancel = true;
            }
        }

    }      
}
