using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GermanDictionary.Models
{


    /// <summary>
    /// Representation of a german word with its corelating translation. 
    /// </summary>
    [Serializable]
    public class GermanWordTranslationPair : INotifyPropertyChanged, IComparable<GermanWordTranslationPair>, IEquatable<GermanWordTranslationPair>
    {
        /// <summary>
        /// German word-translation pair. Case insensitive.
        /// </summary>
        /// <param name="word">German word.</param>
        /// <param name="translation">Translation of the word.</param>
        public GermanWordTranslationPair(string word, string translation = "KEINE ÜBERSETZUNG")
        {
            this.Word = word;
            this.translation = translation;
            WordIsANoun = IsNoun(word);
        }        

        public string Word { get; private set; } 
        public bool WordIsANoun { get; private set; }

        private string translation;
        public string Translation
        {
            get
            { 
                return translation; 
            }
            set
            {
                if (translation != value)
                {
                    translation = value;
                    OnPropertyChanged();
                }
            }            
        }


        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
               

        /// <summary>
        /// Compares just the words, not their translation. Ignores case.
        /// German nouns are compared with other words regardless of their articles.
        /// </summary>
        public int CompareTo(GermanWordTranslationPair other)
        {
            if (other == null)
            {
                return 1;
            }

            string lowerCaseWord = this.Word.ToLower();
            string lowerCaseOtherWord = other.Word.ToLower();
                        
            string articlessWord = "";
            if (this.WordIsANoun)
            {
                articlessWord = lowerCaseWord.Substring(4, Word.Length - 4);
            }
                        
            string otherArticlessWord = "";
            if (other.WordIsANoun)
            {
                otherArticlessWord = lowerCaseOtherWord.Substring(4, other.Word.Length - 4);
            }

            if (this.WordIsANoun && other.WordIsANoun)
            {                
                return articlessWord.CompareTo(otherArticlessWord);
            }
            else if (this.WordIsANoun)
            {
                return articlessWord.CompareTo(lowerCaseOtherWord);
            }
            else if (other.WordIsANoun)
            {
                return lowerCaseWord.CompareTo(otherArticlessWord);
            }
            else
                return lowerCaseWord.CompareTo(lowerCaseOtherWord);
        }

        /// <summary>
        /// Compares Word property of the pairs. Ignores case.
        /// </summary>
        public bool Equals(GermanWordTranslationPair other)
        {
            if (other == null)
                return false;

            if (this.Word.ToLower() == other.Word.ToLower())
                return true;
            else
                return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            GermanWordTranslationPair other = obj as GermanWordTranslationPair;
            return this.Equals(other);
        }

        /// <summary>
        /// Gets hash code of just the Word property.
        /// </summary>
        public override int GetHashCode()
        {
            return this.Word.ToLower().GetHashCode();
        }
        
        public override string ToString()
        {
            return string.Format("{0} = {1}", Word, translation);
        }

        /// <summary>
        /// Checks whether the word begins with der, die or das, then an empty space 
        /// and a word character, i.e. if it's a german noun.
        /// </summary>
        /// <param name="word">Word being checked.</param>
        /// <returns>True if it's a noun, false otherwise.</returns>
        private bool IsNoun(string word)
        {
            bool isNoun = Regex.IsMatch(word, @"^d(er|ie|as) \w", RegexOptions.IgnoreCase);
            return isNoun;
        }
    }
}
