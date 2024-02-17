using System;

namespace ClipTree.Engine.Clipboard.Events
{
    public class ItemInsertedEventArgs : EventArgs
    {
        #region Private Read-Only Variables

        private readonly ClipboardHistoryItem m_item;

        #endregion

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
