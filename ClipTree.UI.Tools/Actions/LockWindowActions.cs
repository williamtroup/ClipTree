using System;
using System.Windows;
using System.Windows.Interop;

namespace ClipTree.UI.Tools.Actions
{
    public class LockWindowActions
    {
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MOVE = 0xF010;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_MAXIMIZE = 0xF030;

        private readonly Window m_window;

        public LockWindowActions(Window window)
        {
            m_window = window;

            m_window.SourceInitialized += Window_OnSourceInitialized;
        }

        public bool LockMoving { get; set; }
        public bool LockMinimizing { private get; set; }
        public bool LockMaximizing { private get; set; }

        private void Hook()
        {
            WindowInteropHelper helper = new WindowInteropHelper(m_window);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);

            if (source != null)
            {
                source.AddHook(WndProc);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_SYSCOMMAND:

                    int command = wParam.ToInt32() & 0xfff0;

                    if (command == SC_MOVE && LockMoving)
                    {
                        handled = true;
                    }

                    else if (command == SC_MINIMIZE && (LockMinimizing || !m_window.ShowInTaskbar))
                    {
                        handled = true;
                    }

                    else if (command == SC_MAXIMIZE && (LockMaximizing || !m_window.ShowInTaskbar))
                    {
                        handled = true;
                    }

                    break;

                default:
                    break;
            }

            return IntPtr.Zero;
        }

        private void Window_OnSourceInitialized(object sender, EventArgs e)
        {
            Hook();
        }
    }
}
