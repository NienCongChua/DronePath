using System.Windows;


namespace DroneNien
{
    public partial class MainWindow : Window
    {
        public DroneViewModel ViewModel { get; set; }

        private bool isDroneConnected = false;
        private ConnectAll connectAll;
        private SendCommand sendCommand;
        private LoadPython loadPython;
        private ProcessControl processControl;

        public MainWindow()
        {
            InitializeComponent();
            connectAll = new ConnectAll();
            sendCommand = new SendCommand();
            loadPython = new LoadPython();
            processControl = new ProcessControl();

            // Kết nối tới cổng UDP 14556 của PX4-Autopilot
            sendCommand.ConnectToUDPPort();

            // Receive data from PX4-Autopilot
            ViewModel = new DroneViewModel();
            DataContext = ViewModel;
            StartReceivingData();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                txtStatus.Text = "Connecting";
                // Khởi chạy các ứng dụng
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

                //Thread.Sleep(7000);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something ERROR: {ex.Message}");
            }
        }

        private async void StartReceivingData()
        {
            await ViewModel.StartReceivingData();
        }

        private void HidePage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BlankPage());    
        }

        private async void btnLoadPythonFile_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => loadPython.ProcessPythonFile("D:/NCKH/DRONE_HOANG/DRONE_HOANG/path.py"));
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            // Ngắt kết nối và dừng các ứng dụng
            connectAll.StopApplications();
            this.Close();
        }

        private void btnChonVungTuanTra_Click(object sender, RoutedEventArgs e)
        {
            //connectAll.showApplication("QGroundControl");
            MainFrame.Visibility = Visibility.Collapsed;
            processControl.HideApp("python");
            processControl.HideApp("UE4Editor");
            processControl.LoadApp("QGroundControl");
        }

        private void btnDetect_Click(object sender, RoutedEventArgs e)
        {
            runDetect();
        }

        private async void runDetect()
        {
            await Task.Run(() => loadPython.ProcessPythonFile(@"D:\NCKH\GitHubNien\AirsimYolo\AirsimYolo\VATesstAirsim.py"));
        }
        private void btnManHinhDetect_Click(object sender, RoutedEventArgs e)
        {
            processControl.HideApp("QGroundControl");
            processControl.HideApp("UE4Editor");
            processControl.LoadApp("Object Detection");
        }

        private void btnUnreal_Click(object sender, RoutedEventArgs e)
        {
            processControl.HideApp("QGroundControl");
            processControl.HideApp("Object Detection");
            processControl.LoadApp("UE4Editor");

            // Ẩn cửa sổ SelectObject nếu nó đang mở
            if (selectObjectWindow != null)
            {
                selectObjectWindow.Close();
                selectObjectWindow = null; // Giải phóng biến
            }
        }


        private Window selectObjectWindow; // Lưu cửa sổ SelectObject

        private void btnChonDoiTuong_Click(object sender, RoutedEventArgs e)
        {
            processControl.HideApp("QGroundControl");
            processControl.HideApp("Object Detection");

            // Nếu cửa sổ SelectObject đã mở, đóng nó trước khi mở lại
            if (selectObjectWindow != null)
            {
                selectObjectWindow.Close();
            }

            // Lấy vị trí chính xác của MainFrame trên màn hình
            Point relativePoint = MainFrame.TransformToAncestor(this).Transform(new Point(0, 0));
            Point screenPoint = this.PointToScreen(relativePoint);

            // Kích thước cố định
            double width = 1220;
            double height = 690;

            // Tạo cửa sổ SelectObject
            selectObjectWindow = new SelectObject();
            selectObjectWindow.WindowStartupLocation = WindowStartupLocation.Manual;

            // ✨ Tuỳ chỉnh vị trí nếu muốn dịch chuyển cửa sổ
            selectObjectWindow.Left = screenPoint.X - 80;  // Dịch phải 50px (thay đổi giá trị này nếu muốn)
            selectObjectWindow.Top = screenPoint.Y ;   // Dịch xuống 30px (thay đổi giá trị này nếu muốn)

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
    }
}