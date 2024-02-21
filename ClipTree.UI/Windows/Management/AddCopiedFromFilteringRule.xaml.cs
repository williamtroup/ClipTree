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

public partial class AddCopiedFromFilteringRule : Window
{
    private readonly IXMLSettings m_settings;
    private readonly CopiedFromFilteringRules m_copiedFromFilteringRules;
    private readonly WindowPosition m_windowPosition;

    public AddCopiedFromFilteringRule(IXMLSettings settings, CopiedFromFilteringRules copiedFromFilteringRules)
    {
        InitializeComponent();

        m_settings = settings;
        m_copiedFromFilteringRules = copiedFromFilteringRules;
        m_windowPosition = new WindowPosition(this, m_settings, Width, Height, GetName);

        SetupDisplay();

        BackgroundAction.Run(() => m_windowPosition.Get());
    }

    public static string GetName
    {
        get
        {
            return string.Format(Settings.WindowNameFormat, nameof(AddCopiedFromFilteringRule), Settings.Window);
        }
    }

    private void SetupDisplay()
    {
        int closeWindowAfterAdding = Convert.ToInt32(m_settings.Read(Settings.AddCopiedFromFilteringRuleWindow.AddCopiedFromFilteringRuleOptions, nameof(Settings.AddCopiedFromFilteringRuleWindow.CloseWindowAfterAdding), Settings.AddCopiedFromFilteringRuleWindow.CloseWindowAfterAdding));

        chkCloseWindowAfterAdding.IsChecked = closeWindowAfterAdding > 0;

        lblErrorMessage.Visibility = Visibility.Hidden;

        txtCopiedFrom.Focus();
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

        m_settings.Write(Settings.AddCopiedFromFilteringRuleWindow.AddCopiedFromFilteringRuleOptions, nameof(Settings.AddCopiedFromFilteringRuleWindow.CloseWindowAfterAdding), chkCloseWindowAfterAdding.IsReallyChecked().ToNumericString());
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
        else if (m_copiedFromFilteringRules.DoesRuleExist(newCopiedFrom))
        {
            lblErrorMessage.Content = ClipTree.Resources.Dialog.RuleAlreadyExists;
            lblErrorMessage.Visibility = Visibility.Visible;

            txtCopiedFrom.Focus();
        }
        else
        {
            string enabled = chkEnabled.IsReallyChecked()
                ? ClipTree.Resources.Statuses.EnabledYes
                : ClipTree.Resources.Statuses.EnabledNo;

            m_copiedFromFilteringRules.AddListItem(newCopiedFrom, enabled);

            if (chkCloseWindowAfterAdding.IsReallyChecked())
            {
                Close();
            }
            else
            {
                txtCopiedFrom.Text = string.Empty;
                chkEnabled.IsChecked = false;
                txtCopiedFrom.Focus();
            }
        }
    }
}