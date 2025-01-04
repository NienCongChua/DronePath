using System.Diagnostics;
using System.IO;
using System.Windows;

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

                    // Đợi Unreal Engine tải xong (thời gian chờ: 20 giây)
                    Console.WriteLine("Waiting for Unreal Engine to load...");
                    System.Threading.Thread.Sleep(5000); // Chờ 20 giây

                    // Gửi phím tắt Alt + P để Play
                    UnrealAutomation.TriggerPlayShortcut();

                    // Thông báo thành công
                    MessageBox.Show("Unreal Engine started and Play mode activated!");
                }
                catch (Exception ex)
                {
                    // Thông báo lỗi nếu không khởi chạy được Unreal hoặc không gửi phím tắt
                    MessageBox.Show($"Failed to start Unreal Engine or Play mode: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Unreal Engine is already running.");
            }
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

                // Thông báo người dùng
                if (process != null)
                {
                    MessageBox.Show("PX4 is running in a WSL terminal window. Please check the terminal for logs.",
                                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to start PX4. Could not create the process.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

                MessageBox.Show("All applications stopped successfully!");
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
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\QGroundControl\QGroundControl.exe"
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
