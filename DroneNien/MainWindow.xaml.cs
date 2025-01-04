using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.WebSockets;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using static MAVLink;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Windows.Threading;
using System.Windows.Controls;
using Microsoft.Win32;

// using System.Drawing.Common;

namespace DroneNien
{
    public partial class MainWindow : Window
    {
        private bool isDroneConnected = false;
        private ConnectAll connectAll = new ConnectAll();
        private SendCommand sendCommand = new SendCommand();
        private LoadPython loadPython = new LoadPython();
        private ProcessDisplay processDisplay;


        public MainWindow()
        {
            InitializeComponent();
            sendCommand.ConnectToUDPPort();
            processDisplay = new ProcessDisplay(SlideshowImage); 
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
            sendCommand._SendCommand("TAKEOFF", param7: 5); // Set height to 5 meters
        }

        private void btnStartSim_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void btnArm_Click(object sender, RoutedEventArgs e)
        {
            sendCommand._SendCommand("ARM");
        }

        private void btnLand_Click(object sender, RoutedEventArgs e)
        {
            sendCommand._SendCommand("LAND");
        }

        private void btnRTL_Click(object sender, RoutedEventArgs e)
        {
            sendCommand._SendCommand("RTL");
        }

        private void btnStopSim_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnLoadMission_Click(object sender, RoutedEventArgs e)
        {
            // Implementation for loading a mission
        }

        private void btnStartMission_Click(object sender, RoutedEventArgs e)
        {
            // CheckConnection();   
        }

        private async void btnLoadPythonFile_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => loadPython.ProcessPythonFile());
        }
    }
}
