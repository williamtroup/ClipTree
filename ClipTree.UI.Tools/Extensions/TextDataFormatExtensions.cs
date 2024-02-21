using System.Windows;

namespace ClipTree.UI.Tools.Extensions;

public static class TextDataFormatExtensions
{
    public static string GetDisplayName(this TextDataFormat value)
    {
        string displayName = value.ToString();

        switch (value)
        {
            case TextDataFormat.Rtf:
                displayName = "RichText";
                break;

            case TextDataFormat.Html:
                displayName = TextDataFormat.Html.ToString().ToUpper();
                break;

            case TextDataFormat.Xaml:
                displayName = TextDataFormat.Xaml.ToString().ToUpper();
                break;
        }

        return displayName;
    }
}