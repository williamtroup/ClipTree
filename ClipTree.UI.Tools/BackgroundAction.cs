using System;
using System.Threading;
using System.Windows;

namespace ClipTree.UI.Tools
{
    public static class BackgroundAction
    {
        public static void Run(Action action)
        {
            Application instance = Application.Current;

            if (instance != null)
            {
                instance.Dispatcher.Invoke(new ThreadStart(delegate {
                    action();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}
