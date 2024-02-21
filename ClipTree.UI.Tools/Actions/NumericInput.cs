using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClipTree.UI.Tools.Actions
{
    public static class NumericInput
    {
        private const string ValidInput = "[^0-9]+";

        public static void Make(TextBox textBox, string minimumValue = null)
        {
            textBox.PreviewTextInput += TextBox_PreviewTextInput;

            if (!string.IsNullOrEmpty(minimumValue))
            {
                textBox.LostFocus += TextBox_LostFocus;
                textBox.Tag = minimumValue;
            }

            DataObject.AddPastingHandler(textBox, TextBox_OnPaste);
        }

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex.IsMatch(e.Text, ValidInput);
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = textBox.Tag.ToString();
            }
        }

        private static void TextBox_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            string text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;

            if (!string.IsNullOrEmpty(text))
            {
                bool isNotJustNumbers = Regex.IsMatch(text, ValidInput);
                if (isNotJustNumbers)
                {
                    e.CancelCommand();
                }
            }
        }
    }
}
