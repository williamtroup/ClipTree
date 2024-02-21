using System;
using System.Drawing;
using System.Windows.Media;

namespace ClipTree.UI.Tools.Extensions;

public static class ColorExtensions
{
    public static System.Drawing.Color GetColor(this string hex)
    {
        return ColorTranslator.FromHtml(hex);
    }

    public static string GetHex(this string color)
    {
        byte[] bytes = GetColorBytes(color);

        System.Drawing.Color newColor = System.Drawing.Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);

        return ColorTranslator.ToHtml(newColor);
    }

    public static SolidColorBrush GetSolidColorBrush(this string color)
    {
        byte[] bytes = GetColorBytes(color);

        SolidColorBrush solidColorBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]));

        return solidColorBrush;
    }

    public static SolidColorBrush GetSolidColorBrush(this System.Drawing.Color color)
    {
        return new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
    }

    private static byte[] GetColorBytes(string color)
    {
        int colorValue = Convert.ToInt32(color);

        return BitConverter.GetBytes(colorValue);
    }
}