using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GermanDictionary.Views
{
    /// <summary>
    /// Attached properties with or without attached behaviors, for different controls of the views.
    /// </summary>
    public static class AttachedProperties
    {
        #region LimitTextRegex with helper properties

        #region Last valid input helper property

        /// <summary>
        /// Dependency property used to hold last valid text from a text box's text property.
        /// </summary>
        private static readonly DependencyProperty LastValidInputProperty =
            DependencyProperty.RegisterAttached("LastValidInput", typeof(string), typeof(AttachedProperties), new PropertyMetadata(""));

        private static string GetLastValidInput(TextBox obj)
        {
            return (string)obj.GetValue(LastValidInputProperty);
        }

        private static void SetLastValidInput(TextBox obj, string value)
        {
            obj.SetValue(LastValidInputProperty, value);
        }

        #endregion

        #region LimitTextRegex

        /// <summary>
        ///  Identifies the LimitText dependency property, which limits the characters that will be shown
        ///  in the text property of the attaching text box.
        /// </summary>
        public static readonly DependencyProperty LimitTextRegexProperty =
            DependencyProperty.RegisterAttached("LimitTextRegex", typeof(string), typeof(AttachedProperties), 
            new PropertyMetadata("", ValidateText));

        /// <summary>
        /// Sets a handler for the given text box's TextChanged event.
        /// </summary>        
        private static void ValidateText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = (TextBox)d;
            // clears previous handler
            textBox.TextChanged -= ValidateText;
            // attaches new handler
            textBox.TextChanged += ValidateText;
        }

        /// <summary>
        /// Validates the text property of the text box sender, based on the LimitText pattern.
        /// </summary>
        private static void ValidateText(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string text = textBox.Text; // the new text
            if (text == GetLastValidInput(textBox))
            {
                // Stops recursive call and returns to the first iteration of this method
                return;
            }

            string pattern = GetLimitTextRegex(textBox);
            bool isValid = Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
            int carretIndex = textBox.CaretIndex;

            if (isValid || text == string.Empty)
            {
                // New text entered is a valid input
                text = GetCleanText(textBox, text);
                SetLastValidInput(textBox, text);
                textBox.Text = text; // starts recursive call
            }
            else
            {
                // New text entered is not valid, so last valid input is used
                textBox.Text = GetLastValidInput(textBox); // starts recursive call
            }
            textBox.Select(carretIndex, 0);
        }

        /// <summary>
        /// Removes excess characters from the text of a given text box.
        /// </summary>        
        /// <returns>Clean text, without excess characters in succession.</returns>
        private static string GetCleanText(TextBox textBox, string text)
        {
            string excessCharacters = GetExcessCharactersRegex(textBox);
            string pattern = "(" + excessCharacters + ")\\1";
            string cleanText = Regex.Replace(text, pattern, "$1", RegexOptions.IgnoreCase);
            return cleanText;
        }

        /// <summary>
        /// Gets the regex pattern set for limiting the text of a given text box.
        /// </summary>
        /// <param name="obj">Text box whose LimitText property you're getting.</param>
        /// <returns>String pattern that the regex engine uses on this text box.</returns>
        public static string GetLimitTextRegex(TextBox obj)
        {
            return (string)obj.GetValue(LimitTextRegexProperty);
        }

        /// <summary>
        /// Sets a regex pattern for the given text box's text property.
        /// Limits the text only to valid matches.
        /// </summary>
        /// <param name="obj">Text box whose property you're setting.</param>
        /// <param name="value">Pattern that the regex engine will use.</param>
        public static void SetLimitTextRegex(TextBox obj, string value)
        {
            obj.SetValue(LimitTextRegexProperty, value);
        }

        #endregion

        #region ExcessCharacterRegex

        /// <summary>
        /// An addition to the LimitText property, which reads the pattern from this property 
        /// in order to remove excess characters.
        /// </summary>
        public static readonly DependencyProperty ExcessCharactersRegexProperty =
            DependencyProperty.RegisterAttached("ExcessCharactersRegex", typeof(string), typeof(AttachedProperties), 
            new PropertyMetadata(""));

        /// <summary>
        /// Gets the pattern, a character class, used by LimitText property for removing multiple occurences 
        /// of certain characters in succession.
        /// </summary>
        public static string GetExcessCharactersRegex(TextBox obj)
        {
            return (string)obj.GetValue(ExcessCharactersRegexProperty);
        }

        /// <summary>
        /// Sets a regex pattern which is used by LimitText property to remove multiple occurences 
        /// of given characters in succession. Excess characters should be set in a character class.
        /// </summary>
        /// <param name="obj">Text box whose property you're setting.</param>
        /// <param name="value">Character class with characters that cannot be used in succession.</param>
        public static void SetExcessCharactersRegex(TextBox obj, string value)
        {
            obj.SetValue(ExcessCharactersRegexProperty, value);
        }

        #endregion

        #endregion

        #region ExclusiveCase

        // The point of ExclusiveCase property is to set the content of a button to only one of these two values        
        public enum CharacterCase { OnlyLower, OnlyUpper};

        /// <summary>
        /// Identifies the ExclusiveCase dependency property.
        /// A property with a behavior for changing character case of a button's content.
        /// </summary>
        /// <remarks>
        /// After the first change only strictly upper or lower case string is set to the button content.
        /// </remarks>
        public static readonly DependencyProperty ExclusiveCaseProperty = DependencyProperty.RegisterAttached("ExclusiveCase", typeof(CharacterCase),
            typeof(AttachedProperties), new PropertyMetadata(CharacterCase.OnlyLower, ExclusiveCaseChanged));

        /// <summary>
        /// Changes case for a string content of a button, if it's not null.
        /// </summary>           
        private static void ExclusiveCaseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Button button = (Button)d;
            string content = button.Content.ToString();
            if (content != null)
            {
                CharacterCase newCase = (CharacterCase)e.NewValue;
                button.Content = newCase == CharacterCase.OnlyUpper ? content.ToUpper() : content.ToLower();
            }
        }

        /// <summary>
        /// Gets CharacterCase value of ExclusiveCase property for the specified button.
        /// </summary>
        /// <param name="button">Button whose ExclusiveCase property you're getting.</param>
        public static CharacterCase GetExclusiveCase(Button button)
        {
            return (CharacterCase)button.GetValue(ExclusiveCaseProperty);
        }

        /// <summary>
        /// Sets CharacterCase value of ExclusiveCase property for the specified button.
        /// </summary>
        /// <param name="button">Button whose ExclusiveCase you're setting.</param>
        /// <param name="value">Character case you wish to set for the ExclusiveCase property.</param>
        public static void SetExclusiveCase(Button button, CharacterCase value)
        {
            button.SetValue(ExclusiveCaseProperty, value);
        }

        #endregion

        #region TextBoxContentReceiver

        /// <summary>
        /// Identifies the TextBoxContentReceiver dependency property. Used for inserting the content (string) of a button
        /// into a text box, on a button click event.
        /// </summary>
        public static readonly DependencyProperty TextBoxContentReceiverProperty = DependencyProperty.RegisterAttached("TextBoxContentReceiver", 
            typeof(TextBox), typeof(AttachedProperties), new PropertyMetadata(TextBoxChanged));

        /// <summary>
        /// Adds or removes the handler for button's click event based on whether the new value of the property is a text box or null.
        /// </summary>        
        private static void TextBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Button button = (Button)d;
            // Remove the previous handler, if there was one
            button.Click -= InsertContentToTextBox;

            if (e.NewValue == null)
            {               
                // There is no TextBox receiver
                return;
            }
            button.Click += InsertContentToTextBox;
        }

        /// <summary>
        /// Inserts content into the receiving text box, if it's not null and the text box is set.
        /// </summary>
        static void InsertContentToTextBox(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            TextBox receiver = GetTextBoxContentReceiver(button);            
            string content = button.Content.ToString();

            if (content != null)
            {
                receiver.SelectedText = content;                
            }
            receiver.Focus();
            // Unselect, but keep caret position
            receiver.Select(receiver.CaretIndex + 1, 0); 
        }

        /// <summary>
        /// Sets a text box to the button's TextBoxContentReceiver property, which on button click
        /// inserts the button's content into the text box.
        /// </summary>
        /// <param name="button">Button whose property you're setting.</param>
        /// <param name="receiver">Text box into which the content of the button will be inserted.</param>
        public static void SetTextBoxContentReceiver(Button button, TextBox receiver)
        {
            button.SetValue(TextBoxContentReceiverProperty, receiver);
        }

        /// <summary>
        /// Gets the text box that on button click receives button's content string.
        /// </summary>
        /// <param name="button">Button whose property you're getting.</param>
        /// <returns>Text box that's receiving the content string of the button.</returns>
        public static TextBox GetTextBoxContentReceiver(Button button)
        {
            return (TextBox)button.GetValue(TextBoxContentReceiverProperty);
        }

        #endregion
    }
}
