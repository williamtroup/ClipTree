using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ClipTree.Engine.Settings.Interfaces;
using ClipTree.UI.Tools;
using ClipTree.UI.Tools.Views;
using ClipTree.UI.Tools.Actions;
using ClipTree.Engine.Windows;
using System.Windows.Controls;

namespace ClipTree.Windows.Report
{
    public partial class About : Window
    {
        #region Private Read-Only Variables

        private readonly IXMLSettings m_settings;
        private readonly WindowPosition m_windowPosition;

        #endregion

        public About(IXMLSettings settings)
        {
            InitializeComponent();

            m_settings = settings;
            m_windowPosition = new WindowPosition(this, m_settings, Width, Height, GetName);

            SetupDisplay();

            BackgroundAction.Run(() => m_windowPosition.Get());
        }

        public static string GetName
        {
            get
            {
                return string.Format(Settings.WindowNameFormat, nameof(About), Settings.Window);
            }
        }

        private void SetupDisplay()
        {
            lblVersion.Text = String.Format("Version {0}", AssemblyVersion);
            lblCopyright.Text = AssemblyCopyright;
            txtDescription.Text = AssemblyDescription;
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

        #region Private "Label" Events

        private void CheckForUpdatesLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Updates updates = new Updates(m_settings)
                {
                    Topmost = Topmost,
                    Owner = this
                };

                updates.ShowDialog();
            }
        }

        private void CompanyLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                TextBlock textBlock = (TextBlock) sender;

                Processes.Start(textBlock.Text);
            }
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

        private static string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);

                if (attributes.Length == 0)
                {
                    return "";
                }

                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        private static string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

                if (attributes.Length == 0)
                {
                    return "";
                }

                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        #endregion
    }
}
