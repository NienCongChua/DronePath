using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;


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
            //try
            //{
            //    txtStatus.Text = "Connecting";
            //    // Khởi chạy các ứng dụng
            //    connectAll.StartUnrealEngine();
            //    connectAll.StartPX4();
            //    connectAll.StartQGroundControl();
            //    Thread.Sleep(1000); // Đợi 1 giây để các ứng dụng khởi động
            //    connectAll.StartNetMode(Display);
            //    connectAll.HideUnrealEngine();
            //    connectAll.StartNetQGCMode(Display);

            //    // Cập nhật trạng thái giao diện
            //    isDroneConnected = true;
            //    txtStatus.Text = "Connected";
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Something ERROR: {ex.Message}");
            //}
        }

        private async void StartReceivingData()
        {
            await ViewModel.StartReceivingData();
        }

        private void btnLoadMission_Click(object sender, RoutedEventArgs e)
        {
            if (isDroneConnected)
            {
                processControl.LoadMission();
            }
            else
            {
                MessageBox.Show("Please connect to the drone first.");
            }
        }

        private async void btnLoadPythonFile_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => loadPython.ProcessPythonFile());
        }


        //Chức năng này đã được thay thế bởi các chức năng khác
        //private void btnAdvance_Click(object sender, RoutedEventArgs e)
        //{
        //    if (AditionalFunction.Visibility == Visibility.Collapsed)
        //    {
        //        AditionalFunction.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        AditionalFunction.Visibility = Visibility.Collapsed;
        //    }
        //}

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            // Ngắt kết nối và dừng các ứng dụng
            connectAll.StopApplications();
            this.Close();
        }

        private void btnChonVungTuanTra_Click(object sender, RoutedEventArgs e)
        {
            // Waiting for the next update
            MainFrame.Visibility = Visibility.Collapsed;
        }

        private void btnManHinhNhanDien_Click(object sender, RoutedEventArgs e)
        {
            // Waiting for the next update
            MainFrame.Visibility = Visibility.Collapsed;
        }

        private void btnManHinhUnreal_Click(object sender, RoutedEventArgs e)
        {
            // Waiting for the next update
            MainFrame.Visibility = Visibility.Collapsed;
        }

        private void btnChonDoiTuong_Click(object sender, RoutedEventArgs e)
        {
            // Waiting for the next update
            MainFrame.Navigate(new SelectObject());
            MainFrame.Visibility = Visibility.Visible;
        }
    }
}