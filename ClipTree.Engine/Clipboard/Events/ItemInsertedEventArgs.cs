using System;

namespace ClipTree.Engine.Clipboard.Events;

public class ItemInsertedEventArgs(ClipboardHistoryItem item) : EventArgs
{
    public ClipboardHistoryItem Item => item;
}