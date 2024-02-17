using System.Windows;

namespace ClipTree.Engine.Clipboard
{
    public struct ClipboardHistoryItem
    {
        public string Name;
        public string Text;
        public string TextDisplay;
        public TextDataFormat Type;
        public string CopiedFrom;
        public string Locked;
        public string DateTime;

        public bool IsTextBased
        {
            get
            {
                return Type == TextDataFormat.Text || Type == TextDataFormat.UnicodeText;
            }
        }
    }
}