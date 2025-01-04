using System.Net.Sockets;
using static MAVLink;
using System.Windows;
using System.Net;
using System.Threading.Tasks;
using System.IO;

namespace DroneNien
{
    internal class SendCommand
    {
        private MavlinkParse mavlinkParser = new MavlinkParse();
        private UdpClient udpClient = new UdpClient();
        // private UdpClient udpListener = new UdpClient(14555);


        public void ConnectToUDPPort()
        {
            try
            {
                // Define the endpoint for PX4-Autopilot
                IPEndPoint px4EndPoint = new IPEndPoint(IPAddress.Loopback, 14556);

                // Connect to PX4-Autopilot
                udpClient.Connect(px4EndPoint);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to PX4-Autopilot: {ex.Message}");
            }
        }

        private void _SendCommand(string command,
            float param1 = 0,
            float param2 = 0,
            float param3 = 0,
            float param4 = 0,
            float param5 = 0,
            float param6 = 0,
            float param7 = 0)
        {

            try
            {
                MAVLink.mavlink_command_long_t cmd = new MAVLink.mavlink_command_long_t
                {
                    command = (ushort)Enum.Parse(typeof(MAVLink.MAV_CMD), command.ToUpper()),
                    target_system = 1,
                    target_component = 1,
                    confirmation = 0,
                    param1 = param1,
                    param2 = param2,
                    param3 = param3,
                    param4 = param4,
                    param5 = param5,
                    param6 = param6,
                    param7 = param7
                };

                byte[] packet = mavlinkParser.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.COMMAND_LONG, cmd);
                udpClient.Send(packet, packet.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send command: {ex.Message}");
            }
        }

        public void ArmDrone()
        {
            _SendCommand("COMPONENT_ARM_DISARM", param1: 1);
        }

        public void ReturnToLaunch()
        {
            _SendCommand("RETURN_TO_LAUNCH");
        }

        public void FlyToAltitude(float altitude)
        {
            _SendCommand("TAKEOFF", param7: altitude);
        }

        public void Land()
        {
            _SendCommand("LAND");
        }
    }
}
