using System.Windows;
using System.IO;
using System.Collections.ObjectModel;

namespace DroneNien
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; }
        private bool isDroneConnected = false;
        private ConnectAll connectAll;
        private SendCommand sendCommand;
        private LoadPython loadPython;
        private ProcessControl processControl;

        public ObservableCollection<string> ObjectNames { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            connectAll = new ConnectAll();
            sendCommand = new SendCommand();
            loadPython = new LoadPython();
            processControl = new ProcessControl();
            ObjectNames = new ObservableCollection<string>();

            // Kết nối tới cổng UDP 14556 của PX4-Autopilot
            sendCommand.ConnectToUDPPort();

            // Receive data from PX4-Autopilot
            ViewModel = new MainViewModel(); 
            DataContext = ViewModel;
            StartReceivingData();
            //DataContext = new MainViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                txtStatus.Text = "Connecting";
                connectAll.StartUnrealEngine();
                connectAll.StartPX4();
                connectAll.StartQGroundControl();
                connectAll.HideUnrealEngine();
                Thread.Sleep(20000);
                connectAll.StartNetMode(Display);


                // Cập nhật trạng thái giao diện
                isDroneConnected = true;
                txtStatus.Text = "Connected";
                runDetect();

                Thread.Sleep(3000);
                connectAll.StartObjectDetection(Display);
                Thread.Sleep(1000);
                // processControl.HideApp("QGroundControl");
                processControl.HideApp("Object Detection");
                processControl.HideApp("UE4Editor");
                Thread.Sleep(1000);
                processControl.LoadApp("Object Detection");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something ERROR: {ex.Message}");
            }
        }

        private async void StartReceivingData()
        {
            await ViewModel.DroneVM.StartReceivingData(); // Chạy cập nhật dữ liệu từ DroneViewModel
        }

        private async void btnLoadPythonFile_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => loadPython.ProcessPythonFile("D:\\NCKH\\DRONE_HOANG\\DRONE_HOANG\\path.py"));
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            // Ngắt kết nối và dừng các ứng dụng
            connectAll.StopApplications();
            this.Close();
        }

        private void btnChonVungTuanTra_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Visibility = Visibility.Collapsed;
            processControl.HideApp("python");
            processControl.HideApp("UE4Editor");
            if (selectObjectWindow != null)
            {
                selectObjectWindow.Close();
                selectObjectWindow = null;
            }
            processControl.LoadApp("QGroundControl");
        }

        private void btnManHinhDetect_Click(object sender, RoutedEventArgs e)
        {
            processControl.HideApp("QGroundControl");
            processControl.HideApp("UE4Editor");
            processControl.LoadApp("Object Detection");
            if (selectObjectWindow != null)
            {
                selectObjectWindow.Close();
                selectObjectWindow = null;
            }
        }

        private void btnUnreal_Click(object sender, RoutedEventArgs e)
        {
            processControl.HideApp("QGroundControl");
            processControl.HideApp("Object Detection");
            processControl.LoadApp("UE4Editor");
            if (selectObjectWindow != null)
            {
                selectObjectWindow.Close();
                selectObjectWindow = null;
            }
        }


        private void btnChonDoiTuong_Click(object sender, RoutedEventArgs e)
        {
            processControl.HideApp("QGroundControl");
            processControl.HideApp("Object Detection");

            if (selectObjectWindow != null)
            {
                selectObjectWindow.Close();
            }

            Point relativePoint = MainFrame.TransformToAncestor(this).Transform(new Point(0, 0));
            Point screenPoint = this.PointToScreen(relativePoint);

            // Kích thước cố định
            double width = 1220;
            double height = 700;

            // Tạo cửa sổ SelectObject
            selectObjectWindow = new SelectObject();
            selectObjectWindow.WindowStartupLocation = WindowStartupLocation.Manual;

            // ✨ Tuỳ chỉnh vị trí nếu muốn dịch chuyển cửa sổ
            selectObjectWindow.Left = screenPoint.X - 80;  // Dịch phải 50px (thay đổi giá trị này nếu muốn)
            selectObjectWindow.Top = screenPoint.Y - 5 ;   // Dịch lên 5px (thay đổi giá trị này nếu muốn)

            selectObjectWindow.Width = width;
            selectObjectWindow.Height = height;

            // Loại bỏ viền và thanh tiêu đề
            selectObjectWindow.WindowStyle = WindowStyle.None;
            selectObjectWindow.ResizeMode = ResizeMode.NoResize;

            selectObjectWindow.Topmost = true; // Hiển thị trên cùng
            selectObjectWindow.Show();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connectAll.StopApplications();
        }
        private Window selectObjectWindow; // Lưu cửa sổ SelectObject
        private async void runDetect()
        {
            await Task.Run(() => loadPython.ProcessPythonFile(@"D:\NCKH\GitHubNien\AirsimYolo\AirsimYolo\VATesstAirsim.py"));
        }

    }
}