using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClipTree.Engine.Clipboard;
using ClipTree.Engine.Clipboard.Interfaces;
using ClipTree.Engine.Extensions;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools.Interfaces;
using ClipTree.UI.Tools.Views;
using ClipTree.Engine.Api;
using ClipTree.UI.Tools;
using ClipTree.Windows.List;
using ClipTree.Windows.Management;
using ClipTree.Windows.Report;
using ClipTree.Engine.Tools;
using ClipTree.Engine.Windows;
using ClipTree.UI.Tools.Actions;
using log4net;
using System.Xml;
using ClipTree.UI.Tools.Enums;
using ClipTree.Windows.Display;
using ClipTree.Engine.Settings;

namespace ClipTree
{
    public partial class Main : Window
    {
        #region Private Constants

        private const int UpdateWindowInterval = 50;
        private const double OpacityIncriment = 0.08;

        #endregion

        #region Private Read-Only Variables

        private readonly IXMLSettings m_settings;
        private readonly WindowPosition m_windowPosition;
        private readonly ListViewSettings m_listViewSettings;
        private readonly IClipboardHistory m_clipboardHistory;
        private readonly IClipboardHistoryItems m_clipboardHistoryItems;
        private readonly FilenameDialog m_filenameDialog;
        private readonly LockWindowActions m_lockWindowActions;

        #endregion

        #region Private Variables

        private bool m_updateWindowThreadRunning = true;
        private bool m_enableOnTopRules;
        private Dictionary<string, string> m_onTopRules;
        private bool m_viewHtmlClipsInDefaultBrowser;
        private bool m_viewURLEntriesInTheDefaultBrowser;
        private bool m_viewPathEntriesInTheDefaultProgram;
        private bool m_viewEmailEntriesInTheDefaultEmailComposer;
        private bool m_showConfirmationMessagesForFileActions;
        private bool m_showShortcutButtonsOnClipWindow;
        private bool m_viewHTMLHexColorsAsAnActualColor;
        private WindowMode m_windowMode;

        #endregion

        public Main(IXMLSettings settings)
        {
            InitializeComponent();

            Opacity = 0;

            m_windowMode = WindowMode.Load;
            m_settings = settings;
            m_windowPosition = new WindowPosition(this, m_settings, Width, Height);
            m_listViewSettings = new ListViewSettings(m_settings, lstvHistory);
            m_clipboardHistory = new ClipboardHistory(ClipTree.Resources.Defaults.Name, ClipTree.Resources.Defaults.CopiedFromName, ClipTree.Resources.Statuses.EnabledYes, ClipTree.Resources.Statuses.EnabledNo);
            m_clipboardHistoryItems = new ClipboardHistoryItems(lstvHistory, m_clipboardHistory, m_settings, ClipTree.Resources.Defaults.Name, ClipTree.Resources.Defaults.CopiedFromName, ClipTree.Resources.Statuses.EnabledNo);
            m_filenameDialog = new FilenameDialog(m_clipboardHistory, m_clipboardHistoryItems, ClipTree.Resources.Dialog.HTMLFilesFilter, ClipTree.Resources.Dialog.RichTextFilesFilter, ClipTree.Resources.Dialog.TextFilesFilter);
            m_lockWindowActions = new LockWindowActions(this)
            {
                LockMaximizing = true
            };

            InitializeDisplaySettings(false);
            InitializeOnTopRules();
            InitializeCopiedFromFilterRules();
            InitializeSettings();

            if (m_clipboardHistoryItems.RememberHistoryBetweenSessions)
            {
                m_clipboardHistoryItems.Load();
            }
            else
            {
                m_clipboardHistory.StartTracking();
            }

            SetupWindowUpdateThread();

            BackgroundAction.Run(() => m_windowPosition.Get(true));
        }

        public void InitializeDisplaySettings(bool markAllItemsAsUpdated = true)
        {
            XmlDocument xmlDocument = m_settings.GetDocument();

            int showInTaskBar = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowInTaskBar), Settings.MainWindow.ShowInTaskBar, xmlDocument));
            int showStatusBar = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowStatusBar), Settings.MainWindow.ShowStatusBar, xmlDocument));
            int showCapsNumLockStatues = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowCapsNumLockStatues), Settings.MainWindow.ShowCapsNumLockStatues, xmlDocument));
            int showTotalItems = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowTotalItems), Settings.MainWindow.ShowTotalItems, xmlDocument));
            int enableRowColors = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.EnableRowColors), Settings.MainWindow.EnableRowColors, xmlDocument));
            int enableOnTopRules = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.EnableOnTopRules), Settings.MainWindow.EnableOnTopRules, xmlDocument));
            int enableCopiedFromFiltering = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.EnableCopiedFromFiltering), Settings.MainWindow.EnableCopiedFromFiltering, xmlDocument));
            int autoSaveHistory = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.AutoSaveHistory), Settings.MainWindow.AutoSaveHistory, xmlDocument));
            int viewHtmlClipsInDefaultBrowser = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewHtmlClipsInDefaultBrowser), Settings.MainWindow.ViewHtmlClipsInDefaultBrowser, xmlDocument));
            int rememberHistoryBetweenSessions = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.RememberHistoryBetweenSessions), Settings.MainWindow.RememberHistoryBetweenSessions, xmlDocument));
            int totalsItemsToShowInList = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.TotalsItemsToShowInList), Settings.MainWindow.TotalsItemsToShowInList, xmlDocument));
            int updateClipboardInterval = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.UpdateClipboardInterval), Settings.MainWindow.UpdateClipboardInterval, xmlDocument));
            int autoTrimTextAndUnicodeEntries = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.AutoTrimTextAndUnicodeEntries), Settings.MainWindow.AutoTrimTextAndUnicodeEntries, xmlDocument));
            int viewURLEntriesInTheDefaultBrowser = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewURLEntriesInTheDefaultBrowser), Settings.MainWindow.ViewURLEntriesInTheDefaultBrowser, xmlDocument));
            int viewPathEntriesInTheDefaultProgram = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewPathEntriesInTheDefaultProgram), Settings.MainWindow.ViewPathEntriesInTheDefaultProgram, xmlDocument));
            int viewEmailAddressesInTheDefaultEmailComposer = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewEmailAddressesInTheDefaultEmailComposer), Settings.MainWindow.ViewEmailAddressesInTheDefaultEmailComposer, xmlDocument));
            int showConfirmationMessagesForFileActions = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowConfirmationMessagesForFileActions), Settings.MainWindow.ShowConfirmationMessagesForFileActions, xmlDocument));
            int showShortcutButtonsOnClipWindow = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowShortcutButtonsOnClipWindow), Settings.MainWindow.ShowShortcutButtonsOnClipWindow, xmlDocument));
            int viewHTMLHexColorsAsAnActualColor = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewHTMLHexColorsAsAnActualColor), Settings.MainWindow.ViewHTMLHexColorsAsAnActualColor, xmlDocument));

            rdStatusBar.Height = showStatusBar > 0 ? new GridLength(30) : new GridLength(0);
            lblCapsLock.Visibility = showCapsNumLockStatues > 0 ? Visibility.Visible : Visibility.Collapsed;
            lblNumLock.Visibility = showCapsNumLockStatues > 0 ? Visibility.Visible : Visibility.Collapsed;
            lblTotalItems.Visibility = showTotalItems > 0 ? Visibility.Visible : Visibility.Collapsed;

            ShowInTaskbar = showInTaskBar > 0;
            m_enableOnTopRules = enableOnTopRules > 0;
            m_clipboardHistoryItems.RowColorsEnabled = enableRowColors > 0;
            m_clipboardHistoryItems.AutoSaveHistory = autoSaveHistory > 0;
            m_viewHtmlClipsInDefaultBrowser = viewHtmlClipsInDefaultBrowser > 0;
            m_clipboardHistoryItems.RememberHistoryBetweenSessions = rememberHistoryBetweenSessions > 0;
            m_clipboardHistoryItems.TotalItemsToShowInList = totalsItemsToShowInList;
            m_clipboardHistory.EnableCopiedFromFiltering = enableCopiedFromFiltering > 0;
            m_clipboardHistory.UpdateInterval = updateClipboardInterval;
            m_clipboardHistory.AutoTrimTextAndUnicodeEntries = autoTrimTextAndUnicodeEntries > 0;
            m_viewURLEntriesInTheDefaultBrowser = viewURLEntriesInTheDefaultBrowser > 0;
            m_viewPathEntriesInTheDefaultProgram = viewPathEntriesInTheDefaultProgram > 0;
            m_viewEmailEntriesInTheDefaultEmailComposer = viewEmailAddressesInTheDefaultEmailComposer > 0;
            m_showConfirmationMessagesForFileActions = showConfirmationMessagesForFileActions > 0;
            m_showShortcutButtonsOnClipWindow = showShortcutButtonsOnClipWindow > 0;
            m_viewHTMLHexColorsAsAnActualColor = viewHTMLHexColorsAsAnActualColor > 0;

            if (markAllItemsAsUpdated)
            {
                m_clipboardHistory.SetAllItemsUpdated();
            }

            InitializeMinimizeButton();
        }

        private void InitializeMinimizeButton()
        {
            string minimizeText = ClipTree.Resources.UIMessages.Minimize;

            if (!ShowInTaskbar)
            {
                minimizeText = ClipTree.Resources.UIMessages.MinimizeToTray;
            }

            bMinimize.ToolTip = minimizeText;
            mnuMinimize.Header = string.Format("_{0}", minimizeText);
        }

        public void InitializeOnTopRules()
        {
            if (m_onTopRules != null)
            {
                m_onTopRules.Clear();
                m_onTopRules = null;
            }

            m_onTopRules = m_settings.ReadAll(Settings.OnTopRules);
        }

        public void InitializeCopiedFromFilterRules()
        {
            if (m_clipboardHistory.CopiedFromFilters != null)
            {
                m_clipboardHistory.CopiedFromFilters.Clear();
                m_clipboardHistory.CopiedFromFilters = null;
            }

            m_clipboardHistory.CopiedFromFilters = m_settings.ReadAll(Settings.CopiedFromFilteringRules);
        }

        private void InitializeSettings()
        {
            XmlDocument xmlDocument = m_settings.GetDocument();

            int locked = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Tools, nameof(Settings.MainWindow.Locked), Settings.MainWindow.Locked, xmlDocument));
            int alwaysOnTop = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Tools, nameof(Settings.MainWindow.AlwaysOnTop), Settings.MainWindow.AlwaysOnTop, xmlDocument));
            int disableClipboard = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Tools, nameof(Settings.MainWindow.DisableClipboard), Settings.MainWindow.DisableClipboard, xmlDocument));
            string lastItemFiltered = m_settings.Read(Settings.MainWindow.Tools, nameof(Settings.MainWindow.LastItemFiltered), Settings.MainWindow.LastItemFiltered, xmlDocument);
            int startEngine = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Tools, nameof(Settings.MainWindow.StartEngine), Settings.MainWindow.StartEngine, xmlDocument));

            m_lockWindowActions.LockMoving = locked > 0;
            Topmost = alwaysOnTop > 0;
            m_clipboardHistory.DisableAllClipboardCopying = disableClipboard > 0;
            m_clipboardHistory.LastItemFiltered = lastItemFiltered;
            m_clipboardHistory.EngineRunning = startEngine > 0;

            UpdateWindowLockedState();
            UpdateWindowAlwaysOnTopState();
            UpdateWindowClipboardDisabledStatus();
            UpdateEngineRunningStatus();
        }

        private void UpdateWindowLockedState()
        {
            ResizeMode = m_lockWindowActions.LockMoving ? ResizeMode.NoResize : ResizeMode.CanResizeWithGrip;

            bLock.Visibility = m_lockWindowActions.LockMoving ? Visibility.Collapsed : Visibility.Visible;
            bUnlock.Visibility = m_lockWindowActions.LockMoving ? Visibility.Visible : Visibility.Collapsed;

            mnuDockToTheRight.IsEnabled = !m_lockWindowActions.LockMoving;
            mnuDockToTheLeft.IsEnabled = !m_lockWindowActions.LockMoving;
            mnuMinimize.IsEnabled = !m_lockWindowActions.LockMoving;

            bMinimize.IsEnabled = !m_lockWindowActions.LockMoving;
        }

        private void UpdateWindowAlwaysOnTopState()
        {
            bAlwaysOnTopOn.Visibility = Topmost ? Visibility.Collapsed : Visibility.Visible;
            bAlwaysOnTopOff.Visibility = Topmost ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateWindowClipboardDisabledStatus()
        {
            bClipboardOff.Visibility = m_clipboardHistory.DisableAllClipboardCopying ? Visibility.Collapsed : Visibility.Visible;
            bClipboardOn.Visibility = m_clipboardHistory.DisableAllClipboardCopying ? Visibility.Visible : Visibility.Collapsed;

            lstvHistory.IsEnabled = !m_clipboardHistory.DisableAllClipboardCopying;
        }

        private void UpdateEngineRunningStatus()
        {
            bStartEngine.Visibility = m_clipboardHistory.EngineRunning ? Visibility.Collapsed : Visibility.Visible;
            bPauseEngine.Visibility = m_clipboardHistory.EngineRunning ? Visibility.Visible : Visibility.Collapsed;
        }

        #region Base Overrides

        public new void Show()
        {
            base.Show();

            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }

        #endregion

        #region Private Window Update Thread

        private void SetupWindowUpdateThread()
        {
            Thread thread = new Thread(WindowUpdateThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void WindowUpdateThread()
        {
            while (m_updateWindowThreadRunning)
            {
                BackgroundAction.Run(() =>
                {
                    int selectedListIndex = m_clipboardHistoryItems.GetSelectedIndex();
                    int selectedItemsCount = lstvHistory.SelectedItems.Count;

                    bool oneItemSelectedThatIsNotTheTopItem = selectedListIndex > 0 && selectedItemsCount == 1;
                    bool oneItemSelectedThatIsNotTheBottomItem = selectedListIndex > -1 && selectedListIndex < lstvHistory.Items.Count - 1 && selectedItemsCount == 1;
                    bool oneItemSelected = selectedListIndex > -1 && selectedItemsCount == 1;
                    bool atLeastOneItemIsSelected = selectedListIndex > -1;
                    bool itemsInTheList = lstvHistory.Items.Count > 0;
                    bool allItemsSelected = selectedItemsCount == lstvHistory.Items.Count;
                    bool twoOrMoreItemsSelected = selectedItemsCount >= 2;

                    bNew.IsEnabled = itemsInTheList;
                    bSaveAs.IsEnabled = itemsInTheList;
                    bMoveUp.IsEnabled = oneItemSelectedThatIsNotTheTopItem;
                    bMoveDown.IsEnabled = oneItemSelectedThatIsNotTheBottomItem;
                    bRemove.IsEnabled = atLeastOneItemIsSelected;

                    mnuSave.IsEnabled = oneItemSelected;
                    mnuEditName.IsEnabled = oneItemSelected;
                    mnuSetAsCurrent.IsEnabled = m_clipboardHistory.EngineRunning && oneItemSelectedThatIsNotTheTopItem;
                    mnuSetAsCurrentText.IsEnabled = m_clipboardHistory.EngineRunning && oneItemSelected;
                    mnuSetAsOnTopRule.IsEnabled = atLeastOneItemIsSelected;
                    mnuMergeAsText.IsEnabled = twoOrMoreItemsSelected;
                    mnuMoveUp.IsEnabled = oneItemSelectedThatIsNotTheTopItem;
                    mnuMoveDown.IsEnabled = oneItemSelectedThatIsNotTheBottomItem;
                    mnuRemove.IsEnabled = atLeastOneItemIsSelected;
                    mnuRemoveAllCopiedFrom.IsEnabled = oneItemSelected;
                    mnuRemoveAllType.IsEnabled = oneItemSelected;
                    mnuSelectAll.IsEnabled = !allItemsSelected && itemsInTheList;
                    mnuSelectNone.IsEnabled = selectedItemsCount > 0;
                    mnuInvertSelection.IsEnabled = itemsInTheList;
                    mnuViewClip.IsEnabled = oneItemSelected;

                    bClipboardOff.IsEnabled = m_clipboardHistory.EngineRunning;
                    bClipboardOn.IsEnabled = m_clipboardHistory.EngineRunning;
                    bTotals.IsEnabled = itemsInTheList;
                    bSearch.IsEnabled = itemsInTheList;
                    bCleanUp.IsEnabled = itemsInTheList;

                    UpdateTotalsItems();
                    UpdateKeyStateStatuses();
                    UpdateWindowForLoadingMode();
                    UpdateWindowForOnTopRules();
                });

                Thread.Sleep(UpdateWindowInterval);
            }
        }

        private void UpdateTotalsItems()
        {
            if (rdStatusBar.Height.Value > 0 && lblTotalItems.IsVisible)
            {
                if (m_clipboardHistory.Items.Count > lstvHistory.Items.Count)
                {
                    lblTotalItems.Content = string.Format(ClipTree.Resources.UIMessages.ShowSpecificNumberOfItems, lstvHistory.Items.Count, m_clipboardHistory.Items.Count);
                }
                else
                {
                    lblTotalItems.Content = string.Format(ClipTree.Resources.UIMessages.TotalItems, lstvHistory.Items.Count);
                }
            }
        }

        private void UpdateKeyStateStatuses()
        {
            if (rdStatusBar.Height.Value > 0 && lblCapsLock.IsVisible && lblNumLock.IsVisible)
            {
                bool capsLock = (((ushort)Win32.GetKeyState(0x14)) & 0xffff) != 0;
                bool numLock = (((ushort)Win32.GetKeyState(0x90)) & 0xffff) != 0;

                lblCapsLock.IsEnabled = capsLock;
                lblNumLock.IsEnabled = numLock;
            }
        }

        private void UpdateWindowForLoadingMode()
        {
            if (m_windowMode == WindowMode.Load)
            {
                if (Opacity < 1.0)
                {
                    Opacity += OpacityIncriment;
                }
            }
            else
            {
                if (Opacity > 0.0)
                {
                    Opacity -= OpacityIncriment;
                }
                else
                {
                    Close();
                }
            }
        }

        private void UpdateWindowForOnTopRules()
        {
            if (bAlwaysOnTopOn.IsVisible && m_enableOnTopRules && m_onTopRules.Count > 0)
            {
                try
                {
                    Process process = Processes.GetFocused();

                    if (process != null)
                    {
                        using (process)
                        {
                            if (m_onTopRules.ContainsKey(process.ProcessName) &&
                                m_onTopRules[process.ProcessName] == ClipTree.Resources.Statuses.EnabledYes)
                            {
                                if (!Topmost)
                                {
                                    Topmost = true;
                                }
                            }
                            else
                            {
                                if (Topmost)
                                {
                                    Topmost = false;
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.ErrorFormat("Unable to set the On Top rules: {0}", exception);
                }
            }
        }

        #endregion

        #region Private "Title Context Menu" Events

        private void ContextMenu_DockToTheRight_OnClick(object sender, RoutedEventArgs e)
        {
            double maximumHeight = SystemParameters.WorkArea.Height;
            double maximumWidth = SystemParameters.WorkArea.Width;

            Left = maximumWidth - Width;
            Top = 0;
            Height = maximumHeight;
        }

        private void ContextMenu_DockToTheLeft_OnClick(object sender, RoutedEventArgs e)
        {
            double maximumHeight = SystemParameters.WorkArea.Height;

            Left = 0;
            Top = 0;
            Height = maximumHeight;
        }

        private void ContextMenu_Minimize_OnClick(object sender, RoutedEventArgs e)
        {
            MinimizeButton_Click(sender, null);
        }

        private void ContextMenu_Exit_OnClick(object sender, RoutedEventArgs e)
        {
            ExitButton_Click(sender, null);
        }

        #endregion

        #region Private "History Context Menu" Events

        private void ContextMenu_Save_OnClick(object sender, RoutedEventArgs e)
        {
            m_filenameDialog.SaveItem(ClipTree.Resources.Dialog.SaveListItem);
        }

        private void ContextMenu_EditName_OnClick(object sender, RoutedEventArgs e)
        {
            int selectedindex = m_clipboardHistoryItems.GetSelectedIndex();
            if (selectedindex > -1)
            {
                EditName editName = new EditName(m_settings, m_clipboardHistory, m_clipboardHistoryItems, selectedindex)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                editName.ShowDialog();
            }
        }

        private void ContextMenu_SetAsCurrent_OnClick(object sender, RoutedEventArgs e)
        {
            m_clipboardHistoryItems.SetAsCurrent();
        }

        private void ContextMenu_SetAsCurrentText_OnClick(object sender, RoutedEventArgs e)
        {
            m_clipboardHistoryItems.SetAsCurrentTextFormat();
        }

        private void ContextMenu_SetAsOnTopRule_OnClick(object sender, RoutedEventArgs e)
        {
            int[] selectedIndexes = m_clipboardHistoryItems.GetSelectedIndexes();

            foreach (int selectedIndex in selectedIndexes)
            {
                string copiedFrom = m_clipboardHistory.Items[selectedIndex].CopiedFrom;

                if (copiedFrom != ClipTree.Resources.Defaults.CopiedFromName)
                {
                    m_onTopRules[copiedFrom] = ClipTree.Resources.Statuses.EnabledYes;
                }
            }

            XmlDocument xmlDocument = m_settings.GetDocument();

            m_settings.RemoveSection(Settings.OnTopRules, xmlDocument);

            foreach (KeyValuePair<string, string> rule in m_onTopRules)
            {
                m_settings.Write(Settings.OnTopRules, rule.Key, rule.Value, xmlDocument);
            }

            m_settings.SaveDocument(xmlDocument);
        }

        private void ContextMenu_MergeAsText_OnClick(object sender, RoutedEventArgs e)
        {
            m_clipboardHistoryItems.MergeAsText();
        }

        private void ContextMenu_MoveUp_OnClick(object sender, RoutedEventArgs e)
        {
            MoveItem(true);
        }

        private void ContextMenu_MoveDown_OnClick(object sender, RoutedEventArgs e)
        {
            MoveItem(false);
        }

        private void ContextMenu_Lock_OnClick(object sender, RoutedEventArgs e)
        {
            int[] selectedIndexes = m_clipboardHistoryItems.GetSelectedIndexes();

            m_clipboardHistory.Lock(selectedIndexes, true);
        }

        private void ContextMenu_Unlock_OnClick(object sender, RoutedEventArgs e)
        {
            int[] selectedIndexes = m_clipboardHistoryItems.GetSelectedIndexes();

            m_clipboardHistory.Lock(selectedIndexes, false);
        }

        private void ContextMenu_Remove_OnClick(object sender, RoutedEventArgs e)
        {
            int[] selectedIndexes = m_clipboardHistoryItems.GetSelectedIndexes();

            if (selectedIndexes.Length >= lstvHistory.Items.Count && m_clipboardHistoryItems.TotalItemsToShowInList <= 0)
            {
                NewButton_Click(sender, null);
            }
            else
            {
                Array.Sort(selectedIndexes, (x, y) => y.CompareTo(x));

                m_clipboardHistory.Remove(selectedIndexes);
            }
        }

        private void ContextMenu_RemoveAllCopiedFrom_OnClick(object sender, RoutedEventArgs e)
        {
            int selectedindex = m_clipboardHistoryItems.GetSelectedIndex();
            if (selectedindex > -1)
            {
                string copiedFrom = m_clipboardHistory.Items[selectedindex].CopiedFrom;

                m_clipboardHistory.RemoveAll(copiedFrom);
            }
        }

        private void ContextMenu_RemoveAllType_OnClick(object sender, RoutedEventArgs e)
        {
            int selectedindex = m_clipboardHistoryItems.GetSelectedIndex();
            if (selectedindex > -1)
            {
                TextDataFormat type = m_clipboardHistory.Items[selectedindex].Type;

                m_clipboardHistory.RemoveAll(type);
            }
        }

        private void ContextMenu_SelectAll_OnClick(object sender, RoutedEventArgs e)
        {
            m_clipboardHistoryItems.SelectAll();
        }

        private void ContextMenu_SelectNone_OnClick(object sender, RoutedEventArgs e)
        {
            m_clipboardHistoryItems.SelectNone();
        }

        private void ContextMenu_InvertSelection_OnClick(object sender, RoutedEventArgs e)
        {
            m_clipboardHistoryItems.InvertSelection();
        }

        private void ContextMenu_ViewClip_OnClick(object sender, RoutedEventArgs e)
        {
            int selectedindex = m_clipboardHistoryItems.GetSelectedIndex();
            if (selectedindex > -1)
            {
                ViewCurrentClip(selectedindex);
            }
        }

        private void MoveItem(bool up)
        {
            int selectedindex = m_clipboardHistoryItems.GetSelectedIndex();
            if (selectedindex > -1)
            {
                int newSelectedIndex = up ? selectedindex - 1 : selectedindex + 1;

                m_clipboardHistory.MoveItem(selectedindex, newSelectedIndex);
            }
        }

        private void ViewCurrentClip(int listItemIndex)
        {
            ClipboardHistoryItem clipboardHistoryItem = m_clipboardHistory.Items[listItemIndex];
            bool run = true;
            bool runAsProcess = false;
            string runAsProcessCommand = null;

            if (m_viewHtmlClipsInDefaultBrowser && clipboardHistoryItem.Type == TextDataFormat.Html)
            {
                runAsProcess = !Html.View(clipboardHistoryItem);
                run = runAsProcess;
            }
            else
            {
                if (m_viewURLEntriesInTheDefaultBrowser && clipboardHistoryItem.IsTextBased && IsHttpBasedUrl(clipboardHistoryItem.Text))
                {
                    runAsProcess = true;
                    runAsProcessCommand = clipboardHistoryItem.Text;
                }
                else if (m_viewPathEntriesInTheDefaultProgram && clipboardHistoryItem.IsTextBased && IsPathBasedUrl(clipboardHistoryItem.Text))
                {
                    runAsProcess = true;
                    runAsProcessCommand = clipboardHistoryItem.Text;
                }
                else if (m_viewEmailEntriesInTheDefaultEmailComposer && clipboardHistoryItem.IsTextBased && IsEmailAddress(clipboardHistoryItem.Text))
                {
                    runAsProcess = true;
                    runAsProcessCommand = string.Format("mailto:{0}", clipboardHistoryItem.Text);
                }
                else if (m_viewEmailEntriesInTheDefaultEmailComposer && clipboardHistoryItem.IsTextBased && IsHTMLHexColor(clipboardHistoryItem.Text))
                {
                    Windows.Report.Color color = new Windows.Report.Color(m_settings, clipboardHistoryItem.Text)
                    {
                        Topmost = Topmost,
                        Owner = this
                    };

                    run = false;

                    color.ShowDialog();
                }
            }

            if (run && (!runAsProcess || !Processes.Start(runAsProcessCommand)))
            {
                Clip clip = new Clip(m_settings, m_clipboardHistory, m_clipboardHistoryItems, clipboardHistoryItem, m_showShortcutButtonsOnClipWindow)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                clip.ShowDialog();
            }
        }

        private static bool IsHttpBasedUrl(string data)
        {
            return Uri.TryCreate(data, UriKind.Absolute, out Uri url) && (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps || url.Scheme == Uri.UriSchemeFtp);
        }

        private static bool IsPathBasedUrl(string data)
        {
            return Uri.TryCreate(data, UriKind.Absolute, out Uri url) && url.Scheme == Uri.UriSchemeFile;
        }

        public bool IsEmailAddress(string data)
        {
            bool isValidEmailAddress = true;

            try
            {
                MailAddress mailAddress = new MailAddress(data);
            }
            catch (FormatException)
            {
                isValidEmailAddress = false;
            }

            return isValidEmailAddress;
        }

        private static bool IsHTMLHexColor(string data)
        {
            return !string.IsNullOrEmpty(data) && data.Length == 7 && data.StartsWith("#");
        }

        #endregion

        #region Private "Title Bar" Events

        private void Title_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !m_lockWindowActions.LockMoving)
            {
                DragMove();

                m_windowPosition.Changed = true;
            }
        }

        private void Title_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!m_lockWindowActions.LockMoving)
            {
                if (Left + Width < (SystemParameters.WorkArea.Width / 2))
                {
                    ContextMenu_DockToTheLeft_OnClick(sender, null);
                }
                else
                {
                    ContextMenu_DockToTheRight_OnClick(sender, null);
                }
            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsActionConfirmation(ClipTree.Resources.UIMessages.NewClipboardHistoryConfirmation, m_showConfirmationMessagesForFileActions))
            {
                m_clipboardHistory.Clear();
            }
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsActionConfirmation(ClipTree.Resources.UIMessages.OpenClipboardHistoryConfirmation, m_showConfirmationMessagesForFileActions))
            {
                m_filenameDialog.Open(ClipTree.Resources.Dialog.SupportFileFilter, ClipTree.Resources.Dialog.OpenClipboardHistory);
            }
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            m_filenameDialog.Save(ClipTree.Resources.Dialog.SupportFileFilter, ClipTree.Resources.Dialog.SaveHistoryListTItle);
        }

        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            m_lockWindowActions.LockMoving = !m_lockWindowActions.LockMoving;

            m_settings.Write(Settings.MainWindow.Tools, nameof(Settings.MainWindow.Locked), m_lockWindowActions.LockMoving.ToNumericString());

            UpdateWindowLockedState();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!m_lockWindowActions.LockMoving)
            {
                if (!ShowInTaskbar)
                {
                    Hide();
                }
                else
                {
                    WindowState = WindowState.Minimized;
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            m_windowMode = WindowMode.Close;
        }

        #endregion

        #region Private "Status Bar" Events

        private void StartPauseEngine_Click(object sender, RoutedEventArgs e)
        {
            if (m_clipboardHistory.EngineRunning)
            {
                m_clipboardHistory.StopTracking();
                m_clipboardHistory.EngineRunning = false;
            }
            else
            {
                m_clipboardHistory.EngineRunning = true;
                m_clipboardHistory.StartTracking();
            }

            m_settings.Write(Settings.MainWindow.Tools, nameof(Settings.MainWindow.StartEngine), m_clipboardHistory.EngineRunning.ToNumericString());

            UpdateEngineRunningStatus();
        }

        private void AlwaysOnTopButton_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;

            m_settings.Write(Settings.MainWindow.Tools, nameof(Settings.MainWindow.AlwaysOnTop), Topmost.ToNumericString());

            UpdateWindowAlwaysOnTopState();
        }

        private void DisableClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            m_clipboardHistory.DisableAllClipboardCopying = !m_clipboardHistory.DisableAllClipboardCopying;

            m_settings.Write(Settings.MainWindow.Tools, nameof(Settings.MainWindow.DisableClipboard), m_clipboardHistory.DisableAllClipboardCopying.ToNumericString());

            UpdateWindowClipboardDisabledStatus();
        }

        private void TotalsButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_clipboardHistory.Items.Count > 0)
            {
                Totals totals = new Totals(m_settings, m_clipboardHistory.GetTotals())
                {
                    Topmost = Topmost,
                    Owner = this
                };

                totals.ShowDialog();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Search search = new Search(m_settings, m_clipboardHistoryItems)
            {
                Topmost = Topmost,
                Owner = this
            };

            search.Show();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            Options options = new Options(m_settings, m_clipboardHistory, m_clipboardHistoryItems, this)
            {
                Topmost = Topmost,
                Owner = this
            };

            options.ShowDialog();
        }

        private void CleanUpButton_Click(object sender, RoutedEventArgs e)
        {
            CleanUp cleanUp = new CleanUp(m_settings, m_clipboardHistory)
            {
                Topmost = Topmost,
                Owner = this
            };

            cleanUp.ShowDialog();
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
            if (KeyStroke.IsControlKey(Key.N) && bNew.IsEnabled)
            {
                NewButton_Click(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.O) && bOpen.IsEnabled)
            {
                OpenButton_Click(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.S) && bSaveAs.IsEnabled)
            {
                SaveAsButton_Click(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.Up) && mnuMoveUp.IsEnabled)
            {
                ContextMenu_MoveUp_OnClick(sender, null);             
            }
            else if (KeyStroke.IsControlKey(Key.Down) && mnuMoveDown.IsEnabled)
            {
                ContextMenu_MoveDown_OnClick(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.L) && mnuLock.IsEnabled)
            {
                ContextMenu_Lock_OnClick(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.U) && mnuUnlock.IsEnabled)
            {
                ContextMenu_Unlock_OnClick(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.A) && mnuSelectAll.IsEnabled)
            {
                ContextMenu_SelectAll_OnClick(sender, null);
            }
            else if ((KeyStroke.IsControlKey(Key.Z) || Keyboard.IsKeyDown(Key.Escape)) && mnuSelectNone.IsEnabled)
            {
                ContextMenu_SelectNone_OnClick(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.F) && bSearch.IsEnabled)
            {
                SearchButton_Click(sender, null);
            }
            else if (KeyStroke.IsControlKey(Key.Delete) && bCleanUp.IsEnabled)
            {
                CleanUpButton_Click(sender, null);
            }
            else if (KeyStroke.IsAltKey(Key.Space))
            {
                e.Handled = true;
            }
            else if (Keyboard.IsKeyDown(Key.F1) && mnuSetAsCurrent.IsEnabled)
            {
                ContextMenu_SetAsCurrent_OnClick(sender, null);
            }
            else if (Keyboard.IsKeyDown(Key.F2) && mnuSetAsCurrentText.IsEnabled)
            {
                ContextMenu_SetAsCurrentText_OnClick(sender, null);
            }
            else if (Keyboard.IsKeyDown(Key.F3) && mnuSetAsOnTopRule.IsEnabled)
            {
                ContextMenu_SetAsOnTopRule_OnClick(sender, null);
            }
            else if (Keyboard.IsKeyDown(Key.Delete) && mnuRemove.IsEnabled)
            {
                ContextMenu_Remove_OnClick(sender, null);
            }
            else if (Keyboard.IsKeyDown(Key.Enter) && mnuViewClip.IsEnabled)
            {
                ContextMenu_ViewClip_OnClick(sender, null);
            }
        }

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_updateWindowThreadRunning = false;

            m_settings.Write(Settings.MainWindow.Tools, nameof(Settings.MainWindow.LastItemFiltered), m_clipboardHistory.LastItemFiltered);

            m_windowPosition.Set();

            Hide();

            m_clipboardHistory.StopTracking();

            if (m_clipboardHistoryItems.RememberHistoryBetweenSessions)
            {
                m_clipboardHistoryItems.Save();
            }

            m_listViewSettings.SetColumnWidths();

            Application.Current.Shutdown();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            m_windowPosition.Changed = true;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            m_windowPosition.Changed = true;

            if (WindowState == WindowState.Minimized)
            {
                m_windowPosition.Set();
            }
        }

        #endregion

        #region Private "ListView" Events

        private void History_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mnuViewClip.IsEnabled)
            {
                ContextMenu_ViewClip_OnClick(sender, null);
            }
        }

        private void History_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);

                if (files.Length == 1)
                {
                    if (IsActionConfirmation(ClipTree.Resources.UIMessages.OpenClipboardHistoryConfirmation, m_showConfirmationMessagesForFileActions))
                    {
                        m_clipboardHistoryItems.Load(new XMLSettings(files[0]));
                        m_clipboardHistory.SetTopItemAsCurrent();
                    }
                }
                else
                {
                    ShowInformationMessage(ClipTree.Resources.UIMessages.ToManyFilesDropped);
                }
            }
        }

        #endregion

        #region Private "Message" Helpers

        private bool IsActionConfirmation(string message, bool showMessage)
        {
            bool confirmed = true;

            if (showMessage)
            {
                MessageQuestion messageBox = new MessageQuestion(message)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                bool? result = messageBox.ShowDialog();

                confirmed = result != null && result.Value;
            }

            return confirmed;
        }

        private void ShowInformationMessage(string message)
        {
            MessageInformation messageInformation = new MessageInformation(message)
            {
                Topmost = Topmost,
                Owner = this
            };

            messageInformation.ShowDialog();
        }

        #endregion

        #region Private "Log" Helpers

        private ILog Log
        {
            get
            {
                return LogManager.GetLogger(GetType());
            }
        }

        #endregion
    }
}