using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools;
using ClipTree.UI.Tools.Actions;
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
    public partial class OnTopRules : Window
    {
        #region Private Read-Only Variables

        private readonly IXMLSettings m_settings;
        private readonly Main m_main;
        private readonly WindowPosition m_windowPosition;

        #endregion

        #region Private Variables

        private bool m_running = true;

        #endregion 

        public OnTopRules(IXMLSettings settings, Main main)
        {
            InitializeComponent();

            Opacity = 0;

            m_settings = settings;
            m_main = main;
            m_windowPosition = new WindowPosition(this, m_settings, Width, Height, GetName);

            SetupDisplay();
            SetupWindowUpdateThread();

            BackgroundAction.Run(() => m_windowPosition.Get());
        }

        public static string GetName
        {
            get
            {
                return string.Format(Settings.WindowNameFormat, nameof(OnTopRules), Settings.Window);
            }
        }

        private void SetupDisplay()
        {
            Dictionary<string, string> rules = m_settings.ReadAll(Settings.OnTopRules);

            foreach (KeyValuePair<string, string> rule in rules)
            {
                AddListItem(rule.Key, rule.Value, false);
            }
        }

        public void AddListItem(string copiedFrom, string enabled, bool setChanged = true)
        {
            OnTopRuleListViewEntry onTopRuleListViewEntry = new OnTopRuleListViewEntry
            {
                CopiedFrom = copiedFrom,
                Enabled = enabled
            };

            lstvRules.Items.Add(onTopRuleListViewEntry);

            if (setChanged)
            {
                Changed = true;
            }
        }

        public bool DoesRuleExist(string copiedFrom)
        {
            return lstvRules.Items.Cast<OnTopRuleListViewEntry>().Any(listViewItem => string.Equals(listViewItem.CopiedFrom, copiedFrom, StringComparison.CurrentCultureIgnoreCase));
        }

        #region Private Properties

        private bool Changed { get; set; }

        #endregion

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
            else if (Keyboard.IsKeyDown(Key.Delete) && bRemove.IsEnabled)
            {
                RemoveButton_Click(sender, null);
            }
        }

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_running = false;

            m_windowPosition.Set();
        }

        #endregion

        #region Private "Editing Button" Events

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddOnTopRule addOnTopRule = new AddOnTopRule(m_settings, this)
            {
                Topmost = Topmost,
                Owner = this
            };

            addOnTopRule.ShowDialog();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = lstvRules.SelectedIndex;
            if (selectedIndex > -1)
            {
                lstvRules.Items.RemoveAt(selectedIndex);
                Changed = true;
            }
        }

        private void EnableButton_Click(object sender, RoutedEventArgs e)
        {
            SetEnabledState(ClipTree.Resources.Statuses.EnabledYes);
        }

        private void DisableButton_Click(object sender, RoutedEventArgs e)
        {
            SetEnabledState(ClipTree.Resources.Statuses.EnabledNo);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            lstvRules.Items.Clear();
            Changed = true;
        }

        private void SetEnabledState(string enabledState)
        {
            int selectedIndex = lstvRules.SelectedIndex;
            if (selectedIndex > -1)
            {
                OnTopRuleListViewEntry onTopRuleListViewEntry = (OnTopRuleListViewEntry)lstvRules.Items[selectedIndex];
                onTopRuleListViewEntry.Enabled = enabledState;

                lstvRules.Items.RemoveAt(selectedIndex);
                lstvRules.Items.Insert(selectedIndex, onTopRuleListViewEntry);

                lstvRules.SelectedIndex = selectedIndex;
                Changed = true;
            }
        }

        #endregion

        #region Private "Updating" Events

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Changed)
            {
                XmlDocument xmlDocument = m_settings.GetDocument();

                m_settings.RemoveSection(Settings.OnTopRules, xmlDocument);

                foreach (object listViewItem in lstvRules.Items)
                {
                    OnTopRuleListViewEntry listItemEntry = (OnTopRuleListViewEntry) listViewItem;

                    m_settings.Write(Settings.OnTopRules, listItemEntry.CopiedFrom, listItemEntry.Enabled, xmlDocument);
                }

                m_main.InitializeOnTopRules();

                m_settings.SaveDocument(xmlDocument);
            }

            Close();
        }

        #endregion

        #region Private "Update Window Thread" Helpers

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
                    bool oneItemSelected = lstvRules.SelectedIndex > -1;
                    bool itemsinList = lstvRules.Items.Count > 0;

                    bRemove.IsEnabled = oneItemSelected;
                    bEnable.IsEnabled = oneItemSelected;
                    bDisable.IsEnabled = oneItemSelected;
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

        #endregion
    }
}