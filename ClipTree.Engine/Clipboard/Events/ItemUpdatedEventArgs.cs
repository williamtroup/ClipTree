using System;

namespace ClipTree.Engine.Clipboard.Events
{
    public class ItemUpdatedEventArgs : EventArgs
    {
        #region Private Read-Only Variables

        private readonly ClipboardHistoryItem m_item;
        private readonly int m_listIndex;

        #endregion

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
