using ClipTree.Engine.Clipboard.Interfaces;
using ClipTree.Engine.Extensions;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools;
using ClipTree.UI.Tools.Actions;
using ClipTree.UI.Tools.Extensions;
using ClipTree.UI.Tools.Interfaces;
using ClipTree.UI.Tools.ViewModels;
using ClipTree.UI.Tools.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace ClipTree.Windows.Management
{
    public partial class RowColors : Window
    {
        private readonly IXMLSettings m_settings;
        private readonly IClipboardHistory m_clipboardHistory;
        private readonly IClipboardHistoryItems m_clipboardHistoryItems;
        private readonly WindowPosition m_windowPosition;

        private bool m_running = true;

        public RowColors(IXMLSettings settings, IClipboardHistory clipboardHistory, IClipboardHistoryItems clipboardHistoryItems)
        {
            InitializeComponent();

            Opacity = 0;

            m_settings = settings;
            m_clipboardHistory = clipboardHistory;
            m_clipboardHistoryItems = clipboardHistoryItems;
            m_windowPosition = new WindowPosition(this, m_settings, Width, Height, GetName);

            SetupDisplay();
            SetupWindowUpdateThread();

            BackgroundAction.Run(() => m_windowPosition.Get());
        }

        public static string GetName
        {
            get
            {
                return string.Format(Settings.WindowNameFormat, nameof(RowColors), Settings.Window);
            }
        }

        private void SetupDisplay()
        {
            int ShowColorsAsTheyWouldAppear = Convert.ToInt32(m_settings.Read(Settings.RowColorsWindow.RowColorsOptions, nameof(Settings.RowColorsWindow.ShowColorsAsTheyWouldAppear), Settings.RowColorsWindow.ShowColorsAsTheyWouldAppear));

            chkShowColorsAsTheyWouldAppear.IsChecked = ShowColorsAsTheyWouldAppear > 0;

            XmlDocument xmlDocument = m_settings.GetDocument();

            Dictionary<string, string> colors = m_settings.ReadAll(Settings.RowColors, xmlDocument);
            Dictionary<string, string> textColors = m_settings.ReadAll(Settings.RowTextColors, xmlDocument);

            foreach (KeyValuePair<string, string> color in colors)
            {
                AddListItem(color.Key, color.Value, textColors[color.Key], false);
            }
        }

        public void AddListItem(string copiedFrom, string color, string textColor, bool setChanged = true)
        {
            RowColorListViewEntry rowColorListViewEntry = new RowColorListViewEntry
            {
                CopiedFrom = copiedFrom,
                Color = color,
                TextColor = textColor
            };

            if (chkShowColorsAsTheyWouldAppear.IsReallyChecked())
            {
                rowColorListViewEntry.ShowColors(true);
            }

            lstvColors.Items.Add(rowColorListViewEntry);

            if (setChanged)
            {
                Changed = true;
            }
        }

        public bool DoesRowColorExist(string copiedFrom)
        {
            return lstvColors.Items.Cast<RowColorListViewEntry>().Any(listViewItem => string.Equals(listViewItem.CopiedFrom, copiedFrom, StringComparison.CurrentCultureIgnoreCase));
        }

        private bool Changed { get; set; }

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
            else if (Keyboard.IsKeyDown(Key.Delete) && bRemove.IsEnabled)
            {
                RemoveButton_Click(sender, null);
            }
        }

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_running = false;

            m_windowPosition.Set();

            m_settings.Write(Settings.RowColorsWindow.RowColorsOptions, nameof(Settings.RowColorsWindow.ShowColorsAsTheyWouldAppear), chkShowColorsAsTheyWouldAppear.IsReallyChecked().ToNumericString());
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddRowColor addRowColor = new AddRowColor(m_settings, this)
            {
                Topmost = Topmost,
                Owner = this
            };

            addRowColor.ShowDialog();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = lstvColors.SelectedIndex;
            if (selectedIndex > -1)
            {
                lstvColors.Items.RemoveAt(selectedIndex);
                Changed = true;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            lstvColors.Items.Clear();
            Changed = true;
        }

        private void ShowColorsAsTheyWouldAppear_Checked(object sender, RoutedEventArgs e)
        {
            SetShowColorMode(true);
        }

        private void ShowColorsAsTheyWouldAppear_Unchecked(object sender, RoutedEventArgs e)
        {
            SetShowColorMode(false);
        }

        private void SetShowColorMode(bool showColors)
        {
            List<RowColorListViewEntry> newItems = new List<RowColorListViewEntry>();

            int selectedIndex = lstvColors.SelectedIndex;

            foreach (object item in lstvColors.Items)
            {
                RowColorListViewEntry rowColorListViewEntry = (RowColorListViewEntry)item;

                rowColorListViewEntry.ShowColors(showColors);

                newItems.Add(rowColorListViewEntry);
            }

            lstvColors.Items.Clear();

            foreach (RowColorListViewEntry rowColorListViewEntry in newItems)
            {
                lstvColors.Items.Add(rowColorListViewEntry);
            }

            if (selectedIndex > -1)
            {
                lstvColors.SelectedItem = lstvColors.Items[selectedIndex];
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Changed)
            {
                XmlDocument xmlDocument = m_settings.GetDocument();

                m_settings.RemoveSection(Settings.RowColors, xmlDocument);
                m_settings.RemoveSection(Settings.RowTextColors, xmlDocument);

                foreach (object listViewItem in lstvColors.Items)
                {
                    RowColorListViewEntry listItemEntry = (RowColorListViewEntry) listViewItem;

                    m_settings.Write(Settings.RowColors, listItemEntry.CopiedFrom, listItemEntry.Color, xmlDocument);
                    m_settings.Write(Settings.RowTextColors, listItemEntry.CopiedFrom, listItemEntry.TextColor, xmlDocument);
                }

                m_clipboardHistoryItems.LoadColors();
                m_clipboardHistory.SetAllItemsUpdated();

                m_settings.SaveDocument(xmlDocument);
            }

            Close();
        }

        private void SetupWindowUpdateThread()
        {
            Thread thread = new Thread(WindowUpdateThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void WindowUpdateThread()
        {
            while (m_running)
            {
                BackgroundAction.Run(() =>
                {
                    bool oneItemSelected = lstvColors.SelectedIndex > -1;
                    bool itemsinList = lstvColors.Items.Count > 0;

                    bRemove.IsEnabled = oneItemSelected;
                    bClear.IsEnabled = itemsinList;

                    bUpdate.IsEnabled = Changed;

                    UpdateOpacityDisplay();
                });

                Thread.Sleep(50);
            }
        }

        private void UpdateOpacityDisplay()
        {
            if (Opacity <= 0)
            {
                Opacity = 1.0;
            }
        }
    }
}