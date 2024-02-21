using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClipTree.Engine.Clipboard;
using ClipTree.Engine.Clipboard.Interfaces;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools;
using ClipTree.UI.Tools.Views;
using System.Globalization;
using ClipTree.UI.Tools.Actions;

namespace ClipTree.Windows.List;

public partial class CleanUp : Window
{
    private readonly IClipboardHistory m_clipboardHistory;
    private readonly WindowPosition m_windowPosition;

    public CleanUp(IXMLSettings settings, IClipboardHistory clipboardHistory)
    {
        InitializeComponent();

        m_clipboardHistory = clipboardHistory;
        m_windowPosition = new WindowPosition(this, settings, Width, Height, GetName);

        SetupDisplay();

        BackgroundAction.Run(() => m_windowPosition.Get());
    }

    public static string GetName => string.Format(Settings.WindowNameFormat, nameof(CleanUp), Settings.Window);

    private void SetupDisplay()
    {
        lblErrorMessage.Visibility = Visibility.Hidden;

        NumericInput.Make(txtDays);

        txtDays.Focus();
    }

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

    private void CleanButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(txtDays.Text) || !int.TryParse(txtDays.Text, out int days))
        {
            lblErrorMessage.Content = ClipTree.Resources.Dialog.CleanNoValidValueForDaysEntered;
            lblErrorMessage.Visibility = Visibility.Visible;

            txtDays.Focus();
        }
        else
        {
            bool itemsRemoved = false;

            for (int itemIndex = m_clipboardHistory.Items.Count; itemIndex-- > 0;)
            {
                ClipboardHistoryItem clipboardHistoryItem = m_clipboardHistory.Items[itemIndex];

                if (clipboardHistoryItem.Locked != ClipTree.Resources.Statuses.EnabledYes)
                {
                    DateTime dateTime = DateTime.ParseExact(clipboardHistoryItem.DateTime, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                    if (dateTime < DateTime.Now.AddDays(-days))
                    {
                        m_clipboardHistory.Items.RemoveAt(itemIndex);
                        itemsRemoved = true;
                    }
                }
            }

            if (itemsRemoved)
            {
                m_clipboardHistory.SetAllItemsUpdated();
            }

            Close();
        }
    }
}