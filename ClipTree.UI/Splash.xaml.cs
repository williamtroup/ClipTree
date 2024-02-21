using ClipTree.Engine.Settings;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.Engine.Windows;
using ClipTree.UI.Tools;
using ClipTree.UI.Tools.Actions;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ClipTree;

public partial class Splash : Window
{
    public Splash()
    {
        InitializeComponent();

        Opacity = 0.0;

        if (Processes.IsRunning())
        {
            Application.Current.Shutdown();
        }
        else
        {
            ShowSplash();
        }
    }

    private void ShowSplash()
    {
        IXMLSettings settings = new XMLSettings();

        int showLoadingSplashScreen = Convert.ToInt32(settings.Read(Settings.MainWindow.Display, nameof(Settings.MainWindow.ShowLoadingSplashScreen), Settings.MainWindow.ShowLoadingSplashScreen));
        if (showLoadingSplashScreen > 0)
        {
            Opacity = 1.0;

            SynchronizationContext synchronizationContext = SynchronizationContext.Current;
            Timer timer = null;

            timer = new Timer((a) =>
            {
                synchronizationContext.Post(b => ShowWindow(settings, timer), null);

            }, null, TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(-1));
        }
        else
        {
            ShowWindow(settings);
        }
    }

    private void ShowWindow(IXMLSettings settings, IDisposable timer = null)
    {
        Main main = new Main(settings);
        main.Show();

        if (timer != null)
        {
            timer.Dispose();
        }

        Close();
    }

    private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (KeyStroke.IsAltKey(Key.Space))
        {
            e.Handled = true;
        }
    }
}