using System.Windows.Controls;

namespace ClipTree.UI.Tools.Extensions
{
    public static class RadioButtonExtensions
    {
        public static bool IsReallyChecked(this RadioButton radioButton)
        {
            return radioButton.IsChecked != null && radioButton.IsChecked.Value;
        }
    }
}
