using System;
using System.Globalization;
using System.Windows;
using ClipTree.Engine.Settings.Interfaces;
using System.Xml;

namespace ClipTree.UI.Tools.Views;

public class WindowPosition
{
    private readonly Window m_window;
    private readonly IXMLSettings m_settings;
    private readonly double m_defaultWidth;
    private readonly double m_defaultHeight;
    private readonly string m_sectionName;

    public WindowPosition(
        Window window,
        IXMLSettings settings,
        double defaultWidth,
        double defaultHeight,
        string sectionName = "Window")
    {
        m_window = window;
        m_settings = settings;
        m_defaultWidth = defaultWidth;
        m_defaultHeight = defaultHeight;
        m_sectionName = sectionName;
    }

    public void Get(bool ignoreWindowResizeMode = false)
    {
        XmlDocument xmlDocument = m_settings.GetDocument();

        double x = Convert.ToDouble(m_settings.Read(m_sectionName, "X", "0", xmlDocument));
        double y = Convert.ToDouble(m_settings.Read(m_sectionName, "Y", "0", xmlDocument));
        int minimized = Convert.ToInt32(m_settings.Read(m_sectionName, "Minimized", "0", xmlDocument));
        int maximized = Convert.ToInt32(m_settings.Read(m_sectionName, "Maximized", "0", xmlDocument));
        int hidden = Convert.ToInt32(m_settings.Read(m_sectionName, "Hidden", "0", xmlDocument));
        int saved = Convert.ToInt32(m_settings.Read(m_sectionName, "Saved", "0", xmlDocument));

        if (saved > 0)
        {
            double width = Convert.ToDouble(m_settings.Read(m_sectionName, "Width", m_defaultWidth.ToString(CultureInfo.InvariantCulture), xmlDocument));
            double height = Convert.ToDouble(m_settings.Read(m_sectionName, "Height", m_defaultHeight.ToString(CultureInfo.InvariantCulture), xmlDocument));

            if (minimized > 0 || maximized > 0)
            {
                m_window.Left = x;
                m_window.Top = y;
                m_window.Width = width;
                m_window.Height = height;
            }

            if (minimized > 0)
            {
                m_window.WindowState = WindowState.Minimized;
            }
            else if (maximized > 0)
            {
                m_window.WindowState = WindowState.Maximized;
            }
            else
            {
                if (IsResizeable || ignoreWindowResizeMode)
                {
                    if (width > 0 && height > 0)
                    {
                        m_window.WindowStartupLocation = WindowStartupLocation.Manual;
                        m_window.Left = x;
                        m_window.Top = y;
                        m_window.Width = width;
                        m_window.Height = height;
                    }
                }
                else
                {
                    m_window.WindowStartupLocation = WindowStartupLocation.Manual;
                    m_window.Left = x;
                    m_window.Top = y;
                }
            }

            if (hidden > 0)
            {
                m_window.Hide();
            }
        }
    }

    public void Set()
    {
        if (Changed)
        {
            bool minimized = m_window.WindowState == WindowState.Minimized;
            bool maximized = m_window.WindowState == WindowState.Maximized;

            XmlDocument xmlDocument = m_settings.GetDocument();

            m_settings.Write(m_sectionName, "X", m_window.Left.ToString(CultureInfo.InvariantCulture), xmlDocument);
            m_settings.Write(m_sectionName, "Y", m_window.Top.ToString(CultureInfo.InvariantCulture), xmlDocument);
            m_settings.Write(m_sectionName, "Minimized", minimized ? "1" : "0", xmlDocument);
            m_settings.Write(m_sectionName, "Maximized", maximized ? "1" : "0", xmlDocument);
            m_settings.Write(m_sectionName, "Hidden", (!m_window.IsVisible) ? "1" : "0", xmlDocument);
            m_settings.Write(m_sectionName, "Saved", "1", xmlDocument);

            if (IsResizeable && !minimized && !maximized)
            {
                m_settings.Write(m_sectionName, "Width", m_window.Width.ToString(CultureInfo.InvariantCulture), xmlDocument);
                m_settings.Write(m_sectionName, "Height", m_window.Height.ToString(CultureInfo.InvariantCulture), xmlDocument);
            }

            m_settings.SaveDocument(xmlDocument);
        }
    }

    public bool Changed { private get; set; }

    private bool IsResizeable
    {
        get
        {
            return m_window.ResizeMode == ResizeMode.CanResizeWithGrip || m_window.ResizeMode == ResizeMode.CanResize;
        }
    }
}