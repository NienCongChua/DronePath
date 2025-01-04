using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        private ProcessDisplay processDisplay;
        private Mission mission;

        public MainWindow()
        {
            InitializeComponent();
            processDisplay = new ProcessDisplay(SlideshowImage); 
            connectAll = new ConnectAll();
            sendCommand = new SendCommand();
            loadPython = new LoadPython();
            mission = new Mission();

            // Kết nối tới cổng UDP 14556 của PX4-Autopilot
            sendCommand.ConnectToUDPPort();

            // Receive data from PX4-Autopilot
            ViewModel = new DroneViewModel();
            DataContext = ViewModel;
            StartReceivingData();
        }

        private async void StartReceivingData()
        {
            await ViewModel.StartReceivingData();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!isDroneConnected)
            {
                try
                {
                    // Khởi chạy các ứng dụng
                    connectAll.StartUnrealEngine();
                    connectAll.StartPX4();
                    connectAll.StartQGroundControl();

                    // Cập nhật trạng thái giao diện
                    isDroneConnected = true;
                    txtStatus.Text = "Connected";
                    btnConnect.Content = "Disconnect";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to connect: {ex.Message}");
                }
            }
            else
            {
                // Ngắt kết nối và dừng các ứng dụng
                connectAll.StopApplications();

                // Cập nhật trạng thái giao diện
                isDroneConnected = false;
                txtStatus.Text = "Disconnected";
                btnConnect.Content = "Connect";
            }
        }

        private void btnTakeoff_Click(object sender, RoutedEventArgs e)
        {
            if (isDroneConnected)
            {
                sendCommand.FlyToAltitude(50);
            }
            else
            {
                MessageBox.Show("Please connect to the drone first.");
            }
        }

        private void btnStartSim_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void btnArm_Click(object sender, RoutedEventArgs e)
        {
            if (isDroneConnected)
            {
                sendCommand.ArmDrone();
            }
            else
            {
                MessageBox.Show("Please connect to the drone first.");
            }
        }

        private void btnLand_Click(object sender, RoutedEventArgs e)
        {
            if (isDroneConnected)
            {
                sendCommand.Land();
            }
            else
            {
                MessageBox.Show("Please connect to the drone first.");
            }
        }

        private void btnRTL_Click(object sender, RoutedEventArgs e)
        {
            if (isDroneConnected)
            {
                sendCommand.ReturnToLaunch();
            }
            else
            {
                MessageBox.Show("Please connect to the drone first.");
            }
        }

        private void btnStopSim_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnLoadMission_Click(object sender, RoutedEventArgs e)
        {
            if (isDroneConnected)
            {
                mission.LoadMission();
            }
            else
            {
                MessageBox.Show("Please connect to the drone first.");
            }
        }

        private void btnStartMission_Click(object sender, RoutedEventArgs e)
        {
            if (isDroneConnected)
            {
                mission.StartMission();
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
    }
}
