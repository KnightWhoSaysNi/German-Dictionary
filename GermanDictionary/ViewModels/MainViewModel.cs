using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;
using GermanDictionary.Models;
using GermanDictionary.Helpers;
using System.Windows.Threading;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Diagnostics;

namespace GermanDictionary.ViewModels
{
    public class MainViewModel : BindableBase
    {
        #region Fields

        private LanguageViewModel languageViewModel;
        private ICollectionView dictionaryView;
        private DispatcherTimer timer;
        private TimeSpan interval;
        private WordType filter;
        private string currentInputWord;
        private string saveLocation;
        private bool filterByTranslation;
        private bool saveRequired;
        private bool openDictionaryAutomatically;

        #endregion

        #region Constructors

        public MainViewModel()
        {
            InitializeFields();
            InitializeDictionary();
            InitializeCommands();
        }

        #endregion  

        #region Events

        public event EventHandler NewDictionaryOpened;
        protected void OnNewDictionaryOpened()
        {
            if (NewDictionaryOpened != null)
            {
                NewDictionaryOpened(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Enums

        public enum WordType { All, Nouns, Verbs, AllButNounsAndVerbs };

        #endregion      
        
        #region Properties

        #region Commands

        public ICommand InsertCommand { get; private set; } 
        public ICommand RemoveCommand { get; private set; }
        public ICommand NewDictionaryCommand { get; private set; } // New on main menu File
        public ICommand OpenDictionaryCommand { get; private set; } // Open on main menu File
        public ICommand SaveDictionaryCommand { get; private set; } // Save or Save As on main menu File
        public ICommand CreateDocxCommand { get; private set; } 
        public ICommand CloseCommand { get; private set; }
        public ICommand ChangeLanguageCommand { get; private set; }

        #endregion

        public LanguageViewModel.Language Language 
        {
            get
            {
                return languageViewModel.CurrentLanguage;
            }
            set
            {
                languageViewModel.CurrentLanguage = value;
                OnPropertyChanged();
                UpdateSavedSettings();
            }
        }
        public SortedObservableCollection<GermanWordTranslationPair> Dictionary { get; set; }
        public ICollectionView DictionaryView
        {
            get
            { 
                return dictionaryView; 
            }
            set
            {
                dictionaryView = value;
                OnPropertyChanged();
            }
        }
        public TimeSpan Interval
        {
            get
            {
                return interval;
            }
            set
            {
                interval = value;
                OnPropertyChanged();
                UpdateSavedSettings();

                if (interval == new TimeSpan(0, 0, 0))
                {
                    timer.Stop();
                }
                else
                {
                    timer.Interval = interval;
                    timer.Start();
                }
            }
        }
        public WordType Filter
        {
            get
            {
                return filter; 
            }
            set
            {
                if (filter != value)
                {
                    filter = value;
                    DictionaryView.Refresh();
                }
            }
        }
        public string CurrentInputWord
        {
            get
            {
                return currentInputWord; 
            }
            set
            {
                currentInputWord = value;
                OnPropertyChanged();
                DictionaryView.Refresh();
            }
        }        
        public string SaveLocation
        {
            get 
            { 
                return saveLocation; 
            }
            set 
            { 
                saveLocation = value;
                OnPropertyChanged();
            }
        }
        public bool FilterByTranslation
        {
            get
            { 
                return filterByTranslation; 
            }
            set
            {
                filterByTranslation = value;
                DictionaryView.Refresh();
            }
        }
        public bool SaveRequired 
        { 
            get
            { 
                return saveRequired; 
            } 
            set
            { 
                saveRequired = value; 
                // TODO make some kind of indicator when save is required and/or when the dictonary is saved
            } 
        }
        public bool OpenDictionaryAutomatically
        {
            get
            {
                return openDictionaryAutomatically;
            }
            set
            {
                openDictionaryAutomatically = value;
                OnPropertyChanged();
                UpdateSavedSettings();
            }
        } 
                
        #endregion 

        #region Methods

        #region Constructor initializations

        /// <summary>
        /// Sets initial values for some fields that need it.
        /// </summary>
        private void InitializeFields()
        {
            languageViewModel = new LanguageViewModel(); // default language = english
            currentInputWord = string.Empty;
            filter = WordType.All;
            timer = new DispatcherTimer();

            timer.Tick += TimerTick;
        }

        /// <summary>
        /// Opens up an existing Dictionary with saved user settings or creates an empty one
        /// with default settings. Sets a filter for the ICollectionView.
        /// </summary>
        private void InitializeDictionary()
        {
            bool isAutomaticallyOpened = IsAutomaticallyOpened();
            if (!isAutomaticallyOpened)
            {
                this.Dictionary = new SortedObservableCollection<GermanWordTranslationPair>();
                this.DictionaryView = CollectionViewSource.GetDefaultView(Dictionary);
                SaveLocation = null;
            }
            DictionaryView.Filter = IsShownInView;
        }

        /// <summary>
        /// Wires up command properties with coresponding relay commands.
        /// </summary>
        private void InitializeCommands()
        {
            InsertCommand = new RelayCommand(InsertWord, CanInsertWord);
            RemoveCommand = new RelayCommand(RemoveWord, CanRemoveWord);
            NewDictionaryCommand = new RelayCommand(StartNewDictionary);
            OpenDictionaryCommand = new RelayCommand(OpenDictionary);
            SaveDictionaryCommand = new RelayCommand(SaveDictionary);
            CreateDocxCommand = new RelayCommand(CreateDocx, CanCreateDocx);
            CloseCommand = new RelayCommand(Close);
            ChangeLanguageCommand = new RelayCommand(ChangeLanguage);
        }

        #endregion


        #region InsertCommand

        /// <summary>
        /// Inserts a new GermanWordTranslationPair into the Dictionary, 
        /// if the word isn't in the Dictionary already.
        /// </summary>
        private void InsertWord(object parameter)
        {
            GermanWordTranslationPair inputPair = new GermanWordTranslationPair(CurrentInputWord);

            if (Dictionary.Contains(inputPair))
            {
                string caption = languageViewModel.GetNotification("InsertWordCaption");
                string message = "\"" + CurrentInputWord + "\"" + languageViewModel.GetNotification("InsertWordMessage");
                MessageBox.Show(message, caption);
            }
            else
            {
                Dictionary.Add(inputPair);
                SaveRequired = true;
            }
        }

        /// <summary>
        /// A check to see if the Insert command can be executed, based on the validity 
        /// of the input word - CurrentInputWord.
        /// </summary>
        /// <returns>True if the CurrentInputWord is a valid input, otherwise false.</returns>
        private bool CanInsertWord(object parameter)
        {
            if (CurrentInputWord.Length != 0 && FilterByTranslation == false)
                return true;

            return false;
        }

        #endregion

        #region RemoveCommand 

        /// <summary>
        /// Removes the GermanWordTranslationPair, given as an argument to the method, from the Dictionary.
        /// </summary>
        /// <param name="parameter">GermanWordTranslationPair to be removed from the Dictionary.</param>
        private void RemoveWord(object parameter)
        {
            GermanWordTranslationPair pair = parameter as GermanWordTranslationPair;
            Dictionary.Remove(pair);
            SaveRequired = true;
        }

        /// <summary>
        /// Checks if the parameter is a GermanWordTranslationPair.
        /// If it is it can be removed from the Dictionary.
        /// </summary>
        /// <returns>True if the parameter is a valid pair, false if it's not.</returns>
        private bool CanRemoveWord(object parameter)
        {
            GermanWordTranslationPair pair = parameter as GermanWordTranslationPair;
            if (pair == null)
                return false;

            return true;
        }

        #endregion

        #region Filtering 

        /// <summary>
        /// Checks if the given object is shown in the view or filtered.
        /// </summary>
        /// <param name="obj">GermanWordTranslationPair to be checked.</param>
        /// <returns>True if it's shown, false if it's hidden.</returns>
        private bool IsShownInView(object obj)
        {
            GermanWordTranslationPair pairToBeChecked = (GermanWordTranslationPair)obj;
            bool isCorrectWordType = IsCorrectWordType(pairToBeChecked);
            if (FilterByTranslation)
            {
                return isCorrectWordType && IsCorrectTranslation(pairToBeChecked);
            }
            else
            {
                return isCorrectWordType && IsCorrectWord(pairToBeChecked);
            }
        }

        /// <summary>
        /// Checks if the word is correct type based on chosen Filter.
        /// </summary>
        private bool IsCorrectWordType(GermanWordTranslationPair pairToBeChecked)
        {
            string word = pairToBeChecked.Word;
            string pattern = string.Empty;
            
            switch (Filter)
            {
                case WordType.Nouns: // Only nouns are visible
                    return pairToBeChecked.WordIsANoun;                    
                case WordType.Verbs:
                    pattern = @"^(?:sich )?\w+en\b"; // Optional start with "sich " and a word ending with "en"
                    break;
                case WordType.AllButNounsAndVerbs:
                    pattern = @"(?x) ^ (?! (?:      # Using negative lookahead, word cannot start with:
                            (?: sich \ \w)          # sich, empty space and then a word character
                            |                       # or
                            (?: d(er|ie|as) \ \w)   # der, die or das, empty space and then a word character
                            |                       # or
                            (?: \w+ en \b)          # a word ending with en              
                            ))";                    // non-capturing groups used for better performance
                    break;
                default: 
                    break; // WordType.All selected so no extra filtering
            }

            bool isCorrectWordType = Regex.IsMatch(word, pattern, RegexOptions.IgnoreCase);
            return isCorrectWordType;
        }

        /// <summary>
        /// Checks if the GermanWordTranslationPair's Word starts with the CurrentInputWord.
        /// </summary>
        /// <returns></returns>
        private bool IsCorrectWord(GermanWordTranslationPair pairToBeChecked)
        {
            string word = pairToBeChecked.Word;
            string revisedWord = string.Empty;

            if (pairToBeChecked.WordIsANoun)
            {
                // Gets a word without articles for gender
                revisedWord = word.Substring(4);
            }
            
            bool isStartingWithInputWord = word.StartsWith(CurrentInputWord, true, CultureInfo.InvariantCulture) ||
                revisedWord.StartsWith(CurrentInputWord, true, CultureInfo.InvariantCulture);

            return isStartingWithInputWord;            
        }

        /// <summary>
        /// Checks if the GermanWordTranslationPair's Translation contains CurrentInputWord.
        /// </summary>
        private bool IsCorrectTranslation(GermanWordTranslationPair pairToBeChecked)
        {
            // For clarity's sake changing variable name into something more appropriate            
            string currentInputTranslation = CurrentInputWord;
            string translation = pairToBeChecked.Translation;
            
            // Case should be ignored
            currentInputTranslation = currentInputTranslation.ToLower();
            translation = translation.ToLower();
            
            bool containsWord = translation.Contains(currentInputTranslation);
            return containsWord;            
        }

        #endregion

        #region NewDictionaryCommand

        /// <summary>
        /// Starts a new Dictionary with unset (null) SaveLocation.
        /// </summary>
        private void StartNewDictionary(object parameter)
        {
            string caption = languageViewModel.GetNotification("StartNewDictionaryCaption");
            AskToSave(caption, false);

            Dictionary.Clear();
            SaveLocation = null;
            SaveRequired = false;
            UpdateSavedSettings();
        }

        #endregion

        #region Open Dictionary methods

        /// <summary>
        /// Reads SavedSettings.txt (if it exists) and if so specified by the user, tries to open the last saved
        /// Dictionary automatically from the given save location and with the chosen settings.
        /// </summary>
        /// <returns>True if the last saved Dictionary is opened automatically, false otherwise.</returns>
        private bool IsAutomaticallyOpened()
        {
            List<object> savedSettings = GetSavedSettings();
            if (savedSettings == null)
            {
                // SavedSettings.txt changed or deleted. Dictionary cannot be opened automatically
                return false;
            }

            string lastSaveLocation = (string)savedSettings[0];
            OpenDictionaryAutomatically = (bool)savedSettings[1];
            Interval = (TimeSpan)savedSettings[2];
            Language = (LanguageViewModel.Language)savedSettings[3];

            if (OpenDictionaryAutomatically == true)
            {
                if (lastSaveLocation == string.Empty)
                {
                    // New dictionary created on last use and it wasn't saved
                    return false;
                }

                if (!File.Exists(lastSaveLocation))
                {
                    string messageBoxCaption = languageViewModel.GetNotification("DictionaryNotOpenedCaption");
                    string message = languageViewModel.GetNotification("IsAutomaticallyOpenedMessage");
                    MessageBox.Show(message, messageBoxCaption);
                    // File doesn't exist, new dictionary is opened with default settings
                    return false;
                }

                // Dictionary should be automatically opened
                // Tries to open the Dictionary from the aquired save location                        
                SaveLocation = lastSaveLocation;
                bool isOpened = IsDictionaryOpened();
                return isOpened;
            }            

            // Dictionary wasn't automatically opened
            return false;
        }

        /// <summary>
        /// Gets saved settings from the SavedSettings.txt as a List of objects. On index 0 should be a 
        /// string, on index 1 a bool value, on index 2 a TimeSpan and on index 3 Language enum. If 
        /// SavedSettings.txt has been tempered with returns null.
        /// </summary>
        /// <returns>List of objects holding the settings, or null if they can't be read.</returns>
        private List<object> GetSavedSettings()
        {
            List<object> settings = new List<object>(4);

            try
            {
                // Trying to read SavedSettings.txt, which should be in format:
                // Line 1: "Last save location            = xxx" where xxx is the full path to the saved Dictionary 
                // Line 2: "Open dictionary automatically = xxx" where xxx is either true or false
                // Line 3: "Automatic save interval       = xxx" where xxx is a TimeSpan
                // Line 4: "Chosen language               = xxx" where xxx is either English or Serbian
                string[] lines = File.ReadAllLines("SavedSettings.txt");
                if (lines.Length < 4)
                {
                    // SavedSettings.txt has been changed
                    return null;
                }
                string firstLine  = lines[0];
                string secondLine = lines[1];
                string thirdLine  = lines[2];
                string fourthLine = lines[3];

                string lastSaveLocation = string.Empty;
                bool openAutomatically = false;
                TimeSpan saveInterval = new TimeSpan(0, 0, 0);
                LanguageViewModel.Language language = LanguageViewModel.Language.English;

                try
                {
                    firstLine  = firstLine.Substring(32).Trim();
                    secondLine = secondLine.Substring(32).Trim();
                    thirdLine  = thirdLine.Substring(32).Trim();
                    fourthLine = fourthLine.Substring(32).Trim();

                    lastSaveLocation = firstLine;
                    bool.TryParse(secondLine, out openAutomatically);
                    TimeSpan.TryParse(thirdLine, out saveInterval);
                    Enum.TryParse(fourthLine, out language);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // SavedSettings.txt has been changed 
                    return null;
                }

                settings.Add(lastSaveLocation);
                settings.Add(openAutomatically);
                settings.Add(saveInterval);
                settings.Add(language);
            }
            catch (FileNotFoundException)
            {
                // SavedSettings.txt deleted  
                Debug.WriteLine("SavedSettings.txt file not found. Starting with default settings.");
                return null;
            }

            return settings;
        }

        /// <summary>
        /// Checks if the Dictionary is opened from the saved location.
        /// Used for automatic opening of the Dictionary on startup.
        /// </summary>
        /// <returns>True if Dictionary is successfully opened, false otherwise.</returns>
        private bool IsDictionaryOpened()
        {
            bool isFileDeserialized = false;
            string messageBoxCaption = languageViewModel.GetNotification("DictionaryNotOpenedCaption");
            try
            {
                Deserialize(SaveLocation);
                isFileDeserialized = true;
            }
            #region Catch blocks   
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, messageBoxCaption); // this should not occur
            }
            catch (System.Runtime.Serialization.SerializationException)
            {
                string message = languageViewModel.GetNotification("IsDictionaryOpenedMessage");
                MessageBox.Show(message, messageBoxCaption);                
            }
            #endregion

            return isFileDeserialized;
        }

        /// <summary>
        /// Deserializes the Dictionary from the given file path.
        /// </summary>
        /// <param name="filePath">Full path of the file holding the saved Dictionary.</param>
        /// <exception cref="System.UnauthorizedAccessException" />
        /// <exception cref="System.IO.IOException" />
        /// <exception cref="System.Runtime.Serialization.SerializationException" />
        private void Deserialize(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                this.Dictionary = (SortedObservableCollection<GermanWordTranslationPair>)formatter.Deserialize(stream);

                // Dictionary is successfully opened
                this.DictionaryView = CollectionViewSource.GetDefaultView(Dictionary);
                DictionaryView.Filter = IsShownInView;
                SaveLocation = filePath;
                SaveRequired = false;

                OnNewDictionaryOpened();
                UpdateSavedSettings();
            }   
        }

        /// <summary>
        /// Opens a saved Dictionary.
        /// </summary>
        private void OpenDictionary(object parameter)
        {
            string messageBoxCaption = languageViewModel.GetNotification("DictionaryNotOpenedCaption");
            string message = languageViewModel.GetNotification("OpenDictionaryCommandDefaultMessage");
            try
            {                
                OpenDictionary();
            }
            #region Catch blocks
            catch (UnauthorizedAccessException)
            {
                message = languageViewModel.GetNotification("OpenDictionaryCommandMessage");
                MessageBox.Show(message, messageBoxCaption);
            }
            catch (IOException)
            {
                MessageBox.Show(message, messageBoxCaption);               
            }
            catch (System.Runtime.Serialization.SerializationException)
            {
               MessageBox.Show(message, messageBoxCaption);
            }
            #endregion
        }

        /// <summary>
        /// Tries to open a saved Dictionary with OpenFileDialog.
        /// </summary>
        private void OpenDictionary()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.DefaultExt = "bin";
            openDialog.Filter = "Bin files (*.bin)|*.bin|All files (*.*)|*.*";
            openDialog.InitialDirectory = Directory.GetCurrentDirectory();
            bool hasCorrectExtension = false;

            while (!hasCorrectExtension)
            {
                if (openDialog.ShowDialog() == true)
                {
                    string filePath = openDialog.FileName;
                    hasCorrectExtension = Path.GetExtension(filePath) == ".bin";
                    if (hasCorrectExtension)
                    {
                        string caption = languageViewModel.GetNotification("OpenDictionaryAskToSaveCaption");
                        AskToSave(caption, false);   
                     
                        Deserialize(filePath);
                    }
                    else
                    {
                        string messageBoxCaption = languageViewModel.GetNotification("DictionaryNotOpenedCaption");
                        string message = languageViewModel.GetNotification("OpenDictionaryMessage");
                        MessageBox.Show(message, messageBoxCaption);
                    }
                }
                else
                {
                    // User doesn't want to open a dictionary
                    return;
                }
            }
        }

        #endregion

        #region Save Dictionary methods and UpdateSavedSettings

        /// <summary>
        /// Asks the user if he/she wishes to save the dictionary. If used on opening a new dictionary
        /// hasCancel parameter should be false. If used on closing the application it should be true.
        /// </summary>
        /// <param name="caption">MessageBox caption, based on where the method is called from.</param>
        /// <param name="hasCancel">True if the app is closing, false otherwise.</param>
        public MessageBoxResult AskToSave(string caption, bool hasCancel)
        {
            // Default result is No, which means that the user doesn't want to save the dictionary
            MessageBoxResult shouldSave = MessageBoxResult.No;

            if (SaveRequired)
            {
                string message = languageViewModel.GetNotification("AskToSaveMessage");
                MessageBoxButton buttons = hasCancel ? MessageBoxButton.YesNoCancel : MessageBoxButton.YesNo;
                shouldSave = MessageBox.Show(message, caption, buttons, MessageBoxImage.Question);

                if (shouldSave == MessageBoxResult.Yes)
                {
                    SaveDictionary(false);
                }                
            }
            return shouldSave;
        }

        /// <summary>
        /// Saves the Dictionary as a binary file. If Save As is called, or the current 
        /// SaveLocation is null, the user is first asked where he/she wants to save the file.
        /// </summary>
        /// <param name="saveAs">
        /// Bool parameter indicating whether Save As is called. Use false if you only
        /// want to use Save, although if there is no previous save location, Save As
        /// will be called anyway.
        /// </param>
        public void SaveDictionary(object saveAs)
        {
            bool isSaveAsCalled = (bool)saveAs;
            if (SaveLocation == null || isSaveAsCalled)
            {
                string filePath = GetSaveLocation("bin");
                if (filePath == null)
                {
                    // The user doesn't want to save the dictionary
                    return;
                }
                Serialize(filePath);
            }
            else
            {
                Serialize(SaveLocation);
            }
        }                

        /// <summary>
        /// Gets save location, i.e. file path, with the help of SaveFileDialog.
        /// Returns null if the user doesn't want to save the file.
        /// </summary>
        /// <param name="extension">Default extension to use.</param>
        /// <returns>Save location or null, if the user doesn't want to save the file.</returns>
        private string GetSaveLocation(string extension)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = extension;
            string docxFilter = "Word document (*.docx)|*.docx";
            string binFilter = string.Format("Bin files (*.bin)|*.bin|All files (*.)|*.*");
            string dialogFilter = extension == "docx" ? docxFilter : binFilter;
            saveDialog.Filter = dialogFilter;
            saveDialog.InitialDirectory = Directory.GetCurrentDirectory(); 
            
            string filePath = null;
            bool hasCorrectExtension = false;

            while (!hasCorrectExtension)
            {
                if (saveDialog.ShowDialog() == true)
                {
                    hasCorrectExtension = saveDialog.FileName.EndsWith("." + extension);
                    if (hasCorrectExtension)
                    {
                        filePath = saveDialog.FileName;                        
                    }
                    else
                    {
                        string messageBoxCaption = languageViewModel.GetNotification("DictionaryNotSavedCaption");
                        string message = languageViewModel.GetSaveDialogMessage(saveDialog.SafeFileName, extension);
                        MessageBox.Show(message, messageBoxCaption);

                        saveDialog.InitialDirectory = saveDialog.FileName; // initial directory is the last one opened
                        saveDialog.FileName = string.Empty;
                    }
                }
                else
                {
                    //User doesn't want to save the dictionary
                    break;
                }
            }
            return filePath;
        }

        /// <summary>
        /// Serializes the Dictionary.
        /// </summary>
        /// <param name="filePath">Full file path of the binary file that will hold the Dictionary.</param>
        private void Serialize(string filePath)
        {
            string messageBoxCaption = languageViewModel.GetNotification("DictionaryNotSavedCaption");
            string message = languageViewModel.GetNotification("SerializeMessage");
            try
            {
                using (Stream stream = File.Open(filePath, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, this.Dictionary);

                    // Dictionary is successfully saved
                    SaveLocation = filePath;
                    UpdateSavedSettings();
                    SaveRequired = false;
                }
            }
            catch (IOException)
            {
                MessageBox.Show(message, messageBoxCaption);
            }
            catch (System.Runtime.Serialization.SerializationException) // may be redundant
            {
                MessageBox.Show(message, messageBoxCaption);
            }
        }
        
        /// <summary>
        /// Updates the SavedSettings.txt file, which holds the information about the
        /// user's preferences and the last save location for the Dictionary.
        /// </summary>
        private void UpdateSavedSettings()
        {
            using (StreamWriter writer = new StreamWriter("SavedSettings.txt", false))
            {
                writer.WriteLine("Last save location            = {0}", SaveLocation);
                writer.WriteLine("Open dictionary automatically = {0}", OpenDictionaryAutomatically );
                writer.WriteLine("Automatic save interval       = {0}", Interval);
                writer.WriteLine("Chosen language               = {0}", Language);

                string notification = languageViewModel.GetNotification("UpdateSavedSettings");
                writer.WriteLine(notification);
            }
        }

        #endregion

        #region Docx creation

        /// <summary>
        /// If there are no words in the Dictionary the command cannot be executed.
        /// </summary>        
        private bool CanCreateDocx(object obj)
        {
            return Dictionary.Count != 0 ? true : false;
        }

        /// <summary>
        /// Creates a word document representation of the Dictionary.
        /// </summary>
        private void CreateDocx(object obj)
        {
            string filePath = GetSaveLocation("docx");
            if (filePath == null)
            {
                // The user doesn't want to save the file
                return;
            }
            CreateDocument(filePath);
        }

        /// <summary>
        /// Creates a WordprocessingDocument.
        /// </summary>
        /// <param name="filePath">Save location for the document.</param>
        private void CreateDocument(string filePath)
        {
            try
            {                
                // Create the Word document
                using (var processingDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
                {     
                    Body body = GetDocumentBody(processingDocument);

                    for (int i = 0; i < Dictionary.Count; i++)
                    {
                        // Create a paragraph instance
                        Paragraph paragraph = body.AppendChild(new Paragraph());

                        var pair = Dictionary[i];
                        string word = pair.Word.TrimEnd(new char[] { ' ', '/', '(' }); // word can't end with these characters
                        string translation = pair.Translation.TrimEnd();
                        translation = Regex.Replace(translation, @"\t|\r|\n", " "); // everything on a single line
                        translation = "  =  " + translation;
                        
                        CreateFirstLetterRun(paragraph, pair, i);                        
                        CreateWordRun(paragraph, word);
                        CreateTranslationRun(paragraph, translation);
                    }
                }
            }
            catch (IOException)
            {
                string caption = languageViewModel.GetNotification("CreateDocumentCaption");
                string message = languageViewModel.GetNotification("CreateDocumentMessage");
                MessageBox.Show(message, caption);
            }
        }

        /// <summary>
        /// Populates the processing document and gets its body element.
        /// </summary>
        /// <returns>Body of the document that will hold the dictionary.</returns>
        private Body GetDocumentBody(WordprocessingDocument processingDocument)
        {
            // Create a MainDocumentPart instance
            MainDocumentPart mainDocPart = processingDocument.AddMainDocumentPart();
            mainDocPart.Document = new Document();

            // Create a Document instance
            Document document = mainDocPart.Document;

            // Create a Body instance
            Body body = document.AppendChild(new Body());


            // Create 2 columns for page layout
            SectionProperties sectionProperties = new SectionProperties();
            Columns columns = new Columns() { ColumnCount = 2 };
            columns.Separator = OnOffValue.FromBoolean(true);
            sectionProperties.Append(columns);

            body.Append(sectionProperties);

            return body;
        }

        /// <summary>
        /// Creates a run on every new letter occurence, holding the first letter as text,
        /// for the specified paragraph.
        /// </summary>
        /// <param name="pair">Pair whose word's first letter is checked.</param>
        private void CreateFirstLetterRun(Paragraph parentParagraph, GermanWordTranslationPair pair, int index)
        {
            // Set font size, bold and underline properties
            RunProperties firstLetterRunProp = new RunProperties(new Bold());
            Underline underlinedLetter = new Underline() { Val = UnderlineValues.Single };
            FontSize letterSize = new FontSize() { Val = StringValue.FromString("26") };
            firstLetterRunProp.Append(underlinedLetter, letterSize);

            // Create run
            Run firstLetterRun = new Run();
            Text firstLetterText = new Text();
            firstLetterRun.Append(firstLetterRunProp);            
            firstLetterRun.Append(firstLetterText, new Break(), new Break(), new Break());
            
            string firstLetter = GetFirstLetter(pair);
            if (index == 0)
            {
                // Write the first letter of the first word
                firstLetterText.Text = firstLetter.ToUpper();
                parentParagraph.Append(firstLetterRun);
            }
            else
            {
                var previousPair = Dictionary[index - 1];
                string previousFirstLetter = GetFirstLetter(previousPair);

                if (firstLetter != previousFirstLetter)
                {
                    // New letter starting, add 2 empty lines before adding the letter
                    firstLetterText.Text = firstLetter.ToUpper();
                    parentParagraph.Append(new Break(), new Break(), firstLetterRun);
                }
            }
        }
        
        /// <summary>
        /// Gets the first letter of the given GermanWordTranslationPair. Ignores first 4 
        /// letters of the Word property string if the word is a noun.
        /// </summary>
        private string GetFirstLetter(GermanWordTranslationPair pair)
        {
            if (pair.WordIsANoun)
            {
                return pair.Word[4].ToString().ToLower();
            }
            else
            {
                return pair.Word[0].ToString().ToLower();
            }
        }

        /// <summary>
        /// Creates a run with the given word as text, for the specified paragraph.
        /// </summary>
        private void CreateWordRun(Paragraph parentParagraph, string word)
        {
            Run wordRun = new Run(new RunProperties(new Bold()));
            Text wordText = new Text();
            wordText.Text = word;
            wordRun.Append(wordText);
            parentParagraph.Append(wordRun);
        }

        /// <summary>
        /// Creates a run with the given translation as text, for the specified paragraph.
        /// </summary>
        private void CreateTranslationRun(Paragraph parentParagraph, string translation)
        {
            Run translationRun = new Run();
            Text translationText = new Text() { Space = SpaceProcessingModeValues.Preserve };
            translationText.Text = translation;
            translationRun.Append(translationText);
            parentParagraph.Append(translationRun);
        }

        #endregion

        #region CloseCommand

        /// <summary>
        /// If window is given as an argument, closes that window.
        /// </summary>
        private void Close(object parameter)
        {
            Window window = parameter as Window;
            if (window != null)
                window.Close();
        }

        #endregion

        #region Timer

        /// <summary>
        /// If save location is not null saves dictionary on each tick.
        /// </summary>        
        private void TimerTick(object sender, EventArgs e)
        {
            if (SaveLocation != null)
            {
                SaveDictionary(false);
            }
        }

        #endregion

        #region ChangeLanguageCommand

        /// <summary>
        /// Changes the language based on the given parameter.
        /// </summary>
        /// <param name="parameter">English or Serbian.</param>
        private void ChangeLanguage(object parameter)
        {
            string chosenLanguage = parameter as string;
            if (chosenLanguage == "Serbian")
            {
                Language = LanguageViewModel.Language.Serbian;
            }
            else
            {
                Language = LanguageViewModel.Language.English;
            }
        }

        #endregion

        #endregion

    }
}
