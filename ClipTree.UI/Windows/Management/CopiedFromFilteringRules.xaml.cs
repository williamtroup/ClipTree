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

namespace ClipTree.Windows.Management;

public partial class CopiedFromFilteringRules : Window
{
    private readonly IXMLSettings m_settings;
    private readonly Main m_main;
    private readonly WindowPosition m_windowPosition;

    private bool m_running = true;

    public CopiedFromFilteringRules(IXMLSettings settings, Main main)
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
            return string.Format(Settings.WindowNameFormat, nameof(CopiedFromFilteringRules), Settings.Window);
        }
    }

    private void SetupDisplay()
    {
        Dictionary<string, string> rules = m_settings.ReadAll(Settings.CopiedFromFilteringRules);

        foreach (KeyValuePair<string, string> rule in rules)
        {
            AddListItem(rule.Key, rule.Value, false);
        }
    }

    public void AddListItem(string copiedFrom, string enabled, bool setChanged = true)
    {
        FilteringRuleListViewEntry filteringRuleListViewEntry = new FilteringRuleListViewEntry
        {
            CopiedFrom = copiedFrom,
            Enabled = enabled
        };

        lstvRules.Items.Add(filteringRuleListViewEntry);

        if (setChanged)
        {
            Changed = true;
        }
    }

    public bool DoesRuleExist(string copiedFrom)
    {
        return lstvRules.Items.Cast<FilteringRuleListViewEntry>().Any(listViewItem => string.Equals(listViewItem.CopiedFrom, copiedFrom, StringComparison.CurrentCultureIgnoreCase));
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
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        AddCopiedFromFilteringRule addCopiedFromFilteringRule = new AddCopiedFromFilteringRule(m_settings, this)
        {
            Topmost = Topmost,
            Owner = this
        };

        addCopiedFromFilteringRule.ShowDialog();
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
            FilteringRuleListViewEntry filteringRuleListViewEntry = (FilteringRuleListViewEntry)lstvRules.Items[selectedIndex];
            filteringRuleListViewEntry.Enabled = enabledState;

            lstvRules.Items.RemoveAt(selectedIndex);
            lstvRules.Items.Insert(selectedIndex, filteringRuleListViewEntry);

            lstvRules.SelectedIndex = selectedIndex;
            Changed = true;
        }
    }

    private void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (Changed)
        {
            XmlDocument xmlDocument = m_settings.GetDocument();

            m_settings.RemoveSection(Settings.CopiedFromFilteringRules, xmlDocument);

            foreach (object listViewItem in lstvRules.Items)
            {
                FilteringRuleListViewEntry listItemEntry = (FilteringRuleListViewEntry) listViewItem;

                m_settings.Write(Settings.CopiedFromFilteringRules, listItemEntry.CopiedFrom, listItemEntry.Enabled, xmlDocument);
            }

            m_main.InitializeCopiedFromFilterRules();

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
}