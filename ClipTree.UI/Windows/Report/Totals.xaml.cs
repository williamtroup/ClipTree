using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools;
using ClipTree.UI.Tools.ViewModels;
using ClipTree.UI.Tools.Views;
using ClipTree.UI.Tools.Actions;
using ClipTree.UI.Tools.Extensions;

namespace ClipTree.Windows.Report
{
    public partial class Totals : Window
    {
        private readonly WindowPosition m_windowPosition;

        public Totals(IXMLSettings settings, Dictionary<TextDataFormat, int> totals)
        {
            InitializeComponent();

            m_windowPosition = new WindowPosition(this, settings, Width, Height, GetName);

            SetupDisplay(totals);

            BackgroundAction.Run(() => m_windowPosition.Get());
        }

        public static string GetName
        {
            get
            {
                return string.Format(Settings.WindowNameFormat, nameof(Totals), Settings.Window);
            }
        }

        private void SetupDisplay(Dictionary<TextDataFormat, int> totals)
        {
            foreach (KeyValuePair<TextDataFormat, int> total in totals)
            {
                TotalListViewEntry newListItemEntry = new TotalListViewEntry()
                {
                    Type = total.Key.GetDisplayName(),
                    Total = total.Value.ToString(),
                };

                lstvTotals.Items.Add(newListItemEntry);
            }
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
    }
}
