using System;

namespace ClipTree.Engine.Clipboard.Events;

public class ItemRemovedEventArgs(int listIndex) : EventArgs
{
    public int ListIndex => listIndex;
}