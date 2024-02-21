using System;

namespace ClipTree.Engine.Clipboard.Events
{
    public class ItemRemovedEventArgs : EventArgs
    {
        private readonly int m_listIndex;

        public ItemRemovedEventArgs(int listIndex)
        {
            m_listIndex = listIndex;
        }

        public int ListIndex
        {
            get { return m_listIndex; }
        }
    }
}
