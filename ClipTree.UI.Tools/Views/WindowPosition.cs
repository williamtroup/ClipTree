using System;
using System.Globalization;
using System.Windows;
using ClipTree.Engine.Settings.Interfaces;
using System.Xml;

namespace ClipTree.UI.Tools.Views;

public class WindowPosition(
    Window window,
    IXMLSettings settings,
    double defaultWidth,
    double defaultHeight,
    string sectionName = "Window")
{
    public void Get(bool ignoreWindowResizeMode = false)
    {
        XmlDocument xmlDocument = settings.GetDocument();

        double x = Convert.ToDouble(settings.Read(sectionName, "X", "0", xmlDocument));
        double y = Convert.ToDouble(settings.Read(sectionName, "Y", "0", xmlDocument));
        int minimized = Convert.ToInt32(settings.Read(sectionName, "Minimized", "0", xmlDocument));
        int maximized = Convert.ToInt32(settings.Read(sectionName, "Maximized", "0", xmlDocument));
        int hidden = Convert.ToInt32(settings.Read(sectionName, "Hidden", "0", xmlDocument));
        int saved = Convert.ToInt32(settings.Read(sectionName, "Saved", "0", xmlDocument));

        if (saved > 0)
        {
            double width = Convert.ToDouble(settings.Read(sectionName, "Width", defaultWidth.ToString(CultureInfo.InvariantCulture), xmlDocument));
            double height = Convert.ToDouble(settings.Read(sectionName, "Height", defaultHeight.ToString(CultureInfo.InvariantCulture), xmlDocument));

            if (minimized > 0 || maximized > 0)
            {
                window.Left = x;
                window.Top = y;
                window.Width = width;
                window.Height = height;
            }

            if (minimized > 0)
            {
                window.WindowState = WindowState.Minimized;
            }
            else if (maximized > 0)
            {
                window.WindowState = WindowState.Maximized;
            }
            else
            {
                if (IsResizeable || ignoreWindowResizeMode)
                {
                    if (width > 0 && height > 0)
                    {
                        window.WindowStartupLocation = WindowStartupLocation.Manual;
                        window.Left = x;
                        window.Top = y;
                        window.Width = width;
                        window.Height = height;
                    }
                }
                else
                {
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    window.Left = x;
                    window.Top = y;
                }
            }

            if (hidden > 0)
            {
                window.Hide();
            }
        }
    }

    public void Set()
    {
        if (Changed)
        {
            bool minimized = window.WindowState == WindowState.Minimized;
            bool maximized = window.WindowState == WindowState.Maximized;

            XmlDocument xmlDocument = settings.GetDocument();

            settings.Write(sectionName, "X", window.Left.ToString(CultureInfo.InvariantCulture), xmlDocument);
            settings.Write(sectionName, "Y", window.Top.ToString(CultureInfo.InvariantCulture), xmlDocument);
            settings.Write(sectionName, "Minimized", minimized ? "1" : "0", xmlDocument);
            settings.Write(sectionName, "Maximized", maximized ? "1" : "0", xmlDocument);
            settings.Write(sectionName, "Hidden", (!window.IsVisible) ? "1" : "0", xmlDocument);
            settings.Write(sectionName, "Saved", "1", xmlDocument);

            if (IsResizeable && !minimized && !maximized)
            {
                settings.Write(sectionName, "Width", window.Width.ToString(CultureInfo.InvariantCulture), xmlDocument);
                settings.Write(sectionName, "Height", window.Height.ToString(CultureInfo.InvariantCulture), xmlDocument);
            }

            settings.SaveDocument(xmlDocument);
        }
    }

    public bool Changed { private get; set; }

    private bool IsResizeable => window.ResizeMode is ResizeMode.CanResizeWithGrip or ResizeMode.CanResize;
}