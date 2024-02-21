using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClipTree.Engine.Extensions;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools;
using ClipTree.UI.Tools.Interfaces;
using ClipTree.UI.Tools.Views;
using ClipTree.UI.Tools.Enums;
using ClipTree.UI.Tools.Actions;
using ClipTree.UI.Tools.Extensions;
using System.Xml;

namespace ClipTree.Windows.List;

public partial class Search : Window
{
    private readonly IXMLSettings m_settings;
    private readonly IClipboardHistoryItems m_clipboardHistoryItems;
    private readonly WindowPosition m_windowPosition;

    private bool m_wasSearchConducted;

    public Search(IXMLSettings settings, IClipboardHistoryItems clipboardHistoryItems)
    {
        InitializeComponent();

        m_settings = settings;
        m_clipboardHistoryItems = clipboardHistoryItems;
        m_windowPosition = new WindowPosition(this, m_settings, Width, Height, GetName);

        SetupDisplay();

        BackgroundAction.Run(() => m_windowPosition.Get());
    }

    public static string GetName
    {
        get
        {
            return string.Format(Settings.WindowNameFormat, nameof(Search), Settings.Window);
        }
    }

    private void SetupDisplay()
    {
        XmlDocument xmlDocument = m_settings.GetDocument();

        int matchCase = Convert.ToInt32(m_settings.Read(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.MatchCase), Settings.SearchWindow.MatchCase, xmlDocument));
        int makeTransparentWhenFocusIsLost = Convert.ToInt32(m_settings.Read(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.MakeTransparentWhenFocusIsLost), Settings.SearchWindow.MakeTransparentWhenFocusIsLost, xmlDocument));
        string searchType = m_settings.Read(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.SearchType), Settings.SearchWindow.SearchType, xmlDocument);
        string lastSearch = m_settings.Read(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.LastSearch), Settings.SearchWindow.LastSearch, xmlDocument);

        SearchType actualSearchType = (!string.IsNullOrEmpty(searchType)) ? (SearchType) Enum.Parse(typeof(SearchType), searchType) : SearchType.Contains;

        chkMatchCase.IsChecked = matchCase > 0;
        chkMakeTransparentWhenFocusIsLost.IsChecked = makeTransparentWhenFocusIsLost > 0;
        txtFind.Text = lastSearch;

        switch (actualSearchType)
        {
            case SearchType.StartsWith:
                opStartsWith.IsChecked = true;
                break;

            case SearchType.EndsWith:
                opEndsWith.IsChecked = true;
                break;

            case SearchType.WholeWordOnly:
                opWholeWordOnly.IsChecked = true;
                break;

            default:
                opContains.IsChecked = true;
                break;
        }

        lblInformation.Visibility = Visibility.Hidden;
        txtFind.Focus();

        if (!string.IsNullOrEmpty(lastSearch))
        {
            txtFind.SelectAll();
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

        if (chkMakeTransparentWhenFocusIsLost.IsReallyChecked())
        {
            Opacity = 1.0;
        }
    }

    private void Window_OnDeactivated(object sender, EventArgs e)
    {
        WindowBorder.Background = Brushes.DarkGray;

        if (chkMakeTransparentWhenFocusIsLost.IsReallyChecked())
        {
            Opacity = 0.5;
        }
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

        if (m_wasSearchConducted)
        {
            XmlDocument xmlDocument = m_settings.GetDocument();

            m_settings.Write(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.MatchCase), chkMatchCase.IsReallyChecked().ToNumericString(), xmlDocument);
            m_settings.Write(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.MakeTransparentWhenFocusIsLost), chkMakeTransparentWhenFocusIsLost.IsReallyChecked().ToNumericString(), xmlDocument);
            m_settings.Write(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.SearchType), GetSearchType.ToString(), xmlDocument);
            m_settings.Write(Settings.SearchWindow.SearchOptions, nameof(Settings.SearchWindow.LastSearch), txtFind.Text, xmlDocument);

            m_settings.SaveDocument(xmlDocument);
        }
    }

    private void LastButton_OnClick(object sender, RoutedEventArgs e)
    {
        m_clipboardHistoryItems.SelectNone();
        m_clipboardHistoryItems.FindNextText(txtFind.Text, goToLast: true, matchCase: chkMatchCase.IsReallyChecked(), searchType: GetSearchType);
        m_wasSearchConducted = true;
    }

    private void NextButton_OnClick(object sender, RoutedEventArgs e)
    {
        int selectedIndex = m_clipboardHistoryItems.GetSelectedIndex();

        m_clipboardHistoryItems.SelectNone();
        m_clipboardHistoryItems.FindNextText(txtFind.Text, selectedIndex, matchCase: chkMatchCase.IsReallyChecked(), searchType: GetSearchType);
        m_wasSearchConducted = true;
    }

    private void PreviousButton_OnClick(object sender, RoutedEventArgs e)
    {
        int selectedIndex = m_clipboardHistoryItems.GetSelectedIndex();

        m_clipboardHistoryItems.SelectNone();
        m_clipboardHistoryItems.FindPreviousText(txtFind.Text, selectedIndex, matchCase: chkMatchCase.IsReallyChecked(), searchType: GetSearchType);
        m_wasSearchConducted = true;
    }

    private void FirstButton_OnClick(object sender, RoutedEventArgs e)
    {
        m_clipboardHistoryItems.SelectNone();
        m_clipboardHistoryItems.FindPreviousText(txtFind.Text, goToFirst: true, matchCase: chkMatchCase.IsReallyChecked(), searchType: GetSearchType);
        m_wasSearchConducted = true;
    }

    private void SelectAllButton_OnClick(object sender, RoutedEventArgs e)
    {
        int totalItemsSelected = m_clipboardHistoryItems.MarkItemsAsSelected(txtFind.Text, chkMatchCase.IsReallyChecked(), GetSearchType);

        lblInformation.Content = string.Format(ClipTree.Resources.UIMessages.TotalItemsSelected, totalItemsSelected);
        lblInformation.Visibility = Visibility.Visible;

        m_wasSearchConducted = true;
    }

    private void CountButton_OnClick(object sender, RoutedEventArgs e)
    {
        int totalItemsFound = m_clipboardHistoryItems.GetTotalNumberOfItems(txtFind.Text, chkMatchCase.IsReallyChecked(), GetSearchType);

        lblInformation.Content = string.Format(ClipTree.Resources.UIMessages.TotalResults, totalItemsFound);
        lblInformation.Visibility = Visibility.Visible;

        m_wasSearchConducted = true;
    }

    private SearchType GetSearchType
    {
        get
        {
            SearchType searchType = SearchType.Contains;

            if (opStartsWith.IsReallyChecked())
            {
                searchType = SearchType.StartsWith;
            }
            else if (opEndsWith.IsReallyChecked())
            {
                searchType = SearchType.EndsWith;
            }
            else if (opWholeWordOnly.IsReallyChecked())
            {
                searchType = SearchType.WholeWordOnly;
            }

            return searchType;
        }
    }
}