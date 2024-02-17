using System;
using System.Diagnostics;
using ClipTree.Engine.Api;

namespace ClipTree.Engine.Windows
{
    public static class Processes
    {
        #region Private Static Variables

        private static object m_lockObject = new object();

        #endregion

        public static Process GetFocused()
        {
            Process process;

            lock (m_lockObject)
            {
                IntPtr handle = IntPtr.Zero;
                handle = Win32.GetForegroundWindow();

                Win32.GetWindowThreadProcessId(handle, out uint processId);

                process = Process.GetProcessById((int) processId);
            }

            return process;
        }

        public static bool IsRunning(string processName = null, int processesAllowed = 1)
        {
            processName = processName ?? Process.GetCurrentProcess().ProcessName;
    
            Process[] processes = Process.GetProcessesByName(processName);

            return processes.Length > processesAllowed;
        }

        public static bool Start(string path)
        {
            bool ran = true;

            try
            {
                Process.Start(path);
            }
            catch
            {
                ran = false;
            }

            return ran;
        }
    }
}
