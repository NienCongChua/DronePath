using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace DroneNien
{
    internal class Mission
    {
        private ConnectAll connectAll = new ConnectAll();
        public void StartMission()
        {
            ActivateQGroundControl();
            MessageBox.Show("Starting mission...");
        }

        public void LoadMission()
        {
            MessageBox.Show("Loading mission...");
        }

        private void ActivateQGroundControl()
        {
            var qgcProcess = Process.GetProcessesByName("QGroundControl").FirstOrDefault();

            if (qgcProcess == null)
            {
                connectAll.StartQGroundControl();
                qgcProcess = Process.GetProcessesByName("QGroundControl").FirstOrDefault();
                if (qgcProcess == null)
                {
                    MessageBox.Show("Failed to start QGroundControl.");
                    return;
                }
            }

            qgcProcess.WaitForInputIdle();

            // Bring QGroundControl to the foreground
            SetForegroundWindow(qgcProcess.MainWindowHandle);

            // Simulate key presses to navigate to the "Fly" section
            Thread.Sleep(100); // Add a short delay to ensure the window is in the foreground

            InputSimulator sim = new InputSimulator();
            sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_1);

            // Add a short delay to ensure the "Fly" section is active
            Thread.Sleep(100);

            // Simulate key presses to navigate to the "Plan" section within "Fly"
            sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_2);
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}

