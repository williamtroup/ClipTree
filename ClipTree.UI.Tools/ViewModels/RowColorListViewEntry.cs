using ClipTree.UI.Tools.Extensions;
using System.Windows.Media;

namespace ClipTree.UI.Tools.ViewModels;

public class RowColorListViewEntry
{
    public RowColorListViewEntry()
    {
        ShowColors(false);
    }

    public string CopiedFrom { get; set; }
    public string Color { get; set; }
    public string TextColor { get; set; }
    public SolidColorBrush BackgroundColor { get; set; }
    public SolidColorBrush ForeColor { get; set; }

    public string ColorHex
    {
        get { return Color.GetHex(); }
    }

    public string TextColorHex
    {
        get { return TextColor.GetHex(); }
    }

    public void ShowColors(bool showColors)
    {
        BackgroundColor = showColors ? Color.GetSolidColorBrush() : Brushes.White;
        ForeColor = showColors ? TextColor.GetSolidColorBrush() : Brushes.Black;
    }
}