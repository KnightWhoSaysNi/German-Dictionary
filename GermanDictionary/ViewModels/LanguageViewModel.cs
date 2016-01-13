using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GermanDictionary.ViewModels
{
    /// <summary>
    /// Main view model helper, holding the language property and
    /// notifications based on the selected language.
    /// </summary>
    public class LanguageViewModel
    {
        #region Fields

        private Dictionary<string, List<string>> notifications;

        #endregion

        #region Contructor

        /// <summary>
        /// Creates a helper view model with the language property, 
        /// which is set to english by default.
        /// </summary>
        /// <param name="language">Default language.</param>
        public LanguageViewModel(Language language = Language.English)
        {
            CurrentLanguage = language;
            notifications = new Dictionary<string, List<string>>();
            FillNotifications();
        }

        #endregion

        #region Language enum and property

        public enum Language { English, Serbian };

        public Language CurrentLanguage { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a string for MessageBox caption or message, in english or
        /// serbian language, based on the given notification key for the
        /// dictionary holding these notifications.
        /// </summary>
        public string GetNotification(string notificationKey)
        {
            if (CurrentLanguage == Language.English)
            {
                return notifications[notificationKey][0];
            }
            else
            {
                return notifications[notificationKey][1];
            }
        }

        
        // Gets a string message for the MessageBox used in GetSaveLocation method
        public string GetSaveDialogMessage(string fileName, string extension)
        {
            string message = string.Format("You cannot save a dictionary as \"{0}\". Also, it's not necessary for you" +
                " to add .{1} extension yourself if you wish to create a new file. (If you wish to replace an existing" +
                " dictionary, know that it is a file with .{1} extension", fileName, extension);

            if (CurrentLanguage == Language.Serbian)
            {
                message = string.Format("Ne mozete snimiti recnik kao \"{0}\". Takodje, nije potrebno sami da " +
                                "dodajete .{1} ekstenziju ukoliko zelite da napravite novi fajl. (Ukoliko zelite da zamenite" +
                                " postojeci recnik, znajte da je to fajl sa .{1} ekstenzijom)", fileName, extension);
            }
            return message;
        }

        /// <summary>
        /// Fills in the notifications dictionary with lists of english and serbian notifications
        /// for the a variety of different keys. Used for MessageBox caption and message
        /// notifications for english and serbian language. [0] in the list is always english
        /// and [1] is always serbian.
        /// </summary>
        private void FillNotifications()
        {
            // Dictionary not opened notification
            notifications.Add("DictionaryNotOpenedCaption", new List<string>());
            notifications["DictionaryNotOpenedCaption"].Add("Dictionary not opened!");
            notifications["DictionaryNotOpenedCaption"].Add("Recnik nije ucitan!");

            // Dictionary not saved notifications
            notifications.Add("DictionaryNotSavedCaption", new List<string>());
            notifications["DictionaryNotSavedCaption"].Add("Dictionary not saved!");
            notifications["DictionaryNotSavedCaption"].Add("Recnik nije snimljen!");

            // InsertWord method notifications
            notifications.Add("InsertWordCaption", new List<string>());
            notifications["InsertWordCaption"].Add("Word is already inserted!");
            notifications["InsertWordCaption"].Add("Rec je vec uneta!");
            
            notifications.Add("InsertWordMessage", new List<string>());
            notifications["InsertWordMessage"].Add(" is already in the dictionary. " +
                "Insert a new word or change the translation.");
            notifications["InsertWordMessage"].Add(" je vec u recniku. Unesite novu" +
                " rec ili izmenite prevod.");

            // StartNewDictionary method notifications
            notifications.Add("StartNewDictionaryCaption", new List<string>());
            notifications["StartNewDictionaryCaption"].Add("Creating a new dictionary.");
            notifications["StartNewDictionaryCaption"].Add("Pravljenje novog recnika.");

            // IsAutomaticallyOpened method notifications
            notifications.Add("IsAutomaticallyOpenedMessage", new List<string>());
            notifications["IsAutomaticallyOpenedMessage"].Add("Last used dictionary is not at its save " +
                "location. Unless you changed the contents of SavedSettings.txt file, the dictionary is" +
                " either deleted or moved to another location on your computer.");
            notifications["IsAutomaticallyOpenedMessage"].Add("Poslednji koriscen recnik se ne nalazi " +
                "na mestu na kom je sacuvan. Ukoliko niste menjali sadrzaj SavedSettings.txt fajla, " +
                " recnik je ili izbrisan ili pomeren na neku drugu lokaciju na vasem kompjuteru.");

            // IsDictionaryOpened method notifications
            notifications.Add("IsDictionaryOpenedMessage", new List<string>());
            notifications["IsDictionaryOpenedMessage"].Add("Bin file that held your dictionary has been " +
                "tempered with and cannot be opened, or the dictionary location in SavedSettings.txt has" +
                " been changed. Try opening the dictionary yourself.");
            notifications["IsDictionaryOpenedMessage"].Add("Fajl koji je sadrzao vas recnik je izmenjen " +
                "i ne moze se otvoriti, ili je njegova lokacija u SavedSettings.txt promenjena. Pokusajte" +
                " sami da ucitate recnik.");

            // OpenDictionary(object parameter) method notifications
            notifications.Add("OpenDictionaryCommandDefaultMessage", new List<string>());
            notifications["OpenDictionaryCommandDefaultMessage"].Add("Error opening the file. ");
            notifications["OpenDictionaryCommandDefaultMessage"].Add("Greska prilikom citanja fajla.");

            notifications.Add("OpenDictionaryCommandMessage", new List<string>());
            notifications["OpenDictionaryCommandMessage"].Add("The file you tried to open is not your saved" +
                " dictionary. You do not have authorization to open the selected file");
            notifications["OpenDictionaryCommandMessage"].Add("Fajl koji ste pokusali da otvorite nije vas " +
                "sacuvani recnik. Nemate ovlascenje da otvorite izabrani fajl.");

            // OpenDictionary method notifications
            notifications.Add("OpenDictionaryAskToSaveCaption", new List<string>());
            notifications["OpenDictionaryAskToSaveCaption"].Add("Opening a new dictionary.");
            notifications["OpenDictionaryAskToSaveCaption"].Add("Otvaranje novog recnika.");

            notifications.Add("OpenDictionaryMessage", new List<string>());
            notifications["OpenDictionaryMessage"].Add("The file that you chose is not your dictionary. Try again.");
            notifications["OpenDictionaryMessage"].Add("Fajl koji ste izabrali nije vas recnik. Pokusajte ponovo.");

            // AskToSave method notifications
            notifications.Add("AskToSaveMessage", new List<string>());
            notifications["AskToSaveMessage"].Add("Do you wish to save the dictionary?");
            notifications["AskToSaveMessage"].Add("Zelite li da snimite recnik?");
                        
            // Serialize method notifications
            notifications.Add("SerializeMessage", new List<string>());
            notifications["SerializeMessage"].Add("Error, unable to save the dictionary. Try again.");
            notifications["SerializeMessage"].Add("Greska prilikom snimanja recnika. Pokusajte ponovo.");

            // UpdateSavedSettings method notifications
            notifications.Add("UpdateSavedSettings", new List<string>());
            notifications["UpdateSavedSettings"].Add("\r\nIMPORTANT! Do not change the contents of this file." +
                "\r\nIf this file is deleted or changed, on the next application startup, the last saved " +
                "dictionary may not be automatically opened and the user preferences may have to be set again");
            notifications["UpdateSavedSettings"].Add("\r\nVAZNO! Ne menjajte sadrzaj ovog fajla.\r\n" + 
                "Ukoliko se ovaj fajl izbrise ili izmeni, pri sledecem pokretanju aplikacije, poslednji " +
                "koriscen recnik mozda nece biti automatski ucitan i mozda cete morati ponovo da podesite " +
                "ostale opcije.");

            // CreateDocument method notifications
            notifications.Add("CreateDocumentCaption", new List<string>());
            notifications["CreateDocumentCaption"].Add("Word document was not created.");
            notifications["CreateDocumentCaption"].Add("Word dokument nije napravljen.");

            notifications.Add("CreateDocumentMessage", new List<string>());
            notifications["CreateDocumentMessage"].Add("Close the Word document and try again.");
            notifications["CreateDocumentMessage"].Add("Zatvorite Word dokument i pokusajte ponovo.");
        }

        #endregion
    }
}
