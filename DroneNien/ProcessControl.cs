using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using WindowsInput;
using WindowsInput.Native;

namespace DroneNien
{
    internal class ProcessControl
    {
        private ConnectAll connectAll = new ConnectAll();
        private WindowEnumerator windowEnumerator = new WindowEnumerator();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags
        );

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        // Delegate for the EnumChildWindows callback
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private const int SW_RESTORE = 1;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_SHOWWINDOW = 0x0040;

        public void LoadApp(string name, bool bringToTop = true)
        {
            ActivateWindow(name, bringToTop);
        }


        private void ActivateWindow(string nameWindow, bool bringToTop)
        {
            IntPtr hWnd = FindWindow(null!, nameWindow);
            var process = Process.GetProcessesByName(nameWindow).FirstOrDefault();

            if (hWnd != IntPtr.Zero) // Nếu cửa sổ đã tồn tại
            {
                if (nameWindow == "QGroundControl")
                {
                    var hwndList = WindowEnumerator.FindWindowsByTitle(nameWindow);

                    //MessageBox.Show($"Tìm thấy {hwndList.Count} cửa sổ có tên chứa \"{nameWindow}\".");
                    if (hwndList.Count == 1)
                    {
                        hWnd = hwndList[0];
                        ShowWindow(hWnd, 9);
                    }
                }
                ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
                BringWindowToTop(hWnd);

                // Nếu bringToTop = true thì đưa lên trên cùng
                IntPtr hWndInsertAfter = bringToTop ? HWND_TOPMOST : HWND_NOTOPMOST;
                SetWindowPos(hWnd, hWndInsertAfter, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);
            }
            else // Nếu chưa khởi chạy ứng dụng
            {
                if (nameWindow == "UE4Editor")
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "D:/Path_To_Unreal/UE4Editor.exe",
                            UseShellExecute = true
                        });
                        Thread.Sleep(5000); // Chờ cho ứng dụng khởi động
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi mở UE4Editor: {ex.Message}");
                    }
                }
                else if (nameWindow == "python")
                {
                    connectAll.StartObjectDetection(null); // Gọi Python nếu chưa chạy
                } else if (nameWindow == "QGroundControl")
                {
                    connectAll.StartQGroundControl();
                }
            }
        }

        public void HideApp(string nameApplication)
        {
            IntPtr hWnd = FindWindow(null!, nameApplication);
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, 0); // Ẩn cửa sổ
            }
        }
    }
}