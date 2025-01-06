using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;

namespace DroneNien
{
    internal class ProcessDisplay
    {
        //Hiển thị ảnh
        private string folderPath = @"D:\NCKH\dataset\images";
        private string? latestImagePath = null;
        private FileSystemWatcher fileWatcher = new FileSystemWatcher();
        private Image SlideshowImage = new Image(); 

        public ProcessDisplay(Image slideshowImage) // Add this constructor to initialize SlideshowImage
        {
            SlideshowImage = slideshowImage;
            CheckAndLoadImage();
        }

        private void CheckAndLoadImage()
        {
            // Kiểm tra nếu thư mục không tồn tại
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Cấu hình FileSystemWatcher
            fileWatcher = new FileSystemWatcher(folderPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.*"
            };

            // Xử lý khi tệp mới được thêm
            fileWatcher.Created += OnNewImageAdded;
            fileWatcher.EnableRaisingEvents = true;

            // Kiểm tra và hiển thị ảnh hiện có (nếu có)
            LoadLatestImage();
        }

        private void OnNewImageAdded(object sender, FileSystemEventArgs e)
        {
            // Kiểm tra nếu là tệp ảnh hợp lệ
            if (IsImageFile(e.FullPath))
            {
                // Cập nhật đường dẫn ảnh mới
                latestImagePath = e.FullPath;

                // Hiển thị ảnh trên giao diện
                Application.Current.Dispatcher.Invoke(() => DisplayImage(latestImagePath));
            }
        }

        private void LoadLatestImage()
        {
            var files = Directory.GetFiles(folderPath, "*.*")
                                 .Where(IsImageFile)
                                 .OrderByDescending(File.GetCreationTime)
                                 .ToList();

            if (files.Any())
            {
                latestImagePath = files.First();
                DisplayImage(latestImagePath);
            }
        }
        private bool IsImageFile(string filePath)
        {
            string[] validExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };
            string extension = System.IO.Path.GetExtension(filePath).ToLower();
            return validExtensions.Contains(extension);
        }

        private async void DisplayImage(string imagePath)
        {
            try
            {
                MessageBox.Show($"Đang cố gắng tải ảnh: {imagePath}"); // In đường dẫn
                if (File.Exists(imagePath))
                {
                    await Task.Run(() =>
                    {
                        using (var bitmap = new System.Drawing.Bitmap(imagePath))
                        {
                            // Khóa bitmap để lấy BitmapData
                            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                            var bitmapSource = System.Windows.Media.Imaging.BitmapSource.Create(
                                bitmap.Width,
                                bitmap.Height,
                                bitmap.HorizontalResolution,
                                bitmap.VerticalResolution,
                                System.Windows.Media.PixelFormats.Bgr24,
                                null,
                                bitmapData.Scan0,
                                bitmapData.Stride * bitmap.Height,
                                bitmapData.Stride
                            );

                            // Mở khóa bitmap sử dụng cùng bitmapData
                            bitmap.UnlockBits(bitmapData);

                            Application.Current.Dispatcher.Invoke(() => SlideshowImage.Source = bitmapSource);
                        } // bitmap.Dispose() được gọi tự động ở cuối using block
                    });
                }
                else
                {
                    MessageBox.Show($"Tệp không tồn tại: {imagePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể tải ảnh: {ex.Message}");
            }
        }
    }
}
