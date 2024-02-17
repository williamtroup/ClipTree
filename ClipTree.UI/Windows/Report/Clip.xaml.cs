using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ClipTree.Engine.Clipboard;
using ClipTree.Engine.Clipboard.Interfaces;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools;
using ClipTree.UI.Tools.Actions;
using ClipTree.UI.Tools.Interfaces;
using ClipTree.UI.Tools.Views;

namespace ClipTree.Windows.Report
{
    public partial class Clip : Window
    {
        #region Private Read-Only Variables

        private readonly IClipboardHistory m_clipboardHistory;
        private readonly IClipboardHistoryItems m_clipboardHistoryItems;
        private readonly ClipboardHistoryItem m_clipboardHistoryItem;
        private readonly WindowPosition m_windowPosition;
        private readonly bool m_showShortcutButtonsOnClipWindow;

        #endregion

        public Clip(
            IXMLSettings settings, 
            IClipboardHistory clipboardHistory, 
            IClipboardHistoryItems clipboardHistoryItems, 
            ClipboardHistoryItem clipboardHistoryItem,
            bool showShortcutButtonsOnClipWindow)
        {
            InitializeComponent();

            m_clipboardHistory = clipboardHistory;
            m_clipboardHistoryItems = clipboardHistoryItems;
            m_clipboardHistoryItem = clipboardHistoryItem;
            m_windowPosition = new WindowPosition(this, settings, Width, Height, GetName);
            m_showShortcutButtonsOnClipWindow = showShortcutButtonsOnClipWindow;

            LockWindowActions lockWindowActions = new LockWindowActions(this)
            {
                LockMaximizing = true,
                LockMinimizing = true
            };

            SetupDisplay();

            BackgroundAction.Run(() => m_windowPosition.Get());
        }

        public static string GetName
        {
            get
            {
                return string.Format(Settings.WindowNameFormat, nameof(Clip), Settings.Window);
            }
        }

        private void SetupDisplay()
        {
            string text = m_clipboardHistoryItem.Text;

            lblTitle.Text = string.Format("\"{0}\" Clip", m_clipboardHistoryItem.Name);

            if (!m_showShortcutButtonsOnClipWindow)
            {
                spShortbutButtons.Visibility = Visibility.Collapsed;
            }

            if (m_clipboardHistoryItem.Type == TextDataFormat.Rtf)
            {
                MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(text));
                rtbClip.Selection.Load(stream, DataFormats.Rtf);
            }
            else
            {
                FlowDocument flowDocument = new FlowDocument();

                flowDocument.Blocks.Add(new Paragraph(new Run(text)));

                rtbClip.Document = flowDocument;
            }

            rtbClip.Focus();
        }

        #region Private "Title Bar" Events

        private void Title_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();

                m_windowPosition.Changed = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Private "Window" Events

        private void Window_OnActivated(object sender, EventArgs e)
        {
            WindowBorder.Background = Brushes.Gray;
        }

        private void Window_OnDeactivated(object sender, EventArgs e)
        {
            WindowBorder.Background = Brushes.DarkGray;
        }

        private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (KeyStroke.IsAltKey(Key.Space))
            {
                e.Handled = true;
            }
        }

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_windowPosition.Set();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            m_windowPosition.Changed = true;
        }

        #endregion

        #region Private "Clipboard Buttons" Events

        private void Button_SetAsCurrent_OnClick(object sender, RoutedEventArgs e)
        {
            m_clipboardHistory.SetCurrentText(m_clipboardHistoryItem.Text, m_clipboardHistoryItem.Type, m_clipboardHistoryItem.CopiedFrom);
            m_clipboardHistoryItems.SelectItem(0);

            Close();
        }

        private void Button_SetAsCurrentStripFormatting_OnClick(object sender, RoutedEventArgs e)
        {
            m_clipboardHistory.SetCurrentText(m_clipboardHistoryItem.TextDisplay, TextDataFormat.Text, m_clipboardHistoryItem.CopiedFrom);
            m_clipboardHistoryItems.SelectItem(0);

            Close();
        }

        private void Button_Remove_OnClick(object sender, RoutedEventArgs e)
        {
            int actualIndex = m_clipboardHistory.Items.IndexOf(m_clipboardHistoryItem);
            if (actualIndex > -1)
            {
                m_clipboardHistory.Remove(new int[] { actualIndex });
            }

            Close();
        }

        #endregion
    }
}