using ClipTree.Engine.Extensions;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools;
using ClipTree.UI.Tools.Actions;
using ClipTree.UI.Tools.Extensions;
using ClipTree.UI.Tools.Views;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ClipTree.Windows.Management;

public partial class AddRowColor : Window
{
    private readonly IXMLSettings m_settings;
    private readonly RowColors m_rowColors;
    private readonly WindowPosition m_windowPosition;

    public AddRowColor(IXMLSettings settings, RowColors rowColors)
    {
        InitializeComponent();

        m_settings = settings;
        m_rowColors = rowColors;
        m_windowPosition = new WindowPosition(this, m_settings, Width, Height, GetName);

        SetupDisplay();

        BackgroundAction.Run(() => m_windowPosition.Get());
    }

    public static string GetName
    {
        get
        {
            return string.Format(Settings.WindowNameFormat, nameof(AddRowColor), Settings.Window);
        }
    }

    private void SetupDisplay()
    {
        int closeWindowAfterAdding = Convert.ToInt32(m_settings.Read(Settings.AddRowColorWindow.AddRowColorOptions, nameof(Settings.AddRowColorWindow.CloseWindowAfterAdding), Settings.AddRowColorWindow.CloseWindowAfterAdding));

        chkCloseWindowAfterAdding.IsChecked = closeWindowAfterAdding > 0;

        SetColorDefaults();

        lblErrorMessage.Visibility = Visibility.Hidden;

        txtCopiedFrom.Focus();
    }

    private void SetColorDefaults()
    {
        cpRowColor.SelectedColor = Colors.White;
        cpTextColor.SelectedColor = Colors.Black;
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

        m_settings.Write(Settings.AddRowColorWindow.AddRowColorOptions, nameof(Settings.AddRowColorWindow.CloseWindowAfterAdding), chkCloseWindowAfterAdding.IsReallyChecked().ToNumericString());
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        string newCopiedFrom = txtCopiedFrom.Text.Trim();

        if (string.IsNullOrEmpty(newCopiedFrom))
        {
            lblErrorMessage.Content = ClipTree.Resources.Dialog.NoCopiedFromEntered;
            lblErrorMessage.Visibility = Visibility.Visible;

            txtCopiedFrom.Focus();
        }
        else if (cpRowColor.SelectedColor == null)
        {
            lblErrorMessage.Content = ClipTree.Resources.Dialog.AddColorNoRowColorSelected;
            lblErrorMessage.Visibility = Visibility.Visible;

            cpRowColor.Focus();
        }
        else if (cpTextColor.SelectedColor == null)
        {
            lblErrorMessage.Content = ClipTree.Resources.Dialog.AddColorNoTextColorSelected;
            lblErrorMessage.Visibility = Visibility.Visible;

            cpTextColor.Focus();
        }
        else if (m_rowColors.DoesRowColorExist(newCopiedFrom))
        {
            lblErrorMessage.Content = ClipTree.Resources.Dialog.AddColorCopiedFromColorAlreadyExists;
            lblErrorMessage.Visibility = Visibility.Visible;

            txtCopiedFrom.Focus();
        }
        else
        {
            Color selectedRowColor = cpRowColor.SelectedColor.Value;
            Color selectedTextColor = cpTextColor.SelectedColor.Value;

            m_rowColors.AddListItem(newCopiedFrom, GetColorInteger(selectedRowColor).ToString(), GetColorInteger(selectedTextColor).ToString());

            if (chkCloseWindowAfterAdding.IsReallyChecked())
            {
                Close();
            }
            else
            {
                SetColorDefaults();

                txtCopiedFrom.Text = string.Empty;
                txtCopiedFrom.Focus();
            }
        }
    }

    private int GetColorInteger(Color color)
    {
        return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
    }
}