using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Windows.Shapes;
using System; //Bổ sung
using Python.Runtime; //Bổ sung
using Microsoft.Win32;

namespace DroneNien
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MAVLink mavlink = new MAVLink();
        private Process pythonProcess;
        private Process unrealProcess;
        private bool isDroneConnected = false;
        private bool isSimulationRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            // InitializePythonBackend();
        }

        private void InitializePythonBackend()
        {
            // Initialize Python environment and required libraries
            string pythonScript = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "drone_control.py");
            pythonProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = pythonScript,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
                
            };
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!isDroneConnected)
            {
                try
                {
                    pythonProcess.Start();
                    isDroneConnected = true;
                    txtStatus.Text = "Connected";
                    btnConnect.Content = "Disconnect";
                    MessageBox.Show("Dauuuu");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to connect: {ex.Message}");
                }
            }
            else
            {
                try
                {
                    pythonProcess.Kill();
                    isDroneConnected = false;
                    txtStatus.Text = "Disconnected";
                    btnConnect.Content = "Connect";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to disconnect: {ex.Message}");
                }
            }
        }

        private void btnArm_Click(object sender, RoutedEventArgs e)
        {
            if (isDroneConnected)
            {
                // Send arm command to Python backend
                SendCommand("arm");
            }
        }

        private void btnTakeoff_Click(object sender, RoutedEventArgs e)
        {
            if (isDroneConnected)
            {
                // Send takeoff command to Python backend
                SendCommand("takeoff");
            }
        }

        private void btnLand_Click(object sender, RoutedEventArgs e)
        {
            if (isDroneConnected)
            {
                // Send land command to Python backend
                SendCommand("land");
            }
        }

        private void btnRTL_Click(object sender, RoutedEventArgs e)
        {
            if (isDroneConnected)
            {
                // Send RTL command to Python backend
                SendCommand("rtl");
            }
        }

        private void btnStartSim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Python Files (*.py)|*.py"; 
                if (openFileDialog.ShowDialog() != true)
                {
                    return;
                }
                string pythonFilePath = openFileDialog.FileName;

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "python.exe"; // Hoặc python3, hoặc đường dẫn đầy đủ nếu cần
                start.Arguments = pythonFilePath;

                // 4. Redirect output và error
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                start.CreateNoWindow = true; // Optional: Ẩn cửa sổ console


                // 5. Khởi chạy process và đọc output/error
                StringBuilder outputBuilder = new StringBuilder();
                StringBuilder errorBuilder = new StringBuilder();

                using (Process process = Process.Start(start))
                {
                    using (StreamReader outputReader = process.StandardOutput)
                    {
                        string line;
                        while ((line = outputReader.ReadLine()) != null)
                        {
                            outputBuilder.AppendLine(line);
                        }
                    }

                    using (StreamReader errorReader = process.StandardError)
                    {
                        string line;
                        while ((line = errorReader.ReadLine()) != null)
                        {
                            errorBuilder.AppendLine(line);
                        }
                    }

                    process.WaitForExit();
                }

                // 6. Hiển thị output hoặc error
                string output = outputBuilder.ToString();
                string error = errorBuilder.ToString();

                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show("Lỗi Python:\n" + error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Kết quả:\n" + output, "Kết quả", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi:\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendCommand(string command)
        {
            try
            {
                // Send command to Python backend
                pythonProcess.StandardInput.WriteLine(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send command: {ex.Message}");
            }
        }

        private void StartUnrealSimulation()
        {
            // Start Unreal Engine simulation process
            // This is a placeholder - implement actual Unreal Engine integration
            MessageBox.Show("Starting Unreal Engine simulation...");
        }

        private void StopUnrealSimulation()
        {
            // Stop Unreal Engine simulation process
            // This is a placeholder - implement actual Unreal Engine integration
            MessageBox.Show("Stopping Unreal Engine simulation...");
        }

        protected override void OnClosed(EventArgs e)
        {
            // Cleanup resources
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                pythonProcess.Kill();
            }
            if (unrealProcess != null && !unrealProcess.HasExited)
            {
                unrealProcess.Kill();
            }
            base.OnClosed(e);
        }

        private void btnLoadMission_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnStartMission_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPauseMission_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnStopSim_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void Display(object sender, RoutedEventArgs e)
        {
            
        }
    }
}