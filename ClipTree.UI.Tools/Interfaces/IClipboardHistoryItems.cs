using ClipTree.Engine.Clipboard;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools.Enums;

namespace ClipTree.UI.Tools.Interfaces
{
    public interface IClipboardHistoryItems
    {
        void Load(IXMLSettings settings = null);
        void Save(IXMLSettings settings = null);
        int GetSelectedIndex();
        int[] GetSelectedIndexes();
        void SetAsCurrent();
        void SetAsCurrentTextFormat();
        void SelectItem(int index);
        void FindPreviousText(string text, int? startIndex = null, bool goToFirst = false, bool matchCase = false, SearchType searchType = SearchType.Contains);
        void FindNextText(string text, int? startIndex = null, bool goToLast = false, bool matchCase = false, SearchType searchType = SearchType.Contains);
        int GetTotalNumberOfItems(string text, bool matchCase = false, SearchType searchType = SearchType.Contains);
        int MarkItemsAsSelected(string text, bool matchCase = false, SearchType searchType = SearchType.Contains);
        void SelectAll();
        void SelectNone();
        void InvertSelection();
        void LoadColors();
        void UpdateItemName(ClipboardHistoryItem clipboardHistoryItem, int selectedIndex);
        void MergeAsText();

        bool RowColorsEnabled { get; set; }
        bool AutoSaveHistory { get; set; }
        bool RememberHistoryBetweenSessions { get; set; }
        int TotalItemsToShowInList { get; set; }
    }
}
