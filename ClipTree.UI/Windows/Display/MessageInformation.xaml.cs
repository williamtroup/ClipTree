using ClipTree.UI.Tools.Actions;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ClipTree.Windows.Display
{
    public partial class MessageInformation : Window
    {
        public MessageInformation(string message)
        {
            InitializeComponent();

            SetupDisplay(message);
        }

        private void SetupDisplay(string message)
        {
            lblMessage.Text = message;
        }

        private void Window_OnActivated(object sender, EventArgs e)
        {
            WindowBorder.BorderBrush = Brushes.Gray;
        }

        private void Window_OnDeactivated(object sender, EventArgs e)
        {
            WindowBorder.BorderBrush = Brushes.DarkGray;
        }

        private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (KeyStroke.IsAltKey(Key.Space))
            {
                e.Handled = true;
            }
        }
    }
}
