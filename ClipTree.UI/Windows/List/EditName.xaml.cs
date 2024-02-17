using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClipTree.Engine.Clipboard;
using ClipTree.Engine.Clipboard.Interfaces;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools;
using ClipTree.UI.Tools.Views;
using ClipTree.UI.Tools.Interfaces;
using ClipTree.UI.Tools.Actions;

namespace ClipTree.Windows.List
{
    public partial class EditName : Window
    {
        #region Private Read-Only Variables

        private readonly IClipboardHistory m_clipboardHistory;
        private readonly IClipboardHistoryItems m_clipboardHistoryItems;
        private readonly int m_selectedIndex;
        private readonly WindowPosition m_windowPosition;

        #endregion

        public EditName(IXMLSettings settings, IClipboardHistory clipboardHistory, IClipboardHistoryItems clipboardHistoryItems, int selectedIndex)
        {
            InitializeComponent();

            clipboardHistory.StopTracking();

            m_clipboardHistory = clipboardHistory;
            m_clipboardHistoryItems = clipboardHistoryItems;
            m_selectedIndex = selectedIndex;
            m_windowPosition = new WindowPosition(this, settings, Width, Height, GetName);

            SetupDisplay();

            BackgroundAction.Run(() => m_windowPosition.Get());
        }

        public static string GetName
        {
            get
            {
                return string.Format(Settings.WindowNameFormat, nameof(EditName), Settings.Window);
            }
        }

        private void SetupDisplay()
        {
            lblErrorMessage.Visibility = Visibility.Hidden;

            txtName.Focus();
            txtName.Text = m_clipboardHistory.Items[m_selectedIndex].Name;
            txtName.SelectionStart = txtName.Text.Length;
            txtName.SelectAll();
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
            m_clipboardHistory.StartTracking();
        }

        #endregion

        #region Private "Updating" Events

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string newName = txtName.Text.Trim();

            ClipboardHistoryItem clipboardHistoryItem = m_clipboardHistory.Items[m_selectedIndex];
            bool hasItemChanged = newName != clipboardHistoryItem.Name;

            if (m_clipboardHistory.DoesNameExist(newName) && hasItemChanged)
            {
                lblErrorMessage.Content = ClipTree.Resources.Dialog.NameAlreadyExists;
                lblErrorMessage.Visibility = Visibility.Visible;

                txtName.Focus();
            }
            else
            {
                if (hasItemChanged)
                {
                    clipboardHistoryItem.Name = newName;

                    m_clipboardHistory.Items[m_selectedIndex] = clipboardHistoryItem;
                    m_clipboardHistoryItems.UpdateItemName(clipboardHistoryItem, m_selectedIndex);
                }

                Close();
            }
        }

        #endregion
    }
}
