using System.Windows.Input;

namespace ClipTree.UI.Tools.Actions;

public static class KeyStroke
{
    public static bool IsControlKey(Key key)
    {
        return Keyboard.Modifiers == ModifierKeys.Control && Keyboard.IsKeyDown(key);
    }

    public static bool IsAltKey(Key key)
    {
        return Keyboard.Modifiers == ModifierKeys.Alt && Keyboard.IsKeyDown(key);
    }
}