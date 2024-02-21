using System;

namespace ClipTree.Engine.Clipboard.Events;

public class ItemMovedEventArgs(int originalListIndex, int newListIndex, ClipboardHistoryItem item)
    : EventArgs
{
    public int OriginalListIndex => originalListIndex;

    public int NewListIndex => newListIndex;

    public ClipboardHistoryItem Item => item;
}