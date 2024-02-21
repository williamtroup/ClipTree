using System;

namespace ClipTree.Engine.Clipboard.Events
{
    public class ItemInsertedEventArgs : EventArgs
    {
        private readonly ClipboardHistoryItem m_item;

        public ItemInsertedEventArgs(ClipboardHistoryItem item)
        {
            m_item = item;
        }

        public ClipboardHistoryItem Item
        {
            get { return m_item; }
        }
    }
}
