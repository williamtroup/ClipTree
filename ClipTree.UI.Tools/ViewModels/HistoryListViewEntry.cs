using System.Windows.Media;

namespace ClipTree.UI.Tools.ViewModels
{
    public class HistoryListViewEntry
    {
        private const int MaximumTextDisplayLength = 250;
        private const string Ellipsis = "...";

        public string Name { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public string CopiedFrom { get; set; }
        public string Locked { get; set; }
        public string DateTime { get; set; }
        public SolidColorBrush BackgroundColor { get; set; }
        public SolidColorBrush ForeColor { get; set; }

        private string m_textDisplay;

        public string TextDisplay
        {
            get { return m_textDisplay; }
            set
            {
                m_textDisplay = value.Length > MaximumTextDisplayLength
                    ? string.Format("{0}{1}", value.Substring(0, MaximumTextDisplayLength), Ellipsis)
                    : value;
            }
        }
    }
}
