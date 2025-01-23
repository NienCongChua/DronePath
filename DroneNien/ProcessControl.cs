using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace DroneNien
{
    internal class ProcessControl
    {
        private ConnectAll connectAll = new ConnectAll();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const int SW_RESTORE = 9;

        public void LoadMission()
        {
            ActivateQGroundControl();
        }

        private void ActivateQGroundControl()
        {
            IntPtr hWnd = FindWindow(null, "QGroundControl");
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
            }
            else
            {
                MessageBox.Show("QGroundControl window not found.");
            }
        }
    }
}

