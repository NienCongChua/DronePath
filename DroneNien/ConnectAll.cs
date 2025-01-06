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
        public void StartUnrealEngine()
        {
            var process = Process.GetProcessesByName("UE4Editor").FirstOrDefault();
            if (process == null)
            {
                try
                {
                    // Mở Unreal Engine Editor
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = @"C:\Program Files\Epic Games\UE_4.27\Engine\Binaries\Win64\UE4Editor.exe",
                        Arguments = @"A:\ScienceResearch\AirSim\Unreal\Environments\Blocks\Blocks.uproject",
                        // Thay đổi đường dẫn tùy theo máy của bạn
                        UseShellExecute = true
                    });

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
                MessageBox.Show("Unreal Engine is already running.");
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public void StartNetMode(Border parentBorder)
        {
            var process = Process.GetProcessesByName("UE4Editor").FirstOrDefault();
            if (process == null)
            {
                try
                {
                    // Mở Unreal Engine Editor
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = @"C:\Program Files\Epic Games\UE_4.27\Engine\Binaries\Win64\UE4Editor.exe",
                        Arguments = @"A:\ScienceResearch\AirSim\Unreal\Environments\Blocks\Blocks.uproject",
                        UseShellExecute = true
                    });

                    // Đợi Unreal Engine tải xong (thời gian chờ: 2 giây)
                    Thread.Sleep(2000); // Chờ 2 giây
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể khởi động Unreal Engine hoặc nhúng cửa sổ: {ex.Message}");
                }
            }
            else
            {
                // Tìm cửa sổ Unreal Engine
                IntPtr ue4WindowHandle = FindUnrealEngineWindow();

                if (ue4WindowHandle != IntPtr.Zero)
                {
                    // Nhúng cửa sổ Unreal Engine vào Border
                    WindowInteropHelper helper = new WindowInteropHelper(Window.GetWindow(parentBorder));
                    SetParent(ue4WindowHandle, helper.Handle);

                    // Di chuyển và thay đổi kích thước cửa sổ Unreal Engine
                    MoveWindow(ue4WindowHandle, 0, 0, (int)parentBorder.ActualWidth, (int)parentBorder.ActualHeight, true);

                    // Đăng ký sự kiện thay đổi kích thước của Border
                    parentBorder.SizeChanged += (s, e) =>
                    {
                        MoveWindow(ue4WindowHandle, 0, 0, (int)parentBorder.ActualWidth, (int)parentBorder.ActualHeight, true);
                    };
                }
                else
                {
                    MessageBox.Show("Không tìm thấy cửa sổ NetMode.");
                }
            }
        }


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
                if (windowText.ToString().Contains("Blocks Environment"))
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
                    UseShellExecute = true, // Hiển thị cửa sổ
                    CreateNoWindow = false // Không ẩn cửa sổ
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
                    MessageBox.Show("QGroundControl is already running.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start QGroundControl: {ex.Message}");
            }
        }

        public void StopApplications()
        {
            try
            {
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
                // MessageBox.Show("All applications stopped successfully!");
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


        private string? GetQGroundControlPath()
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
    }
}
