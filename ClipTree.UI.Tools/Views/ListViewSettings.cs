using System.Globalization;
using System.Windows.Controls;
using ClipTree.Engine.Settings.Interfaces;
using System.Xml;

namespace ClipTree.UI.Tools.Views;

public class ListViewSettings
{
    private readonly IXMLSettings m_settings;
    private readonly ListView m_listView;
    private readonly GridView m_gridView;
    private readonly string m_sectionName;

    public ListViewSettings(IXMLSettings settings, ListView listView, string sectionName = "Columns")
    {
        m_settings = settings;
        m_listView = listView;
        m_gridView = (GridView)m_listView.View;
        m_sectionName = sectionName;

        GetColumnWidths();
    }

    public void SetColumnWidths()
    {
        int columnIndex = 0;

        XmlDocument xmlDocument = m_settings.GetDocument();

        foreach (GridViewColumn column in m_gridView.Columns)
        {
            m_settings.Write(m_sectionName, columnIndex.ToString(), column.Width.ToString(CultureInfo.InvariantCulture), xmlDocument);
            columnIndex++;
        }

        m_settings.SaveDocument(xmlDocument);
    }

    private void GetColumnWidths()
    {
        int columnIndex = 0;

        XmlDocument xmlDocument = m_settings.GetDocument();

        foreach (GridViewColumn column in m_gridView.Columns)
        {
            string width = m_settings.Read(m_sectionName, columnIndex.ToString(), "0", xmlDocument);

            if (double.TryParse(width, out double newWidth) && newWidth > 0)
            {
                column.Width = newWidth;
            }

            columnIndex++;
        }
    }
}