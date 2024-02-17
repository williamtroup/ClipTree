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

namespace ClipTree.Windows.Management
{
    public partial class AddOnTopRule : Window
    {
        #region Private Read-Only Variables

        private readonly IXMLSettings m_settings;
        private readonly OnTopRules m_onTopRules;
        private readonly WindowPosition m_windowPosition;

        #endregion

        public AddOnTopRule(IXMLSettings settings, OnTopRules onTopRules)
        {
            InitializeComponent();

            m_settings = settings;
            m_onTopRules = onTopRules;
            m_windowPosition = new WindowPosition(this, m_settings, Width, Height, GetName);

            SetupDisplay();

            BackgroundAction.Run(() => m_windowPosition.Get());
        }

        public static string GetName
        {
            get
            {
                return string.Format(Settings.WindowNameFormat, nameof(AddOnTopRule), Settings.Window);
            }
        }

        private void SetupDisplay()
        {
            int closeWindowAfterAdding = Convert.ToInt32(m_settings.Read(Settings.AddOnTopRuleWindow.AddOnTopRuleOptions, nameof(Settings.AddOnTopRuleWindow.CloseWindowAfterAdding), Settings.AddOnTopRuleWindow.CloseWindowAfterAdding));

            chkCloseWindowAfterAdding.IsChecked = closeWindowAfterAdding > 0;

            lblErrorMessage.Visibility = Visibility.Hidden;

            txtCopiedFrom.Focus();
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

            m_settings.Write(Settings.AddOnTopRuleWindow.AddOnTopRuleOptions, nameof(Settings.AddOnTopRuleWindow.CloseWindowAfterAdding), chkCloseWindowAfterAdding.IsReallyChecked().ToNumericString());
        }

        #endregion

        #region Private "Adding" Events

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string newCopiedFrom = txtCopiedFrom.Text.Trim();

            if (string.IsNullOrEmpty(newCopiedFrom))
            {
                lblErrorMessage.Content = ClipTree.Resources.Dialog.NoCopiedFromEntered;
                lblErrorMessage.Visibility = Visibility.Visible;

                txtCopiedFrom.Focus();
            }
            else if (m_onTopRules.DoesRuleExist(newCopiedFrom))
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

                m_onTopRules.AddListItem(newCopiedFrom, enabled);

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

        #endregion
    }
}
