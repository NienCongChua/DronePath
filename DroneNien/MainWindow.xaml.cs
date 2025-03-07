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
            // Tạm ngừng để giảm thời gian khởi động ứng dụng
            try
            {
                txtStatus.Text = "Connecting";
                // Khởi chạy các ứng dụng
                connectAll.StartUnrealEngine();
                connectAll.StartPX4();
                connectAll.StartQGroundControl();
                connectAll.StartNetMode(Display);
                connectAll.HideUnrealEngine();
                

                // Cập nhật trạng thái giao diện
                isDroneConnected = true;
                txtStatus.Text = "Connected";
                runDetect();

                Thread.Sleep(7000);
                processControl.HideApp("QGroundControl");
                processControl.HideApp("python3.10");
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
            processControl.HideApp("python3.10");
            processControl.HideApp("UE4Editor");
            processControl.LoadApp("QGroundControl");
        }

        private void btnManHinhNhanDien_Click(object sender, RoutedEventArgs e)
        {
            processControl.HideApp("QGroundControl");
            processControl.HideApp("UE4Editor");
            processControl.LoadApp("python3.10");
        }

        private async void runDetect()
        {
            await Task.Run(() => loadPython.ProcessPythonFile("C:\\Users\\NienNguyen\\Desktop\\DronePath\\DroneNien\\source\\detect\\VATesstAirsim.py"));
        }
        private void btnManHinhDetect_Click(object sender, RoutedEventArgs e)
        {
            // Man hinh detect
            processControl.HideApp("QGroundControl");
            processControl.HideApp("UE4Editor");
            processControl.LoadApp("python3.10");
        }

        private void btnChonDoiTuong_Click(object sender, RoutedEventArgs e)
        {
            // Waiting for the next update
            HidePage(sender, e);
            MainFrame.Navigate(new SelectObject());
            MainFrame.Visibility = Visibility.Visible;
        }

        private void btnUnreal_Click(object sender, RoutedEventArgs e)
        {
            processControl.HideApp("QGroundControl");
            processControl.HideApp("python3.10");
            processControl.LoadApp("UE4Editor");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Ngắt kết nối và dừng các ứng dụng
            connectAll.StopApplications();
        }
    }
}