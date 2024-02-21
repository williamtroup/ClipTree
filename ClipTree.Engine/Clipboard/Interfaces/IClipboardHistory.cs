using ClipTree.Engine.Clipboard.Events;
using System.Collections.Generic;
using System.Windows;

namespace ClipTree.Engine.Clipboard.Interfaces;

public delegate void ItemRemovedHandler(object sender, ItemRemovedEventArgs e);
public delegate void ItemMovedHandler(object sender, ItemMovedEventArgs e);
public delegate void ItemsClearedHandler(object sender, ItemsClearedEventArgs e);
public delegate void ItemsInsertedHandler(object sender, ItemInsertedEventArgs e);
public delegate void AllItemsUpdatedHandler(object sender, AllItemsUpdatedEvent e);
public delegate void ItemUpdatedHandler(object sender, ItemUpdatedEventArgs e);

public interface IClipboardHistory
{
    event ItemRemovedHandler OnItemRemoved;
    event ItemMovedHandler OnItemMoved;
    event ItemsClearedHandler OnItemsCleared;
    event ItemsInsertedHandler OnItemInserted;
    event AllItemsUpdatedHandler OnAllItemsUpdated;
    event ItemUpdatedHandler OnItemUpdated;

    void StartTracking();
    void StopTracking();
    void Remove(IEnumerable<int> indexes);
    void RemoveAll(TextDataFormat type);
    void RemoveAll(string copiedFrom);
    void Clear();
    void ClearClipboard();
    void SetTopItemAsCurrent();
    void MoveItem(int originalIndex, int newIndex, bool setCurrentText = true, string newCopiedFrom = null);
    Dictionary<TextDataFormat, int> GetTotals();
    bool DoesNameExist(string name);
    void SetCurrentText(string value, TextDataFormat type, string overrideDefaultCopiedFrom = null);
    void SetAllItemsUpdated();
    string GetDateTimeStamp();
    void Lock(IEnumerable<int> indexes, bool locked);

    bool DisableAllClipboardCopying { get; set; }
    List<ClipboardHistoryItem> Items { get; }
    bool EnableCopiedFromFiltering { get; set; }
    Dictionary<string, string> CopiedFromFilters { get; set; }
    string LastItemFiltered { get; set; }
    int UpdateInterval { get; set; }
    bool EngineRunning { get; set; }
    bool AutoTrimTextAndUnicodeEntries { get; set; }
}