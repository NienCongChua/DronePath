using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using static MAVLink;

namespace DroneNien
{
    public class DroneViewModel : INotifyPropertyChanged
    {
        private double _altitude = 0;
        private double _speed = 0;
        private double _battery = 0;
        private string? _gps = "None";
        private string? _status = "Disconnected";
        private string? _mode = "None";

        public double Altitude
        {
            get => _altitude;
            set
            {
                _altitude = Math.Round(value, 2);
                OnPropertyChanged();
                OnPropertyChanged(nameof(AltitudeWithUnit));
            }
        }

        public double Speed
        {
            get => _speed;
            set
            {
                _speed = Math.Round(value, 2);
                OnPropertyChanged();
                OnPropertyChanged(nameof(SpeedWithUnit));
            }
        }

        public double Battery
        {
            get => _battery;
            set
            {
                _battery = Math.Round(value, 2);
                OnPropertyChanged();
                OnPropertyChanged(nameof(BatteryWithUnit));
            }
        }

        public string GPS
        {
            get => _gps ?? string.Empty;
            set
            {
                _gps = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GPSWithUnit));
            }
        }

        public string Status
        {
            get => _status ?? string.Empty;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public string Mode
        {
            get => _mode ?? string.Empty;
            set
            {
                _mode = value;
                OnPropertyChanged();
            }
        }

        public string AltitudeWithUnit => $"{Altitude} m";
        public string SpeedWithUnit => $"{Speed} m/s";
        public string BatteryWithUnit => $"{Battery} %";
        public string GPSWithUnit => $"{GPS}";

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task StartReceivingData()
        {
            var mavlink = new MavlinkParse();
            using var client = new UdpClient(14555);

            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var result = await client.ReceiveAsync();
                        var packet = mavlink.ReadPacket(new MemoryStream(result.Buffer));

                        if (packet != null)
                        {
                            switch (packet.msgid)
                            {
                                case 33: // MAVLINK_MSG_ID_GLOBAL_POSITION_INT
                                    var globalPosition = packet.ToStructure<mavlink_global_position_int_t>();
                                    Altitude = globalPosition.relative_alt / 1000.0;
                                    break;
                                case 24: // MAVLINK_MSG_ID_GPS_RAW_INT
                                    var gpsRaw = packet.ToStructure<mavlink_gps_raw_int_t>();
                                    GPS = $"{gpsRaw.lat / 1E7}, {gpsRaw.lon / 1E7}";
                                    break;
                                case 30: // MAVLINK_MSG_ID_ATTITUDE
                                    var attitude = packet.ToStructure<mavlink_attitude_t>();
                                    Speed = attitude.yawspeed;
                                    break;
                                case 147: // MAVLINK_MSG_ID_BATTERY_STATUS
                                    var batteryStatus = packet.ToStructure<mavlink_battery_status_t>();
                                    Battery = batteryStatus.battery_remaining;
                                    break;
                                case 0: // MAVLINK_MSG_ID_HEARTBEAT
                                    var heartbeat = packet.ToStructure<mavlink_heartbeat_t>();
                                    Mode = heartbeat.custom_mode.ToString();
                                    Status = heartbeat.system_status.ToString();
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the exception as needed
                        Console.WriteLine($"Error receiving data: {ex.Message}");
                    }
                }
            });
        }
    }
}
