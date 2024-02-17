using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools;
using ClipTree.UI.Tools.Views;
using ClipTree.UI.Tools.Actions;
using System.Net;
using System.IO;
using System.Threading;
using ClipTree.Engine.Settings;
using ClipTree.Engine.Windows;

namespace ClipTree.Windows.Report
{
    public partial class Updates : Window
    {
        #region Private Constants

        private const string UpdateFilename = "cliptree.xml";

        #endregion

        #region Private Variables

        private string m_downloadLink;

        #endregion

        #region Private Read-Only Variables

        private readonly WindowPosition m_windowPosition;

        #endregion

        public Updates(IXMLSettings settings)
        {
            InitializeComponent();

            m_windowPosition = new WindowPosition(this, settings, Width, Height, GetName);

            SetupDisplay();

            BackgroundAction.Run(() => m_windowPosition.Get());
        }

        public static string GetName
        {
            get
            {
                return string.Format(Settings.WindowNameFormat, nameof(Updates), Settings.Window);
            }
        }

        private void SetupDisplay()
        {
            SetVisibleState(Visibility.Hidden);

            StartCheckingForUpdates();
        }

        private void SetVisibleState(Visibility state)
        {
            bDownload.Visibility = state;
            lblVersionHeader.Visibility = state;
            lblReleasedHeader.Visibility = state;
            lblVersion.Visibility = state;
            lblReleased.Visibility = state;
        }

        private void StartCheckingForUpdates()
        {
            Thread thread = new Thread(CheckForUpdates);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void CheckForUpdates()
        {
            BackgroundAction.Run(() =>
            {
                if (File.Exists(UpdateFilename))
                {
                    File.Delete(UpdateFilename);
                }

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(string.Format("http://www.bunoon.com/updates/{0}", UpdateFilename), UpdateFilename);
                }

                CheckUpdateFile();
            });
        }

        private void CheckUpdateFile()
        {
            if (File.Exists(UpdateFilename))
            {
                bool updateFound = true;

                try
                {
                    IXMLSettings updateConfiguration = new XMLSettings(UpdateFilename);

                    Version version = Version.Parse(updateConfiguration.Read("Current", "Version", ""));
                    Version currentVersion = Version.Parse(AssemblyVersion);

                    string released = updateConfiguration.Read("Current", "Released", "");
                    string downloadLink = updateConfiguration.Read("Current", "DownloadLink", "");

                    if (version > currentVersion)
                    {
                        lblDescription.Content = ClipTree.Resources.UIMessages.AnUpdateHasBeenFound;
                        lblVersion.Content = version.ToString();
                        lblReleased.Content = released;

                        SetVisibleState(Visibility.Visible);

                        m_downloadLink = downloadLink;
                    }
                    else
                    {
                        updateFound = false;
                    }
                }
                catch
                {
                    updateFound = false;
                }

                if (!updateFound)
                {
                    lblDescription.Content = ClipTree.Resources.Dialog.NoUpdatesAvailable;
                }
            }
        }

        #region Private "Title Bar" Events

        private void Title_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();

                m_windowPosition.Changed = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Private "Window" Events

        private void Window_OnActivated(object sender, EventArgs e)
        {
            WindowBorder.Background = Brushes.Gray;
        }

        private void Window_OnDeactivated(object sender, EventArgs e)
        {
            WindowBorder.Background = Brushes.DarkGray;
        }

        private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (KeyStroke.IsAltKey(Key.Space))
            {
                e.Handled = true;
            }
        }

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_windowPosition.Set();
        }

        #endregion

        #region Private "Assembly Attribute Accessors" Helpers

        private static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public object Proceses { get; private set; }

        #endregion

        #region Private "Button" Events

        private void Button_Download_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(m_downloadLink))
            {
                Processes.Start(m_downloadLink);

                Close();
            }
        }

        #endregion
    }
}
