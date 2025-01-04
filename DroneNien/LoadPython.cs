using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DroneNien
{
    internal class LoadPython
    {
        public async void ProcessPythonFile()
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

                // 1. Khởi tạo ProcessStartInfo
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "python.exe";
                start.Arguments = pythonFilePath;

                // 2. Redirect output và error
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                start.CreateNoWindow = true; // Optional: Ẩn cửa sổ console

                // 3. Khởi chạy process trong background thread
                StringBuilder outputBuilder = new StringBuilder();
                StringBuilder errorBuilder = new StringBuilder();

                // Sử dụng Task để chạy Python script trong background thread
                await Task.Run(() =>
                {
                    using (Process process = Process.Start(start) ?? throw new InvalidOperationException("Failed to start process"))
                    {
                        // Đọc output
                        using (StreamReader outputReader = process.StandardOutput)
                        {
                            string? line;
                            while ((line = outputReader.ReadLine()) != null)
                            {
                                outputBuilder.AppendLine(line);
                            }
                        }

                        // Đọc error
                        using (StreamReader errorReader = process.StandardError)
                        {
                            string? line;
                            while ((line = errorReader.ReadLine()) != null)
                            {
                                errorBuilder.AppendLine(line);
                            }
                        }

                        process.WaitForExit();
                    }
                });

                // 4. Hiển thị output hoặc error
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
    }
}
