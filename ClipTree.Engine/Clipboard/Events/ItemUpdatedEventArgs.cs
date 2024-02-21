using System;

namespace ClipTree.Engine.Clipboard.Events;

public class ItemUpdatedEventArgs(ClipboardHistoryItem item, int listIndex) : EventArgs
{
    public ClipboardHistoryItem Item => item;

    public int ListIndex => listIndex;
}