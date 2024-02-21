using System;

namespace ClipTree.Engine.Clipboard.Events
{
    public class ItemUpdatedEventArgs : EventArgs
    {
        private readonly ClipboardHistoryItem m_item;
        private readonly int m_listIndex;

        public ItemUpdatedEventArgs(ClipboardHistoryItem item, int listIndex)
        {
            m_item = item;
            m_listIndex = listIndex;
        }

        public ClipboardHistoryItem Item
        {
            get { return m_item; }
        }

        public int ListIndex
        {
            get { return m_listIndex; }
        }
    }
}
