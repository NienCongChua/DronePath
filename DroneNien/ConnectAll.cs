using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Text;
using System.Windows.Controls;

namespace DroneNien
{
    internal class ConnectAll
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int SW_HIDE = 0;
        private const int SW_RESTORE = 9;
        public void StartUnrealEngine()
        {
            var ue4Process = Process.GetProcessesByName("UE4Editor").FirstOrDefault();
            if (ue4Process == null)
            {
                try
                {
                    // Mở Unreal Engine Editor
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = GetUnrealPath(),
                        Arguments = GetBlocksPath(),
                        UseShellExecute = true,
                        CreateNoWindow = true,
                    });

                    if (ue4Process != null)
                    {
                        ue4Process.WaitForInputIdle(); // Wait for the process to be ready for input
                        IntPtr hWnd = ue4Process.MainWindowHandle;
                        if (hWnd != IntPtr.Zero)
                        {
                            ShowWindow(hWnd, SW_HIDE); // Hide the window
                        }
                    }

                    // Gửi phím tắt Alt + P để Play
                    UnrealAutomation.TriggerPlayShortcut();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to start Unreal Engine or Play mode: {ex.Message}");
                }
            }
            else
            {
                // MessageBox.Show("Unreal Engine is already running.");
            }
        }

        public void HideUnrealEngine()
        {
        var ue4Process = Process.GetProcessesByName("UE4Editor").FirstOrDefault();
            try
            {
                if (ue4Process != null)
                {
                    ue4Process.WaitForInputIdle(); // Wait for the process to be ready for input
                    IntPtr hWnd = ue4Process.MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, SW_HIDE); // Hide the window
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to hide Unreal Engine: {ex.Message}");
            }
        }


        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public void StartNetMode(Border parentBorder)
        {
            var process = Process.GetProcessesByName("UE4Editor").FirstOrDefault();
            IntPtr ue4WindowHandle = FindUnrealEngineWindow();
            if (ue4WindowHandle != IntPtr.Zero)
            {
                // Lưu offset và scale
                int offsetX = 78;
                int offsetY = 5;
                double scaleFactor = 1.25;

                Window parentWindow = Window.GetWindow(parentBorder);
                WindowInteropHelper helper = new WindowInteropHelper(parentWindow);

                // Thiết lập parent
                SetParent(ue4WindowHandle, helper.Handle);

                void UpdateUnrealWindowPosition()
                {
                    Point borderPos = parentBorder.PointToScreen(new Point(0, 0));
                    borderPos = parentWindow.PointFromScreen(borderPos);

                    SetWindowPos(
                        ue4WindowHandle,
                        IntPtr.Zero,
                        (int)borderPos.X + offsetX,
                        (int)borderPos.Y + offsetY,
                        (int)(parentBorder.ActualWidth * scaleFactor),
                        (int)(parentBorder.ActualHeight * scaleFactor),
                        SWP_SHOWWINDOW
                    );
                }

                // Thiết lập ban đầu
                UpdateUnrealWindowPosition();
                // Xử lý khi Border thay đổi kích thước hoặc vị trí
                parentBorder.SizeChanged += (s, e) =>
                {
                    UpdateUnrealWindowPosition(); // Gọi hàm cập nhật
                };
            }
        }
        // Thêm constant
        private const int SWP_SHOWWINDOW = 0x0040;

        // Thêm P/Invoke
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags
        );
        //-----------------------------------------------------------

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private IntPtr FindUnrealEngineWindow()
        {
            IntPtr foundWindow = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
            {
                StringBuilder windowText = new StringBuilder(256);
                GetWindowText(hWnd, windowText, windowText.Capacity);
                if (windowText.ToString().Contains("Preview"))
                {
                    foundWindow = hWnd;
                    return false; // Stop enumeration
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);

            return foundWindow;
        }

        // Px4 nè
        public void StartPX4()
        {
            try
            {
                // Lệnh đầy đủ để kiểm tra, dừng PX4, xóa build cũ và khởi chạy PX4 SITL
                string fullCommand = "killall -9 px4; cd ~/PX4-Autopilot && make px4_sitl jmavsim && echo 'Simulation complete. Press Enter to exit...' && read";

                // Cấu hình process để chạy trực tiếp WSL
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe", // Chạy WSL trực tiếp
                    Arguments = $"-e bash -c \"{fullCommand}\"", // Thực thi lệnh đầy đủ trong Bash
                    UseShellExecute = false, // Hiển thị cửa sổ
                    CreateNoWindow = true // Không ẩn cửa sổ
                };

                // Khởi động tiến trình
                Process? process = Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi mở WSL
                MessageBox.Show($"An error occurred while starting PX4: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Hàm khởi chạy QGroundControl
        public void StartQGroundControl()
        {
            try
            {
                var process = Process.GetProcessesByName("QGroundControl").FirstOrDefault();
                if (process == null)
                {
                    _StartQGroundControl();
                }
                else
                {
                    // MessageBox.Show("QGroundControl is already running.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start QGroundControl: {ex.Message}");
            }
        }

        public void StartQGroundControl(Border parentBorder)
        {
            var process = Process.GetProcessesByName("QGroundControl").FirstOrDefault();
            if (process == null)
            {
                string? qgcPath = GetQGroundControlPath();
                if (!string.IsNullOrEmpty(qgcPath))
                {
                    Process.Start(qgcPath);
                    Thread.Sleep(3000); // Wait for QGC to open
                    process = Process.GetProcessesByName("QGroundControl").FirstOrDefault();
                }
            }

            if (process != null)
            {
                IntPtr qgcWindowHandle = FindWindow(null!, "QGroundControl");
                if (qgcWindowHandle != IntPtr.Zero)
                {
                    // Define offset and scaling factors
                    int offsetX = 78;
                    int offsetY = 5;
                    double scaleFactor = 1.25;

                    Window parentWindow = Window.GetWindow(parentBorder);
                    WindowInteropHelper helper = new WindowInteropHelper(parentWindow);

                    // Set QGroundControl as child of the WPF window
                    SetParent(qgcWindowHandle, helper.Handle);

                    void UpdateQGCPosition()
                    {
                        Point borderPos = parentBorder.PointToScreen(new Point(0, 0));
                        borderPos = parentWindow.PointFromScreen(borderPos);

                        SetWindowPos(
                            qgcWindowHandle,
                            IntPtr.Zero,
                            (int)borderPos.X + offsetX,
                            (int)borderPos.Y + offsetY,
                            (int)(parentBorder.ActualWidth * scaleFactor),
                            (int)(parentBorder.ActualHeight * scaleFactor),
                            SWP_SHOWWINDOW
                        );
                    }

                    // Initial position update
                    UpdateQGCPosition();

                    // Update QGC position when the Border size changes
                    parentBorder.SizeChanged += (s, e) => UpdateQGCPosition();
                }
            }
            else
            {
                MessageBox.Show("Failed to start QGroundControl.");
            }
        }


        public void StopApplications()
        {
            try
            {
                // Stop Python
                foreach (var process in Process.GetProcessesByName("python3.10"))
                {
                    process.Kill();
                    process.WaitForExit();
                }

                // Dừng Unreal Engine
                foreach (var process in Process.GetProcessesByName("UE4Editor"))
                {
                    process.Kill();
                }

                // Dừng PX4 (nếu cần)
                foreach (var process in Process.GetProcessesByName("bash")) // PX4 chạy qua WSL
                {
                    process.Kill();
                }

                // Dừng QGroundControl
                foreach (var process in Process.GetProcessesByName("QGroundControl"))
                {
                    process.Kill();
                }

                // Dừng màn hình Detect
                try
                {
                    foreach (var process in Process.GetProcessesByName("python"))
                    {
                        process.Kill();
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("There is no window named Object Detection");
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to stop applications: {ex.Message}");
            }
        }

        private void _StartQGroundControl()
        {
            try
            {
                string? qgcPath = GetQGroundControlPath();
                if (!string.IsNullOrEmpty(qgcPath))
                {
                    Process.Start(qgcPath);
                }
                else
                {
                    MessageBox.Show("QGroundControl not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start QGroundControl: {ex.Message}");
            }
        }

        public void showApplication(string nameApplication)
        {
            try
            {
                var process = Process.GetProcessesByName(nameApplication).FirstOrDefault();
                if (process == null)
                {
                    try
                    {
                        string? applicationPath = GetQGroundControlPath();
                        if (!string.IsNullOrEmpty(applicationPath))
                        {
                            Process.Start(applicationPath);
                        }
                        else
                        {
                            MessageBox.Show($"Application path is empty. ",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while starting Application: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    IntPtr hWnd = FindWindow(null!, nameApplication);
                    ShowWindow(hWnd, SW_RESTORE);
                    SetForegroundWindow(hWnd);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start QGroundControl: {ex.Message}");
            }
        }
        public string? GetQGroundControlPath()
        {
            string[] possiblePaths = 
            {   
                @"C:\Program Files\QGroundControl\QGroundControl.exe",
                @"C:\Program Files (x86)\QGroundControl\QGroundControl.exe",
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\QGroundControl\QGroundControl.exe",
                @"D:\CSDL\QGroundControl\QGroundControl.exe"

            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private string? GetUnrealPath()
        {
            string[] possiblePaths =
            {
                //@"C:\Program Files\Epic Games\UE_4.27\Engine\Binaries\Win64\UE4Editor.exe",
                //@"C:\Users\" + Environment.UserName + @"\AppData\Local\QGroundControl\UE4Editor.exe",
                @"D:\UE_4.27\Engine\Binaries\Win64\UE4Editor.exe"

            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }


        private string? GetBlocksPath()
        {
            string[] possiblePaths =
            {
                @"D:\CityPark\CityParkEnvironment.uproject",
                //@"C:\Users\" + Environment.UserName + @"\Documents\AirSim\Unreal\Environments\Blocks\Blocks.uproject",
                //@"C:\Users\vannha2004\source\repos\AirSim\Unreal\Environments\Blocks\Blocks.uproject"

            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }
    }
}
