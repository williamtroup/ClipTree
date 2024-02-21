using ClipTree.Engine.Clipboard;
using ClipTree.Engine.Clipboard.Interfaces;
using ClipTree.Engine.Settings;
using ClipTree.Engine.Tools;
using ClipTree.UI.Tools.Interfaces;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ClipTree.UI.Tools.Views;

public class FilenameDialog
{
    private readonly IClipboardHistory m_clipboardHistory;
    private readonly IClipboardHistoryItems m_clipboardHistoryItems;
    private readonly string m_htmlFilesFilter;
    private readonly string m_richTextFilesFilter;
    private readonly string m_textFilesFilter;

    public FilenameDialog(
        IClipboardHistory clipboardHistory, 
        IClipboardHistoryItems clipboardHistoryItems, 
        string htmlFilesFilter, 
        string richTextFilesFilter, 
        string textFilesFilter)
    {
        m_clipboardHistory = clipboardHistory;
        m_clipboardHistoryItems = clipboardHistoryItems;
        m_htmlFilesFilter = htmlFilesFilter;
        m_richTextFilesFilter = richTextFilesFilter;
        m_textFilesFilter = textFilesFilter;
    }

    public void Open(string filter, string title)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = filter,
            Title = title
        };

        bool? result = openFileDialog.ShowDialog();
        if (result != null && result.Value)
        {
            m_clipboardHistoryItems.Load(new XMLSettings(openFileDialog.FileName));

            m_clipboardHistory.SetTopItemAsCurrent();
        }
    }

    public void Save(string filter, string title)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = filter,
            Title = title
        };

        bool? result = saveFileDialog.ShowDialog();
        if (result != null && result.Value)
        {
            if (File.Exists(saveFileDialog.FileName))
            {
                File.Delete(saveFileDialog.FileName);
            }

            m_clipboardHistoryItems.Save(new XMLSettings(saveFileDialog.FileName));
        }
    }

    public void SaveItem(string title)
    {
        int selectedIndex = m_clipboardHistoryItems.GetSelectedIndex();
        if (selectedIndex > -1)
        {
            ClipboardHistoryItem clipboardHistoryItem = m_clipboardHistory.Items[selectedIndex];
            string data = null;

            string filter;
            switch (clipboardHistoryItem.Type)
            {
                case TextDataFormat.Html:
                    filter = m_htmlFilesFilter;
                    data = Html.StripHeader(clipboardHistoryItem);
                    break;

                case TextDataFormat.Rtf:
                    filter = m_richTextFilesFilter;
                    break;

                default:
                    filter = m_textFilesFilter;
                    data = clipboardHistoryItem.Text;
                    break;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                Title = title
            };

            bool? result = saveFileDialog.ShowDialog();
            if (result != null && result.Value)
            {
                if (File.Exists(saveFileDialog.FileName))
                {
                    File.Delete(saveFileDialog.FileName);
                }

                if (clipboardHistoryItem.Type != TextDataFormat.Rtf)
                {
                    File.WriteAllText(saveFileDialog.FileName, data);
                }
                else
                {
                    WriteRichText(saveFileDialog.FileName, clipboardHistoryItem);
                }
            }
        }
    }

    private static void WriteRichText(string filename, ClipboardHistoryItem clipboardHistoryItem)
    {
        RichTextBox richTextBox = new RichTextBox();

        FlowDocument flowDocument = new FlowDocument();
        flowDocument.Blocks.Add(new Paragraph(new Run(clipboardHistoryItem.Text)));

        richTextBox.Document = flowDocument;

        TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
        using (FileStream fileStream = new FileStream(filename, FileMode.Create))
        {
            range.Save(fileStream, DataFormats.Text);
            fileStream.Close();
        }
    }
}