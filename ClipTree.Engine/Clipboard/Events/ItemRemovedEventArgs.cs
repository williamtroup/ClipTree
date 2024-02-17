using System;

namespace ClipTree.Engine.Clipboard.Events
{
    public class ItemRemovedEventArgs : EventArgs
    {
        #region Private Read-Only Variables

        private readonly int m_listIndex;

        #endregion

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
