using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using ClipTree.Engine.Clipboard;
using ClipTree.Engine.Clipboard.Interfaces;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools.Interfaces;
using ClipTree.UI.Tools.ViewModels;
using System.Windows.Media;
using ClipTree.UI.Tools.Enums;
using ClipTree.UI.Tools.Extensions;

namespace ClipTree.UI.Tools.Views
{
    public class ClipboardHistoryItems : IClipboardHistoryItems
    {
        #region Private Read-Only Variables

        private readonly ListView m_listView;
        private readonly IClipboardHistory m_clipboardHistory;
        private readonly IXMLSettings m_settings;
        private readonly string m_defaultName;
        private readonly string m_defaultProcessName;
        private readonly string m_defaultLockedState;

        #endregion

        public ClipboardHistoryItems(
            ListView listView, 
            IClipboardHistory clipboardHistory, 
            IXMLSettings settings,
            string defaultName,
            string defaultProcessName,
            string defaultLockedState)
        {
            m_listView = listView;
            m_clipboardHistory = clipboardHistory;
            m_settings = settings;
            m_defaultName = defaultName;
            m_defaultProcessName = defaultProcessName;
            m_defaultLockedState = defaultLockedState;

            LoadColors();
            AssignClipboardHistoryEvents();
        }

        public void Load(IXMLSettings settings = null)
        {
            IXMLSettings actualSettings = settings ?? m_settings;
            XmlDocument xmlDocument = actualSettings.GetDocument();

            m_clipboardHistory.StopTracking();

            string totalItemsValue = actualSettings.Read(Settings.History.HistoryItems, nameof(Settings.History.Total), Settings.History.Total, xmlDocument);
            if (int.TryParse(totalItemsValue, out int totalItems) && totalItems > 0)
            {
                m_clipboardHistory.Clear();

                ClipboardHistoryItem clipboardHistoryItem = new ClipboardHistoryItem();
                string defaultType = TextDataFormat.Text.ToString();
                string defaultDateTime = m_clipboardHistory.GetDateTimeStamp();

                for (int itemIndex = 0; itemIndex < totalItems; itemIndex++)
                {
                    string name = actualSettings.Read(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.Name)), m_defaultName, xmlDocument);
                    string text = actualSettings.Read(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.Text)), "", xmlDocument);
                    string textDisplay = actualSettings.Read(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.TextDisplay)), "", xmlDocument);
                    string type = actualSettings.Read(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.Type)), defaultType, xmlDocument);
                    string copiedFrom = actualSettings.Read(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.CopiedFrom)), m_defaultProcessName, xmlDocument);
                    string locked = actualSettings.Read(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.Locked)), m_defaultLockedState, xmlDocument);
                    string dateTime = actualSettings.Read(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.DateTime)), defaultDateTime, xmlDocument);

                    TextDataFormat actualType = (TextDataFormat) Enum.Parse(typeof(TextDataFormat), type);

                    AddListItem(name, text, textDisplay, actualType, copiedFrom, locked, dateTime);
                    AddHistoryItem(name, text, textDisplay, actualType, copiedFrom, locked, dateTime);
                }

                m_listView.UpdateLayout();
            }

            m_clipboardHistory.StartTracking();
        }

        public void Save(IXMLSettings settings = null)
        {
            IXMLSettings actualSettings = settings ?? m_settings;

            XmlDocument xmlDocument = actualSettings.GetDocument();

            actualSettings.RemoveSection(Settings.History.HistoryItems, xmlDocument);
            actualSettings.Write(Settings.History.HistoryItems, nameof(Settings.History.Total), m_clipboardHistory.Items.Count.ToString(), xmlDocument);

            int itemIndex = 0;

            foreach (ClipboardHistoryItem clipboardHistoryItem in m_clipboardHistory.Items)
            {
                actualSettings.Write(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.Name)), clipboardHistoryItem.Name, xmlDocument);
                actualSettings.Write(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.Text)), clipboardHistoryItem.Text, xmlDocument);
                actualSettings.Write(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.TextDisplay)), clipboardHistoryItem.TextDisplay, xmlDocument);
                actualSettings.Write(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.Type)), clipboardHistoryItem.Type.ToString(), xmlDocument);
                actualSettings.Write(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.CopiedFrom)), clipboardHistoryItem.CopiedFrom, xmlDocument);
                actualSettings.Write(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.Locked)), clipboardHistoryItem.Locked, xmlDocument);
                actualSettings.Write(Settings.History.HistoryItems, GetColumnName(itemIndex, nameof(clipboardHistoryItem.DateTime)), clipboardHistoryItem.DateTime, xmlDocument);

                itemIndex++;
            }

            actualSettings.SaveDocument(xmlDocument);
        }

        public int GetSelectedIndex()
        {
            return m_listView.SelectedIndex;
        }

        public int[] GetSelectedIndexes()
        {
            return (from object selectedItem in m_listView.SelectedItems select m_listView.Items.IndexOf(selectedItem)).ToArray();
        }

        public void SetAsCurrent()
        {
            int selectedindex = GetSelectedIndex();
            if (selectedindex > -1)
            {
                ClipboardHistoryItem clipboardHistoryItem = m_clipboardHistory.Items[selectedindex];
                m_clipboardHistory.SetCurrentText(clipboardHistoryItem.Text, clipboardHistoryItem.Type, clipboardHistoryItem.CopiedFrom);

                SelectItem(0);
            }
        }

        public void SetAsCurrentTextFormat()
        {
            int selectedindex = GetSelectedIndex();
            if (selectedindex > -1)
            {
                ClipboardHistoryItem clipboardHistoryItem = m_clipboardHistory.Items[selectedindex];
                m_clipboardHistory.SetCurrentText(clipboardHistoryItem.TextDisplay, TextDataFormat.Text, clipboardHistoryItem.CopiedFrom);

                SelectItem(0);
            }
        }

        public void SelectItem(int index)
        {
            if (index > -1)
            {
                m_listView.SelectedItem = m_listView.Items[index];
                m_listView.UpdateLayout();

                FocusSelectedItem(index);
            }
        }

        public void FindPreviousText(string text, int? startIndex = null, bool goToFirst = false, bool matchCase = false, SearchType searchType = SearchType.Contains)
        {
            if (!string.IsNullOrEmpty(text))
            {
                int selectedIndex = -1;
                int actualStartIndex = startIndex != null ? startIndex.Value - 1 : m_listView.Items.Count - 1;

                actualStartIndex = actualStartIndex < 0 ? m_listView.Items.Count - 1 : actualStartIndex;

                for (int listItemIndex = actualStartIndex; listItemIndex > 0; listItemIndex--)
                {
                    ClipboardHistoryItem clipboardHistoryItem = m_clipboardHistory.Items[listItemIndex];

                    if (DoesListViewItemContainText(clipboardHistoryItem, text, matchCase, searchType))
                    {
                        selectedIndex = listItemIndex;

                        if (!goToFirst)
                        {
                            break;
                        }
                    }
                }

                SelectItem(selectedIndex);
            }
        }

        public void FindNextText(string text, int? startIndex = null, bool goToLast = false, bool matchCase = false, SearchType searchType = SearchType.Contains)
        {
            if (!string.IsNullOrEmpty(text))
            {
                int selectedIndex = -1;
                int actualStartIndex = startIndex == null ? 0 : startIndex.Value + 1;

                for (int listItemIndex = actualStartIndex; listItemIndex < m_listView.Items.Count - 1; listItemIndex++)
                {
                    ClipboardHistoryItem clipboardHistoryItem = m_clipboardHistory.Items[listItemIndex];

                    if (DoesListViewItemContainText(clipboardHistoryItem, text, matchCase, searchType))
                    {
                        selectedIndex = listItemIndex;

                        if (!goToLast)
                        {
                            break;
                        }
                    }
                }

                SelectItem(selectedIndex);
            }
        }

        public int GetTotalNumberOfItems(string text, bool matchCase = false, SearchType searchType = SearchType.Contains)
        {
            int totalItemsFound = 0;

            if (!string.IsNullOrEmpty(text))
            {
                for (int listItemIndex = 0; listItemIndex < m_listView.Items.Count - 1; listItemIndex++)
                {
                    ClipboardHistoryItem clipboardHistoryItem = m_clipboardHistory.Items[listItemIndex];

                    if (DoesListViewItemContainText(clipboardHistoryItem, text, matchCase, searchType))
                    {
                        totalItemsFound++;
                    }
                }
            }

            return totalItemsFound;
        }

        public int MarkItemsAsSelected(string text, bool matchCase = false, SearchType searchType = SearchType.Contains)
        {
            int totalItemsFound = 0;

            if (!string.IsNullOrEmpty(text))
            {
                m_listView.SelectedItems.Clear();

                for (int listItemIndex = 0; listItemIndex < m_listView.Items.Count - 1; listItemIndex++)
                {
                    ClipboardHistoryItem clipboardHistoryItem = m_clipboardHistory.Items[listItemIndex];

                    if (DoesListViewItemContainText(clipboardHistoryItem, text, matchCase, searchType))
                    {
                        m_listView.SelectedItems.Add(m_listView.Items[listItemIndex]);
                        totalItemsFound++;
                    }
                }
            }

            return totalItemsFound;
        }

        public void SelectAll()
        {
            m_listView.SelectAll();
        }

        public void SelectNone()
        {
            m_listView.SelectedItems.Clear();
        }

        public void InvertSelection()
        {
            List<int> selectedIndexes = (from object t in m_listView.SelectedItems select m_listView.Items.IndexOf(t)).ToList();

            m_listView.SelectedItems.Clear();

            for (int itemIndex = 0; itemIndex < m_listView.Items.Count; itemIndex++)
            {
                if (!selectedIndexes.Contains(itemIndex))
                {
                    m_listView.SelectedItems.Add(m_listView.Items[itemIndex]);
                }
            }
        }

        public void LoadColors()
        {
            bool markAllItemsAsUpdated = false;

            XmlDocument xmlDocument = m_settings.GetDocument();

            Dictionary<string, string> rowColors = m_settings.ReadAll(Settings.RowColors, xmlDocument);
            Dictionary<string, string> rowTextColors = m_settings.ReadAll(Settings.RowTextColors, xmlDocument);

            if (RowColors == null || RowTextColors == null)
            {
                RowColors = new Dictionary<string, int>();
                RowTextColors = new Dictionary<string, int>();
            }
            else
            {
                markAllItemsAsUpdated = true;
            }

            foreach (KeyValuePair<string, string> rowColor in rowColors)
            {
                int actualColor = Convert.ToInt32(rowColor.Value);

                RowColors[rowColor.Key.ToLower()] = actualColor;
            }

            foreach (KeyValuePair<string, string> rowTextColor in rowTextColors)
            {
                int actualColor = Convert.ToInt32(rowTextColor.Value);

                RowTextColors[rowTextColor.Key.ToLower()] = actualColor;
            }

            if (markAllItemsAsUpdated)
            {
                m_clipboardHistory.SetAllItemsUpdated();
            }
        }

        public void UpdateItemName(ClipboardHistoryItem clipboardHistoryItem, int selectedIndex)
        {
            if (selectedIndex > -1)
            {
                HistoryListViewEntry originalItem = (HistoryListViewEntry)m_listView.Items[selectedIndex];
                originalItem.Name = clipboardHistoryItem.Name;

                m_listView.Items.RemoveAt(selectedIndex);
                m_listView.Items.Insert(selectedIndex, originalItem);

                SelectItem(selectedIndex);
            }
        }

        public void MergeAsText()
        {
            int[] selectedIndexes = GetSelectedIndexes();
            string mergedClipboardItem = string.Join("\n\n", selectedIndexes.Select(selectedIndex => m_clipboardHistory.Items[selectedIndex].TextDisplay).ToArray());

            m_clipboardHistory.SetCurrentText(mergedClipboardItem, TextDataFormat.Text);
        }

        public bool RowColorsEnabled { get; set; }
        public bool AutoSaveHistory { get; set; }
        public bool RememberHistoryBetweenSessions { get; set; }
        public int TotalItemsToShowInList { get; set; }

        #region Private Properties

        private Dictionary<string, int> RowColors { get; set; }
        private Dictionary<string, int> RowTextColors { get; set; }

        #endregion

        #region Private Adding Helpers

        private void AddListItem(ClipboardHistoryItem clipboardTextItem, int insertIndex = -1)
        {
            AddListItem(
                clipboardTextItem.Name,
                clipboardTextItem.Text,
                clipboardTextItem.TextDisplay,
                clipboardTextItem.Type,
                clipboardTextItem.CopiedFrom,
                clipboardTextItem.Locked,
                clipboardTextItem.DateTime,
                insertIndex);
        }

        private void AddListItem(string name, string text, string textDisplay, TextDataFormat type, string copiedFrom, string locked, string dateTime, int insertIndex = -1)
        {
            if (TotalItemsToShowInList == 0 || m_listView.Items.Count < TotalItemsToShowInList)
            {
                HistoryListViewEntry newListItemEntry = new HistoryListViewEntry()
                {
                    Name = name,
                    Text = text,
                    TextDisplay = !string.IsNullOrEmpty(textDisplay) ? textDisplay : text,
                    Type = type.GetDisplayName(),
                    CopiedFrom = copiedFrom,
                    Locked = locked,
                    DateTime = dateTime,
                    BackgroundColor = GetColor(RowColors, copiedFrom, Brushes.White),
                    ForeColor = GetColor(RowTextColors, copiedFrom, Brushes.Black)
                };

                if (insertIndex == -1)
                {
                    m_listView.Items.Add(newListItemEntry);
                }
                else
                {
                    m_listView.Items.Insert(insertIndex, newListItemEntry);
                }
            }
        }

        private void AddHistoryItem(string name, string text, string textDisplay, TextDataFormat type, string copiedFrom, string locked, string dateTime)
        {
            ClipboardHistoryItem newClipboardTextItem = new ClipboardHistoryItem
            {
                Name = name,
                Type = type,
                Text = text,
                TextDisplay = textDisplay,
                CopiedFrom = copiedFrom,
                Locked = locked,
                DateTime = dateTime
            };

            m_clipboardHistory.Items.Add(newClipboardTextItem);
        }

        private SolidColorBrush GetColor(Dictionary<string, int> colors, string copiedFrom, SolidColorBrush defaultColor)
        {
            SolidColorBrush color = defaultColor;

            if (colors.ContainsKey(copiedFrom) && RowColorsEnabled)
            {
                int colorValue = colors[copiedFrom];

                byte[] bytes = BitConverter.GetBytes(colorValue);
                color = new SolidColorBrush(Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]));
            }

            return color;
        }

        #endregion

        #region Private Search Helpers

        private static bool DoesListViewItemContainText(ClipboardHistoryItem clipboardHistoryItem, string text, bool matchCase, SearchType searchType)
        {
            bool returnFlag = false;

            string entryName = matchCase ? clipboardHistoryItem.Name : clipboardHistoryItem.Name.ToLower();
            string entryText = matchCase ? clipboardHistoryItem.Text : clipboardHistoryItem.Text.ToLower();
            string entryTextDisplay = matchCase ? clipboardHistoryItem.TextDisplay : clipboardHistoryItem.TextDisplay.ToLower();
            string entrySearchText = matchCase ? text : text.ToLower();

            switch (searchType)
            {
                case SearchType.Contains:
                    returnFlag = entryName.Contains(entrySearchText) ||
                                 entryText.Contains(entrySearchText) ||
                                 entryTextDisplay.Contains(entrySearchText);
                    break;

                case SearchType.StartsWith:
                    returnFlag = entryName.StartsWith(entrySearchText) ||
                                 entryText.StartsWith(entrySearchText) ||
                                 entryTextDisplay.StartsWith(entrySearchText);
                    break;

                case SearchType.EndsWith:
                    returnFlag = entryName.EndsWith(entrySearchText) ||
                                 entryText.EndsWith(entrySearchText) ||
                                 entryTextDisplay.EndsWith(entrySearchText);
                    break;

                case SearchType.WholeWordOnly:

                    string regEx = string.Format("\\b{0}\\b", entrySearchText);

                    returnFlag = Regex.IsMatch(entryName, regEx) ||
                                 Regex.IsMatch(entryText, regEx) ||
                                 Regex.IsMatch(entryTextDisplay, regEx);
                    break;
            }

            return returnFlag;
        }

        #endregion

        #region Private "Clipboard History" Events

        private void AssignClipboardHistoryEvents()
        {
            m_clipboardHistory.OnItemRemoved += ClipboardHistory_OnItemRemoved;
            m_clipboardHistory.OnItemMoved += ClipboardHistory_OnItemMoved;
            m_clipboardHistory.OnItemsCleared += ClipboardHistory_OnItemsCleared;
            m_clipboardHistory.OnItemInserted += ClipboardHistory_OnItemInserted;
            m_clipboardHistory.OnAllItemsUpdated += ClipboardHistory_OnAllItemsUpdated;
            m_clipboardHistory.OnItemUpdated += ClipboardHistory_OnItemUpdated;
        }

        private void ClipboardHistory_OnItemRemoved(object sender, Engine.Clipboard.Events.ItemRemovedEventArgs e)
        {
            m_listView.Items.RemoveAt(e.ListIndex);

            if (TotalItemsToShowInList > 0 && m_clipboardHistory.Items.Count >= TotalItemsToShowInList)
            {
                AddListItem(m_clipboardHistory.Items[TotalItemsToShowInList]);
            }
        }

        private void ClipboardHistory_OnItemMoved(object sender, Engine.Clipboard.Events.ItemMovedEventArgs e)
        {
            HistoryListViewEntry originalItem = (HistoryListViewEntry) m_listView.Items[e.OriginalListIndex];
            originalItem.CopiedFrom = e.Item.CopiedFrom;
            originalItem.DateTime = e.Item.DateTime;

            m_listView.Items.RemoveAt(e.OriginalListIndex);
            m_listView.Items.Insert(e.NewListIndex, originalItem);

            SelectItem(e.NewListIndex);
        }

        private void ClipboardHistory_OnItemsCleared(object sender, Engine.Clipboard.Events.ItemsClearedEventArgs e)
        {
            if (m_clipboardHistory.Items.Count == 0)
            {
                m_listView.Items.Clear();
            }
            else
            {
                ClipboardHistory_OnAllItemsUpdated(sender, null);
            }
            
        }

        private void ClipboardHistory_OnItemInserted(object sender, Engine.Clipboard.Events.ItemInsertedEventArgs e)
        {
            if (TotalItemsToShowInList > 0 && (m_listView.Items.Count + 1) > TotalItemsToShowInList)
            {
                m_listView.Items.RemoveAt(TotalItemsToShowInList - 1);
            }

            AddListItem(e.Item, 0);

            if (AutoSaveHistory && RememberHistoryBetweenSessions)
            {
                Save();
            }
        }

        private void ClipboardHistory_OnAllItemsUpdated(object sender, Engine.Clipboard.Events.AllItemsUpdatedEvent e)
        {
            int selectedindex = GetSelectedIndex();
            
            m_clipboardHistory.StopTracking();
            m_listView.Items.Clear();

            foreach (ClipboardHistoryItem clipboardTextItem in m_clipboardHistory.Items)
            {
                AddListItem(clipboardTextItem);
            }

            if (selectedindex > -1 && m_listView.Items.Count > 0 && selectedindex < m_listView.Items.Count)
            {
                SelectItem(selectedindex);
            }

            m_listView.UpdateLayout();
            m_clipboardHistory.StartTracking();
        }

        private void ClipboardHistory_OnItemUpdated(object sender, Engine.Clipboard.Events.ItemUpdatedEventArgs e)
        {
            HistoryListViewEntry originalItem = (HistoryListViewEntry) m_listView.Items[e.ListIndex];
            originalItem.Name = e.Item.Name;
            originalItem.Text = e.Item.Text;
            originalItem.TextDisplay = e.Item.TextDisplay;
            originalItem.Type = e.Item.Type.GetDisplayName();
            originalItem.CopiedFrom = e.Item.CopiedFrom;
            originalItem.Locked = e.Item.Locked;
            originalItem.DateTime = e.Item.DateTime;

            m_listView.Items.RemoveAt(e.ListIndex);
            m_listView.Items.Insert(e.ListIndex, originalItem);

            m_listView.SelectedItems.Add(originalItem);
        }

        #endregion

        #region Private Helpers

        private void FocusSelectedItem(int listIndex)
        {
            ItemContainerGenerator itemContainerGenerator = m_listView.ItemContainerGenerator;
            DependencyObject dependencyObject = itemContainerGenerator.ContainerFromIndex(listIndex);

            if (dependencyObject != null)
            {
                ListViewItem listViewItem = (ListViewItem) dependencyObject;
                listViewItem.Focus();
            }
        }

        private static string GetColumnName(params object[] parameters)
        {
            return string.Format("{0}{1}", parameters.ToArray());
        }

        #endregion
    }
}
