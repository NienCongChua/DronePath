using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using static MAVLink;

namespace DroneNien
{
    public class DroneViewModel : INotifyPropertyChanged
    {
        private double _altitude;
        private double _speed;
        private double _battery;
        private string? _gps;
        private string? _status;
        private string? _mode;

        public double Altitude
        {
            get => _altitude;
            set
            {
                _altitude = value;
                OnPropertyChanged();
            }
        }

        public double Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnPropertyChanged();
            }
        }

        public double Battery
        {
            get => _battery;
            set
            {
                _battery = value;
                OnPropertyChanged();
            }
        }

        public string GPS
        {
            get => _gps ?? string.Empty;
            set
            {
                _gps = value;
                OnPropertyChanged();
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

        public event PropertyChangedEventHandler ? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public async Task StartReceivingData()
        {
            var mavlink = new MavlinkParse();
            using var client = new UdpClient(14555);

            await Task.Run(async () =>
            {
                while (true)
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
            });
        }
    }
}
