using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools;
using ClipTree.UI.Tools.Views;
using ClipTree.UI.Tools.Actions;
using ClipTree.UI.Tools.Extensions;

namespace ClipTree.Windows.Report;

public partial class Color : Window
{
    private readonly IXMLSettings m_settings;
    private readonly WindowPosition m_windowPosition;

    public Color(IXMLSettings settings, string hexColor)
    {
        InitializeComponent();

        m_settings = settings;
        m_windowPosition = new WindowPosition(this, m_settings, Width, Height, GetName);

        SetupDisplay(hexColor);

        BackgroundAction.Run(() => m_windowPosition.Get());
    }

    public static string GetName => string.Format(Settings.WindowNameFormat, nameof(Color), Settings.Window);

    private void SetupDisplay(string hexColor)
    {
        System.Drawing.Color color = hexColor.GetColor();

        SolidColorBrush solidColorBrush = color.GetSolidColorBrush();

        InnerWindowBorder.Background = solidColorBrush;
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