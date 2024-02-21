using System;

namespace ClipTree.Engine.Clipboard.Events
{
    public class ItemMovedEventArgs : EventArgs
    {
        private readonly int m_originalListIndex;
        private readonly int m_newListIndex;
        private readonly ClipboardHistoryItem m_item;

        public ItemMovedEventArgs(int originalListIndex, int newListIndex, ClipboardHistoryItem item)
        {
            m_originalListIndex = originalListIndex;
            m_newListIndex = newListIndex;
            m_item = item;
        }

        public int OriginalListIndex
        {
            get { return m_originalListIndex; }
        }

        public int NewListIndex
        {
            get { return m_newListIndex; }
        }

        public ClipboardHistoryItem Item
        {
            get { return m_item; }
        }
    }
}
