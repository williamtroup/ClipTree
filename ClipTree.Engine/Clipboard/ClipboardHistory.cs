using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using ClipTree.Engine.Clipboard.Interfaces;
using ClipTree.Engine.Windows;
using System.Linq;
using ClipTree.Engine.Clipboard.Events;
using ClipTree.Engine.Api;

namespace ClipTree.Engine.Clipboard;

public class ClipboardHistory(string defaultName, string defaultProcessName, string enabledYes, string enabledNo)
    : Logging, IClipboardHistory
{
    private readonly SynchronizationContext m_synchronizationContext = SynchronizationContext.Current;

    private bool m_running;
    private bool m_firstPassWhenRunning = true;
    private string m_overrideDefaultCopiedFrom;

    public event ItemRemovedHandler OnItemRemoved;
    public event ItemMovedHandler OnItemMoved;
    public event ItemsClearedHandler OnItemsCleared;
    public event ItemsInsertedHandler OnItemInserted;
    public event AllItemsUpdatedHandler OnAllItemsUpdated;
    public event ItemUpdatedHandler OnItemUpdated;

    public void StartTracking()
    {
        if (!m_running && EngineRunning)
        {
            Thread thread = new Thread(PopulateHistoryThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            m_running = true;
        }
    }

    public void StopTracking()
    {
        if (EngineRunning)
        {
            m_running = false;
        }
    }

    public void Remove(IEnumerable<int> indexes)
    {
        bool setFirstItem = false;

        foreach (int index in indexes)
        {
            ClipboardHistoryItem clipboardTextItem = Items[index];

            if (clipboardTextItem.Locked != enabledYes)
            {
                if (index == 0)
                {
                    setFirstItem = true;
                }

                OnItemRemoved?.Invoke(this, new ItemRemovedEventArgs(index));

                Items.RemoveAt(index);
            }
        }

        if (setFirstItem && Items.Count > 0)
        {
            SetTopItemAsCurrent();
        }
    }

    public void RemoveAll(TextDataFormat type)
    {
        bool topItemRemoved = false;

        for (int listItemIndex = Items.Count - 1; listItemIndex >= 0; listItemIndex--)
        {
            ClipboardHistoryItem clipboardTextItem = Items[listItemIndex];

            if (clipboardTextItem.Type == type && clipboardTextItem.Locked != enabledYes)
            {
                OnItemRemoved?.Invoke(this, new ItemRemovedEventArgs(listItemIndex));

                Items.RemoveAt(listItemIndex);

                if (listItemIndex == 0)
                {
                    topItemRemoved = true;
                }
            }
        }

        if (topItemRemoved && Items.Count > 0)
        {
            SetTopItemAsCurrent();
        }
    }

    public void RemoveAll(string copiedFrom)
    {
        bool topItemRemoved = false;

        for (int listItemIndex = Items.Count - 1; listItemIndex >= 0; listItemIndex--)
        {
            ClipboardHistoryItem clipboardTextItem = Items[listItemIndex];

            if (string.Equals(clipboardTextItem.CopiedFrom, copiedFrom, StringComparison.CurrentCultureIgnoreCase) && clipboardTextItem.Locked != enabledYes)
            {
                OnItemRemoved?.Invoke(this, new ItemRemovedEventArgs(listItemIndex));

                Items.RemoveAt(listItemIndex);

                if (listItemIndex == 0)
                {
                    topItemRemoved = true;
                }
            }
        }

        if (topItemRemoved && Items.Count > 0)
        {
            SetTopItemAsCurrent();
        }
    }

    public void Clear()
    {
        bool topItemRemoved = false;

        for (int listItemIndex = Items.Count - 1; listItemIndex >= 0; listItemIndex--)
        {
            ClipboardHistoryItem clipboardTextItem = Items[listItemIndex];

            if (clipboardTextItem.Locked != enabledYes)
            {
                Items.RemoveAt(listItemIndex);

                if (listItemIndex == 0)
                {
                    topItemRemoved = true;
                }
            }
        }

        if (topItemRemoved && Items.Count > 0)
        {
            SetTopItemAsCurrent();
        }

        ClearClipboard();

        OnItemsCleared?.Invoke(this, new ItemsClearedEventArgs());
    }

    public void ClearClipboard()
    {
        System.Windows.Clipboard.Clear();
    }

    public void SetTopItemAsCurrent()
    {
        SetCurrentText(Items[0].Text, Items[0].Type);
    }

    public void MoveItem(int originalIndex, int newIndex, bool setCurrentText = true, string newCopiedFrom = null)
    {
        if (newIndex >= 0 && newIndex <= Items.Count)
        {
            ClipboardHistoryItem clipboardTextItem = Items[originalIndex];

            if (newIndex == 0)
            {
                clipboardTextItem.DateTime = GetDateTimeStamp();
            }

            if (!string.IsNullOrEmpty(newCopiedFrom))
            {
                clipboardTextItem.CopiedFrom = newCopiedFrom;
            }

            m_synchronizationContext.Post(
                o => OnItemMoved?.Invoke(this, new ItemMovedEventArgs(originalIndex, newIndex, clipboardTextItem)),
                null);

            Items.RemoveAt(originalIndex);
            Items.Insert(newIndex, clipboardTextItem);

            if (newIndex == 0 && setCurrentText)
            {
                SetCurrentText(clipboardTextItem.Text, clipboardTextItem.Type);
            }
            else if (originalIndex == 0 && setCurrentText)
            {
                SetTopItemAsCurrent();

                ClipboardHistoryItem topClipboardTextItem = Items[0];
                topClipboardTextItem.DateTime = GetDateTimeStamp();

                Items[0] = topClipboardTextItem;
            }
        }
    }

    public Dictionary<TextDataFormat, int> GetTotals()
    {
        Dictionary<TextDataFormat, int> totals = new Dictionary<TextDataFormat, int>();

        foreach (TextDataFormat type in Enum.GetValues(typeof(TextDataFormat)))
        {
            totals[type] = 0;
        }

        totals.Remove(TextDataFormat.Xaml);

        foreach (ClipboardHistoryItem clipboardTextItem in Items)
        {
            totals[clipboardTextItem.Type]++;
        }

        return totals;
    }

    public bool DoesNameExist(string name)
    {
        return Items.Any(listViewItem => string.Equals(listViewItem.Name, name, StringComparison.CurrentCultureIgnoreCase));
    }

    public void SetCurrentText(string value, TextDataFormat type, string overrideDefaultCopiedFrom = null)
    {
        try
        {
            m_overrideDefaultCopiedFrom = overrideDefaultCopiedFrom;

            System.Windows.Clipboard.SetText(value, type);
        }
        catch (Exception exception)
        {
            Log.ErrorFormat("Error setting current clipboard text: {0}", exception);
        }
    }

    public void SetAllItemsUpdated()
    {
        OnAllItemsUpdated?.Invoke(this, new AllItemsUpdatedEvent());
    }

    public string GetDateTimeStamp()
    {
        return DateTime.Now.ToString(CultureInfo.InvariantCulture);
    }

    public void Lock(IEnumerable<int> indexes, bool locked)
    {
        string status = locked ? enabledYes : enabledNo;

        foreach (int index in indexes)
        {
            ClipboardHistoryItem clipboardTextItem = Items[index];
            clipboardTextItem.Locked = status;

            Items[index] = clipboardTextItem;

            OnItemUpdated?.Invoke(this, new ItemUpdatedEventArgs(clipboardTextItem, index));
        }
    }

    public bool DisableAllClipboardCopying { get; set; }
    public List<ClipboardHistoryItem> Items { get; } = new();
    public bool EnableCopiedFromFiltering { get; set; }
    public Dictionary<string, string> CopiedFromFilters { get; set; } = new();
    public string LastItemFiltered { get; set; } = string.Empty;
    public int UpdateInterval { get; set; }
    public bool EngineRunning { get; set; } = true;
    public bool AutoTrimTextAndUnicodeEntries { get; set; }

    private void PopulateHistoryThread()
    {
        while (m_running)
        {
            TextDataFormat type;
            string currentText = GetCurrentText(out type);

            if (!string.IsNullOrEmpty(currentText) && currentText != LastItemFiltered)
            {
                bool add = false;

                if (Items.Count == 0)
                {
                    add = true;
                }
                else
                {
                    int currentTextIndex = GetTextItemIndex(currentText);

                    if (currentTextIndex < 0)
                    {
                        add = true;
                    }
                    else if (currentTextIndex > 0)
                    {
                        if (!GetCurrentProcessName(out string processName))
                        {
                            MoveItem(currentTextIndex, 0, false, processName);
                        }
                        else
                        {
                            LastItemFiltered = currentText;
                        }
                    }
                }

                if (add)
                {
                    if (!GetCurrentProcessName(out string processName))
                    {
                        ClipboardHistoryItem newClipboardTextItem = new ClipboardHistoryItem
                        {
                            Name = defaultName,
                            Type = type,
                            Text = currentText,
                            TextDisplay = currentText,
                            CopiedFrom = processName,
                            Locked = enabledNo,
                            DateTime = GetDateTimeStamp()
                        };

                        if (type != TextDataFormat.Text && type != TextDataFormat.UnicodeText)
                        {
                            newClipboardTextItem.TextDisplay = GetTextBasedCurrentText();
                        }

                        Items.Insert(0, newClipboardTextItem);

                        Log.InfoFormat("New clipboard entry: {0}", newClipboardTextItem.TextDisplay);

                        m_synchronizationContext.Post(o => OnItemInserted?.Invoke(this, new ItemInsertedEventArgs(newClipboardTextItem)), null);
                    }
                    else
                    {
                        LastItemFiltered = currentText;
                    }
                }
            }

            m_firstPassWhenRunning = false;

            Thread.Sleep(UpdateInterval);
        }
    }

    private static bool IsClipboardBeingUsedByAnotherProcess()
    {
        IntPtr handle = Win32.GetOpenClipboardWindow();

        return handle != IntPtr.Zero;
    }

    private string GetCurrentText(out TextDataFormat type)
    {
        string data = null;

        type = TextDataFormat.Text;

        try
        {
            if (!IsClipboardBeingUsedByAnotherProcess())
            {
                if (System.Windows.Clipboard.ContainsText(TextDataFormat.Html))
                {
                    data = System.Windows.Clipboard.GetText(TextDataFormat.Html);
                    type = TextDataFormat.Html;
                }
                else if (System.Windows.Clipboard.ContainsText(TextDataFormat.Rtf))
                {
                    data = System.Windows.Clipboard.GetText(TextDataFormat.Rtf);
                    type = TextDataFormat.Rtf;
                }
                else if (System.Windows.Clipboard.ContainsText(TextDataFormat.UnicodeText))
                {
                    data = System.Windows.Clipboard.GetText(TextDataFormat.UnicodeText);
                    type = TextDataFormat.UnicodeText;
                }
                else if (System.Windows.Clipboard.ContainsText(TextDataFormat.CommaSeparatedValue))
                {
                    data = System.Windows.Clipboard.GetText(TextDataFormat.CommaSeparatedValue);
                    type = TextDataFormat.CommaSeparatedValue;
                }
                else
                {
                    data = GetTextBasedCurrentText();
                }

                if (!string.IsNullOrEmpty(data))
                {
                    if (DisableAllClipboardCopying)
                    {
                        System.Windows.Clipboard.Clear();
                        type = TextDataFormat.Text;
                        data = null;
                    }
                    else
                    {
                        if (AutoTrimTextAndUnicodeEntries && type is TextDataFormat.Text or TextDataFormat.UnicodeText)
                        {
                            data = data.Trim();
                        }
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Log.ErrorFormat("Error getting current clipboard data: {0}", exception);
        }

        return data;
    }

    private string GetTextBasedCurrentText()
    {
        string data = null;

        try
        {
            if (System.Windows.Clipboard.ContainsText(TextDataFormat.Text))
            {
                data = System.Windows.Clipboard.GetText(TextDataFormat.Text);
            }
        }
        catch (Exception exception)
        {
            Log.ErrorFormat("Error getting current non-RTF clipboard data: {0}", exception);
        }

        return data;
    }

    private bool GetCurrentProcessName(out string currentProcessName)
    {
        string processName = m_overrideDefaultCopiedFrom ?? defaultProcessName;

        if (!m_firstPassWhenRunning && string.IsNullOrEmpty(m_overrideDefaultCopiedFrom))
        {
            try
            {
                Process process = Processes.GetFocused();

                if (process != null)
                {
                    using (process)
                    {
                        if (process.Id != Process.GetCurrentProcess().Id && !string.IsNullOrEmpty(process.ProcessName))
                        {
                            processName = process.ProcessName;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Error getting focused process name: {0}", exception);
            }
        }

        currentProcessName = processName;

        m_overrideDefaultCopiedFrom = null;

        return IsProcessNameFilteredInCopiedFromFilters(processName);
    }

    private bool IsProcessNameFilteredInCopiedFromFilters(string processName)
    {
        return EnableCopiedFromFiltering && CopiedFromFilters.ContainsKey(processName) && CopiedFromFilters[processName] == enabledYes;
    }

    private int GetTextItemIndex(string text)
    {
        int index = -1;
        int currentIndex = -1;

        foreach (ClipboardHistoryItem clipboardTextItem in Items)
        {
            currentIndex++;

            if (clipboardTextItem.Text == text)
            {
                index = currentIndex;
                break;
            }
        }

        return index;
    }
}