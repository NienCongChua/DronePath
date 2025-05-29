from __future__ import print_function
from dronekit import connect, Command, LocationGlobal
from pymavlink import mavutil
import time, sys, argparse, math
# Set up option parsing to get connection string
import argparse
from mission import*
import msgpack


connection_string       = '127.0.0.1:14551'
import_mission_filename = 'D:/code/drone/waypoints/B3.waypoints'
export_mission_filename = 'exportedmission.txt'

# parser = argparse.ArgumentParser(description='Demonstrates mission import/export from a file.')
# parser.add_argument('--connect',
#                     help="127.0.0.1:14580")
# args = parser.parse_args()

# connection_string = args.connect
#connection_string       = '127.0.0.1:14540'
sitl = None

# Start SITL if no connection string specified
if not connection_string:
    import dronekit_sitl
    sitl = dronekit_sitl.start_default()
    connection_string = sitl.connection_string()

# Connect to the Vehicle
print('Connecting to vehicle on: %s' % connection_string)
vehicle = connect(connection_string, wait_ready=True)
while not vehicle.is_armable:
    print(" Waiting for vehicle to initialise...")
    time.sleep(1)

# Upload mission from file
upload_mission(vehicle, import_mission_filename)
arm_and_takeoff(vehicle, 50)
vehicle.mode = VehicleMode("AUTO")
vehicle.groundspeed = 20

while True:
    nextwaypoint = vehicle.commands.next
    time.sleep(1)
    if nextwaypoint == vehicle.commands.count and distance_to_current_waypoint(vehicle) < 1:
        break;
    print('Obstacles Scanning')

time.sleep(2)
print('Return to launch')
vehicle.mode = VehicleMode("RTL")





#me here



# Close vehicle object before exiting script
print("Close vehicle object")
vehicle.close()



print("\nShow original and uploaded/downloaded files:")
# Print original file (for demo purposes only)
printfile(import_mission_filename)
# Print exported file (for demo purposes only)
#printfile(export_mission_filename)