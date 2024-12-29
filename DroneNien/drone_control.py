#!/usr/bin/env python3

import sys
import time
from pymavlink import mavutil
from threading import Thread

class DroneController:
    def __init__(self):
        # Connect to the drone (using SITL for simulation)
        self.connection = mavutil.mavlink_connection('udp:127.0.0.1:14550')
        self.connection.wait_heartbeat()
        print("Connected to drone")
        
        # Start telemetry thread
        self.telemetry_thread = Thread(target=self.telemetry_loop)
        self.telemetry_thread.daemon = True
        self.telemetry_thread.start()

    def arm(self):
        self.connection.mav.command_long_send(
            self.connection.target_system,
            self.connection.target_component,
            mavutil.mavlink.MAV_CMD_COMPONENT_ARM_DISARM,
            0, 1, 0, 0, 0, 0, 0, 0)
        print("Arming command sent")

    def takeoff(self, altitude=10):
        self.connection.mav.command_long_send(
            self.connection.target_system,
            self.connection.target_component,
            mavutil.mavlink.MAV_CMD_NAV_TAKEOFF,
            0, 0, 0, 0, 0, 0, 0, altitude)
        print(f"Takeoff command sent, target altitude: {altitude}m")

    def land(self):
        self.connection.mav.command_long_send(
            self.connection.target_system,
            self.connection.target_component,
            mavutil.mavlink.MAV_CMD_NAV_LAND,
            0, 0, 0, 0, 0, 0, 0, 0)
        print("Land command sent")

    def return_to_launch(self):
        self.connection.mav.command_long_send(
            self.connection.target_system,
            self.connection.target_component,
            mavutil.mavlink.MAV_CMD_NAV_RETURN_TO_LAUNCH,
            0, 0, 0, 0, 0, 0, 0, 0)
        print("Return to Launch command sent")

    def telemetry_loop(self):
        while True:
            msg = self.connection.recv_match(blocking=True)
            if msg is not None:
                msg_type = msg.get_type()
                if msg_type == "GLOBAL_POSITION_INT":
                    # Convert altitude from millimeters to meters
                    alt = msg.relative_alt / 1000.0
                    print(f"Altitude: {alt:.1f}m")
                elif msg_type == "VFR_HUD":
                    print(f"Groundspeed: {msg.groundspeed:.1f}m/s")
                elif msg_type == "BATTERY_STATUS":
                    print(f"Battery: {msg.battery_remaining}%")

def main():
    controller = DroneController()
    
    while True:
        try:
            command = input().strip().lower()
            if command == "arm":
                controller.arm()
            elif command == "takeoff":
                controller.takeoff()
            elif command == "land":
                controller.land()
            elif command == "rtl":
                controller.return_to_launch()
            elif command == "exit":
                break
        except KeyboardInterrupt:
            break
        except Exception as e:
            print(f"Error: {str(e)}")

if __name__ == "__main__":
    main()