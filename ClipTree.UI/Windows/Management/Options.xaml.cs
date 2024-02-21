using ClipTree.Engine.Extensions;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools;
using ClipTree.UI.Tools.Interfaces;
using ClipTree.UI.Tools.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClipTree.Engine.Clipboard.Interfaces;
using ClipTree.UI.Tools.Actions;
using Microsoft.Win32;
using System.Diagnostics;
using ClipTree.Windows.List;
using ClipTree.Windows.Report;
using ClipTree.UI.Tools.Extensions;
using System.Xml;

namespace ClipTree.Windows.Management;

public partial class Options : Window
{
    private const string AssemblyProductName = "ClipTree";

    private readonly IXMLSettings m_settings;
    private readonly IClipboardHistoryItems m_clipboardHistoryItems;
    private readonly IClipboardHistory m_clipboardHistory;
    private readonly Main m_main;
    private readonly WindowPosition m_windowPosition;
    private readonly RegistryKey m_startUpRegistryKey;

    private bool m_savePosition = true;

    public Options(IXMLSettings settings, IClipboardHistory clipboardHistory, IClipboardHistoryItems clipboardHistoryItems, Main main)
    {
        InitializeComponent();

        m_settings = settings;
        m_clipboardHistoryItems = clipboardHistoryItems;
        m_clipboardHistory = clipboardHistory;
        m_main = main;
        m_windowPosition = new WindowPosition(this, m_settings, Width, Height, GetName);
        m_startUpRegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        SetupDisplay();

        BackgroundAction.Run(() => m_windowPosition.Get());
    }

    public static string GetName
    {
        get
        {
            return string.Format(Settings.WindowNameFormat, nameof(Options), Settings.Window);
        }
    }

    private void SetupDisplay()
    {
        XmlDocument xmlDocument = m_settings.GetDocument();

        string productStartUp = m_startUpRegistryKey.GetValue(AssemblyProductName, "").ToString();

        int showInTaskBar = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowInTaskBar), Settings.MainWindow.ShowInTaskBar, xmlDocument));
        int showStatusBar = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowStatusBar), Settings.MainWindow.ShowStatusBar, xmlDocument));
        int showCapsNumLockStatues = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowCapsNumLockStatues), Settings.MainWindow.ShowCapsNumLockStatues, xmlDocument));
        int showTotalItems = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowTotalItems), Settings.MainWindow.ShowTotalItems, xmlDocument));
        int enableRowColors = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.EnableRowColors), Settings.MainWindow.EnableRowColors, xmlDocument));
        int enableOnTopRules = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.EnableOnTopRules), Settings.MainWindow.EnableOnTopRules, xmlDocument));
        int enableCopiedFromFiltering = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.EnableCopiedFromFiltering), Settings.MainWindow.EnableCopiedFromFiltering, xmlDocument));
        int viewHtmlClipsInDefaultBrowser = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewHtmlClipsInDefaultBrowser), Settings.MainWindow.ViewHtmlClipsInDefaultBrowser, xmlDocument));
        int rememberHistoryBetweenSessions = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.RememberHistoryBetweenSessions), Settings.MainWindow.RememberHistoryBetweenSessions, xmlDocument));
        int autoSaveHistory = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.AutoSaveHistory), Settings.MainWindow.AutoSaveHistory, xmlDocument));
        int totalsItemsToShowInList = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.TotalsItemsToShowInList), Settings.MainWindow.TotalsItemsToShowInList, xmlDocument));
        int updateClipboardInterval = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.UpdateClipboardInterval), Settings.MainWindow.UpdateClipboardInterval, xmlDocument));
        int autoTrimTextAndUnicodeEntries = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.AutoTrimTextAndUnicodeEntries), Settings.MainWindow.AutoTrimTextAndUnicodeEntries, xmlDocument));
        int viewURLEntriesInTheDefaultBrowser = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewURLEntriesInTheDefaultBrowser), Settings.MainWindow.ViewURLEntriesInTheDefaultBrowser, xmlDocument));
        int viewPathEntriesInTheDefaultProgram = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewPathEntriesInTheDefaultProgram), Settings.MainWindow.ViewPathEntriesInTheDefaultProgram, xmlDocument));
        int showLoadingSplashScreen = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowLoadingSplashScreen), Settings.MainWindow.ShowLoadingSplashScreen, xmlDocument));
        int viewEmailAddressesInTheDefaultEmailComposer = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewEmailAddressesInTheDefaultEmailComposer), Settings.MainWindow.ViewEmailAddressesInTheDefaultEmailComposer, xmlDocument));
        int showConfirmationMessagesForFileActions = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowConfirmationMessagesForFileActions), Settings.MainWindow.ShowConfirmationMessagesForFileActions, xmlDocument));
        int showShortcutButtonsOnClipWindow = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowShortcutButtonsOnClipWindow), Settings.MainWindow.ShowShortcutButtonsOnClipWindow, xmlDocument));
        int viewHTMLHexColorsAsAnActualColor = Convert.ToInt32(m_settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewHTMLHexColorsAsAnActualColor), Settings.MainWindow.ViewHTMLHexColorsAsAnActualColor, xmlDocument));

        chkShowInTaskBar.IsChecked = showInTaskBar > 0;
        chkStartOnWindowsStartup.IsChecked = !string.IsNullOrEmpty(productStartUp);
        chkShowStatusBar.IsChecked = showStatusBar > 0;
        chkShowCapsNumLockStatuses.IsChecked = showCapsNumLockStatues > 0;
        chkShowTotalsItems.IsChecked = showTotalItems > 0;
        chkEnableRowColors.IsChecked = enableRowColors > 0;
        chkEnableOnTopRules.IsChecked = enableOnTopRules > 0;
        chkEnableCopiedFromFiltering.IsChecked = enableCopiedFromFiltering > 0;
        chkViewHtmlClipsInDefaultBrowser.IsChecked = viewHtmlClipsInDefaultBrowser > 0;
        chkRememberHistoryBetweenSessions.IsChecked = rememberHistoryBetweenSessions > 0;
        chkAutoSaveHistory.IsChecked = autoSaveHistory > 0;
        txtTotalsItemsToShowInList.Text = totalsItemsToShowInList.ToString();
        txtUpdateClipboardInterval.Text = updateClipboardInterval.ToString();
        chkAutoTrimTextBasedEntries.IsChecked = autoTrimTextAndUnicodeEntries > 0;
        chkViewURLBasedEntriesInBrowser.IsChecked = viewURLEntriesInTheDefaultBrowser > 0;
        chkViewPathBasedEntriesInDefaultProgram.IsChecked = viewPathEntriesInTheDefaultProgram > 0;
        chkShowLoadingSplashScreen.IsChecked = showLoadingSplashScreen > 0;
        chkViewEmailAddressesInTheDefaultEmailComposer.IsChecked = viewEmailAddressesInTheDefaultEmailComposer > 0;
        chkShowConfirmationMessagesForFileActions.IsChecked = showConfirmationMessagesForFileActions > 0;
        chkShowShortcutButtonsOnClipWindow.IsChecked = showShortcutButtonsOnClipWindow > 0;
        chkViewHTMLHexColorsAsAnActualColor.IsChecked = viewHTMLHexColorsAsAnActualColor > 0;

        NumericInput.Make(txtTotalsItemsToShowInList, Settings.MainWindow.TotalsItemsToShowInList);
        NumericInput.Make(txtUpdateClipboardInterval, Settings.MainWindow.UpdateClipboardInterval);
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
        if (m_savePosition)
        {
            m_windowPosition.Set();
        }
    }

    private void RowColorsButton_Click(object sender, RoutedEventArgs e)
    {
        RowColors rowColors = new RowColors(m_settings, m_clipboardHistory, m_clipboardHistoryItems)
        {
            Topmost = Topmost,
            Owner = this
        };

        rowColors.ShowDialog();
    }

    private void OnTopRulesButton_Click(object sender, RoutedEventArgs e)
    {
        OnTopRules onTopRules = new OnTopRules(m_settings, m_main)
        {
            Topmost = Topmost,
            Owner = this
        };

        onTopRules.ShowDialog();
    }

    private void EditFilteringRulesButton_Click(object sender, RoutedEventArgs e)
    {
        CopiedFromFilteringRules copiedFromFilteringRules = new CopiedFromFilteringRules(m_settings, m_main)
        {
            Topmost = Topmost,
            Owner = this
        };

        copiedFromFilteringRules.ShowDialog();
    }

    private void ClearClipboardButton_Click(object sender, RoutedEventArgs e)
    {
        Button senderButton = (Button) sender;

        m_clipboardHistory.ClearClipboard();

        senderButton.IsEnabled = false;
    }

    private void DefaultsButton_Click(object sender, RoutedEventArgs e)
    {
        m_startUpRegistryKey.DeleteValue(AssemblyProductName, false);

        XmlDocument xmlDocument = m_settings.GetDocument();

        m_settings.RemoveSection(Settings.MainWindow.Display, xmlDocument);

        m_settings.RemoveSection(CleanUp.GetName, xmlDocument);
        m_settings.RemoveSection(EditName.GetName, xmlDocument);
        m_settings.RemoveSection(Search.GetName, xmlDocument);

        m_settings.RemoveSection(AddCopiedFromFilteringRule.GetName, xmlDocument);
        m_settings.RemoveSection(AddOnTopRule.GetName, xmlDocument);
        m_settings.RemoveSection(AddRowColor.GetName, xmlDocument);
        m_settings.RemoveSection(CopiedFromFilteringRules.GetName, xmlDocument);
        m_settings.RemoveSection(OnTopRules.GetName, xmlDocument);
        m_settings.RemoveSection(GetName, xmlDocument);
        m_settings.RemoveSection(RowColors.GetName, xmlDocument);

        m_settings.RemoveSection(Report.Clip.GetName, xmlDocument);
        m_settings.RemoveSection(Report.Color.GetName, xmlDocument);
        m_settings.RemoveSection(Totals.GetName, xmlDocument);

        m_settings.SaveDocument(xmlDocument);

        m_main.InitializeDisplaySettings();
        m_savePosition = false;

        Close();
    }

    private void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        string assemblyLocation = Process.GetCurrentProcess().MainModule.FileName;

        if (chkStartOnWindowsStartup.IsReallyChecked())
        {
            m_startUpRegistryKey.SetValue(AssemblyProductName, string.Format("\"{0}\"", assemblyLocation));
        }
        else
        {
            m_startUpRegistryKey.DeleteValue(AssemblyProductName, false);
        }

        XmlDocument xmlDocument = m_settings.GetDocument();

        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowInTaskBar), IsChecked(chkShowInTaskBar), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowStatusBar), IsChecked(chkShowStatusBar), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowCapsNumLockStatues), IsChecked(chkShowCapsNumLockStatuses), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowTotalItems), IsChecked(chkShowTotalsItems), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.EnableRowColors), IsChecked(chkEnableRowColors), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.EnableOnTopRules), IsChecked(chkEnableOnTopRules), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.EnableCopiedFromFiltering), IsChecked(chkEnableCopiedFromFiltering), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewHtmlClipsInDefaultBrowser), IsChecked(chkViewHtmlClipsInDefaultBrowser), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.RememberHistoryBetweenSessions), IsChecked(chkRememberHistoryBetweenSessions), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.AutoSaveHistory), IsChecked(chkAutoSaveHistory), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.TotalsItemsToShowInList), txtTotalsItemsToShowInList.Text, xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.UpdateClipboardInterval), txtUpdateClipboardInterval.Text, xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.AutoTrimTextAndUnicodeEntries), IsChecked(chkAutoTrimTextBasedEntries), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewURLEntriesInTheDefaultBrowser), IsChecked(chkViewURLBasedEntriesInBrowser), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewPathEntriesInTheDefaultProgram), IsChecked(chkViewPathBasedEntriesInDefaultProgram), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowLoadingSplashScreen), IsChecked(chkShowLoadingSplashScreen), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewEmailAddressesInTheDefaultEmailComposer), IsChecked(chkViewEmailAddressesInTheDefaultEmailComposer), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowConfirmationMessagesForFileActions), IsChecked(chkShowConfirmationMessagesForFileActions), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowShortcutButtonsOnClipWindow), IsChecked(chkShowShortcutButtonsOnClipWindow), xmlDocument);
        m_settings.Write(Settings.MainWindow.Display, nameof(Settings.MainWindow.ViewHTMLHexColorsAsAnActualColor), IsChecked(chkViewHTMLHexColorsAsAnActualColor), xmlDocument);
        m_settings.SaveDocument(xmlDocument);

        m_main.InitializeDisplaySettings();

        Close();
    }

    private string IsChecked(CheckBox checkbox)
    {
        return checkbox.IsReallyChecked().ToNumericString();
    }
}